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

        public async Task<TipoUsuarioCatalogDto> CadastrarAsync(string tipoUsuario, CancellationToken ct = default)
        {
            var novo = new SISTEMA_FASTSURVEY.MODEL.Models.TipoUsuario
            {
                TipoUsuario1 = tipoUsuario.Trim()
            };

            _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.TipoUsuario>().Add(novo);
            await _ctx.SaveChangesAsync(ct);

            return new TipoUsuarioCatalogDto
            {
                TipoUsuarioId = novo.TipoUsuarioId,
                TipoUsuario = novo.TipoUsuario1
            };
        }

        public async Task<bool> AlterarAsync(int id, string tipoUsuario, CancellationToken ct = default)
        {
            var existente = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.TipoUsuario>()
                .FirstOrDefaultAsync(x => x.TipoUsuarioId == id, ct);

            if (existente == null)
                return false;

            existente.TipoUsuario1 = tipoUsuario.Trim();
            await _ctx.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> ExcluirAsync(int id, CancellationToken ct = default)
        {
            var existente = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.TipoUsuario>()
                .FirstOrDefaultAsync(x => x.TipoUsuarioId == id, ct);

            if (existente == null)
                return false;

            // Verificar se há usuários usando este tipo
            var usuariosComTipo = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.Login>()
                .AnyAsync(x => x.TipoUsuarioId == id, ct);

            if (usuariosComTipo)
                throw new InvalidOperationException("Não é possível excluir um tipo de usuário que está sendo usado.");

            _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.TipoUsuario>().Remove(existente);
            await _ctx.SaveChangesAsync(ct);

            return true;
        }
    }
}
