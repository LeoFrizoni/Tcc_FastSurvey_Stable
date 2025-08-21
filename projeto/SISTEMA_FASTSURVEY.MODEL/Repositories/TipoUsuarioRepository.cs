#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public interface ITipoUsuarioRepository : IRepository<Tipousuario>
    {
        Task<List<Tipousuario>> ListarAsync(CancellationToken ct = default);
    }

    public class TipoUsuarioRepository : Repository<Tipousuario>, ITipoUsuarioRepository
    {
        public TipoUsuarioRepository(FastSurveyContext context) : base(context) { }

        public async Task<List<Tipousuario>> ListarAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                             .OrderBy(x => x.Tipousuario1) // usa o nome (string) para ordenar
                             .ToListAsync(ct);
        }
    }
}
