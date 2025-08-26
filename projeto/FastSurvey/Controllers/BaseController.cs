#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        // >>> Ajuste aos seus perfis padronizados <<<
        // 13 = Normal, 14 = Premium, 15 = Admin
        protected const int TipoUsuarioAdmin = 15;

        // Centralização dos nomes das claims usadas no backend
        protected static class JwtClaims
        {
            public const string LoginId = "LoginId";
            public const string TipoUsuarioId = "TipoUsuarioId";
            public const string Email = "email";
            public const string Nome = "nome";
        }

        // ---- Helpers de leitura (com exceção quando for obrigatório) ----

        protected int GetLoginIdFromToken()
        {
            if (TryGetLoginId(out var id))
                return id;

            throw new InvalidOperationException("LoginId não encontrado no token");
        }

        protected int GetTipoUsuarioIdFromToken()
        {
            if (TryGetTipoUsuarioId(out var tipo))
                return tipo;

            throw new InvalidOperationException("TipoUsuarioId não encontrado no token");
        }

        protected string GetEmailFromToken()
        {
            if (TryGetEmail(out var email) && !string.IsNullOrWhiteSpace(email))
                return email;

            throw new InvalidOperationException("Email não encontrado no token");
        }

        protected string? GetNomeFromToken()
        {
            // Nome pode não existir; retornamos null em vez de lançar
            if (TryGetNome(out var nome))
                return nome;
            return null;
        }

        // ---- Versões "TryGet" seguras (não lançam exceção) ----

        protected bool TryGetLoginId(out int loginId)
        {
            loginId = 0;
            var raw =
                User.FindFirst(JwtClaims.LoginId)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // fallback
            return int.TryParse(raw, out loginId);
        }

        protected bool TryGetTipoUsuarioId(out int tipoUsuarioId)
        {
            tipoUsuarioId = 0;
            var raw = User.FindFirst(JwtClaims.TipoUsuarioId)?.Value;
            return int.TryParse(raw, out tipoUsuarioId);
        }

        protected bool TryGetEmail(out string? email)
        {
            email =
                User.FindFirst(JwtClaims.Email)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value; // fallback
            return !string.IsNullOrEmpty(email);
        }

        protected bool TryGetNome(out string? nome)
        {
            nome = User.FindFirst(JwtClaims.Nome)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value; // fallback
            return !string.IsNullOrEmpty(nome);
        }

        // ---- Helpers de regra ----

        protected bool HasTipoUsuario(int tipoUsuarioId) =>
            TryGetTipoUsuarioId(out var current) && current == tipoUsuarioId;

        protected bool IsAdmin() => HasTipoUsuario(TipoUsuarioAdmin);
    }
}
