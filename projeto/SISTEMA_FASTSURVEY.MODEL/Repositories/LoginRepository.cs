#nullable enable
using System.Linq; // Where/OrderBy
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class LoginRepository : Repository<Login>, ILoginRepository
    {
        public LoginRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<Login?> ObterPorEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // Normaliza entrada (trim). Para Postgres, usamos ILIKE (case-insensitive).
            var norm = email.Trim();

            return await _set.AsNoTracking()
                .FirstOrDefaultAsync(l => EF.Functions.ILike(l.Email, norm), ct);

            // OBS: Se não estiver usando Npgsql, poderia cair no fallback:
            // .FirstOrDefaultAsync(l => l.Email.ToLower() == norm.ToLower(), ct)
            // (mas evite por custo; mantenha ILIKE no Postgres)
        }

        public async Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var norm = email.Trim();

            return await _set.AsNoTracking().AnyAsync(l => EF.Functions.ILike(l.Email, norm), ct);

            // Fallback não recomendado (comentado):
            // .AnyAsync(l => l.Email.ToLower() == norm.ToLower(), ct);
        }

        public async Task<Login?> ObterComAvatarAsync(int loginId, CancellationToken ct = default)
        {
            // AsSplitQuery não é necessário aqui (só 1 include)
            return await _set.AsNoTracking()
                .Include(l => l.LoginAvatar)
                .FirstOrDefaultAsync(l => l.LoginId == loginId, ct);
        }

        public async Task<Login?> ObterComTokensAsync(int loginId, CancellationToken ct = default)
        {
            // Se Tokens for coleção grande, AsSplitQuery ajuda a evitar join gigante.
            var entity = await _set.AsNoTracking()
                .AsSplitQuery()
                .Include(l => l.Tokens)
                .FirstOrDefaultAsync(l => l.LoginId == loginId, ct);

            // (Opcional) ordena tokens em memória se fizer sentido para UI
            if (entity?.Tokens is not null)
            {
                // Ajuste os campos conforme seu Model (ex.: DataCriacao, ExpiraEm, TokenId)
                entity.Tokens = entity
                    .Tokens.OrderByDescending(t => EF.Property<DateTime?>(t, "DataCriacao"))
                    .ThenByDescending(t => EF.Property<int>(t, "TokenId"))
                    .ToList();
            }

            return entity;
        }
    }
}
