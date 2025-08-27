#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IFeatureFlagsRepository : IRepository<FeatureFlags>
    {
        /// <summary>Verifica se uma feature flag está ativa para um usuário.</summary>
        Task<bool> IsFeatureEnabledAsync(int loginId, string flag, CancellationToken ct = default);

        /// <summary>Adiciona ou atualiza uma feature flag para um usuário.</summary>
        Task<FeatureFlags> SetFeatureFlagAsync(
            int loginId, 
            string flag, 
            bool ativa, 
            CancellationToken ct = default
        );

        /// <summary>Remove uma feature flag de um usuário.</summary>
        Task<bool> RemoveFeatureFlagAsync(int loginId, string flag, CancellationToken ct = default);

        /// <summary>Lista todas as feature flags de um usuário.</summary>
        Task<List<FeatureFlags>> ListarPorLoginAsync(int loginId, CancellationToken ct = default);

        /// <summary>Lista todos os usuários que têm uma feature flag específica ativa.</summary>
        Task<List<FeatureFlags>> ListarPorFlagAsync(string flag, CancellationToken ct = default);
    }
}
