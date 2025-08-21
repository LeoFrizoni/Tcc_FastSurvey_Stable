#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RespostaRepository : Repository<Respostas>, IRespostaRepository
    {
        public RespostaRepository(FastSurveyContext context) : base(context) { }

        public async Task<List<Respostas>> ListarPorPerguntaAsync(
            int perguntaId,
            bool incluirOpcoes = true,
            bool incluirAnexos = true,
            int skip = 0,
            int take = 200,
            CancellationToken ct = default)
        {
            if (take <= 0) take = 200;

            IQueryable<Respostas> q = _set.AsNoTracking()
                                          .Where(r => r.Perguntaid == perguntaId);

            if (incluirOpcoes) q = q.Include(r => r.Opcao); // ICollection<Opcoespergunta>
            if (incluirAnexos) q = q.Include(r => r.Anexo); // ICollection<Anexos>

            q = q.OrderBy(r => r.Dataresposta);

            if (skip > 0) q = q.Skip(skip);
            q = q.Take(take);

            return await q.ToListAsync(ct);
        }
    }
}
