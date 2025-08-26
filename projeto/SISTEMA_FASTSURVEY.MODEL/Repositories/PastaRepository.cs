#nullable enable
using System.Linq; // Where/OrderBy/ThenBy/Skip/Take
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class PastaRepository : Repository<Pastas>, IPastaRepository
    {
        public PastaRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<List<Pastas>> ListarPorLoginAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .Where(p => p.LoginId == loginId)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.PastaId) // estabilidade
                .ToListAsync(ct);
        }

        public async Task<List<Pastas>> ListarPorLoginAsync(
            int loginId,
            int skip,
            int take,
            CancellationToken ct = default
        )
        {
            if (skip < 0)
                skip = 0;
            if (take <= 0)
                take = 50;

            return await _set.AsNoTracking()
                .Where(p => p.LoginId == loginId)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.PastaId)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public IQueryable<Pastas> QueryPorLogin(int loginId)
        {
            return _set.AsNoTracking()
                .Where(p => p.LoginId == loginId)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.PastaId);
        }

        public async Task<Pastas?> ObterPorNomeAsync(
            int loginId,
            string nome,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(nome))
                return null;

            var norm = nome.Trim();

            // Comparação exata (mantida); se quiser case-insensitive e usa Postgres/Npgsql,
            // pode trocar por EF.Functions.ILike(p.Nome, norm)
            return await _set.AsNoTracking()
                .FirstOrDefaultAsync(p => p.LoginId == loginId && p.Nome == norm, ct);
        }

        public async Task<bool> ExisteComNomeAsync(
            int loginId,
            string nome,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var norm = nome.Trim();

            // Idem ao método acima: pode usar ILIKE se precisar de insensitive sem mexer em índice.
            return await _set.AsNoTracking()
                .AnyAsync(p => p.LoginId == loginId && p.Nome == norm, ct);
        }

        public async Task<int> CountPorLoginAsync(int loginId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking().CountAsync(p => p.LoginId == loginId, ct);
        }

        // ===== Operações sem SaveChanges (UoW decide) =====

        public async Task<Pastas> CriarAsync(
            int loginId,
            string nome,
            CancellationToken ct = default
        )
        {
            var entity = new Pastas { LoginId = loginId, Nome = (nome ?? string.Empty).Trim() };

            await _set.AddAsync(entity, ct);
            return entity; // SaveChangesAsync fica a cargo do Service/UoW
        }

        public async Task<int> RenomearAsync(
            int pastaId,
            string novoNome,
            CancellationToken ct = default
        )
        {
            var norm = (novoNome ?? string.Empty).Trim();

            // ⚡ EF Core 7+: atualiza direto no banco sem materializar
            var affected = await _set.Where(p => p.PastaId == pastaId)
                .ExecuteUpdateAsync(updates => updates.SetProperty(p => p.Nome, norm), ct);

            return affected;

            // Fallback EF Core 6:
            // var pasta = await _set.FirstOrDefaultAsync(p => p.PastaId == pastaId, ct);
            // if (pasta is null) return 0;
            // pasta.Nome = norm;
            // return 1;
        }

        public async Task<int> ExcluirPorIdAsync(int pastaId, CancellationToken ct = default)
        {
            // ⚡ EF Core 7+: delete direto, retorna linhas afetadas
            var affected = await _set.Where(p => p.PastaId == pastaId).ExecuteDeleteAsync(ct);

            return affected;

            // Fallback EF Core 6:
            // var pasta = await _set.FirstOrDefaultAsync(p => p.PastaId == pastaId, ct);
            // if (pasta is null) return 0;
            // _set.Remove(pasta);
            // return 1;
        }
    }
}
