#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IBlocklistRepository : IRepository<Blocklist>
    {
        /// <summary>Verifica se um IP está na blocklist.</summary>
        Task<bool> IsBlockedAsync(string ipAddress, CancellationToken ct = default);

        /// <summary>Adiciona um IP à blocklist.</summary>
        Task<Blocklist> AddToBlocklistAsync(
            string ipAddress, 
            string motivo, 
            string? userAgent = null, 
            CancellationToken ct = default
        );

        /// <summary>Remove um IP da blocklist.</summary>
        Task<bool> RemoveFromBlocklistAsync(string ipAddress, CancellationToken ct = default);

        /// <summary>Lista todos os IPs bloqueados.</summary>
        Task<List<Blocklist>> ListarTodosAsync(CancellationToken ct = default);

        /// <summary>Limpa entradas antigas da blocklist.</summary>
        Task<int> LimparAntigasAsync(DateTime antesDe, CancellationToken ct = default);
    }
}
