#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class PesquisaRepository : Repository<Pesquisas>, IPesquisaRepository
    {
        public PesquisaRepository(FastSurveyContext context) : base(context) { }

        public async Task<Pesquisas?> GetDetalheAsync(
            int pesquisaId,
            bool incluirPerguntas = true,
            bool incluirAnexos = false,
            CancellationToken ct = default)
        {
            IQueryable<Pesquisas> q = _set.AsNoTracking()
                                          .Where(p => p.Pesquisaid == pesquisaId);

            if (incluirPerguntas)
            {
                // Perguntas + filhos corretos (nomes conforme seu scaffold)
                q = q.Include(p => p.Perguntas)
                     .ThenInclude(pg => pg.Opcoespergunta);

                q = q.Include(p => p.Perguntas)
                     .ThenInclude(pg => pg.Anexos);

                // Opcional: incluir também tipo da pergunta e respostas
                // q = q.Include(p => p.Perguntas)
                //      .ThenInclude(pg => pg.Tipopergunta);
                // q = q.Include(p => p.Perguntas)
                //      .ThenInclude(pg => pg.Respostas);
            }

            if (incluirAnexos)
            {
                // Anexos ligados diretamente à Pesquisa
                q = q.Include(p => p.Anexos);
            }

            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task<List<Pesquisas>> ListarPorLoginAsync(
            int loginId,
            int? pastaId = null,
            int? tipoPesquisaId = null,
            int skip = 0,
            int take = 50,
            CancellationToken ct = default)
        {
            if (take <= 0) take = 50;

            IQueryable<Pesquisas> q = _set.AsNoTracking()
                                          .Where(p => p.Loginid == loginId);

            if (pastaId is not null) q = q.Where(p => p.Pastaid == pastaId);
            if (tipoPesquisaId is not null) q = q.Where(p => p.Tipopesquisaid == tipoPesquisaId);

            q = q.OrderByDescending(p => p.Dataatualizacao ?? p.Datacriacao)
                 .ThenByDescending(p => p.Pesquisaid)
                 .Skip(skip)
                 .Take(take);

            return await q.ToListAsync(ct);
        }
    }
}
