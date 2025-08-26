#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface ITipoUsuarioRepository : IRepository<TipoUsuario>
    {
        Task<List<TipoUsuario>> ListarAsync(
            bool incluirDesabilitados = false,
            CancellationToken ct = default
        );

        /// <summary>Busca por ID.</summary>
        Task<TipoUsuario?> ObterAsync(int id, CancellationToken ct = default);

        /// <summary>Busca por nome (case-insensitive).</summary>
        Task<TipoUsuario?> ObterPorNomeAsync(string nome, CancellationToken ct = default);

        /// <summary>Verifica existência por ID.</summary>
        Task<bool> ExisteAsync(int id, CancellationToken ct = default);
    }
}
