#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using PerguntaEntity = SISTEMA_FASTSURVEY.MODEL.Models.Perguntas;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class PerguntaRepository : Repository<PerguntaEntity>, IPerguntaRepository
    {
        public PerguntaRepository(FastSurveyContext context) : base(context) { }

        public async Task<List<PerguntaEntity>> ListarPorPesquisaAsync(
            int pesquisaId,
            bool incluirOpcoes = true,
            bool incluirAnexos = true,
            CancellationToken ct = default)
        {
            IQueryable<PerguntaEntity> q = _set
                .AsNoTracking()
                .Where(p => p.Pesquisaid == pesquisaId);

            if (incluirOpcoes) q = q.Include(p => p.Opcoespergunta);
            if (incluirAnexos) q = q.Include(p => p.Anexos);

            return await q.OrderBy(p => p.Ordem).ToListAsync(ct);
        }
    }
}
