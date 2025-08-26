using System.Text.Json;
using FASTSURVEY.Dtos.Analytics;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        // Tipos internos para evitar anônimos em coleções/dicionários
        private sealed class RespLite
        {
            public int PerguntaId { get; set; }
            public string? Texto { get; set; }
            public DateTime DataResposta { get; set; }
        }

        private sealed class OpcaoLite
        {
            public int OpcaoId { get; set; }
            public string Texto { get; set; } = string.Empty;
        }

        private sealed class PerguntaLite
        {
            public int PerguntaId { get; set; }
            public string Texto { get; set; } = string.Empty;
            public string Tipo { get; set; } = "Desconhecido";
            public List<OpcaoLite> Opcoes { get; set; } = new();
        }

        // Para o comparativo
        private sealed class PerguntaResumoLite
        {
            public int PerguntaId { get; set; }
            public int PesquisaId { get; set; }
            public string Texto { get; set; } = string.Empty;
        }

        private readonly FastSurveyContext _ctx;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(FastSurveyContext ctx, ILogger<AnalyticsService> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public async Task<MetricasPerformanceResponse> GetMetricasPerformanceAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            try
            {
                // Pega todas as pesquisas do usuário (somente IDs)
                var pesquisas = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Where(p => p.LoginId == loginId)
                    .Select(p => p.PesquisaId)
                    .ToListAsync(ct);

                if (!pesquisas.Any())
                    return new MetricasPerformanceResponse
                    {
                        TempoMedioResposta = 0,
                        TaxaConclusao = 0,
                        PesquisasMaisPopulares = 0,
                        MetricasTempoReal = new List<MetricaTempoReal>(),
                    };

                // Ex.: top pesquisas por volume de respostas
                var pesquisasMaisPopulares = await _ctx
                    .Respostas.AsNoTracking()
                    .Where(r => pesquisas.Contains(r.Pergunta.Pesquisa.PesquisaId))
                    .GroupBy(r => r.Pergunta.Pesquisa.PesquisaId)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .CountAsync(ct);

                // Simulados (mantendo sua semântica original)
                var tempoMedioResposta = 120.0; // segundos
                var taxaConclusao = 85.5; // %

                var agora = DateTime.Now;
                var metricasTempoReal = new List<MetricaTempoReal>
                {
                    new()
                    {
                        Timestamp = agora.AddMinutes(-5),
                        RespostasAtivas = 12,
                        UsuariosConectados = 8,
                    },
                    new()
                    {
                        Timestamp = agora.AddMinutes(-4),
                        RespostasAtivas = 15,
                        UsuariosConectados = 10,
                    },
                    new()
                    {
                        Timestamp = agora.AddMinutes(-3),
                        RespostasAtivas = 18,
                        UsuariosConectados = 12,
                    },
                    new()
                    {
                        Timestamp = agora.AddMinutes(-2),
                        RespostasAtivas = 14,
                        UsuariosConectados = 9,
                    },
                    new()
                    {
                        Timestamp = agora.AddMinutes(-1),
                        RespostasAtivas = 16,
                        UsuariosConectados = 11,
                    },
                    new()
                    {
                        Timestamp = agora,
                        RespostasAtivas = 13,
                        UsuariosConectados = 7,
                    },
                };

                return new MetricasPerformanceResponse
                {
                    TempoMedioResposta = tempoMedioResposta,
                    TaxaConclusao = taxaConclusao,
                    PesquisasMaisPopulares = pesquisasMaisPopulares,
                    MetricasTempoReal = metricasTempoReal,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao obter métricas de performance para loginId {LoginId}",
                    loginId
                );
                throw;
            }
        }

        public async Task<DashboardAnalyticsResponse> GetDashboardAnalyticsAsync(
            int loginId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisasBase = _ctx.Pesquisas.AsNoTracking().Where(p => p.LoginId == loginId);

                if (startDate.HasValue)
                    pesquisasBase = pesquisasBase.Where(p => p.DataCriacao >= startDate.Value);
                if (endDate.HasValue)
                    pesquisasBase = pesquisasBase.Where(p => p.DataCriacao <= endDate.Value);

                // 1) Lista básica de pesquisas + ids
                var pesquisas = await pesquisasBase
                    .Select(p => new
                    {
                        p.PesquisaId,
                        p.TipoPesquisaId,
                        p.DataCriacao,
                    })
                    .ToListAsync(ct);

                var pesquisaIds = pesquisas.Select(p => p.PesquisaId).ToList();
                var totalPesquisas = pesquisas.Count;

                // Se não houver pesquisas, devolve vazio rápido
                if (totalPesquisas == 0)
                {
                    return new DashboardAnalyticsResponse
                    {
                        TotalPesquisas = 0,
                        TotalRespostas = 0,
                        PesquisasAtivas = 0,
                        TaxaRespostaMedia = 0,
                        RespostasPorDia = new(),
                        PesquisasPorTipo = new(),
                        PerformanceMensal = new(),
                        PesquisasRecentes = new(),
                    };
                }

                // 2) Agregados de respostas por pesquisa (uma única consulta)
                var agregadosRespostasPorPesquisa = await _ctx
                    .Respostas.AsNoTracking()
                    .Where(r => pesquisaIds.Contains(r.Pergunta.Pesquisa.PesquisaId))
                    .GroupBy(r => r.Pergunta.Pesquisa.PesquisaId)
                    .Select(g => new { PesquisaId = g.Key, Count = g.Count() })
                    .ToListAsync(ct);

                var respostasCountByPesquisaId = agregadosRespostasPorPesquisa.ToDictionary(
                    x => x.PesquisaId,
                    x => x.Count
                );

                var totalRespostas = agregadosRespostasPorPesquisa.Sum(x => x.Count);
                var pesquisasAtivas = agregadosRespostasPorPesquisa.Count; // grupos distintos com >= 1 resposta
                var taxaRespostaMedia =
                    totalPesquisas > 0 ? (double)pesquisasAtivas / totalPesquisas * 100.0 : 0.0;

                // 3) Respostas por dia (últimos 30 dias) — uma query
                var dataInicio = DateTime.Now.Date.AddDays(-30);
                var respostasPorDia = await _ctx
                    .Respostas.AsNoTracking()
                    .Where(r =>
                        pesquisaIds.Contains(r.Pergunta.Pesquisa.PesquisaId)
                        && r.DataResposta >= dataInicio
                    )
                    .GroupBy(r => r.DataResposta.Date)
                    .Select(g => new GraficoRespostasPorDia
                    {
                        Data = g.Key,
                        TotalRespostas = g.Count(),
                        PesquisasAtivas = g.Select(x => x.Pergunta.Pesquisa.PesquisaId)
                            .Distinct()
                            .Count(),
                    })
                    .OrderBy(x => x.Data)
                    .ToListAsync(ct);

                // 4) Pesquisas por tipo — uma query + cálculo do percentual em memória
                var porTipo = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Where(p => p.LoginId == loginId)
                    .Join(
                        _ctx.TipoPesquisa.AsNoTracking(),
                        p => p.TipoPesquisaId,
                        t => t.TipoPesquisaId,
                        (p, t) => new { t.TipoPesquisa1 }
                    )
                    .GroupBy(x => x.TipoPesquisa1)
                    .Select(g => new { Tipo = g.Key, Qtde = g.Count() })
                    .ToListAsync(ct);

                var pesquisasPorTipo = porTipo
                    .Select(x => new GraficoTipoPesquisa
                    {
                        TipoPesquisa = x.Tipo,
                        Quantidade = x.Qtde,
                        Percentual =
                            totalPesquisas > 0 ? (double)x.Qtde / totalPesquisas * 100.0 : 0.0,
                    })
                    .ToList();

                // 5) Performance mensal (últimos 6 meses) — 2 queries e merge em memória
                var inicio6Meses = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(
                    -5
                );

                var pesquisasCriadasPorMes = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Where(p => p.LoginId == loginId && p.DataCriacao >= inicio6Meses)
                    .GroupBy(p => new { p.DataCriacao.Year, p.DataCriacao.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Count = g.Count(),
                    })
                    .ToListAsync(ct);

                var respostasPorMes = await _ctx
                    .Respostas.AsNoTracking()
                    .Where(r =>
                        pesquisaIds.Contains(r.Pergunta.Pesquisa.PesquisaId)
                        && r.DataResposta >= inicio6Meses
                    )
                    .GroupBy(r => new { r.DataResposta.Year, r.DataResposta.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Count = g.Count(),
                    })
                    .ToListAsync(ct);

                var perfMensal = new List<GraficoPerformance>();
                for (int i = 5; i >= 0; i--)
                {
                    var mesRef = DateTime.Now.AddMonths(-i);
                    var y = mesRef.Year;
                    var m = mesRef.Month;

                    var criadas =
                        pesquisasCriadasPorMes
                            .FirstOrDefault(x => x.Year == y && x.Month == m)
                            ?.Count ?? 0;
                    var recebidas =
                        respostasPorMes.FirstOrDefault(x => x.Year == y && x.Month == m)?.Count
                        ?? 0;

                    perfMensal.Add(
                        new GraficoPerformance
                        {
                            Mes = mesRef.ToString("MMM/yyyy"),
                            PesquisasCriadas = criadas,
                            RespostasRecebidas = recebidas,
                            TaxaEngajamento = criadas > 0 ? (double)recebidas / criadas : 0.0,
                        }
                    );
                }

                // 6) Pesquisas recentes (top 5) sem subqueries por item
                var pesquisasRecentesBasicas = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Where(p => p.LoginId == loginId)
                    .OrderByDescending(p => p.DataCriacao)
                    .Take(5)
                    .Select(p => new
                    {
                        p.PesquisaId,
                        p.Titulo,
                        p.DataCriacao,
                    })
                    .ToListAsync(ct);

                var idsRecentes = pesquisasRecentesBasicas.Select(p => p.PesquisaId).ToList();

                var respostasCountRecentes = await _ctx
                    .Respostas.AsNoTracking()
                    .Where(r => idsRecentes.Contains(r.Pergunta.Pesquisa.PesquisaId))
                    .GroupBy(r => r.Pergunta.Pesquisa.PesquisaId)
                    .Select(g => new { PesquisaId = g.Key, Count = g.Count() })
                    .ToListAsync(ct);

                var mapRecentes = respostasCountRecentes.ToDictionary(
                    x => x.PesquisaId,
                    x => x.Count
                );

                var pesquisasRecentes = pesquisasRecentesBasicas
                    .Select(p => new PesquisaRecente
                    {
                        PesquisaId = p.PesquisaId,
                        Titulo = p.Titulo,
                        DataCriacao = p.DataCriacao,
                        TotalRespostas = mapRecentes.TryGetValue(p.PesquisaId, out var c) ? c : 0,
                        Status =
                            (mapRecentes.TryGetValue(p.PesquisaId, out var c2) && c2 > 0)
                                ? "Ativa"
                                : "Inativa",
                    })
                    .ToList();

                return new DashboardAnalyticsResponse
                {
                    TotalPesquisas = totalPesquisas,
                    TotalRespostas = totalRespostas,
                    PesquisasAtivas = pesquisasAtivas,
                    TaxaRespostaMedia = taxaRespostaMedia,
                    RespostasPorDia = respostasPorDia,
                    PesquisasPorTipo = pesquisasPorTipo,
                    PerformanceMensal = perfMensal,
                    PesquisasRecentes = pesquisasRecentes,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao obter analytics do dashboard para loginId {LoginId}",
                    loginId
                );
                throw;
            }
        }

        public async Task<RelatorioPesquisaResponse> GetRelatorioPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Where(p => p.PesquisaId == pesquisaId)
                    .Select(p => new
                    {
                        p.PesquisaId,
                        p.Titulo,
                        p.Descricao,
                        p.DataCriacao,
                    })
                    .FirstOrDefaultAsync(ct);

                if (pesquisa == null)
                    throw new ArgumentException("Pesquisa não encontrada");

                var perguntas = await _ctx
                    .Perguntas.AsNoTracking()
                    .Where(p => p.PesquisaId == pesquisaId)
                    .OrderBy(p => p.Ordem)
                    .Select(p => new PerguntaLite
                    {
                        PerguntaId = p.PerguntaId,
                        Texto = p.Texto,
                        Tipo =
                            p.TipoPergunta != null ? p.TipoPergunta.TipoPergunta1 : "Desconhecido",
                        Opcoes = p
                            .OpcoesPergunta.Select(o => new OpcaoLite
                            {
                                OpcaoId = o.OpcaoId,
                                Texto = o.Texto,
                            })
                            .ToList(),
                    })
                    .ToListAsync(ct);

                var perguntaIds = perguntas.Select(p => p.PerguntaId).ToList();

                var respostas = await _ctx
                    .Respostas.AsNoTracking()
                    .Where(r => perguntaIds.Contains(r.PerguntaId))
                    .Select(r => new RespLite
                    {
                        PerguntaId = r.PerguntaId,
                        Texto = r.Texto,
                        DataResposta = r.DataResposta,
                    })
                    .ToListAsync(ct);

                var totalRespostas = respostas.Count;

                var perguntasById = perguntas.ToDictionary(p => p.PerguntaId, p => p);
                var respostasPorPergunta = respostas
                    .GroupBy(r => r.PerguntaId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var estatisticasPerguntas = new List<EstatisticaPergunta>();

                foreach (var p in perguntas)
                {
                    if (!respostasPorPergunta.TryGetValue(p.PerguntaId, out var listaResp))
                        listaResp = new List<RespLite>();

                    var estat = new EstatisticaPergunta
                    {
                        PerguntaId = p.PerguntaId,
                        Texto = p.Texto,
                        TipoPergunta = p.Tipo,
                        TotalRespostas = listaResp.Count,
                        Opcoes = new List<OpcaoEstatistica>(),
                        RespostasDiscursivas = new List<string>(),
                    };

                    if (p.Opcoes.Any())
                    {
                        estat.Opcoes = p
                            .Opcoes.Select(op =>
                            {
                                int qtd = listaResp.Count(r =>
                                {
                                    if (int.TryParse(r.Texto, out var asInt))
                                        return asInt == op.OpcaoId;
                                    return string.Equals(
                                        r.Texto,
                                        op.OpcaoId.ToString(),
                                        StringComparison.Ordinal
                                    );
                                });

                                return new OpcaoEstatistica
                                {
                                    Texto = op.Texto,
                                    Quantidade = qtd,
                                    Percentual =
                                        listaResp.Count > 0
                                            ? (double)qtd / listaResp.Count * 100.0
                                            : 0.0,
                                };
                            })
                            .ToList();
                    }
                    else
                    {
                        estat.RespostasDiscursivas = listaResp
                            .Select(r => r.Texto)
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .Take(10)
                            .ToList()!;
                    }

                    estatisticasPerguntas.Add(estat);
                }

                var respostasRecentes = respostas
                    .OrderByDescending(r => r.DataResposta)
                    .Take(10)
                    .Select(r => new RespostaRecente
                    {
                        DataResposta = r.DataResposta,
                        Resposta = r.Texto ?? string.Empty,
                        Pergunta = perguntasById.TryGetValue(r.PerguntaId, out var pp)
                            ? pp.Texto
                            : "N/A",
                    })
                    .ToList();

                var metricasGerais = new Dictionary<string, object>
                {
                    ["TempoMedioResposta"] = await CalcularTempoMedioResposta(pesquisaId, ct),
                    ["TaxaConclusao"] = await CalcularTaxaConclusao(pesquisaId, ct),
                    ["PerguntaMaisRespondida"] = await ObterPerguntaMaisRespondida(pesquisaId, ct),
                    ["HorarioPico"] = await ObterHorarioPico(pesquisaId, ct),
                };

                var taxaResposta = totalRespostas; // mantido como no seu código original

                return new RelatorioPesquisaResponse
                {
                    PesquisaId = pesquisa.PesquisaId,
                    Titulo = pesquisa.Titulo,
                    Descricao = pesquisa.Descricao ?? string.Empty,
                    DataCriacao = pesquisa.DataCriacao,
                    TotalRespostas = totalRespostas,
                    TaxaResposta = taxaResposta,
                    EstatisticasPerguntas = estatisticasPerguntas,
                    RespostasRecentes = respostasRecentes,
                    MetricasGerais = metricasGerais,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao gerar relatório da pesquisa {PesquisaId}",
                    pesquisaId
                );
                throw;
            }
        }

        public async Task<List<RelatorioComparativoResponse>> GetRelatorioComparativoAsync(
            int loginId,
            List<int> pesquisaIds,
            CancellationToken ct = default
        )
        {
            try
            {
                pesquisaIds = pesquisaIds?.Distinct().ToList() ?? new List<int>();
                if (pesquisaIds.Count == 0)
                    return new List<RelatorioComparativoResponse>();

                var pesquisasInfo = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Where(p => pesquisaIds.Contains(p.PesquisaId) && p.LoginId == loginId)
                    .Select(p => new
                    {
                        p.PesquisaId,
                        p.Titulo,
                        p.DataCriacao,
                    })
                    .ToListAsync(ct);

                var perguntasAll = await _ctx
                    .Perguntas.AsNoTracking()
                    .Where(p => pesquisaIds.Contains(p.PesquisaId))
                    .Select(p => new PerguntaResumoLite
                    {
                        PerguntaId = p.PerguntaId,
                        PesquisaId = p.PesquisaId,
                        Texto = p.Texto,
                    })
                    .ToListAsync(ct);

                var respostasAll = await _ctx
                    .Respostas.AsNoTracking()
                    .Where(r => perguntasAll.Select(p => p.PerguntaId).Contains(r.PerguntaId))
                    .Select(r => new { r.PerguntaId })
                    .ToListAsync(ct);

                var perguntasByPesquisa = perguntasAll
                    .GroupBy(p => p.PesquisaId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var respostasCountByPergunta = respostasAll
                    .GroupBy(r => r.PerguntaId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var saida = new List<RelatorioComparativoResponse>();

                foreach (var pz in pesquisasInfo)
                {
                    if (
                        !perguntasByPesquisa.TryGetValue(pz.PesquisaId, out var perguntasDaPesquisa)
                    )
                        perguntasDaPesquisa = new List<PerguntaResumoLite>();

                    var totalRespostas = perguntasDaPesquisa.Sum(pp =>
                        respostasCountByPergunta.TryGetValue(pp.PerguntaId, out var c) ? c : 0
                    );

                    var comparacoes = perguntasDaPesquisa
                        .Select(pp => new ComparacaoPergunta
                        {
                            Pergunta = pp.Texto,
                            ResultadosPorPesquisa = new Dictionary<string, double>
                            {
                                [pz.Titulo] = respostasCountByPergunta.TryGetValue(
                                    pp.PerguntaId,
                                    out var c
                                )
                                    ? c
                                    : 0,
                            },
                        })
                        .ToList();

                    saida.Add(
                        new RelatorioComparativoResponse
                        {
                            PesquisaId = pz.PesquisaId,
                            Titulo = pz.Titulo,
                            TotalRespostas = totalRespostas,
                            TaxaResposta = totalRespostas, // mantendo sua semântica original
                            DataCriacao = pz.DataCriacao,
                            Comparacoes = comparacoes,
                        }
                    );
                }

                return saida;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao gerar relatório comparativo para loginId {LoginId}",
                    loginId
                );
                throw;
            }
        }

        public Task<bool> AgendarRelatorioAsync(
            AgendarRelatorioRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                _logger.LogInformation(
                    "Relatório agendado: {TipoRelatorio} para {EmailDestino}",
                    request.TipoRelatorio,
                    request.EmailDestino
                );
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao agendar relatório");
                return Task.FromResult(false);
            }
        }

        public Task<List<RelatorioAgendadoResponse>> GetRelatoriosAgendadosAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            try
            {
                var agora = DateTime.Now;
                var lista = new List<RelatorioAgendadoResponse>
                {
                    new()
                    {
                        RelatorioId = 1,
                        TipoRelatorio = "Dashboard Semanal",
                        Frequencia = "semanal",
                        Status = "Ativo",
                        ProximaExecucao = agora.AddDays(7),
                        DataCriacao = agora.AddDays(-7),
                    },
                    new()
                    {
                        RelatorioId = 2,
                        TipoRelatorio = "Relatório Mensal",
                        Frequencia = "mensal",
                        Status = "Ativo",
                        ProximaExecucao = agora.AddMonths(1),
                        DataCriacao = agora.AddDays(-30),
                    },
                };

                return Task.FromResult(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao obter relatórios agendados para loginId {LoginId}",
                    loginId
                );
                return Task.FromResult(new List<RelatorioAgendadoResponse>());
            }
        }

        public Task<bool> CancelarRelatorioAgendadoAsync(
            int relatorioId,
            CancellationToken ct = default
        )
        {
            try
            {
                _logger.LogInformation("Relatório agendado cancelado: {RelatorioId}", relatorioId);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao cancelar relatório agendado {RelatorioId}",
                    relatorioId
                );
                return Task.FromResult(false);
            }
        }

        // Métodos auxiliares privados
        private Task<double> CalcularTempoMedioResposta(int pesquisaId, CancellationToken ct) =>
            Task.FromResult(120.0);

        private Task<double> CalcularTaxaConclusao(int pesquisaId, CancellationToken ct) =>
            Task.FromResult(85.5);

        private async Task<string> ObterPerguntaMaisRespondida(int pesquisaId, CancellationToken ct)
        {
            var pergunta = await _ctx
                .Perguntas.AsNoTracking()
                .Where(p => p.PesquisaId == pesquisaId)
                .Select(p => new { p.PerguntaId, p.Texto })
                .OrderByDescending(p => _ctx.Respostas.Count(r => r.PerguntaId == p.PerguntaId))
                .FirstOrDefaultAsync(ct);

            return pergunta?.Texto ?? "N/A";
        }

        private Task<string> ObterHorarioPico(int pesquisaId, CancellationToken ct) =>
            Task.FromResult("14:00 - 16:00");
    }
}
