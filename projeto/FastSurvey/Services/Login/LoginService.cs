#nullable enable
using FASTSURVEY.Dtos.Login;
using FASTSURVEY.Services.Email;        // <-- adiciona
using FASTSURVEY.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbToken = SISTEMA_FASTSURVEY.MODEL.Models.Tokens;
// ==== ALIASES para evitar conflitos de nomes (DTOs x Models) ====
using ModelLogin = SISTEMA_FASTSURVEY.MODEL.Models.Login;

namespace FASTSURVEY.Services.Login
{
    public sealed class LoginService : ILoginService
    {
        private readonly FastSurveyContext _ctx;
        private readonly ILoginRepository _loginRepo;
        private readonly IConfiguration _cfg;
        private readonly IEmailSender _email;   // <-- injeta o sender

        public LoginService(
            FastSurveyContext ctx,
            ILoginRepository loginRepo,
            IConfiguration cfg,
            IEmailSender email)                // <-- injeta o sender
        {
            _ctx = ctx;
            _loginRepo = loginRepo;
            _cfg = cfg;
            _email = email;
        }

        /* =========================================================
         *                    AUTENTICA��O / CADASTRO
         * ========================================================= */

        public async Task<LoginResponse> AutenticarAsync(LoginRequest req, CancellationToken ct = default)
        {
            var userNorm = req.Usuario?.Trim().ToLower() ?? string.Empty;

            var login = await _ctx.Login
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Usuario.ToLower() == userNorm, ct);

            if (login is null)
                throw new UnauthorizedAccessException("Usu�rio ou senha inv�lidos.");

            if (!PasswordHasher.VerifyPassword(req.Senha, login.Senha))
                throw new UnauthorizedAccessException("Usu�rio ou senha inv�lidos.");

            var token = JwtHelper.GenerateToken(login, _cfg);

            return new LoginResponse(
                Id: login.Loginid,
                Usuario: login.Usuario,
                TipoUsuarioId: login.Tipousuarioid ?? 0,
                Token: token
            );
        }

        public async Task<CadastrarLoginResponse> CadastrarAsync(CadastrarLoginRequest req, CancellationToken ct = default)
        {
            var userNorm = req.Usuario?.Trim().ToLower() ?? string.Empty;
            var emailNorm = req.Email?.Trim().ToLower() ?? string.Empty;

            // Verificar se j� existe
            var existe = await _ctx.Login
                .AsNoTracking()
                .AnyAsync(l => l.Usuario.ToLower() == userNorm || l.Email.ToLower() == emailNorm, ct);

            if (existe)
                throw new InvalidOperationException("Usu�rio ou email j� cadastrado.");

            var login = new ModelLogin
            {
                Usuario = req.Usuario.Trim(),
                Email = req.Email.Trim(),
                Senha = PasswordHasher.HashPassword(req.Senha),
                Tipousuarioid = 10, // Usu�rio padr�o
                Dataregistro = DateTime.UtcNow
            };

            var criado = await _loginRepo.AddAsync(login);

            return new CadastrarLoginResponse(
                Id: criado.Loginid,
                Usuario: criado.Usuario,
                Email: criado.Email,
                TipoUsuarioId: criado.Tipousuarioid ?? 0
            );
        }

        public async Task<bool> SolicitarResetSenhaAsync(ForgotPasswordRequest req, CancellationToken ct = default)
        {
            var login = await _ctx.Login
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Email.ToLower() == req.Email.ToLower(), ct);

            if (login is null) return false; // N�o revelar se o email existe

            // Gerar token de reset
            var token = Guid.NewGuid().ToString("N");
            var tokenEntity = new DbToken
            {
                Token = token,
                Dataregistro = DateTime.UtcNow,
                Dataexpirado = DateTime.UtcNow.AddHours(24),
                Loginid = login.Loginid
            };

            await _ctx.Tokens.AddAsync(tokenEntity, ct);
            await _ctx.SaveChangesAsync(ct);

            // Enviar email
            var resetUrl = $"{_cfg["AppUrl"]}/reset-password?token={token}";
            var htmlBody = $@"
                <h2>Reset de Senha - FastSurvey</h2>
                <p>Ol� {login.Usuario},</p>
                <p>Voc� solicitou um reset de senha. Clique no link abaixo para continuar:</p>
                <p><a href='{resetUrl}'>Resetar Senha</a></p>
                <p>Este link expira em 24 horas.</p>
                <p>Se voc� n�o solicitou este reset, ignore este email.</p>";

            await _email.SendAsync(login.Email, "Reset de Senha - FastSurvey", htmlBody, ct);

            return true;
        }

        public async Task<bool> ResetarSenhaAsync(ResetPasswordRequest req, CancellationToken ct = default)
        {
            var token = await _ctx.Tokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == req.Token, ct);

            if (token is null || token.Dataexpirado < DateTime.UtcNow)
                return false;

            var login = await _ctx.Login.FindAsync(token.Loginid);
            if (login is null) return false;

            login.Senha = PasswordHasher.HashPassword(req.NovaSenha);
            // Note: Tokens model doesn't have Ativo property, so we can't invalidate it this way
            // In the future, add an Ativo field to the database and model

            await _ctx.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> EnviarConfirmacaoEmailAsync(string email, CancellationToken ct = default)
        {
            var login = await _ctx.Login
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower(), ct);

            if (login is null) return false;

            // Implementar l�gica de confirma��o de email
            return true;
        }

        public async Task<bool> ConfirmarEmailAsync(VerifyEmailRequest req, CancellationToken ct = default)
        {
            // Implementar l�gica de confirma��o de email
            return true;
        }

        public async Task<LoginResponse> LoginGoogleAsync(ExternalLoginRequest req, CancellationToken ct = default)
        {
            // Implementar login com Google
            throw new NotImplementedException("Login com Google ainda n�o implementado");
        }

        public async Task<PerfilResponse?> ObterPerfilAsync(int loginId, CancellationToken ct = default)
        {
            var login = await _ctx.Login
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Loginid == loginId, ct);

            if (login is null) return null;

            return new PerfilResponse(
                Id: login.Loginid,
                Usuario: login.Usuario,
                Email: login.Email,
                TipoUsuarioId: login.Tipousuarioid ?? 0,
                AvatarUrl: null // Implementar quando tiver avatar
            );
        }

        public async Task<bool> AtualizarPerfilAsync(int loginId, string? email, string? senha, CancellationToken ct = default)
        {
            var login = await _ctx.Login.FindAsync(loginId);
            if (login is null) return false;

            if (!string.IsNullOrEmpty(email))
            {
                var emailExiste = await _ctx.Login
                    .AsNoTracking()
                    .AnyAsync(l => l.Email.ToLower() == email.ToLower() && l.Loginid != loginId, ct);

                if (emailExiste) return false;

                login.Email = email.Trim();
            }

            if (!string.IsNullOrEmpty(senha))
            {
                login.Senha = PasswordHasher.HashPassword(senha);
            }

            await _ctx.SaveChangesAsync(ct);
            return true;
        }
    }
}
