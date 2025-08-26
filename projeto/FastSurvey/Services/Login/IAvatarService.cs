using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Login.Avatar;

namespace FASTSURVEY.Services.Login
{
    public interface IAvatarService
    {
        Task<AvatarResponse> UploadAsync(
            int loginId,
            AvatarUploadRequest req,
            CancellationToken ct = default
        );
        Task<bool> RemoverAsync(int loginId, CancellationToken ct = default);
    }
}
