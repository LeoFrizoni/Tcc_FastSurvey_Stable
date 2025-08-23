#nullable enable
using FASTSURVEY.Dtos.Tokens;
using FASTSURVEY.Services.Result;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Tokens
{
    public interface ITokenService
    {
        Task<ServiceResult<TokenDto>> CriarAsync(int validadeMinutos = 10, CancellationToken ct = default);
        Task<ServiceResult<TokenDto>> ObterPorIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<List<TokenDto>>> ListarAsync(bool apenasAtivos, int? pagina = null, int? tamanho = null, CancellationToken ct = default);
        Task<ServiceResult<ValidarTokenResponse>> ValidarAsync(string token, CancellationToken ct = default);
        Task<ServiceResult<TokenDto>> ProrrogarAsync(int tokenId, int minutos, CancellationToken ct = default);
        Task<ServiceResult<bool>> RevogarAsync(int tokenId, CancellationToken ct = default);
        Task<ServiceResult<bool>> RemoverAsync(int tokenId, CancellationToken ct = default);
    }
}
