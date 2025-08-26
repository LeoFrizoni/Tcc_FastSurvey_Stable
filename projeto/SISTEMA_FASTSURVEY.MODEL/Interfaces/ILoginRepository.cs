#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces;

public interface ILoginRepository : IRepository<Login>
{
    Task<Login?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    Task<Login?> ObterComAvatarAsync(int loginId, CancellationToken ct = default);
    Task<Login?> ObterComTokensAsync(int loginId, CancellationToken ct = default);
}
