// Services/Email/IEmailSender.cs
#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace FASTSURVEY.Services.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    }
}
