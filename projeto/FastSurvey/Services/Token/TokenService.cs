#nullable enable
using System.Security.Cryptography;
using System.Text;
using FASTSURVEY.Dtos.Tokens;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

// Alias para evitar conflito entre namespace FASTSURVEY.Services.Tokens e a entidade Tokens
using TokenEntity = SISTEMA_FASTSURVEY.MODEL.Models.Tokens;

namespace FASTSURVEY.Services.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly FastSurveyContext _ctx;
        public TokenService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<ServiceResult<TokenDto>> CriarAsync(int validadeMinutos = 10, CancellationToken ct = default)
        {
            if (validadeMinutos <= 0) validadeMinutos = 10;

            // Gera token �nico (6 chars alfanum�ricos) com algumas tentativas
            string novo;
            int tentativas = 0;
            do
            {
                if (tentativas++ > 10)
                    return ServiceResult<TokenDto>.Fail("TOKEN_GEN", "N�o foi poss�vel gerar um token �nico.");
                novo = GerarCodigo(6);
            }
            while (await _ctx.Set<TokenEntity>().AnyAsync(t => t.Token == novo, ct));

            var agora = DateTime.UtcNow;
            var entidade = new TokenEntity
            {
                Token = novo,
                Dataregistro = agora,
                Dataexpirado = agora.AddMinutes(validadeMinutos)
            };

            _ctx.Set<TokenEntity>().Add(entidade);

            try
            {
                await _ctx.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("uq_tokens_token", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Raro: colis�o por concorr�ncia
                return ServiceResult<TokenDto>.Fail("TOKEN_DUP", "Colis�o de token. Tente novamente.");
            }

            return ServiceResult<TokenDto>.Ok(Map(entidade));
        }

        public async Task<ServiceResult<TokenDto>> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var t = await _ctx.Set<TokenEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Tokenid == id, ct);
            if (t is null) return ServiceResult<TokenDto>.Fail("NOT_FOUND", "Token n�o encontrado.");
            return ServiceResult<TokenDto>.Ok(Map(t));
        }

        public async Task<ServiceResult<List<TokenDto>>> ListarAsync(bool apenasAtivos, int? pagina = null, int? tamanho = null, CancellationToken ct = default)
        {
            int skip = 0, take = 100;
            if (pagina is int p && tamanho is int z && p > 0 && z > 0)
            {
                skip = (p - 1) * z;
                take = z;
            }

            var q = _ctx.Set<TokenEntity>().AsNoTracking();
            if (apenasAtivos) q = q.Where(x => x.Dataexpirado > DateTime.UtcNow);

            var lista = await q.OrderByDescending(x => x.Dataregistro).Skip(skip).Take(take).ToListAsync(ct);
            return ServiceResult<List<TokenDto>>.Ok(lista.Select(Map).ToList());
        }

        public async Task<ServiceResult<ValidarTokenResponse>> ValidarAsync(string token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return ServiceResult<ValidarTokenResponse>.Fail("INVALID", "Token vazio.");

            var t = await _ctx.Set<TokenEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Token == token, ct);
            if (t is null)
                return ServiceResult<ValidarTokenResponse>.Ok(new ValidarTokenResponse
                {
                    valido = false,
                    motivoInvalidez = "Token inexistente."
                });

            bool valido = t.Dataexpirado > DateTime.UtcNow;
            return ServiceResult<ValidarTokenResponse>.Ok(new ValidarTokenResponse
            {
                valido = valido,
                tokenid = t.Tokenid,
                expiraEm = t.Dataexpirado,
                motivoInvalidez = valido ? null : "Token expirado."
            });
        }

        public async Task<ServiceResult<TokenDto>> ProrrogarAsync(int tokenId, int minutos, CancellationToken ct = default)
        {
            if (minutos <= 0) return ServiceResult<TokenDto>.Fail("INVALID", "Minutos deve ser > 0.");

            var t = await _ctx.Set<TokenEntity>().FirstOrDefaultAsync(x => x.Tokenid == tokenId, ct);
            if (t is null) return ServiceResult<TokenDto>.Fail("NOT_FOUND", "Token n�o encontrado.");

            // se j� expirou, come�a a contar a partir de agora
            var baseTime = t.Dataexpirado > DateTime.UtcNow ? t.Dataexpirado : DateTime.UtcNow;
            t.Dataexpirado = baseTime.AddMinutes(minutos);

            await _ctx.SaveChangesAsync(ct);
            return ServiceResult<TokenDto>.Ok(Map(t));
        }

        public async Task<ServiceResult<bool>> RevogarAsync(int tokenId, CancellationToken ct = default)
        {
            var t = await _ctx.Set<TokenEntity>().FirstOrDefaultAsync(x => x.Tokenid == tokenId, ct);
            if (t is null) return ServiceResult<bool>.Fail("NOT_FOUND", "Token n�o encontrado.");

            t.Dataexpirado = DateTime.UtcNow;
            await _ctx.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> RemoverAsync(int tokenId, CancellationToken ct = default)
        {
            var t = await _ctx.Set<TokenEntity>().FirstOrDefaultAsync(x => x.Tokenid == tokenId, ct);
            if (t is null) return ServiceResult<bool>.Fail("NOT_FOUND", "Token n�o encontrado.");

            _ctx.Remove(t);
            await _ctx.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        private static TokenDto Map(TokenEntity t) => new()
        {
            tokenid = t.Tokenid,
            token = t.Token,
            dataregistro = t.Dataregistro,
            dataexpirado = t.Dataexpirado
        };

        private static string GerarCodigo(int tamanho)
        {
            const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sem confusos
            var bytes = RandomNumberGenerator.GetBytes(tamanho);
            var sb = new StringBuilder(tamanho);
            foreach (var b in bytes) sb.Append(alfabeto[b % alfabeto.Length]);
            return sb.ToString();
        }
    }
}
