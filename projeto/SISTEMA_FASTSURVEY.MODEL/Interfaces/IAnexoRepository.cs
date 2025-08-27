#nullable enable
using AnexoEntity = SISTEMA_FASTSURVEY.MODEL.Models.Anexos;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IAnexoRepository : IRepository<AnexoEntity>
    {
        Task<List<AnexoEntity>> ListarPorPesquisaAsync(
            int PesquisaId,
            CancellationToken ct = default
        );

        Task<List<AnexoEntity>> ListarPorPerguntaAsync(
            int PerguntaId,
            CancellationToken ct = default
        );

        // Aditivos não‑quebrantes
        Task<bool> ExisteNaPesquisaAsync(
            int pesquisaId,
            string nome,
            CancellationToken ct = default
        );

        /// <summary>
        /// Variante case-insensitive (útil no Postgres/Npgsql com ILIKE).
        /// Mantém o original para compatibilidade.
        /// </summary>
        Task<bool> ExisteNaPesquisaInsensitiveAsync(
            int pesquisaId,
            string nome,
            CancellationToken ct = default
        );

        Task<int> CountPorPesquisaAsync(int pesquisaId, CancellationToken ct = default);

        /// <summary>Consulta componível (apenas leitura).</summary>
        IQueryable<AnexoEntity> QueryPorPesquisa(int pesquisaId);

        /// <summary>Consulta componível por pergunta (simetria com pesquisa).</summary>
        IQueryable<AnexoEntity> QueryPorPergunta(int perguntaId);
    }
}
