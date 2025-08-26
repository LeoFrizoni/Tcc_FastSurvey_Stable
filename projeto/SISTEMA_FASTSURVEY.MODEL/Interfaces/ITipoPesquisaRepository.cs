#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface ITipoPesquisaRepository : IRepository<TipoPesquisa>
    {
        Task<List<TipoPesquisa>> ListarAtivosAsync(CancellationToken ct = default);

        /// <summary>Busca por ID, opcionalmente exigindo que esteja ativo.</summary>
        Task<TipoPesquisa?> ObterAsync(
            int id,
            bool somenteAtivo = true,
            CancellationToken ct = default
        );

        /// <summary>Busca por nome (case-insensitive), opcionalmente exigindo ativo.</summary>
        Task<TipoPesquisa?> ObterPorNomeAsync(
            string nome,
            bool somenteAtivo = true,
            CancellationToken ct = default
        );

        /// <summary>Verifica existência por ID, opcionalmente apenas ativos.</summary>
        Task<bool> ExisteAsync(int id, bool somenteAtivo = true, CancellationToken ct = default);
    }
}
