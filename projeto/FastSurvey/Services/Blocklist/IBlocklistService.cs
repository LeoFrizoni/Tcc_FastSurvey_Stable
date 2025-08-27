using FASTSURVEY.Dtos.Blocklist;

namespace FASTSURVEY.Services.Blocklist
{
    public interface IBlocklistService
    {
        Task<bool> IsBlockedAsync(string ipAddress, CancellationToken ct = default);
        Task<BlocklistResponse> AddToBlocklistAsync(BlocklistRequest request, CancellationToken ct = default);
        Task<bool> RemoveFromBlocklistAsync(string ipAddress, CancellationToken ct = default);
        Task<List<BlocklistResponse>> ListarTodosAsync(CancellationToken ct = default);
        Task<BlocklistStatusResponse> GetStatusAsync(string ipAddress, CancellationToken ct = default);
        Task<int> LimparAntigasAsync(DateTime antesDe, CancellationToken ct = default);
    }
}
