using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class ParticipanteSessaoRepository : Repository<ParticipantesSessao>, IParticipanteSessaoRepository
    {
        public ParticipanteSessaoRepository(FastSurveyContext context) : base(context)
        {
        }

        public async Task<ParticipantesSessao?> ObterPorNomeESessaoAsync(string nome, string sessaoId)
        {
            return await _context.Set<ParticipantesSessao>()
                .FirstOrDefaultAsync(p => p.NomeParticipante == nome && p.SessaoId == sessaoId && p.SaiuEm == null);
        }

        public async Task<IEnumerable<ParticipantesSessao>> ObterParticipantesAtivosAsync(string sessaoId)
        {
            return await _context.Set<ParticipantesSessao>()
                .Where(p => p.SessaoId == sessaoId && p.SaiuEm == null)
                .OrderBy(p => p.EntrouEm)
                .ToListAsync();
        }

        public async Task<int> ContarParticipantesAtivosAsync(string sessaoId)
        {
            return await _context.Set<ParticipantesSessao>()
                .CountAsync(p => p.SessaoId == sessaoId && p.SaiuEm == null);
        }

        public async Task<IEnumerable<ParticipantesSessao>> ObterParticipantesPorSessaoAsync(string sessaoId)
        {
            return await _context.Set<ParticipantesSessao>()
                .Where(p => p.SessaoId == sessaoId)
                .OrderBy(p => p.EntrouEm)
                .ToListAsync();
        }

        public async Task MarcarSaidaParticipantesAsync(string sessaoId)
        {
            var participantes = await _context.Set<ParticipantesSessao>()
                .Where(p => p.SessaoId == sessaoId && p.SaiuEm == null)
                .ToListAsync();

            foreach (var participante in participantes)
            {
                participante.SaiuEm = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
