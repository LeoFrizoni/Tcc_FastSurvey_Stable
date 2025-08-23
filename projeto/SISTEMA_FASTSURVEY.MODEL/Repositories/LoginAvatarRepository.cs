using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Threading.Tasks;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class LoginAvatarRepository : Repository<Loginavatar>, ILoginAvatarRepository
    {
        private readonly FastSurveyContext _context;

        public LoginAvatarRepository(FastSurveyContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Loginavatar> GetByLoginIdAsync(int loginId)
        {
            return await _context.Loginavatar
                .FirstOrDefaultAsync(x => x.Loginid == loginId);
        }
    }
}
