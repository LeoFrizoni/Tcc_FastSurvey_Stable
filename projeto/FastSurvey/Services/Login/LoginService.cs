using FASTSURVEY.Dtos.Auth;
using FASTSURVEY.Services.Security;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Data;
// 👇 alias para a entidade (evita conflito com o namespace do service)
using LoginEntity = SISTEMA_FASTSURVEY.MODEL.Models.Login;

namespace FASTSURVEY.Services.Login
{
    public class LoginService : ILoginService
    {
        private readonly FastSurveyContext _ctx;
        private readonly IConfiguration _cfg;

        public LoginService(FastSurveyContext ctx, IConfiguration cfg)
        {
            _ctx = ctx;
            _cfg = cfg;
        }

        // ========================= AUTENTICAR =========================
        public async Task<LoginResponse> AutenticarAsync(LoginRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Usuario) || string.IsNullOrWhiteSpace(req.Senha))
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

            var key = req.Usuario.Trim().ToLowerInvariant();

            // tracking ON para permitir rehash (migração de legado)
            var login = await _ctx.Set<LoginEntity>()
                                  .AsTracking()
                                  .FirstOrDefaultAsync(l =>
                                       l.Usuario.ToLower() == key || l.Email.ToLower() == key, ct);

            if (login is null)
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

            var saved = login.Senha ?? string.Empty;
            var ok = false;

            if (PasswordHasher.IsPbkdf2(saved))
            {
                ok = PasswordHasher.Verify(req.Senha, saved);
            }
            else
            {
                // legado: senha em texto puro
                ok = req.Senha == saved;
                if (ok)
                {
                    login.Senha = PasswordHasher.Hash(req.Senha); // migra para PBKDF2
                    await _ctx.SaveChangesAsync(ct);
                }
            }

            if (!ok)
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

            var tipoId = login.Tipousuarioid ?? 13; // 13 = Usuário (padrão)
            var token = GerarJwtToken(login.Loginid, tipoId, login.Usuario);

            return new LoginResponse(
                Id: login.Loginid,
                Usuario: login.Usuario,
                TipoUsuarioId: tipoId,
                Token: token
            );
        }

        // ========================= CADASTRAR =========================
        public async Task<CadastrarLoginResponse> CadastrarAsync(CadastrarLoginRequest req, CancellationToken ct)
        {
            if (!SenhaForte(req.Senha))
                throw new InvalidOperationException("A senha deve ter pelo menos 8 caracteres, 1 maiúscula e 1 dígito.");

            var usuario = req.Usuario?.Trim() ?? "";
            var email = req.Email?.Trim().ToLowerInvariant() ?? "";

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Usuário e e-mail são obrigatórios.");

            var usuarioExiste = await _ctx.Set<LoginEntity>().AnyAsync(l => l.Usuario == usuario, ct);
            if (usuarioExiste)
                throw new DuplicateNameException("Usuário já existe.");

            var emailExiste = await _ctx.Set<LoginEntity>().AnyAsync(l => l.Email.ToLower() == email, ct);
            if (emailExiste)
                throw new DuplicateNameException("E-mail já cadastrado.");

            // Opção A: SEM ler do request — sempre usuário comum
            var tipoId = 13; // 13 = Usuário

            // valida FK (evita 23503)
            var tipoOk = await _ctx.Set<Tipousuario>().AnyAsync(t => t.Tipousuarioid == tipoId, ct);
            if (!tipoOk)
                throw new InvalidOperationException("Tipo de usuário inválido.");

            var novo = new LoginEntity
            {
                Usuario = usuario,
                Email = email,
                Senha = PasswordHasher.Hash(req.Senha), // PBKDF2
                Dataregistro = DateTime.UtcNow,
                Tipousuarioid = tipoId,
                Tipousuariotexto = "Usuário"
            };

            _ctx.Set<LoginEntity>().Add(novo);
            await _ctx.SaveChangesAsync(ct);

            return new CadastrarLoginResponse(
                Id: novo.Loginid,
                Usuario: novo.Usuario,
                Email: novo.Email,
                TipoUsuarioId: novo.Tipousuarioid ?? 13
            );
        }

        // ========================= AUXILIARES =========================
        public string GerarJwtToken(int loginId, int tipoUsuarioId, string usuario)
            => JwtHelper.Gerar(loginId, tipoUsuarioId, usuario, _cfg);

        private static bool SenhaForte(string s)
            => !string.IsNullOrEmpty(s) && s.Length >= 8 && s.Any(char.IsUpper) && s.Any(char.IsDigit);
    }
}
