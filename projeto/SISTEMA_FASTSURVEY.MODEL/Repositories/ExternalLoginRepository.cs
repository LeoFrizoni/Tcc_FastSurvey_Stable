#nullable enable
using System.Linq; // Where/OrderBy
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class ExternalLoginRepository : Repository<ExternalLogins>, IExternalLoginRepository
    {
        public ExternalLoginRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<ExternalLogins?> GetByProviderAsync(
            string provider,
            string providerUserId,
            CancellationToken ct = default
        )
        {
            // normalização leve (não quebra consulta e evita espaços acidentais)
            provider = provider?.Trim() ?? string.Empty;
            providerUserId = providerUserId?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Provider == provider && x.ProviderUserId == providerUserId,
                    ct
                );
        }

        public async Task<ExternalLogins?> GetByLoginIdAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking().FirstOrDefaultAsync(x => x.LoginId == loginId, ct);
        }

        public async Task<bool> ExistsAsync(
            string provider,
            string providerUserId,
            CancellationToken ct = default
        )
        {
            provider = provider?.Trim() ?? string.Empty;
            providerUserId = providerUserId?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .AnyAsync(x => x.Provider == provider && x.ProviderUserId == providerUserId, ct);
        }

        public async Task<bool> ExistsForLoginAsync(
            int loginId,
            string provider,
            CancellationToken ct = default
        )
        {
            provider = provider?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .AnyAsync(x => x.LoginId == loginId && x.Provider == provider, ct);
        }

        // ---------- Opcionais (não-quebrantes) ----------

        public async Task<bool> ExistsInsensitiveAsync(
            string provider,
            string providerUserId,
            CancellationToken ct = default
        )
        {
            // Variante para Postgres/Npgsql com ILIKE (case-insensitive).
            // Mantemos ExistsAsync (case-sensitive) para compatibilidade.
            provider = provider?.Trim() ?? string.Empty;
            providerUserId = providerUserId?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .AnyAsync(
                    x =>
                        EF.Functions.ILike(x.Provider, provider)
                        && EF.Functions.ILike(x.ProviderUserId, providerUserId),
                    ct
                );
        }

        public async Task<List<ExternalLogins>> GetAllByLoginIdAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .Where(x => x.LoginId == loginId)
                .OrderBy(x => x.Provider) // ordenação estável e previsível
                .ThenBy(x => x.ProviderUserId)
                .ToListAsync(ct);
        }

        // --------- (Opcional avançado) compiled queries para hot paths ----------
        // Se quiser performance extra, podemos adicionar compiled queries estáticas aqui.
        // Não mudei agora para manter simples e compatível.
    }
}
