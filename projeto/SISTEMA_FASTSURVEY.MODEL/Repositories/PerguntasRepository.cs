#nullable enable
using System.Linq; // <-- garante extensões LINQ (OrderBy/ThenBy/Where/etc)
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
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
            bool incluirTipoPergunta = false,
            bool incluirRespostas = false,
            bool asNoTracking = true,
            CancellationToken ct = default
        )
        {
            IQueryable<PerguntaEntity> q = _set
                .Where(p => p.PesquisaId == pesquisaId)
                .OrderBy(p => p.Ordem)        // ordenação previsível
                .ThenBy(p => p.PerguntaId);   // estabilidade sem índice único

            // Split só é realmente necessário quando há múltiplas coleções;
            // manter sempre ligado é seguro, então pode ficar como está:
            q = q.AsSplitQuery();

            if (asNoTracking)
                q = q.AsNoTracking();

            if (incluirOpcoes)
                q = q.Include(p => p.OpcoesPergunta);
            if (incluirAnexos)
                q = q.Include(p => p.Anexos);
            if (incluirTipoPergunta)
                q = q.Include(p => p.TipoPergunta);
            if (incluirRespostas)
                q = q.Include(p => p.Respostas); // ⚠ pode ser volumoso

            var list = await q.ToListAsync(ct);

            // EF não ordena coleções incluídas → ordenar em memória é correto.
            foreach (var p in list)
            {
                if (incluirOpcoes && p.OpcoesPergunta is not null)
                    p.OpcoesPergunta = p.OpcoesPergunta
                        .OrderBy(o => o.Ordem)   // ajuste se não existir "Ordem"
                        .ThenBy(o => o.OpcaoId)  // fallback estável
                        .ToList();

                if (incluirAnexos && p.Anexos is not null)
                    p.Anexos = p.Anexos
                        .OrderBy(a => a.AnexoId)
                        .ToList();

                if (incluirRespostas && p.Respostas is not null)
                    p.Respostas = p.Respostas
                        .OrderBy(r => r.RespostaId) // já estava; acrescente ThenBy se existir outro critério
                        .ToList();
            }

            return list;
        }

        public async Task<List<PerguntaResumoDto>> ListarResumoPorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            // Projeção leve: não carrega coleções, só contadores (COUNT no SQL)
            return await _set.AsNoTracking()
                .Where(p => p.PesquisaId == pesquisaId)
                .OrderBy(p => p.Ordem)
                .ThenBy(p => p.PerguntaId)
                .Select(p => new PerguntaResumoDto
                {
                    PerguntaId = p.PerguntaId,
                    Texto = p.Texto,
                    Ordem = p.Ordem,
                    TipoPerguntaId = p.TipoPerguntaId,
                    TemGabarito = p.TemGabarito,
                    PermiteMultiplasSelecao = p.PermiteMultiplasSelecao,
                    QtdeOpcoes = p.OpcoesPergunta.Count, // COUNT(*) no banco
                    QtdeAnexos = p.Anexos.Count,
                })
                .ToListAsync(ct);
        }
    }
}
