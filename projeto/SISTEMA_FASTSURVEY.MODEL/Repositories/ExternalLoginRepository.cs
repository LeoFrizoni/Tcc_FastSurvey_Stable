using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Threading.Tasks;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class ExternalLoginRepository : Repository<Externallogins>, IExternalLoginRepository
    {
        private readonly FastSurveyContext _context;

        public ExternalLoginRepository(FastSurveyContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Externallogins> GetByProviderAsync(string provider, string providerUserId)
        {
            return await _context.Externallogins
                .FirstOrDefaultAsync(x => x.Provider == provider && x.Provideruserid == providerUserId);
        }
    }
}
