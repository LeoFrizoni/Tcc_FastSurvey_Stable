using FASTSURVEY.Dtos.Blocklist;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;

namespace FASTSURVEY.Services.Blocklist
{
    public class BlocklistService : IBlocklistService
    {
        private readonly IBlocklistRepository _blocklistRepository;

        public BlocklistService(IBlocklistRepository blocklistRepository)
        {
            _blocklistRepository = blocklistRepository;
        }

        public async Task<bool> IsBlockedAsync(string ipAddress, CancellationToken ct = default)
        {
            return await _blocklistRepository.IsBlockedAsync(ipAddress, ct);
        }

        public async Task<BlocklistResponse> AddToBlocklistAsync(BlocklistRequest request, CancellationToken ct = default)
        {
            var blocklist = await _blocklistRepository.AddToBlocklistAsync(
                request.IPAddress, 
                request.Motivo, 
                request.UserAgent, 
                ct
            );

            return new BlocklistResponse
            {
                Id = blocklist.Id,
                IPAddress = blocklist.IPAddress,
                UserAgent = blocklist.UserAgent,
                Motivo = blocklist.Motivo,
                CriadoEm = blocklist.CriadoEm
            };
        }

        public async Task<bool> RemoveFromBlocklistAsync(string ipAddress, CancellationToken ct = default)
        {
            return await _blocklistRepository.RemoveFromBlocklistAsync(ipAddress, ct);
        }

        public async Task<List<BlocklistResponse>> ListarTodosAsync(CancellationToken ct = default)
        {
            var blocklist = await _blocklistRepository.ListarTodosAsync(ct);
            
            return blocklist.Select(b => new BlocklistResponse
            {
                Id = b.Id,
                IPAddress = b.IPAddress,
                UserAgent = b.UserAgent,
                Motivo = b.Motivo,
                CriadoEm = b.CriadoEm
            }).ToList();
        }

        public async Task<BlocklistStatusResponse> GetStatusAsync(string ipAddress, CancellationToken ct = default)
        {
            var isBlocked = await _blocklistRepository.IsBlockedAsync(ipAddress, ct);
            
            if (!isBlocked)
            {
                return new BlocklistStatusResponse
                {
                    IPAddress = ipAddress,
                    IsBlocked = false
                };
            }

            // Buscar detalhes do bloqueio
            var blocklist = await _blocklistRepository.ListarTodosAsync(ct);
            var blockedEntry = blocklist.FirstOrDefault(b => b.IPAddress == ipAddress);

            return new BlocklistStatusResponse
            {
                IPAddress = ipAddress,
                IsBlocked = true,
                Motivo = blockedEntry?.Motivo,
                BloqueadoEm = blockedEntry?.CriadoEm
            };
        }

        public async Task<int> LimparAntigasAsync(DateTime antesDe, CancellationToken ct = default)
        {
            return await _blocklistRepository.LimparAntigasAsync(antesDe, ct);
        }
    }
}
