// FASTSURVEY/Controllers/LoginController.cs
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Controllers
{
    public class LoginRequestDto
    {
        public string usuario { get; set; } = "";
        public string senha { get; set; } = "";
    }

    public class LoginCreateDto
    {
        public string usuario { get; set; } = "";
        public string senha { get; set; } = "";
        public string? email { get; set; }
        public int? tipousuarioid { get; set; }
    }

    public class LoginUpdateDto
    {
        public int loginid { get; set; }
        public string? usuario { get; set; }
        public string? senha { get; set; }
        public string? email { get; set; }
        public int? tipousuarioid { get; set; }
    }

    [ApiController]
    [Route("api/login")] // rota base fixa e minúscula: /api/login
    [Produces("application/json")]
    public class LoginController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryLogin _repoLogin;

        public LoginController(FastSurveyContext context, RepositoryLogin repoLogin)
        {
            _context = context;
            _repoLogin = repoLogin;
        }

        // ===== Helpers (PBKDF2) =====

        private const int SaltSize = 16;        // 128 bits
        private const int KeySize = 32;         // 256 bits
        private const int Iterations = 100_000;
        private const string HashPrefix = "PBKDF2";

        private static bool PasswordStrongEnough(string s) => s.Length >= 6;

        private static string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(KeySize);

            return $"{HashPrefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return false;

            if (!stored.StartsWith(HashPrefix + "$", StringComparison.Ordinal))
                return password == stored;

            try
            {
                var parts = stored.Split('$', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4) return false;

                var iter = int.Parse(parts[1]);
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
                var actual = pbkdf2.GetBytes(expected.Length);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch { return false; }
        }

        private static string GenerateDevToken() =>
            Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        // ===== Endpoints =====

        // POST: /api/login  (cadastro)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LoginCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.usuario) || string.IsNullOrWhiteSpace(dto.senha))
                return BadRequest("Usuário e senha são obrigatórios.");

            var usuario = dto.usuario.Trim();
            var senha = dto.senha.Trim();
            var email = dto.email?.Trim();

            if (!PasswordStrongEnough(senha))
                return BadRequest("A senha precisa ter ao menos 6 caracteres.");

            try
            {
                var existeUsuario = await _context.login.AsNoTracking()
                    .AnyAsync(l => EF.Functions.ILike(l.usuario, usuario));
                if (existeUsuario) return Conflict("Usuário já cadastrado.");

                if (!string.IsNullOrWhiteSpace(email))
                {
                    var existeEmail = await _context.login.AsNoTracking()
                        .AnyAsync(l => EF.Functions.ILike(l.email, email));
                    if (existeEmail) return Conflict("E-mail já cadastrado.");
                }

                var entity = new login
                {
                    usuario = usuario,
                    email = email,
                    senha = HashPassword(senha),
                    tipousuarioid = dto.tipousuarioid
                };

                var criado = await _repoLogin.IncluirAsync(entity);

                return CreatedAtAction(nameof(GetById), new { id = criado.loginid }, new {
                    loginid = criado.loginid,
                    usuario = criado.usuario,
                    email = criado.email,
                    tipousuarioid = criado.tipousuarioid,
                    dataregistro = criado.dataregistro
                });
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao salvar login: {ex.Message}");
            }
        }

        // POST: /api/login/autenticar  (autenticação)
        [HttpPost("autenticar")]
        [Consumes("application/json")]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequestDto cred)
        {
            if (cred == null || string.IsNullOrWhiteSpace(cred.usuario) || string.IsNullOrWhiteSpace(cred.senha))
                return BadRequest("Usuário e senha são obrigatórios.");

            var usuario = cred.usuario.Trim();
            var senha = cred.senha.Trim();

            try
            {
                var l = await _context.login
                    .FirstOrDefaultAsync(x => EF.Functions.ILike(x.usuario, usuario));

                if (l == null || !VerifyPassword(senha, l.senha))
                    return Unauthorized("Usuário ou senha inválidos.");

                // upgrade de hash transparente
                if (!l.senha.StartsWith(HashPrefix + "$", StringComparison.Ordinal))
                {
                    l.senha = HashPassword(senha);
                    await _repoLogin.AlterarAsync(l);
                }

                var token = GenerateDevToken();

                return Ok(new
                {
                    id = l.loginid,
                    usuario = l.usuario,
                    email = l.email,
                    tipousuarioid = l.tipousuarioid,
                    token
                });
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao autenticar: {ex.Message}");
            }
        }

        // GET: /api/login
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var logins = await _repoLogin.SelecionarTodosAsync();
                if (logins == null || logins.Count == 0) return NotFound("Nenhum login encontrado.");

                var data = logins.Select(l => new {
                    loginid = l.loginid,
                    usuario = l.usuario,
                    email = l.email,
                    tipousuarioid = l.tipousuarioid,
                    dataregistro = l.dataregistro
                });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao listar logins: {ex.Message}");
            }
        }

        // GET: /api/login/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");
            try
            {
                var l = await _repoLogin.SelecionarChaveAsync(id);
                if (l == null) return NotFound("Login não encontrado.");

                return Ok(new {
                    loginid = l.loginid,
                    usuario = l.usuario,
                    email = l.email,
                    tipousuarioid = l.tipousuarioid,
                    dataregistro = l.dataregistro
                });
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao buscar login: {ex.Message}");
            }
        }

        // PUT: /api/login/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] LoginUpdateDto dto)
        {
            if (id <= 0 || dto == null || id != dto.loginid)
                return BadRequest("Dados inválidos.");

            try
            {
                var existing = await _repoLogin.SelecionarChaveAsync(id);
                if (existing == null) return NotFound("Login não encontrado.");

                if (!string.IsNullOrWhiteSpace(dto.usuario))
                {
                    var novoUsuario = dto.usuario.Trim();
                    if (!novoUsuario.Equals(existing.usuario, StringComparison.OrdinalIgnoreCase))
                    {
                        var existeUsuario = await _context.login.AsNoTracking()
                            .AnyAsync(l => EF.Functions.ILike(l.usuario, novoUsuario) && l.loginid != id);
                        if (existeUsuario) return Conflict("Usuário já cadastrado.");
                    }
                    existing.usuario = novoUsuario;
                }

                if (!string.IsNullOrWhiteSpace(dto.email))
                {
                    var novoEmail = dto.email.Trim();
                    if (!string.Equals(novoEmail, existing.email, StringComparison.OrdinalIgnoreCase))
                    {
                        var existeEmail = await _context.login.AsNoTracking()
                            .AnyAsync(l => EF.Functions.ILike(l.email, novoEmail) && l.loginid != id);
                        if (existeEmail) return Conflict("E-mail já cadastrado.");
                    }
                    existing.email = novoEmail;
                }

                if (!string.IsNullOrWhiteSpace(dto.senha))
                {
                    var novaSenha = dto.senha.Trim();
                    if (!PasswordStrongEnough(novaSenha))
                        return BadRequest("A senha precisa ter ao menos 6 caracteres.");
                    existing.senha = HashPassword(novaSenha);
                }

                if (dto.tipousuarioid.HasValue)
                    existing.tipousuarioid = dto.tipousuarioid;

                await _repoLogin.AlterarAsync(existing);
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao atualizar login: {ex.Message}");
            }
        }

        // DELETE: /api/login/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var l = await _repoLogin.SelecionarChaveAsync(id);
                if (l == null) return NotFound("Login não encontrado.");

                await _repoLogin.ExcluirAsync(l);
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao excluir login: {ex.Message}");
            }
        }

        // GET: /api/login/me/{id}
        [HttpGet("me/{id:int}")]
        public async Task<IActionResult> GetUsuarioPorId(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var l = await _repoLogin.SelecionarChaveAsync(id);
                if (l == null) return NotFound("Usuário não encontrado.");

                return Ok(new
                {
                    id = l.loginid,
                    nome = l.usuario,
                    email = l.email,
                    status = "Ativo",
                    dataregistro = l.dataregistro
                });
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao buscar usuário: {ex.Message}");
            }
        }
    }
}
