using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class SessaoInterativaRepository : Repository<SessoesInterativas>, ISessaoInterativaRepository
    {
        public SessaoInterativaRepository(FastSurveyContext context) : base(context)
        {
        }

        public async Task<SessoesInterativas?> ObterPorCodigoAcessoAsync(string codigoAcesso)
        {
            return await _context.Set<SessoesInterativas>()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                .FirstOrDefaultAsync(s => s.CodigoAcesso == codigoAcesso);
        }

        public async Task<SessoesInterativas?> ObterPorPesquisaIdAsync(int pesquisaId)
        {
            return await _context.Set<SessoesInterativas>()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                .FirstOrDefaultAsync(s => s.PesquisaId == pesquisaId);
        }

        public async Task<bool> ExisteSessaoAtivaAsync(int pesquisaId)
        {
            return await _context.Set<SessoesInterativas>()
                .AnyAsync(s => s.PesquisaId == pesquisaId && s.Ativa);
        }

        public async Task<IEnumerable<SessoesInterativas>> ObterSessoesAtivasAsync()
        {
            return await _context.Set<SessoesInterativas>()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                .Where(s => s.Ativa)
                .ToListAsync();
        }

        public async Task<IEnumerable<SessoesInterativas>> ObterSessoesPorUsuarioAsync(int loginId)
        {
            return await _context.Set<SessoesInterativas>()
                .Include(s => s.Pesquisa)
                .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                .Where(s => s.Pesquisa.Loginid == loginId)
                .OrderByDescending(s => s.CriadaEm)
                .ToListAsync();
        }
    }
}
