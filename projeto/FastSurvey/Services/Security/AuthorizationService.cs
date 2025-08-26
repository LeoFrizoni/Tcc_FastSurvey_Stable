using System.Security.Claims;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.Security
{
    public interface IAuthorizationService
    {
        bool IsAdmin(int tipoUsuarioId);
        bool IsPremium(int tipoUsuarioId);
        bool IsNormalUser(int tipoUsuarioId);
        bool CanAccessAdmin(int tipoUsuarioId);
        bool CanAccessPremiumFeatures(int tipoUsuarioId);
        bool CanAccessResource(int tipoUsuarioId, string resource, string action);
        int GetCurrentUserId(ClaimsPrincipal user);
        int GetCurrentUserType(ClaimsPrincipal user);
        bool ValidateUserAccess(ClaimsPrincipal user, int requiredUserType);
        ServiceResult<bool> ValidateResourceAccess(
            ClaimsPrincipal user,
            string resource,
            string action
        );
    }

    public class AuthorizationService : IAuthorizationService
    {
        private const int USER_TYPE_NORMAL = 13;
        private const int USER_TYPE_PREMIUM = 14;
        private const int USER_TYPE_ADMIN = 15;

        private readonly Dictionary<int, Dictionary<string, List<string>>> _permissions = new()
        {
            [USER_TYPE_NORMAL] = new()
            {
                ["pesquisas"] = new() { "read", "create", "update", "delete" },
                ["resultados"] = new() { "read" },
                ["perfil"] = new() { "read", "update" },
                ["admin"] = new(),
            },
            [USER_TYPE_PREMIUM] = new()
            {
                ["pesquisas"] = new() { "read", "create", "update", "delete" },
                ["resultados"] = new() { "read", "export" },
                ["pesquisa-interativa"] = new() { "create", "manage" },
                ["perfil"] = new() { "read", "update" },
                ["admin"] = new(),
            },
            [USER_TYPE_ADMIN] = new()
            {
                ["pesquisas"] = new() { "read", "create", "update", "delete", "manage" },
                ["resultados"] = new() { "read", "export", "manage" },
                ["pesquisa-interativa"] = new() { "create", "manage" },
                ["perfil"] = new() { "read", "update" },
                ["admin"] = new() { "read", "create", "update", "delete", "manage" },
                ["usuarios"] = new() { "read", "create", "update", "delete", "manage" },
                ["sistema"] = new() { "read", "manage" },
            },
        };

        public bool IsAdmin(int tipoUsuarioId) => tipoUsuarioId == USER_TYPE_ADMIN;

        public bool IsPremium(int tipoUsuarioId) => tipoUsuarioId == USER_TYPE_PREMIUM;

        public bool IsNormalUser(int tipoUsuarioId) => tipoUsuarioId == USER_TYPE_NORMAL;

        public bool CanAccessAdmin(int tipoUsuarioId) => IsAdmin(tipoUsuarioId);

        public bool CanAccessPremiumFeatures(int tipoUsuarioId) =>
            IsPremium(tipoUsuarioId) || IsAdmin(tipoUsuarioId);

        public bool CanAccessResource(int tipoUsuarioId, string resource, string action)
        {
            resource = resource.ToLowerInvariant();
            action = action.ToLowerInvariant();

            if (!_permissions.TryGetValue(tipoUsuarioId, out var res))
                return false;
            if (!res.TryGetValue(resource, out var actions))
                return false;
            return actions.Contains(action);
        }

        public int GetCurrentUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("loginId") ?? user.FindFirst("userId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
            // alternativa: int.Parse com try/catch se preferir
        }

        public int GetCurrentUserType(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("tipoUsuarioId") ?? user.FindFirst("tipousuarioid");
            return claim != null && int.TryParse(claim.Value, out var t) ? t : USER_TYPE_NORMAL;
        }

        public bool ValidateUserAccess(ClaimsPrincipal user, int requiredUserType)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;
            var userType = GetCurrentUserType(user);
            return userType >= requiredUserType;
        }

        public ServiceResult<bool> ValidateResourceAccess(
            ClaimsPrincipal user,
            string resource,
            string action
        )
        {
            if (user?.Identity?.IsAuthenticated != true)
                return ServiceResult<bool>.Fail("UNAUTHORIZED", "Usuário não autenticado");

            var userType = GetCurrentUserType(user);

            if (!CanAccessResource(userType, resource, action))
                return ServiceResult<bool>.Fail(
                    "FORBIDDEN",
                    $"Acesso negado para {resource}:{action}"
                );

            return ServiceResult<bool>.Ok(true);
        }
    }
}
