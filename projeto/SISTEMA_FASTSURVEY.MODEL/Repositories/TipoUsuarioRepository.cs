#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class TipoUsuarioRepository : Repository<Tipousuario>, ITipoUsuarioRepository
    {
        public TipoUsuarioRepository(FastSurveyContext context) : base(context) { }

        public async Task<List<Tipousuario>> ListarAsync(bool incluirDesabilitados = false, CancellationToken ct = default)
        {
            var query = _set.AsNoTracking();
            
            // Note: Tipousuario model doesn't have Desabilitado property, so we just return all items
            // In the future, if needed, add a Desabilitado field to the database and model
            
            return await query.OrderBy(x => x.Tipousuario1) // usa o nome (string) para ordenar
                             .ToListAsync(ct);
        }
    }
}
