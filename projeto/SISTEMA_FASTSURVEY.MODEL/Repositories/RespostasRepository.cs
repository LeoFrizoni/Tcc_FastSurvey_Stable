#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RespostaRepository : Repository<Respostas>, IRespostaRepository
    {
        public RespostaRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<List<Respostas>> ListarPorPerguntaAsync(
            int perguntaId,
            bool incluirOpcoes = true,
            bool incluirAnexos = true,
            int skip = 0,
            int take = 200,
            CancellationToken ct = default
        )
        {
            if (take <= 0)
                take = 200;

            IQueryable<Respostas> q = _set.AsNoTracking().Where(r => r.PerguntaId == perguntaId);

            if (incluirOpcoes)
                q = q.Include(r => r.Opcao); // ICollection<OpcoesPergunta>
            if (incluirAnexos)
                q = q.Include(r => r.Anexo); // ICollection<Anexos>

            q = q.OrderBy(r => r.DataResposta);

            if (skip > 0)
                q = q.Skip(skip);
            q = q.Take(take);

            return await q.ToListAsync(ct);
        }

        public async Task<List<Respostas>> ListarPorPesquisaAsync(
            int pesquisaId,
            bool incluirOpcoes = false,
            bool incluirAnexos = false,
            int skip = 0,
            int take = 500,
            CancellationToken ct = default
        )
        {
            if (take <= 0)
                take = 500;

            IQueryable<Respostas> q = _set.AsNoTracking()
                .Where(r => r.Pergunta.PesquisaId == pesquisaId); // via navegação

            if (incluirOpcoes)
                q = q.Include(r => r.Opcao);
            if (incluirAnexos)
                q = q.Include(r => r.Anexo);

            q = q.OrderBy(r => r.DataResposta);

            if (skip > 0)
                q = q.Skip(skip);
            q = q.Take(take);

            return await q.ToListAsync(ct);
        }

        public async Task<List<Respostas>> ListarPorSessaoAsync(
            string sessaoId,
            int? pesquisaId = null,
            bool incluirOpcoes = false,
            bool incluirAnexos = false,
            int skip = 0,
            int take = 500,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(sessaoId))
                return new();
            if (take <= 0)
                take = 500;

            IQueryable<Respostas> q = _set.AsNoTracking().Where(r => r.SessaoId == sessaoId);

            if (pesquisaId is not null)
                q = q.Where(r => r.Pergunta.PesquisaId == pesquisaId);

            if (incluirOpcoes)
                q = q.Include(r => r.Opcao);
            if (incluirAnexos)
                q = q.Include(r => r.Anexo);

            q = q.OrderBy(r => r.DataResposta);

            if (skip > 0)
                q = q.Skip(skip);
            q = q.Take(take);

            return await q.ToListAsync(ct);
        }

        public async Task<int> ContarPorPerguntaAsync(
            int perguntaId,
            bool somenteNaoAnonimas = false,
            string? sessaoId = null,
            CancellationToken ct = default
        )
        {
            IQueryable<Respostas> q = _set.AsNoTracking().Where(r => r.PerguntaId == perguntaId);

            if (somenteNaoAnonimas)
                q = q.Where(r => !r.RespostaAnonima);

            if (!string.IsNullOrWhiteSpace(sessaoId))
                q = q.Where(r => r.SessaoId == sessaoId);

            return await q.CountAsync(ct);
        }

        public async Task<RespostaResumoDto> ResumoDaPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            var baseQ = _set.AsNoTracking().Where(r => r.PerguntaId == perguntaId);

            // Totais
            var total = await baseQ.CountAsync(ct);
            var totalAnonimas = await baseQ.Where(r => r.RespostaAnonima).CountAsync(ct);
            var totalTexto = await baseQ
                .Where(r => r.Texto != null && r.Texto != "")
                .CountAsync(ct);
            var totalAnexo = await baseQ.Where(r => r.Anexo.Any()).CountAsync(ct);
            var totalOpcao = await baseQ.Where(r => r.Opcao.Any()).CountAsync(ct);

            // Faixa temporal
            var primeiro =
                (total > 0) ? await baseQ.MinAsync(r => r.DataResposta, ct) : (DateTime?)null;
            var ultimo =
                (total > 0) ? await baseQ.MaxAsync(r => r.DataResposta, ct) : (DateTime?)null;

            return new RespostaResumoDto
            {
                PerguntaId = perguntaId,
                Total = total,
                TotalAnonimas = totalAnonimas,
                TotalComTexto = totalTexto,
                TotalComAnexo = totalAnexo,
                TotalComOpcao = totalOpcao,
                PrimeiroRegistro = primeiro,
                UltimoRegistro = ultimo,
            };
        }

        public async Task<List<OpcaoContagemDto>> ContagemPorOpcaoAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            // Observação: sem alterar DB, usamos a N:N Resposta<->Opcoes via Respostas.Opcao
            // A consulta projeta counts por OpcaoId.
            return await _set.AsNoTracking()
                .Where(r => r.PerguntaId == perguntaId)
                .SelectMany(r => r.Opcao) // "explode" seleção de opções
                .GroupBy(op => op.OpcaoId)
                .Select(g => new OpcaoContagemDto { OpcaoId = g.Key, Qtd = g.Count() })
                .OrderByDescending(x => x.Qtd)
                .ToListAsync(ct);
        }

        public async Task<List<RespostaTextoDto>> ListarTextosPorPerguntaAsync(
            int perguntaId,
            int skip = 0,
            int take = 500,
            CancellationToken ct = default
        )
        {
            if (take <= 0)
                take = 500;

            IQueryable<Respostas> q = _set.AsNoTracking()
                .Where(r => r.PerguntaId == perguntaId && r.Texto != null && r.Texto != "")
                .OrderByDescending(r => r.DataResposta);

            if (skip > 0)
                q = q.Skip(skip);
            q = q.Take(take);

            return await q.Select(r => new RespostaTextoDto
                {
                    RespostaId = r.RespostaId,
                    PerguntaId = r.PerguntaId,
                    DataResposta = r.DataResposta,
                    SessaoId = r.SessaoId,
                    RespostaAnonima = r.RespostaAnonima,
                    Texto = r.Texto!,
                })
                .ToListAsync(ct);
        }

        public async Task<int> RemoverPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            // Sem alterar DB: buscamos IDs e removemos em lote.
            var ids = await _set.AsNoTracking()
                .Where(r => r.PerguntaId == perguntaId)
                .Select(r => r.RespostaId)
                .ToListAsync(ct);

            if (ids.Count == 0)
                return 0;

            var stubList = ids.Select(id => new Respostas { RespostaId = id }).ToList();
            _set.RemoveRange(stubList);
            return await _context.SaveChangesAsync(ct);
        }

        public async Task<int> RemoverPorSessaoAsync(
            string sessaoId,
            int? pesquisaId = null,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(sessaoId))
                return 0;

            var q = _set.AsNoTracking().Where(r => r.SessaoId == sessaoId);

            if (pesquisaId is not null)
                q = q.Where(r => r.Pergunta.PesquisaId == pesquisaId);

            var ids = await q.Select(r => r.RespostaId).ToListAsync(ct);
            if (ids.Count == 0)
                return 0;

            var stubList = ids.Select(id => new Respostas { RespostaId = id }).ToList();
            _set.RemoveRange(stubList);
            return await _context.SaveChangesAsync(ct);
        }
    }
}
