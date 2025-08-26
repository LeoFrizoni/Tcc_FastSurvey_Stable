using FASTSURVEY.Dtos.Pastas;

namespace FASTSURVEY.Services.Pasta
{
    public interface IPastaService
    {
        Task<List<PastaResponse>> ListarAsync(int loginId, CancellationToken ct = default);
        Task<PastaResponse> CriarAsync(CriarPastaRequest req, CancellationToken ct = default);
        Task<bool> RenomearAsync(
            int pastaId,
            int loginId,
            string novoNome,
            CancellationToken ct = default
        );
        Task<bool> ExcluirAsync(int pastaId, int loginId, CancellationToken ct = default);
    }
}
