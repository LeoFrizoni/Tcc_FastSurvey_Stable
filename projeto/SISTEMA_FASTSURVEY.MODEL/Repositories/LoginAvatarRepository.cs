#nullable enable
using System.Linq; // Where/OrderBy/ThenBy
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class LoginAvatarRepository : Repository<LoginAvatar>, ILoginAvatarRepository
    {
        public LoginAvatarRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<LoginAvatar?> GetByLoginIdAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            // Pega o avatar "mais recente": AtualizadoEm (se houver) senão CriadoEm; fallback por AvatarId
            return await _set.AsNoTracking()
                .Where(x => x.LoginId == loginId)
                .OrderByDescending(x => x.AtualizadoEm ?? x.CriadoEm)
                .ThenByDescending(x => x.AvatarId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(int loginId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking().AnyAsync(x => x.LoginId == loginId, ct);
        }

        public async Task<int> DeleteByLoginIdAsync(int loginId, CancellationToken ct = default)
        {
            // Apaga direto no banco (EF Core 7+), retorna linhas afetadas
            var affected = await _set.Where(x => x.LoginId == loginId).ExecuteDeleteAsync(ct);

            return affected;

            // Versão antiga (mantida como comentário):
            // var list = await _set.Where(x => x.LoginId == loginId).ToListAsync(ct);
            // if (list.Count == 0) return 0;
            // _set.RemoveRange(list);
            // return list.Count;
        }

        public IQueryable<LoginAvatar> QueryByLoginId(int loginId)
        {
            // Consulta componível, já ordenada por "mais recente"
            return _set.AsNoTracking()
                .Where(x => x.LoginId == loginId)
                .OrderByDescending(x => x.AtualizadoEm ?? x.CriadoEm)
                .ThenByDescending(x => x.AvatarId);
        }
    }
}
