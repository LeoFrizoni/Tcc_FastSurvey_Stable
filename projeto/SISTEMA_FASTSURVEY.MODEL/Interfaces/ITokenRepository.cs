#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface ITokenRepository : IRepository<Tokens>
    {
        Task<Tokens?> ObterPorIdAsync(int id, CancellationToken ct = default);
        Task<Tokens?> ObterPorCodigoAsync(string token, CancellationToken ct = default);

        /// <summary>
        /// Lista tokens; quando apenasAtivos=true, considera não expirados e não usados.
        /// </summary>
        Task<List<Tokens>> ListarAsync(
            bool apenasAtivos,
            int? skip = null,
            int? take = null,
            CancellationToken ct = default
        );

        // ===== Utilidades práticas =====

        /// <summary>Obtém token ativo por código (DataExpirado > now && UsadoEm == null).</summary>
        Task<Tokens?> ObterAtivoPorCodigoAsync(string token, CancellationToken ct = default);

        /// <summary>Obtém os tokens ativos de um login, opcionalmente por finalidade.</summary>
        Task<List<Tokens>> ListarAtivosPorLoginAsync(
            int loginId,
            string? finalidade = null,
            int? skip = null,
            int? take = null,
            CancellationToken ct = default
        );

        /// <summary>Valida se um token está ativo (não expirado e não usado).</summary>
        Task<bool> EhAtivoAsync(string token, CancellationToken ct = default);

        /// <summary>Marca um token como usado (seta UsadoEm = agora UTC).</summary>
        Task<bool> MarcarComoUsadoAsync(
            string token,
            DateTime? quandoUtc = null,
            CancellationToken ct = default
        );

        /// <summary>Revoga (expira) um token imediatamente (seta DataExpirado = agora UTC).</summary>
        Task<bool> RevogarAsync(
            string token,
            DateTime? quandoUtc = null,
            CancellationToken ct = default
        );

        /// <summary>Remove definitivamente tokens já expirados (e opcionalmente usados também).</summary>
        Task<int> LimparExpiradosAsync(bool incluirUsados = true, CancellationToken ct = default);
    }
}
