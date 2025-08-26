#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface ITipoPerguntaRepository : IRepository<TipoPergunta>
    {
        Task<List<TipoPergunta>> ListarAtivosAsync(CancellationToken ct = default);

        /// <summary>Busca por ID, opcionalmente exigindo que esteja ativo.</summary>
        Task<TipoPergunta?> ObterAsync(
            int id,
            bool somenteAtivo = true,
            CancellationToken ct = default
        );

        /// <summary>Busca por nome (case-insensitive), opcionalmente exigindo que esteja ativo.</summary>
        Task<TipoPergunta?> ObterPorNomeAsync(
            string nome,
            bool somenteAtivo = true,
            CancellationToken ct = default
        );

        /// <summary>Verifica existência por ID, opcionalmente apenas ativos.</summary>
        Task<bool> ExisteAsync(int id, bool somenteAtivo = true, CancellationToken ct = default);
    }
}
