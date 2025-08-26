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
            // === Config ===
            var host = _cfg["Email:SmtpServer"];
            var portStr = _cfg["Email:SmtpPort"];
            var username = _cfg["Email:Username"];
            var password = _cfg["Email:Password"];
            var enableSsl = bool.TryParse(_cfg["Email:EnableSsl"], out var ssl) ? ssl : true;
            var fromName = _cfg["Email:FromName"] ?? "FastSurvey";
            var fromAddress = _cfg["Email:FromAddress"] ?? username; // fallback

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
                // Decide o modo TLS de forma previsível:
                // - 465: SSL na conexão
                // - 587: STARTTLS (se enableSsl)
                // - outros: Auto (ou StartTls se enableSsl=true)
                SecureSocketOptions socketOpts;
                if (port == 465 && enableSsl)
                    socketOpts = SecureSocketOptions.SslOnConnect;
                else if (port == 587 && enableSsl)
                    socketOpts = SecureSocketOptions.StartTls;
                else
                    socketOpts = enableSsl
                        ? SecureSocketOptions.StartTlsWhenAvailable
                        : SecureSocketOptions.Auto;

                await smtp.ConnectAsync(host, port, socketOpts, ct);
                await smtp.AuthenticateAsync(username, password, ct);
                await smtp.SendAsync(message, ct);
                await smtp.DisconnectAsync(true, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("Envio de e-mail cancelado. Para: {To}", toEmail);
                throw;
            }
            catch (Exception ex)
            {
                // Log estruturado sem estourar exceção em produção (ajuste conforme política do app)
                _logger.LogError(
                    ex,
                    "Erro ao enviar e-mail. Para: {To}; Assunto: {Subject}",
                    toEmail,
                    subject
                );
                // Se preferir falhar a chamada, re-lance:
                // throw;
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
