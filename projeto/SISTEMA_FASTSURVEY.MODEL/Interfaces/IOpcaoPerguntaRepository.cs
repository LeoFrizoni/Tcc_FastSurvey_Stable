#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IOpcaoPerguntaRepository : IRepository<OpcoesPergunta>
    {
        Task<List<OpcoesPergunta>> ListarPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        );

        Task<List<OpcoesPergunta>> ListarAtivasPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        );

        // Conveniências:
        Task<int> CountPorPerguntaAsync(int perguntaId, CancellationToken ct = default);
        Task<int> CountAtivasPorPerguntaAsync(int perguntaId, CancellationToken ct = default);
        Task<int> CountCorretasPorPerguntaAsync(int perguntaId, CancellationToken ct = default);

        Task<bool> ExisteTextoDuplicadoAsync(
            int perguntaId,
            string textoNormalizado,
            CancellationToken ct = default
        );

        /// <summary>
        /// Variante case-insensitive sem ToLower() na coluna (Postgres/Npgsql).
        /// Mantém o método original para compatibilidade.
        /// </summary>
        Task<bool> ExisteTextoDuplicadoInsensitiveAsync(
            int perguntaId,
            string texto,
            CancellationToken ct = default
        );

        Task<List<int>> ListarIdsCorretasAsync(int perguntaId, CancellationToken ct = default);

        Task<OpcoesPergunta?> ObterPorOrdemAsync(
            int perguntaId,
            int ordem,
            CancellationToken ct = default
        );

        /// <summary>Consulta somente leitura para compor no service (projeções/paginação custom).</summary>
        IQueryable<OpcoesPergunta> QueryPorPergunta(int perguntaId);

        // ---------- Opcionais úteis (não-quebrantes) ----------
        Task<int> GetMaxOrdemAsync(int perguntaId, CancellationToken ct = default);

        /// <summary>Remove todas as opções de uma pergunta. Retorna linhas afetadas.</summary>
        Task<int> DeleteByPerguntaIdAsync(int perguntaId, CancellationToken ct = default);
    }
}
