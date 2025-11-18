#nullable enable
namespace FASTSURVEY.Services.PesquisaPasta
{
    public interface IPesquisaPastaService
    {
        Task<int?> ObterPastaIdAsync(int pesquisaId, CancellationToken ct = default);

        Task<bool> DefinirPastaAsync(
            int pesquisaId,
            int loginId,
            int? pastaId,
            CancellationToken ct = default
        );

        Task<Dictionary<int, int?>> MapearPastaPorPesquisaAsync(
            IEnumerable<int> pesquisaIds,
            CancellationToken ct = default
        );
    }
}
