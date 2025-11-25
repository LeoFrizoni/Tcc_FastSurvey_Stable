#nullable enable
using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Analises;
using FASTSURVEY.Services.Cache;
using FASTSURVEY.Services.Resultados;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using RespostaModel = SISTEMA_FASTSURVEY.MODEL.Models.Respostas;

namespace FASTSURVEY.Services.Resposta
{
    public class RespostaService : IRespostaService
    {
        private readonly FastSurveyContext _ctx;
        private readonly ISentimentAnalysisDispatcher _sentimentDispatcher;
        private readonly ICacheService _cache;

        public RespostaService(
            FastSurveyContext ctx,
            ISentimentAnalysisDispatcher sentimentDispatcher,
            ICacheService cache
        )
        {
            _ctx = ctx;
            _sentimentDispatcher = sentimentDispatcher;
            _cache = cache;
        }

        // --- helpers ---
        private static RespostaDto MapToDto(Respostas r, string? sessaoCodigo = null) =>
            new()
            {
                RespostaId = r.RespostaId,
                PerguntaId = r.PerguntaId,
                Texto = r.Texto,
                DataResposta = r.DataResposta,
                RespondidaEm = r.RespondidaEm,
                Opcoes = r.Opcao?.Select(o => o.OpcaoId).ToList() ?? new(),
                RespostaAnonima = r.RespostaAnonima,
                SessaoId = r.SessaoId,
                SessaoCodigo = sessaoCodigo,
                ParticipanteId = r.ParticipanteId,
                ParticipanteNome = r.Participante?.NomeParticipante,
            };

        // --- criação unitária ---

        private Task InvalidarResultadosAsync(
            int pesquisaId,
            IEnumerable<int>? perguntaIds,
            CancellationToken ct
        ) => ResultadosCacheHelper.InvalidatePesquisaAsync(_cache, pesquisaId, perguntaIds, ct);

        public async Task<RespostaModel> CriarDiscursivaAsync(
            CriarRespostaDiscursivaRequest req,
            CancellationToken ct = default
        )
        {
            var pergunta = await _ctx
                .Perguntas.AsNoTracking()
                .Select(p => new { p.PerguntaId, p.PesquisaId })
                .FirstOrDefaultAsync(p => p.PerguntaId == req.PerguntaId, ct);
            if (pergunta is null)
                throw new InvalidOperationException("Pergunta não encontrada.");

            var pesquisaId = req.PesquisaId ?? pergunta.PesquisaId;

            if (req.PesquisaId.HasValue && pergunta.PesquisaId != req.PesquisaId.Value)
            {
                throw new InvalidOperationException(
                    "A pergunta informada não pertence à pesquisa enviada."
                );
            }

            var textoAjustado = (req.Texto ?? string.Empty).Trim();

            var entity = new RespostaModel
            {
                PerguntaId = req.PerguntaId,
                Texto = textoAjustado,
                DataResposta = DateTime.UtcNow,
                RespondidaEm = DateTime.UtcNow,
                RespostaAnonima = req.RespostaAnonima,
                SessaoId = req.SessaoId,
                ParticipanteId = req.ParticipanteId,
            };

            _ctx.Respostas.Add(entity);
            await _ctx.SaveChangesAsync(ct);
            await _sentimentDispatcher.AnalyzeAsync(entity.RespostaId, textoAjustado, ct);
            await InvalidarResultadosAsync(pesquisaId, new[] { req.PerguntaId }, ct);
            return entity;
        }

        public async Task<RespostaModel> CriarOpcoesAsync(
            CriarRespostaOpcoesRequest req,
            CancellationToken ct = default
        )
        {
            if (req.OpcoesSelecionadas is null || req.OpcoesSelecionadas.Count == 0)
                throw new InvalidOperationException("Envie ao menos uma opção selecionada.");

            var opSel = req.OpcoesSelecionadas.Distinct().ToList();

            var pergunta = await _ctx
                .Perguntas.AsNoTracking()
                .Select(p => new { p.PerguntaId, p.PesquisaId })
                .FirstOrDefaultAsync(p => p.PerguntaId == req.PerguntaId, ct);
            if (pergunta is null)
                throw new InvalidOperationException("Pergunta não encontrada.");

            var pesquisaId = req.PesquisaId ?? pergunta.PesquisaId;

            if (req.PesquisaId.HasValue && pergunta.PesquisaId != req.PesquisaId.Value)
                throw new InvalidOperationException(
                    "A pergunta informada não pertence à pesquisa enviada."
                );

            var opcoes = await _ctx
                .OpcoesPergunta.Where(o => opSel.Contains(o.OpcaoId))
                .ToListAsync(ct);

            if (opcoes.Count != opSel.Count)
                throw new InvalidOperationException("Uma ou mais opções não foram encontradas.");

            if (opcoes.Any(o => o.PerguntaId != req.PerguntaId))
                throw new InvalidOperationException(
                    "Foram enviadas opções que não pertencem a essa pergunta."
                );

            var textoOpcoes = string.Join(", ", opcoes.Select(o => $"{o.OpcaoId}"));

            var entity = new RespostaModel
            {
                PerguntaId = req.PerguntaId,
                DataResposta = DateTime.UtcNow,
                RespondidaEm = DateTime.UtcNow,
                RespostaAnonima = req.RespostaAnonima,
                SessaoId = req.SessaoId,
                ParticipanteId = req.ParticipanteId,
                Texto = textoOpcoes, // Campo obrigatório - armazena os IDs das opções
                Opcao = opcoes,
            };

            _ctx.Respostas.Add(entity);
            await _ctx.SaveChangesAsync(ct);
            await InvalidarResultadosAsync(pesquisaId, new[] { req.PerguntaId }, ct);
            return entity;
        }

        // --- criação em lote (completa) ---

        public async Task<int> EnviarRespostaCompletaAsync(
            int pesquisaId,
            EnviarRespostaCompletaRequest request,
            CancellationToken ct = default
        )
        {
            var pesquisa =
                await _ctx
                    .Pesquisas.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct)
                ?? throw new InvalidOperationException("Pesquisa não encontrada.");

            var respostasCriadas = new List<Respostas>();

            foreach (var item in request.Respostas)
            {
                if (!string.IsNullOrWhiteSpace(item.Texto))
                {
                    respostasCriadas.Add(
                        new Respostas
                        {
                            PerguntaId = item.PerguntaId,
                            Texto = item.Texto.Trim(),
                            DataResposta = DateTime.UtcNow,
                            RespondidaEm = DateTime.UtcNow,
                            RespostaAnonima = request.RespostaAnonima,
                            SessaoId = request.SessaoId,
                            ParticipanteId = request.ParticipanteId,
                        }
                    );
                }
                else if (item.OpcoesSelecionadas?.Any() == true)
                {
                    var ids = item.OpcoesSelecionadas.Distinct().ToList();
                    var opcoes = await _ctx
                        .OpcoesPergunta.Where(o => ids.Contains(o.OpcaoId))
                        .ToListAsync(ct);

                    respostasCriadas.Add(
                        new Respostas
                        {
                            PerguntaId = item.PerguntaId,
                            DataResposta = DateTime.UtcNow,
                            RespondidaEm = DateTime.UtcNow,
                            RespostaAnonima = request.RespostaAnonima,
                            SessaoId = request.SessaoId,
                            ParticipanteId = request.ParticipanteId,
                            Texto = string.Join(", ", opcoes.Select(o => o.OpcaoId)),
                            Opcao = opcoes,
                        }
                    );
                }
            }

            _ctx.Respostas.AddRange(respostasCriadas);
            await _ctx.SaveChangesAsync(ct);

            var tarefasAnalise = respostasCriadas
                .Where(r => !string.IsNullOrWhiteSpace(r.Texto) && (r.Opcao == null || r.Opcao.Count == 0))
                .Select(r => _sentimentDispatcher.AnalyzeAsync(r.RespostaId, r.Texto!, ct));
            await Task.WhenAll(tarefasAnalise);

            if (respostasCriadas.Count > 0)
            {
                await InvalidarResultadosAsync(
                    pesquisaId,
                    respostasCriadas.Select(r => r.PerguntaId),
                    ct
                );
            }

            return respostasCriadas.FirstOrDefault()?.RespostaId ?? 0;
        }

        // --- leitura ---

        public async Task<RespostaDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var r = await _ctx
                .Respostas.Include(x => x.Opcao)
                .Include(x => x.Participante)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RespostaId == id, ct);
            if (r is null)
                return null;

            string? sessaoCodigo = null;
            if (!string.IsNullOrWhiteSpace(r.SessaoId))
            {
                sessaoCodigo = await _ctx
                    .SessoesInterativas.AsNoTracking()
                    .Where(s => s.SessaoId == r.SessaoId)
                    .Select(s => s.CodigoAcesso)
                    .FirstOrDefaultAsync(ct);
            }

            return MapToDto(r, sessaoCodigo);
        }

        public async Task<IEnumerable<RespostaDto>> ListarPorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            var list = await _ctx
                .Respostas.Include(r => r.Opcao)
                .Include(r => r.Pergunta)
                .Include(r => r.Participante)
                .AsNoTracking()
                .Where(r => r.Pergunta.PesquisaId == pesquisaId)
                .OrderBy(r => r.DataResposta)
                .ToListAsync(ct);

            var sessaoIds = list
                .Select(r => r.SessaoId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var codigos = sessaoIds.Count == 0
                ? new Dictionary<string, string>()
                : await _ctx
                    .SessoesInterativas.AsNoTracking()
                    .Where(s => sessaoIds.Contains(s.SessaoId))
                    .ToDictionaryAsync(s => s.SessaoId, s => s.CodigoAcesso, ct);

            return list.Select(r =>
            {
                var codigo = r.SessaoId != null
                    && codigos.TryGetValue(r.SessaoId, out var value)
                    ? value
                    : null;
                return MapToDto(r, codigo);
            });
        }

        public async Task<int> ObterTotalRespostasAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            return await _ctx
                .Respostas.Include(r => r.Pergunta)
                .Where(r => r.Pergunta.PesquisaId == pesquisaId)
                .CountAsync(ct);
        }

        // --- analytics (baseline simples) ---

        public async Task<AnalyticsResponse?> ObterAnalyticsAsync(
            int pesquisaId,
            EstatisticasPesquisaRequest request,
            CancellationToken ct = default
        )
        {
            var pesquisa = await _ctx
                .Pesquisas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (pesquisa is null)
                return null;

            var q = _ctx
                .Respostas.AsNoTracking()
                .Include(r => r.Pergunta)
                .Where(r => r.Pergunta.PesquisaId == pesquisaId);

            if (request.DataInicio.HasValue)
                q = q.Where(r => r.DataResposta >= request.DataInicio.Value);
            if (request.DataFim.HasValue)
                q = q.Where(r => r.DataResposta <= request.DataFim.Value);

            var total = await q.CountAsync(ct);

            var porDia = await q.GroupBy(r => r.DataResposta.Date)
                .Select(g => new { Dia = g.Key, Cnt = g.Count() })
                .OrderBy(x => x.Dia)
                .ToListAsync(ct);

            return new AnalyticsResponse
            {
                PesquisaId = pesquisaId,
                TotalRespostas = total,
                TaxaConclusao = 100.0, // TODO: calcular com base em iniciadas vs concluídas
                TempoMedioResposta = TimeSpan.Zero, // TODO: calcular quando houver métrica
                RespostasPorPeriodo = porDia.ToDictionary(
                    x => x.Dia.ToString("yyyy-MM-dd"),
                    x => x.Cnt
                ),
                RespostasPorOrigem = new(),
                EstatisticasPerguntas = new(),
            };
        }

        // --- validação ---

        public async Task<ValidacaoRespostaResponse> ValidarRespostaAsync(
            int pesquisaId,
            ValidarRespostaRequest request,
            CancellationToken ct = default
        )
        {
            var v = new ValidacaoRespostaResponse { Valida = true };

            var pesquisa = await _ctx
                .Pesquisas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (pesquisa is null)
            {
                v.Valida = false;
                v.Erros.Add("Pesquisa não encontrada");
                return v;
            }

            if (!pesquisa.Ativa)
            {
                v.Valida = false;
                v.Erros.Add("Pesquisa não está ativa");
            }

            if (
                pesquisa.TemLimitadorTempo
                && pesquisa.DataFechamento.HasValue
                && pesquisa.DataFechamento.Value < DateTime.UtcNow
            )
            {
                v.Valida = false;
                v.Erros.Add("Pesquisa expirou");
            }

            if (request.Respostas.Count == 0)
            {
                v.Valida = false;
                v.Erros.Add("Nenhuma resposta enviada.");
                return v;
            }

            var idsPerguntas = request.Respostas.Select(r => r.PerguntaId).Distinct().ToList();

            var perguntas = await _ctx
                .Perguntas.AsNoTracking()
                .Where(p => p.PesquisaId == pesquisaId && idsPerguntas.Contains(p.PerguntaId))
                .Select(p => new { p.PerguntaId, p.Texto })
                .ToDictionaryAsync(p => p.PerguntaId, ct);

            foreach (var item in request.Respostas)
            {
                if (!perguntas.TryGetValue(item.PerguntaId, out var pergunta))
                {
                    v.Valida = false;
                    v.Erros.Add($"Pergunta {item.PerguntaId} não encontrada na pesquisa.");
                    continue;
                }

                var temConteudo =
                    !string.IsNullOrWhiteSpace(item.Texto)
                    || (item.OpcoesSelecionadas?.Any() == true);

                if (!temConteudo)
                {
                    v.Valida = false;
                    v.Erros.Add($"Pergunta '{pergunta.Texto}' é obrigatória.");
                    continue;
                }

                if (item.OpcoesSelecionadas?.Any() == true)
                {
                    var opIds = item.OpcoesSelecionadas.Distinct().ToList();
                    var opCount = await _ctx
                        .OpcoesPergunta.AsNoTracking()
                        .Where(o => o.PerguntaId == item.PerguntaId && opIds.Contains(o.OpcaoId))
                        .CountAsync(ct);

                    if (opCount != opIds.Count)
                    {
                        v.Valida = false;
                        v.Erros.Add(
                            $"Uma ou mais opções inválidas para a pergunta '{pergunta.Texto}'."
                        );
                    }
                }
            }

            return v;
        }

        // --- exclusão ---

        public async Task<bool> ExcluirAsync(int id, CancellationToken ct = default)
        {
            var r = await _ctx
                .Respostas.Include(x => x.Pergunta)
                .FirstOrDefaultAsync(x => x.RespostaId == id, ct);
            if (r is null)
                return false;

            var pesquisaId = r.Pergunta?.PesquisaId;
            var perguntaId = r.PerguntaId;

            _ctx.Respostas.Remove(r);
            await _ctx.SaveChangesAsync(ct);
            if (pesquisaId.HasValue)
            {
                await InvalidarResultadosAsync(pesquisaId.Value, new[] { perguntaId }, ct);
            }
            return true;
        }
    }
}
