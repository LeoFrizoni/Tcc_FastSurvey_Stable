// FASTSURVEY/Services/Tipos/TipoUsuarioService.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Tipos;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models; // TipoUsuario, FastSurveyContext

namespace FASTSURVEY.Services.Tipos
{
    public class TipoUsuarioService : ITipoUsuarioService
    {
        private readonly FastSurveyContext _ctx;

        public TipoUsuarioService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<IReadOnlyList<TipoUsuarioCatalogDto>> ListarAsync(
            CancellationToken ct = default
        )
        {
            return await _ctx.Set<TipoUsuario>()
                .AsNoTracking()
                .OrderBy(x => string.IsNullOrWhiteSpace(x.TipoUsuario1) ? null : x.TipoUsuario1)
                .ThenBy(x => x.TipoUsuarioId)
                .Select(x => new TipoUsuarioCatalogDto
                {
                    TipoUsuarioId = x.TipoUsuarioId,
                    TipoUsuario = x.TipoUsuario1 ?? string.Empty,
                })
                .ToListAsync(ct);
        }
    }
}
