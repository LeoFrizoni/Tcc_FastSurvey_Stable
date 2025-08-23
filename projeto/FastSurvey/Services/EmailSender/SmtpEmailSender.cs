#nullable enable
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FASTSURVEY.Services.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            var host = _cfg["Email:SmtpServer"];
            var portStr = _cfg["Email:SmtpPort"];
            var username = _cfg["Email:Username"];
            var password = _cfg["Email:Password"];
            var enableSsl = bool.TryParse(_cfg["Email:EnableSsl"], out var ssl) ? ssl : true;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Configura��o SMTP incompleta. Verifique Email:SmtpServer, Email:Username e Email:Password");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("FastSurvey", username));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            var port = int.TryParse(portStr, out var p) ? p : 587;

            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(host, port, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
                await smtp.AuthenticateAsync(username, password, ct);
                await smtp.SendAsync(message, ct);
                await smtp.DisconnectAsync(true, ct);
            }
            catch (Exception ex)
            {
                // Em desenvolvimento, apenas logar o erro
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                // Em produ��o, voc� pode querer re-throw ou logar em um sistema de logs
            }
        }
    }
}
