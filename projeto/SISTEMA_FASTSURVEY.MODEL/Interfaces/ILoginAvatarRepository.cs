#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface ILoginAvatarRepository : IRepository<LoginAvatar>
    {
        /// <summary>Retorna o avatar do usuário ou null se não existir.</summary>
        Task<LoginAvatar?> GetByLoginIdAsync(int loginId, CancellationToken ct = default);

        /// <summary>Verifica se o usuário possui avatar.</summary>
        Task<bool> ExistsAsync(int loginId, CancellationToken ct = default);

        /// <summary>Remove todos os avatares do usuário (normalmente 1:1). Retorna a quantidade removida.</summary>
        Task<int> DeleteByLoginIdAsync(int loginId, CancellationToken ct = default);

        /// <summary>Query somente leitura para compor buscas no service.</summary>
        IQueryable<LoginAvatar> QueryByLoginId(int loginId);
    }
}
