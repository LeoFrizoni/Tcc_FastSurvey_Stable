// Services/Email/SmtpEmailSender.cs
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace FASTSURVEY.Services.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration cfg, ILogger<SmtpEmailSender> logger)
        {
            _cfg = cfg;
            _logger = logger;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            CancellationToken ct = default
        )
        {
            _logger.LogInformation("Iniciando envio de email para: {ToEmail}", toEmail);
            
            // === Config ===
            var host = _cfg["Email:SmtpServer"];
            var portStr = _cfg["Email:SmtpPort"];
            var username = _cfg["Email:Username"];
            var password = _cfg["Email:Password"];
            var enableSsl = bool.TryParse(_cfg["Email:EnableSsl"], out var ssl) ? ssl : true;
            var fromName = _cfg["Email:FromName"] ?? "FastSurvey";
            var fromAddress = _cfg["Email:FromAddress"] ?? username; // fallback

            _logger.LogInformation("Configuração SMTP - Host: {Host}, Port: {Port}, Username: {Username}, SSL: {Ssl}", 
                host, portStr, username, enableSsl);

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Configuração SMTP ausente: Email:SmtpServer.");
            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Configuração SMTP ausente: Email:Username.");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Configuração SMTP ausente: Email:Password.");
            if (string.IsNullOrWhiteSpace(fromAddress))
                throw new InvalidOperationException(
                    "Configuração SMTP ausente: Email:FromAddress (ou Username)."
                );
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Destino (toEmail) é obrigatório.", nameof(toEmail));

            var port = int.TryParse(portStr, out var p) ? p : 587;

            // === Mensagem ===
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(toEmail, toEmail)); // nome = email (ok); adapte se tiver display name
            message.Subject = subject ?? string.Empty;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = StripHtml(htmlBody ?? string.Empty), // fallback texto puro para clientes sem HTML
            };
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            // timeouts razoáveis
            smtp.Timeout = 20_000; // 20s

            try
            {
                _logger.LogInformation("Conectando ao servidor SMTP {Host}:{Port}", host, port);
                
                // Para Gmail, usar configurações específicas
                SecureSocketOptions socketOpts;
                if (host.Contains("gmail.com"))
                {
                    if (port == 465)
                        socketOpts = SecureSocketOptions.SslOnConnect;
                    else if (port == 587)
                        socketOpts = SecureSocketOptions.StartTls;
                    else
                        socketOpts = SecureSocketOptions.StartTls;
                }
                else
                {
                    // Decide o modo TLS de forma previsível para outros servidores:
                    // - 465: SSL na conexão
                    // - 587: STARTTLS (se enableSsl)
                    // - outros: Auto (ou StartTls se enableSsl=true)
                    if (port == 465 && enableSsl)
                        socketOpts = SecureSocketOptions.SslOnConnect;
                    else if (port == 587 && enableSsl)
                        socketOpts = SecureSocketOptions.StartTls;
                    else
                        socketOpts = enableSsl
                            ? SecureSocketOptions.StartTlsWhenAvailable
                            : SecureSocketOptions.Auto;
                }

                _logger.LogInformation("Conectando com opções de socket: {SocketOpts}", socketOpts);
                await smtp.ConnectAsync(host, port, socketOpts, ct);
                _logger.LogInformation("Conectado ao servidor SMTP");
                
                _logger.LogInformation("Autenticando com usuário: {Username}", username);
                await smtp.AuthenticateAsync(username, password, ct);
                _logger.LogInformation("Autenticado com sucesso");
                
                _logger.LogInformation("Enviando mensagem...");
                await smtp.SendAsync(message, ct);
                _logger.LogInformation("Mensagem enviada com sucesso");
                
                await smtp.DisconnectAsync(true, ct);
                _logger.LogInformation("Desconectado do servidor SMTP");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("Envio de e-mail cancelado. Para: {To}", toEmail);
                throw;
            }
            catch (Exception ex)
            {
                // Log estruturado e relançar exceção para debug
                _logger.LogError(
                    ex,
                    "Erro ao enviar e-mail. Para: {To}; Assunto: {Subject}; Host: {Host}; Port: {Port}",
                    toEmail,
                    subject,
                    host,
                    port
                );
                // Relançar exceção para debug
                throw;
            }
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;
            // Fallback simples: remove tags básicas
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}
