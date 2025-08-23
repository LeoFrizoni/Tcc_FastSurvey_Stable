// FASTSURVEY/Services/Pesquisa/IPesquisaService.cs
#nullable enable
using FASTSURVEY.Dtos.Pesquisas;

namespace FASTSURVEY.Services.Pesquisa
{
    public interface IPesquisaService
    {
        // CRUD / opera��es principais
        Task<PesquisaResponse?> ObterPorIdAsync(int id, CancellationToken ct = default);
        Task<PagedResult<PesquisaListItemResponse>> ListarAsync(PesquisaFiltroRequest filtro, CancellationToken ct = default);
        Task<int> CriarAsync(CriarPesquisaRequest req, CancellationToken ct = default);
        Task<bool> AtualizarAsync(int id, AtualizarPesquisaRequest req, CancellationToken ct = default);
        Task<bool> AtualizarTemplateAsync(int id, string templateJson, CancellationToken ct = default);
        Task<bool> ExcluirAsync(int id, CancellationToken ct = default);

        // ===== Endpoints usados pelo Home.jsx =====
        /// <summary>
        /// Lista pesquisas de um usu�rio espec�fico (lista simples, sem pagina��o).
        /// Compat�vel com GET /api/pesquisas/usuario/{loginId}.
        /// </summary>
        Task<List<PesquisaListItemResponse>> ListarPorLoginAsync(int loginId, CancellationToken ct = default);

        /// <summary>
        /// Define/atualiza a pasta de uma pesquisa (null = sem pasta).
        /// Compat�vel com PATCH /api/pesquisas/{id}/mover-pasta.
        /// </summary>
        Task<bool> DefinirPastaAsync(int pesquisaId, int? pastaId, CancellationToken ct = default);
    }
}
