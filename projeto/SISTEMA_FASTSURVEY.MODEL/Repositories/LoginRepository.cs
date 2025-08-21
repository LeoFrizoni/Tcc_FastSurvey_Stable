#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class LoginRepository : Repository<Login>, ILoginRepository
    {
        public LoginRepository(FastSurveyContext context) : base(context) { }

        public async Task<Login?> ObterPorEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var norm = email.Trim().ToLowerInvariant();

            return await _set
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Email.ToLower() == norm, ct);
        }
    }
}
