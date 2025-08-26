#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class SessaoInterativaRepository
        : Repository<SessoesInterativas>,
            ISessaoInterativaRepository
    {
        public SessaoInterativaRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<SessoesInterativas?> ObterPorCodigoAcessoAsync(
            string codigoAcesso,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(codigoAcesso))
                return null;

            // Postgres: busca case-insensitive por código
            var q = _set.AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null));

            // Se usar Npgsql, ILike compila para ILIKE:
            return await q.FirstOrDefaultAsync(
                s => EF.Functions.ILike(s.CodigoAcesso, codigoAcesso),
                ct
            );

            // Alternativa sem ILike:
            // return await q.FirstOrDefaultAsync(
            //     s => s.CodigoAcesso.ToLower() == codigoAcesso.ToLower(), ct);
        }

        public async Task<SessoesInterativas?> ObterPorPesquisaIdAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            // Mantém seu comportamento original (pega a primeira encontrada)
            return await _set.AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                .Where(s => s.PesquisaId == pesquisaId)
                .OrderByDescending(s => s.CriadaEm)
                .FirstOrDefaultAsync(ct);
        }

        // OPCIONAL: quando você quer forçar ativa + mais recente
        public async Task<SessoesInterativas?> ObterAtivaMaisRecentePorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                .Where(s => s.PesquisaId == pesquisaId && s.Ativa)
                .OrderByDescending(s => s.IniciadaEm ?? s.CriadaEm)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExisteSessaoAtivaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .AnyAsync(s => s.PesquisaId == pesquisaId && s.Ativa, ct);
        }

        public async Task<IEnumerable<SessoesInterativas>> ObterSessoesAtivasAsync(
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                .Where(s => s.Ativa)
                .OrderByDescending(s => s.IniciadaEm ?? s.CriadaEm)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<SessoesInterativas>> ObterSessoesPorUsuarioAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                .Where(s => s.Pesquisa.LoginId == loginId)
                .OrderByDescending(s => s.CriadaEm)
                .ToListAsync(ct);
        }
    }
}
