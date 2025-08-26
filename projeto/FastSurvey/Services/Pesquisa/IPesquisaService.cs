#nullable enable
using FASTSURVEY.Dtos.Pesquisas;

namespace FASTSURVEY.Services.Pesquisa
{
    public interface IPesquisaService
    {
        Task<PesquisaResponse?> ObterPorIdAsync(int id, CancellationToken ct = default);
        Task<PagedResult<PesquisaListItemResponse>> ListarAsync(
            PesquisaFiltroRequest filtro,
            CancellationToken ct = default
        );
        Task<int> CriarAsync(CriarPesquisaRequest req, CancellationToken ct = default);
        Task<bool> AtualizarAsync(
            int id,
            AtualizarPesquisaRequest req,
            CancellationToken ct = default
        );
        Task<bool> AtualizarTemplateAsync(
            int id,
            string templateJson,
            CancellationToken ct = default
        );
        Task<bool> ExcluirAsync(int id, CancellationToken ct = default);

        // Extras que teu Service já implementa
        Task<List<PesquisaListItemResponse>> ListarPorLoginAsync(
            int loginId,
            CancellationToken ct = default
        );
        Task<bool> DefinirPastaAsync(int pesquisaId, int? pastaId, CancellationToken ct = default);
        Task<PesquisaResponse?> ObterPorSlugAsync(string slug, CancellationToken ct = default);
        Task<int> DuplicarAsync(
            int pesquisaId,
            DuplicarPesquisaRequest request,
            CancellationToken ct = default
        );
        Task<object?> GerarQRCodeAsync(int pesquisaId, CancellationToken ct = default);
        Task<byte[]?> ExportarPDFAsync(int pesquisaId, CancellationToken ct = default);
        Task<object?> ValidarAcessoAsync(
            int pesquisaId,
            ValidarPesquisaRequest request,
            CancellationToken ct = default
        );
        Task<object?> ObterEstatisticasAsync(
            int pesquisaId,
            EstatisticasPesquisaRequest request,
            CancellationToken ct = default
        );
        Task<StatusPesquisaResponse?> ObterStatusAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
        Task<object> ListarTodasPesquisasAsync(CancellationToken ct = default);
        Task<object> ObterEstatisticasGeraisAsync(CancellationToken ct = default);
    }
}
