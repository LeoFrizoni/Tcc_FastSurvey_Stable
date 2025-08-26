#nullable enable
using System.Linq; // Where/OrderBy/Select
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class OpcaoPerguntaRepository : Repository<OpcoesPergunta>, IOpcaoPerguntaRepository
    {
        public OpcaoPerguntaRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<List<OpcoesPergunta>> ListarPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .Where(o => o.PerguntaId == perguntaId)
                .OrderBy(o => o.Ordem)
                .ThenBy(o => o.OpcaoId) // estabilidade
                .ToListAsync(ct);
        }

        public async Task<List<OpcoesPergunta>> ListarAtivasPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .Where(o => o.PerguntaId == perguntaId && o.Ativa)
                .OrderBy(o => o.Ordem)
                .ThenBy(o => o.OpcaoId)
                .ToListAsync(ct);
        }

        // ---------- Conveniências ----------
        public IQueryable<OpcoesPergunta> QueryPorPergunta(int perguntaId)
        {
            return _set.AsNoTracking()
                .Where(o => o.PerguntaId == perguntaId)
                .OrderBy(o => o.Ordem)
                .ThenBy(o => o.OpcaoId);
        }

        public async Task<int> CountPorPerguntaAsync(int perguntaId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking().CountAsync(o => o.PerguntaId == perguntaId, ct);
        }

        public async Task<int> CountAtivasPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .CountAsync(o => o.PerguntaId == perguntaId && o.Ativa, ct);
        }

        public async Task<int> CountCorretasPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .CountAsync(o => o.PerguntaId == perguntaId && o.Correta, ct);
        }

        public async Task<bool> ExisteTextoDuplicadoAsync(
            int perguntaId,
            string textoNormalizado,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(textoNormalizado))
                return false;

            var norm = textoNormalizado.Trim().ToLowerInvariant();

            // Mantém sua versão original (case-insensitive via ToLower); pode afetar uso de índice
            return await _set.AsNoTracking()
                .AnyAsync(o => o.PerguntaId == perguntaId && o.Texto.ToLower() == norm, ct);
        }

        public async Task<bool> ExisteTextoDuplicadoInsensitiveAsync(
            int perguntaId,
            string texto,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            var norm = texto.Trim();
            // Postgres/Npgsql: ILIKE (case-insensitive) sem mexer na coluna
            return await _set.AsNoTracking()
                .AnyAsync(o => o.PerguntaId == perguntaId && EF.Functions.ILike(o.Texto, norm), ct);

            // Se não usar Npgsql, fallback seria ToLower(), já implementado acima.
        }

        public async Task<List<int>> ListarIdsCorretasAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .Where(o => o.PerguntaId == perguntaId && o.Correta)
                .OrderBy(o => o.Ordem)
                .ThenBy(o => o.OpcaoId)
                .Select(o => o.OpcaoId)
                .ToListAsync(ct);
        }

        public async Task<OpcoesPergunta?> ObterPorOrdemAsync(
            int perguntaId,
            int ordem,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .FirstOrDefaultAsync(o => o.PerguntaId == perguntaId && o.Ordem == ordem, ct);
        }

        // ---------- Opcionais úteis ----------
        public async Task<int> GetMaxOrdemAsync(int perguntaId, CancellationToken ct = default)
        {
            // Se não existir nenhuma, Max retorna null → coalesce para 0
            var maxOrdem = await _set.AsNoTracking()
                .Where(o => o.PerguntaId == perguntaId)
                .Select(o => (int?)o.Ordem)
                .MaxAsync(ct);

            return maxOrdem ?? 0;
        }

        public async Task<int> DeleteByPerguntaIdAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            // EF Core 7+: apaga direto; retorna linhas afetadas
            return await _set.Where(o => o.PerguntaId == perguntaId).ExecuteDeleteAsync(ct);

            // Versão anterior (se estiver em EF Core 6):
            // var list = await _set.Where(o => o.PerguntaId == perguntaId).ToListAsync(ct);
            // if (list.Count == 0) return 0;
            // _set.RemoveRange(list);
            // return list.Count;
        }
    }
}
