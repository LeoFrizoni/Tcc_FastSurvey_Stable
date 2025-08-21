#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using AnexoEntity = SISTEMA_FASTSURVEY.MODEL.Models.Anexos;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class AnexoRepository : Repository<AnexoEntity>, IAnexoRepository
    {
        private readonly FastSurveyContext _ctx;
        public AnexoRepository(FastSurveyContext context) : base(context) => _ctx = context;

        public Task<List<AnexoEntity>> ListarPorPesquisaAsync(int pesquisaId, CancellationToken ct = default)
        {
            return _ctx.Anexos.AsNoTracking()
                .Where(a => a.Pesquisaid == pesquisaId)
                .ToListAsync(ct);
        }

        public Task<List<AnexoEntity>> ListarPorPerguntaAsync(int perguntaId, CancellationToken ct = default)
        {
            return _ctx.Anexos.AsNoTracking()
                .Where(a => a.Perguntaid == perguntaId)
                .ToListAsync(ct);
        }
    }
}
