#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Login;
using FASTSURVEY.Services.Email;
using FASTSURVEY.Services.Security;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;
using ModelExternalLogins = SISTEMA_FASTSURVEY.MODEL.Models.ExternalLogins;
// Aliases para evitar conflitos de namespace
using ModelLogin = SISTEMA_FASTSURVEY.MODEL.Models.Login;
using ModelTokens = SISTEMA_FASTSURVEY.MODEL.Models.Tokens;

namespace FASTSURVEY.Services.Login
{
    /// <summary>Serviço para autenticação e gerenciamento de usuários.</summary>
    public class LoginService : ILoginService
    {
        private readonly FastSurveyContext _context;
        private readonly IRepository<ModelLogin> _loginRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public LoginService(
            FastSurveyContext context,
            IRepository<ModelLogin> loginRepository,
            IConfiguration configuration,
            IEmailSender emailSender
        )
        {
            _context = context;
            _loginRepository = loginRepository;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        // =========================================================
        //                    AUTENTICAÇÃO
        // =========================================================

        /// <summary>Autentica por nome de usuário **ou e‑mail** + senha.</summary>
        public async Task<LoginResponse> AutenticarAsync(
            LoginRequest request,
            CancellationToken ct = default
        )
        {
            var entrada = request.Usuario?.Trim().ToLower() ?? string.Empty; // pode ser usuário OU email
            var senha = request.Senha ?? string.Empty;

            var login = await _context
                .Login.AsNoTracking()
                .FirstOrDefaultAsync(
                    l => l.Usuario.ToLower() == entrada || l.Email.ToLower() == entrada,
                    ct
                );

            if (login == null || !PasswordHasher.VerifyPassword(senha, login.Senha))
                throw new UnauthorizedAccessException("Usuário/email ou senha inválidos.");

            if (!login.EmailConfirmado)
                throw new UnauthorizedAccessException(
                    "Email não confirmado. Verifique sua caixa de entrada."
                );

            var token = JwtHelper.GenerateToken(login, _configuration);

            return new LoginResponse(
                LoginId: login.LoginId,
                Usuario: login.Usuario,
                TipoUsuarioId: login.TipoUsuarioId ?? 13,
                Token: token
            );
        }

        public async Task<LoginResponse> LoginExternoAsync(
            ExternalLoginRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                // 1) Valida o id_token no provedor (Google)
                var userInfo = await ValidarTokenExternoAsync(
                    request.IdToken,
                    request.Provider,
                    ct
                );
                if (userInfo is null)
                    throw new UnauthorizedAccessException("Token inválido.");

                var email = userInfo.Email;
                var nome = userInfo.Name ?? userInfo.GivenName ?? "Usuario";
                var providerUserId = userInfo.Sub;

                // 2) Tenta achar vínculo já existente
                var existingExternalLogin = await _context
                    .ExternalLogins.AsNoTracking()
                    .Include(el => el.Login)
                    .FirstOrDefaultAsync(
                        el =>
                            el.Provider == request.Provider && el.ProviderUserId == providerUserId,
                        ct
                    );

                ModelLogin? login;

                if (existingExternalLogin is not null)
                {
                    login = existingExternalLogin.Login;
                }
                else
                {
                    // 3) Procura usuário por e‑mail
                    login = await _context
                        .Login.AsNoTracking()
                        .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower(), ct);

                    if (login is not null)
                    {
                        // 4) Garante vínculo externo
                        var vinculo = await _context
                            .ExternalLogins.AsNoTracking()
                            .FirstOrDefaultAsync(
                                el =>
                                    el.LoginId == login.LoginId && el.Provider == request.Provider,
                                ct
                            );

                        if (vinculo is null)
                        {
                            var externalLogin = new ModelExternalLogins
                            {
                                LoginId = login.LoginId,
                                Provider = request.Provider,
                                ProviderUserId = providerUserId,
                                CriadoEm = DateTime.UtcNow,
                            };
                            await _context.ExternalLogins.AddAsync(externalLogin, ct);
                            await _context.SaveChangesAsync(ct);
                        }
                    }
                    else
                    {
                        // 5) Cria usuário + vínculo em transação
                        using var tx = await _context.Database.BeginTransactionAsync(ct);
                        login = await CriarUsuarioExternoAsync(
                            email,
                            nome,
                            request.Provider,
                            providerUserId,
                            ct
                        );
                        await tx.CommitAsync(ct);
                    }
                }

                // 6) Retorna JWT
                var token = JwtHelper.GenerateToken(login, _configuration);
                return new LoginResponse(
                    login.LoginId,
                    login.Usuario,
                    login.TipoUsuarioId ?? 13,
                    token
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro no login externo: {ex.Message}", ex);
            }
        }

        public async Task<CadastrarLoginResponse> CadastrarAsync(
            CadastrarLoginRequest request,
            CancellationToken ct = default
        )
        {
            var usuario = request.Usuario?.Trim() ?? string.Empty;
            var email = request.Email?.Trim().ToLower() ?? string.Empty;
            var senha = request.Senha ?? string.Empty;

            if (
                string.IsNullOrEmpty(usuario)
                || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(senha)
            )
                throw new ArgumentException("Todos os campos são obrigatórios.");

            if (await UsuarioExisteAsync(usuario, ct))
                throw new InvalidOperationException("Nome de usuário já existe.");

            if (await EmailExisteAsync(email, ct))
                throw new InvalidOperationException("Email já cadastrado.");

            var login = new ModelLogin
            {
                Usuario = usuario,
                Email = email,
                Senha = PasswordHasher.HashPassword(senha),
                TipoUsuarioId = 13,
                DataRegistro = DateTime.UtcNow,
                EmailConfirmado = false,
            };

            await _loginRepository.AddAsync(login, ct);
            await _context.SaveChangesAsync(ct);

            await EnviarConfirmacaoEmailAsync(email, ct);

            return new CadastrarLoginResponse(
                LoginId: login.LoginId,
                Usuario: login.Usuario,
                Email: login.Email,
                TipoUsuarioId: login.TipoUsuarioId ?? 13
            );
        }

        // =========================================================
        //                    RESET DE SENHA
        // =========================================================

        public async Task<bool> SolicitarResetSenhaAsync(
            ForgotPasswordRequest request,
            CancellationToken ct = default
        )
        {
            var email = request.Email?.Trim().ToLower() ?? string.Empty;

            var login = await _context
                .Login.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Email.ToLower() == email, ct);

            if (login is null)
                return true; // não revela existência do email

            // invalida tokens anteriores não usados
            var tokensAntigos = await _context
                .Tokens.Where(t =>
                    t.LoginId == login.LoginId
                    && t.Finalidade == "ResetSenha"
                    && !t.UsadoEm.HasValue
                )
                .ToListAsync(ct);

            foreach (var t in tokensAntigos)
                t.UsadoEm = DateTime.UtcNow;

            // cria novo token
            var novoToken = Guid.NewGuid().ToString("N");
            var tokenEntity = new ModelTokens
            {
                Token = novoToken,
                DataRegistro = DateTime.UtcNow,
                DataExpirado = DateTime.UtcNow.AddHours(24),
                LoginId = login.LoginId,
                Finalidade = "ResetSenha",
            };

            await _context.Tokens.AddAsync(tokenEntity, ct);
            await _context.SaveChangesAsync(ct);

            // envia email
            var appUrl = _configuration["AppUrl"] ?? "";
            var resetUrl = $"{appUrl.TrimEnd('/')}/reset-password?token={novoToken}";
            var htmlBody =
                $@"
                <h2>Reset de Senha - FastSurvey</h2>
                <p>Olá {login.Usuario},</p>
                <p>Você solicitou um reset de senha. Clique no link abaixo para continuar:</p>
                <p><a href='{resetUrl}'>Resetar Senha</a></p>
                <p>Este link expira em 24 horas.</p>
                <p>Se você não solicitou este reset, ignore este email.</p>";

            await _emailSender.SendAsync(login.Email, "Reset de Senha - FastSurvey", htmlBody, ct);
            return true;
        }

        public async Task<bool> ResetarSenhaAsync(
            ResetPasswordRequest request,
            CancellationToken ct = default
        )
        {
            var token = request.Token?.Trim() ?? string.Empty;
            var novaSenha = request.NovaSenha ?? string.Empty;

            var tokenEntity = await _context.Tokens.FirstOrDefaultAsync(
                t => t.Token == token && t.Finalidade == "ResetSenha",
                ct
            );

            if (
                tokenEntity is null
                || tokenEntity.DataExpirado < DateTime.UtcNow
                || tokenEntity.UsadoEm.HasValue
            )
                return false;

            var login = await _context.Login.FindAsync(new object?[] { tokenEntity.LoginId }, ct);
            if (login is null)
                return false;

            login.Senha = PasswordHasher.HashPassword(novaSenha);
            tokenEntity.UsadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        // =========================================================
        //                    CONFIRMAÇÃO DE EMAIL
        // =========================================================

        public async Task<bool> EnviarConfirmacaoEmailAsync(
            string email,
            CancellationToken ct = default
        )
        {
            var e = email?.Trim().ToLower() ?? string.Empty;

            var login = await _context
                .Login.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Email.ToLower() == e, ct);

            if (login is null || login.EmailConfirmado)
                return false;

            // invalida confirmações anteriores
            var antigos = await _context
                .Tokens.Where(t =>
                    t.LoginId == login.LoginId
                    && t.Finalidade == "ConfirmacaoEmail"
                    && !t.UsadoEm.HasValue
                )
                .ToListAsync(ct);
            foreach (var t in antigos)
                t.UsadoEm = DateTime.UtcNow;

            var token = Guid.NewGuid().ToString("N");
            var tokenEntity = new ModelTokens
            {
                Token = token,
                DataRegistro = DateTime.UtcNow,
                DataExpirado = DateTime.UtcNow.AddHours(24),
                LoginId = login.LoginId,
                Finalidade = "ConfirmacaoEmail",
            };

            await _context.Tokens.AddAsync(tokenEntity, ct);
            await _context.SaveChangesAsync(ct);

            var appUrl = _configuration["AppUrl"] ?? "";
            var confirmUrl = $"{appUrl.TrimEnd('/')}/confirm-email?token={token}";
            var htmlBody =
                $@"
                <h2>Confirmação de Email - FastSurvey</h2>
                <p>Olá {login.Usuario},</p>
                <p>Clique no link abaixo para confirmar seu email:</p>
                <p><a href='{confirmUrl}'>Confirmar Email</a></p>
                <p>Este link expira em 24 horas.</p>";

            await _emailSender.SendAsync(
                login.Email,
                "Confirmação de Email - FastSurvey",
                htmlBody,
                ct
            );
            return true;
        }

        public async Task<bool> ConfirmarEmailAsync(
            VerifyEmailRequest request,
            CancellationToken ct = default
        )
        {
            var token = request.Token?.Trim() ?? string.Empty;

            var tokenEntity = await _context.Tokens.FirstOrDefaultAsync(
                t => t.Token == token && t.Finalidade == "ConfirmacaoEmail",
                ct
            );

            if (
                tokenEntity is null
                || tokenEntity.DataExpirado < DateTime.UtcNow
                || tokenEntity.UsadoEm.HasValue
            )
                return false;

            var login = await _context.Login.FindAsync(new object?[] { tokenEntity.LoginId }, ct);
            if (login is null)
                return false;

            login.EmailConfirmado = true;
            tokenEntity.UsadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        // =========================================================
        //                    PERFIL DO USUÁRIO
        // =========================================================

        public async Task<PerfilResponse?> ObterPerfilAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            var login = await _context
                .Login.AsNoTracking()
                .Include(l => l.LoginAvatar)
                .FirstOrDefaultAsync(l => l.LoginId == loginId, ct);

            if (login is null)
                return null;

            return new PerfilResponse(
                LoginId: login.LoginId,
                Usuario: login.Usuario,
                Email: login.Email,
                TipoUsuarioId: login.TipoUsuarioId ?? 13,
                AvatarUrl: login.LoginAvatar?.StorageUrl,
                EmailConfirmado: login.EmailConfirmado
            );
        }

        public async Task<bool> AtualizarPerfilAsync(
            int loginId,
            AtualizarPerfilRequest request,
            CancellationToken ct = default
        )
        {
            var login = await _context.Login.FindAsync(new object?[] { loginId }, ct);
            if (login is null)
                return false;

            var alterado = false;
            var emailMudou = false;

            if (!string.IsNullOrWhiteSpace(request.Usuario))
            {
                var novoUsuario = request.Usuario.Trim();
                if (!string.Equals(novoUsuario, login.Usuario, StringComparison.Ordinal))
                {
                    if (await UsuarioExisteAsync(novoUsuario, ct))
                        return false;
                    login.Usuario = novoUsuario;
                    alterado = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var novoEmail = request.Email.Trim().ToLower();
                if (!string.Equals(novoEmail, login.Email, StringComparison.OrdinalIgnoreCase))
                {
                    if (await EmailExisteAsync(novoEmail, ct))
                        return false;
                    login.Email = novoEmail;
                    login.EmailConfirmado = false;
                    alterado = true;
                    emailMudou = true;
                }
            }

            if (!string.IsNullOrEmpty(request.Senha))
            {
                login.Senha = PasswordHasher.HashPassword(request.Senha);
                alterado = true;
            }

            if (alterado)
            {
                await _context.SaveChangesAsync(ct);
                if (emailMudou)
                    _ = EnviarConfirmacaoEmailAsync(login.Email, ct); // fire-and-forget
            }

            return true;
        }

        public async Task<bool> AtualizarNomeAsync(
            int loginId,
            AtualizarNomeRequest request,
            CancellationToken ct = default
        )
        {
            var login = await _context.Login.FindAsync(new object?[] { loginId }, ct);
            if (login is null)
                return false;

            var novoNome = request.NovoNome?.Trim();
            if (string.IsNullOrEmpty(novoNome))
                return false;
            if (string.Equals(novoNome, login.Usuario, StringComparison.Ordinal))
                return true;

            if (await UsuarioExisteAsync(novoNome, ct))
                return false;

            login.Usuario = novoNome;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // =========================================================
        //                    VALIDAÇÕES
        // =========================================================

        public async Task<bool> EmailExisteAsync(string email, CancellationToken ct = default)
        {
            var e = email?.Trim().ToLower() ?? string.Empty;
            return await _context.Login.AsNoTracking().AnyAsync(l => l.Email.ToLower() == e, ct);
        }

        public async Task<bool> UsuarioExisteAsync(string usuario, CancellationToken ct = default)
        {
            var u = usuario?.Trim().ToLower() ?? string.Empty;
            return await _context.Login.AsNoTracking().AnyAsync(l => l.Usuario.ToLower() == u, ct);
        }

        // =========================================================
        //                    ADMIN
        // =========================================================

        public async Task<IEnumerable<PerfilResponse>> ListarTodosUsuariosAsync(
            CancellationToken ct = default
        )
        {
            var usuarios = await _context
                .Login.Include(l => l.LoginAvatar)
                .AsNoTracking()
                .OrderBy(l => l.Usuario)
                .Select(l => new PerfilResponse(
                    l.LoginId,
                    l.Usuario,
                    l.Email,
                    l.TipoUsuarioId ?? 13,
                    l.LoginAvatar != null ? l.LoginAvatar.StorageUrl : null,
                    l.EmailConfirmado
                ))
                .ToListAsync(ct);

            return usuarios;
        }

        public async Task<bool> ExcluirUsuarioAsync(int loginId, CancellationToken ct = default)
        {
            var login = await _context
                .Login.Include(l => l.LoginAvatar)
                .Include(l => l.ExternalLogins)
                .FirstOrDefaultAsync(l => l.LoginId == loginId, ct);

            if (login is null)
                return false;

            if (login.LoginAvatar is not null)
                _context.LoginAvatar.Remove(login.LoginAvatar);

            if (login.ExternalLogins?.Any() == true)
                _context.ExternalLogins.RemoveRange(login.ExternalLogins);

            _context.Login.Remove(login);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AtivarUsuarioAsync(int loginId, CancellationToken ct = default)
        {
            var login = await _context.Login.FindAsync(new object?[] { loginId }, ct);
            if (login is null)
                return false;

            // Ex.: login.Ativo = true;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DesativarUsuarioAsync(int loginId, CancellationToken ct = default)
        {
            var login = await _context.Login.FindAsync(new object?[] { loginId }, ct);
            if (login is null)
                return false;

            // Ex.: login.Ativo = false;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // =========================================================
        //                    PRIVADOS
        // =========================================================

        private async Task<ModelLogin> CriarUsuarioExternoAsync(
            string email,
            string nome,
            string provider,
            string providerUserId,
            CancellationToken ct
        )
        {
            // Sanitiza username (apenas letras/dígitos)
            var baseNome = new string(
                (nome ?? "user")
                    .Normalize(System.Text.NormalizationForm.FormD)
                    .Where(ch => char.IsLetterOrDigit(ch))
                    .ToArray()
            );
            if (string.IsNullOrWhiteSpace(baseNome))
                baseNome = "user";

            var counter = 1;
            var nomeUsuario = baseNome;
            while (await UsuarioExisteAsync(nomeUsuario, ct))
                nomeUsuario = $"{baseNome}{counter++}";

            var login = new ModelLogin
            {
                Usuario = nomeUsuario,
                Email = email,
                Senha = PasswordHasher.HashPassword(Guid.NewGuid().ToString()),
                TipoUsuarioId = 13,
                DataRegistro = DateTime.UtcNow,
                EmailConfirmado = true,
            };

            await _loginRepository.AddAsync(login, ct);
            await _context.SaveChangesAsync(ct);

            var externalLogin = new ModelExternalLogins
            {
                LoginId = login.LoginId,
                Provider = provider,
                ProviderUserId = providerUserId,
                CriadoEm = DateTime.UtcNow,
            };

            await _context.ExternalLogins.AddAsync(externalLogin, ct);
            await _context.SaveChangesAsync(ct);

            return login;
        }

        private async Task<GoogleUserInfo?> ValidarTokenExternoAsync(
            string idToken,
            string provider,
            CancellationToken ct
        )
        {
            if (!string.Equals(provider, "google", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] },
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return new GoogleUserInfo
                {
                    Email = payload.Email ?? "",
                    Name = payload.Name ?? "",
                    GivenName = payload.GivenName ?? "",
                    FamilyName = payload.FamilyName ?? "",
                    Sub = payload.Subject ?? "",
                };
            }
            catch
            {
                return null;
            }
        }

        private class GoogleUserInfo
        {
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string GivenName { get; set; } = string.Empty;
            public string FamilyName { get; set; } = string.Empty;
            public string Sub { get; set; } = string.Empty;
        }
    }
}
