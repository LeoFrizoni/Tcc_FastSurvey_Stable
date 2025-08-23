using FASTSURVEY.Services.Result;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FASTSURVEY.Services.Security
{
    public interface IAuthorizationService
    {
        bool IsAdmin(int tipoUsuarioId);
        bool IsPremium(int tipoUsuarioId);
        bool IsNormalUser(int tipoUsuarioId);
        bool CanAccessAdmin(int tipoUsuarioId);
        bool CanAccessPremiumFeatures(int tipoUsuarioId);
        int GetCurrentUserId(ClaimsPrincipal user);
        int GetCurrentUserType(ClaimsPrincipal user);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private const int USER_TYPE_NORMAL = 13;
        private const int USER_TYPE_PREMIUM = 14;
        private const int USER_TYPE_ADMIN = 15; // Baseado nos dados reais da tabela

        public bool IsAdmin(int tipoUsuarioId) => tipoUsuarioId == USER_TYPE_ADMIN;
        public bool IsPremium(int tipoUsuarioId) => tipoUsuarioId == USER_TYPE_PREMIUM;
        public bool IsNormalUser(int tipoUsuarioId) => tipoUsuarioId == USER_TYPE_NORMAL;
        
        public bool CanAccessAdmin(int tipoUsuarioId) => IsAdmin(tipoUsuarioId);
        
        public bool CanAccessPremiumFeatures(int tipoUsuarioId) => 
            IsPremium(tipoUsuarioId) || IsAdmin(tipoUsuarioId);

        public int GetCurrentUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("loginId") ?? user.FindFirst("userId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public int GetCurrentUserType(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("tipoUsuarioId") ?? user.FindFirst("tipousuarioid");
            return claim != null ? int.Parse(claim.Value) : USER_TYPE_NORMAL;
        }
    }
}
