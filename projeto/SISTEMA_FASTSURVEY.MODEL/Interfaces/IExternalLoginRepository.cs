#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IExternalLoginRepository : IRepository<ExternalLogins>
    {
        // Já existentes
        Task<ExternalLogins?> GetByProviderAsync(
            string provider,
            string providerUserId,
            CancellationToken ct = default
        );

        Task<ExternalLogins?> GetByLoginIdAsync(int loginId, CancellationToken ct = default);

        Task<bool> ExistsAsync(
            string provider,
            string providerUserId,
            CancellationToken ct = default
        );

        Task<bool> ExistsForLoginAsync(
            int loginId,
            string provider,
            CancellationToken ct = default
        );

        // ---------- Opcionais (não-quebrantes) ----------

        /// <summary>
        /// Variante case-insensitive (útil para Postgres/Npgsql com ILIKE).
        /// Mantém os métodos originais para compatibilidade.
        /// </summary>
        Task<bool> ExistsInsensitiveAsync(
            string provider,
            string providerUserId,
            CancellationToken ct = default
        );

        /// <summary>
        /// Lista todos os logins externos de um usuário.
        /// Ordenação estável por Provider (alfabética) e depois por ProviderUserId.
        /// </summary>
        Task<List<ExternalLogins>> GetAllByLoginIdAsync(
            int loginId,
            CancellationToken ct = default
        );
    }
}
