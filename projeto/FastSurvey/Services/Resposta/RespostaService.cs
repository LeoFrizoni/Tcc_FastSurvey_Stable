#nullable enable
using FASTSURVEY.Dtos.Respostas;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using RespostaModel = SISTEMA_FASTSURVEY.MODEL.Models.Respostas;

namespace FASTSURVEY.Services.Resposta
{
    public class RespostaService : IRespostaService
    {
        private readonly FastSurveyContext _ctx;

        public RespostaService(FastSurveyContext ctx) => _ctx = ctx;

        // --- helpers ---
        private static RespostaDto MapToDto(Respostas r) =>
            new()
            {
                RespostaId = r.RespostaId,
                PerguntaId = r.PerguntaId,
                Texto = r.Texto,
                DataResposta = r.DataResposta,
                Opcoes = r.Opcao?.Select(o => o.OpcaoId).ToList() ?? new(),
                RespostaAnonima = r.RespostaAnonima,
                SessaoId = r.SessaoId,
                ParticipanteId = r.ParticipanteId,
            };

        // --- criação unitária ---

        public async Task<RespostaModel> CriarDiscursivaAsync(
            CriarRespostaDiscursivaRequest req,
            CancellationToken ct = default
        )
        {
            var perguntaExiste = await _ctx
                .Perguntas.AsNoTracking()
                .AnyAsync(p => p.PerguntaId == req.PerguntaId, ct);
            if (!perguntaExiste)
                throw new InvalidOperationException("Pergunta não encontrada.");

            if (req.PesquisaId.HasValue)
            {
                var pertence = await _ctx
                    .Perguntas.AsNoTracking()
                    .AnyAsync(
                        p => p.PerguntaId == req.PerguntaId && p.PesquisaId == req.PesquisaId.Value,
                        ct
                    );
                if (!pertence)
                    throw new InvalidOperationException(
                        "A pergunta informada não pertence à pesquisa enviada."
                    );
            }

            var entity = new RespostaModel
            {
                PerguntaId = req.PerguntaId,
                Texto = (req.Texto ?? string.Empty).Trim(),
                DataResposta = DateTime.UtcNow,
                RespostaAnonima = req.RespostaAnonima,
                SessaoId = req.SessaoId,
                ParticipanteId = req.ParticipanteId,
            };

            _ctx.Respostas.Add(entity);
            await _ctx.SaveChangesAsync(ct);
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
                .FirstOrDefaultAsync(p => p.PerguntaId == req.PerguntaId, ct);
            if (pergunta is null)
                throw new InvalidOperationException("Pergunta não encontrada.");

            if (req.PesquisaId.HasValue)
            {
                var pertence = await _ctx
                    .Perguntas.AsNoTracking()
                    .AnyAsync(
                        p => p.PerguntaId == req.PerguntaId && p.PesquisaId == req.PesquisaId.Value,
                        ct
                    );
                if (!pertence)
                    throw new InvalidOperationException(
                        "A pergunta informada não pertence à pesquisa enviada."
                    );
            }

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
                RespostaAnonima = req.RespostaAnonima,
                SessaoId = req.SessaoId,
                ParticipanteId = req.ParticipanteId,
                Texto = textoOpcoes, // Campo obrigatório - armazena os IDs das opções
                Opcao = opcoes,
            };

            _ctx.Respostas.Add(entity);
            await _ctx.SaveChangesAsync(ct);
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
                            RespostaAnonima = request.RespostaAnonima,
                            SessaoId = request.SessaoId,
                            ParticipanteId = request.ParticipanteId,
                            Opcao = opcoes,
                        }
                    );
                }
            }

            _ctx.Respostas.AddRange(respostasCriadas);
            await _ctx.SaveChangesAsync(ct);

            return respostasCriadas.FirstOrDefault()?.RespostaId ?? 0;
        }

        // --- leitura ---

        public async Task<RespostaDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var r = await _ctx
                .Respostas.Include(x => x.Opcao)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RespostaId == id, ct);
            return r is null ? null : MapToDto(r);
        }

        public async Task<IEnumerable<RespostaDto>> ListarPorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            var list = await _ctx
                .Respostas.Include(r => r.Opcao)
                .Include(r => r.Pergunta)
                .AsNoTracking()
                .Where(r => r.Pergunta.PesquisaId == pesquisaId)
                .OrderBy(r => r.DataResposta)
                .ToListAsync(ct);

            return list.Select(MapToDto);
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
            var r = await _ctx.Respostas.FirstOrDefaultAsync(x => x.RespostaId == id, ct);
            if (r is null)
                return false;

            _ctx.Respostas.Remove(r);
            await _ctx.SaveChangesAsync(ct);
            return true;
        }
    }
}
