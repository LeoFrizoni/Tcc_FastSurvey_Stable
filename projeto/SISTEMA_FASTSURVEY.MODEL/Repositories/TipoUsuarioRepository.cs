#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class TipoUsuarioRepository : Repository<TipoUsuario>, ITipoUsuarioRepository
    {
        public TipoUsuarioRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<List<TipoUsuario>> ListarAsync(
            bool incluirDesabilitados = false, // mantido por compatibilidade; ignorado
            CancellationToken ct = default
        )
        {
            // Model não tem "Desabilitado": retornamos todos, ordenados por nome
            return await _set.AsNoTracking()
                .OrderBy(x => x.TipoUsuario1 ?? string.Empty)
                .ThenBy(x => x.TipoUsuarioId)
                .ToListAsync(ct);
        }

        public async Task<TipoUsuario?> ObterAsync(int id, CancellationToken ct = default)
        {
            return await _set.AsNoTracking().FirstOrDefaultAsync(t => t.TipoUsuarioId == id, ct);
        }

        public async Task<TipoUsuario?> ObterPorNomeAsync(
            string nome,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(nome))
                return null;

            nome = nome.Trim();
            // Se estiver usando Npgsql, pode usar ILIKE:
            // return await _set.AsNoTracking()
            //     .FirstOrDefaultAsync(t => EF.Functions.ILike(t.TipoUsuario1!, nome), ct);

            return await _set.AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.TipoUsuario1 != null && t.TipoUsuario1.ToLower() == nome.ToLower(),
                    ct
                );
        }

        public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        {
            return await _set.AsNoTracking().AnyAsync(t => t.TipoUsuarioId == id, ct);
        }
    }
}
