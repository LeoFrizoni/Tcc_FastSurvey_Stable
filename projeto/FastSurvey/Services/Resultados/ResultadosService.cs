#nullable enable
using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Cache;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Resultados
{
    public class ResultadosService : IResultadosService
    {
        private readonly FastSurveyContext _ctx;
        private readonly ICacheService _cache;

        private const string CACHE_PESQUISA_PREFIX = "resultados:pesquisa:";
        private const string CACHE_PERGUNTA_PREFIX = "resultados:pergunta:";
        private const string CACHE_GRAFICOS_PREFIX = "resultados:graficos:";
        private const string CACHE_DASHBOARD_PREFIX = "resultados:dashboard:";

        public ResultadosService(FastSurveyContext ctx, ICacheService cache)
        {
            _ctx = ctx;
            _cache = cache;
        }

        // ====================== RESULTADOS DA PESQUISA ======================
        public async Task<ServiceResult<object>> ObterResultadosPesquisaAsync(
            EstatisticasPesquisaRequest request,
            CancellationToken ct = default
        )
        {
            var cacheKey = $"{CACHE_PESQUISA_PREFIX}{request.PesquisaId}";

            try
            {
                var payload = await _cache.GetOrSetAsync<object>(
                    cacheKey,
                    async token =>
                    {
                        var pesquisa = await _ctx
                            .Pesquisas.AsNoTracking()
                            .Where(p => p.PesquisaId == request.PesquisaId)
                            .Select(p => new
                            {
                                p.PesquisaId,
                                p.Titulo,
                                p.Descricao,
                                p.DataCriacao,
                                Perguntas = p
                                    .Perguntas.OrderBy(pg => pg.Ordem)
                                    .Select(pg => new
                                    {
                                        pg.PerguntaId,
                                        pg.Texto,
                                        pg.TipoPerguntaId,
                                        TotalRespostas = pg.Respostas.Count,
                                        Opcoes = pg
                                            .OpcoesPergunta.OrderBy(o => o.Ordem)
                                            .Select(o => new
                                            {
                                                o.OpcaoId,
                                                o.Texto,
                                                o.Ordem,
                                            })
                                            .ToList(),
                                        RespostasDiscursivas = pg
                                            .Respostas.Where(r => !string.IsNullOrEmpty(r.Texto))
                                            .Select(r => new { r.Texto, r.DataResposta })
                                            .ToList(),
                                    })
                                    .ToList(),
                            })
                            .FirstOrDefaultAsync(token);

                        if (pesquisa is null)
                            throw new InvalidOperationException(
                                "NOT_FOUND:Pesquisa não encontrada"
                            );

                        var totalRespostas = pesquisa.Perguntas.Sum(p => p.TotalRespostas);

                        var estatisticasTask = CalcularEstatisticasAsync(request.PesquisaId, token);
                        var opcoesContagensTasks = Task.WhenAll(
                            pesquisa.Perguntas.Select(p =>
                                CalcularContagemOpcoesAsync(p.PerguntaId, token)
                            )
                        );

                        await Task.WhenAll(estatisticasTask, opcoesContagensTasks);

                        var countsByPerguntaId = pesquisa
                            .Perguntas.Select(
                                (p, i) =>
                                    new { p.PerguntaId, counts = opcoesContagensTasks.Result[i] }
                            )
                            .ToDictionary(x => x.PerguntaId, x => x.counts);

                        var resultado = new
                        {
                            pesquisa = new
                            {
                                id = pesquisa.PesquisaId,
                                titulo = pesquisa.Titulo,
                                descricao = pesquisa.Descricao,
                                dataCriacao = pesquisa.DataCriacao,
                                totalRespostas,
                            },
                            perguntas = pesquisa
                                .Perguntas.Select(pergunta =>
                                {
                                    var cont = countsByPerguntaId[pergunta.PerguntaId];
                                    return new
                                    {
                                        id = pergunta.PerguntaId,
                                        texto = pergunta.Texto,
                                        tipo = pergunta.TipoPerguntaId,
                                        totalRespostas = pergunta.TotalRespostas,
                                        opcoes = pergunta
                                            .Opcoes.Select(opcao =>
                                            {
                                                var totalVotos = cont.GetValueOrDefault(
                                                    opcao.OpcaoId,
                                                    0
                                                );
                                                return new
                                                {
                                                    id = opcao.OpcaoId,
                                                    texto = opcao.Texto,
                                                    totalRespostas = totalVotos,
                                                    percentual = pergunta.TotalRespostas > 0
                                                        ? (double)totalVotos
                                                            * 100.0
                                                            / pergunta.TotalRespostas
                                                        : 0.0,
                                                };
                                            })
                                            .ToList(),
                                        respostasDiscursivas = pergunta.RespostasDiscursivas,
                                    };
                                })
                                .ToList(),
                            estatisticas = await estatisticasTask,
                        };

                        return (object)resultado;
                    },
                    absoluteExpiration: TimeSpan.FromMinutes(30),
                    ct: ct
                );

                return ServiceResult<object>.Ok(payload);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("NOT_FOUND"))
            {
                return ServiceResult<object>.Fail("NOT_FOUND", ex.Message.Split(':', 2).Last());
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail(
                    "ERROR",
                    $"Erro ao obter resultados: {ex.Message}"
                );
            }
        }

        // ====================== ESTATÍSTICAS DA PERGUNTA ======================
        public async Task<ServiceResult<object>> ObterEstatisticasPerguntaAsync(
            EstatisticasPerguntaRequest request,
            CancellationToken ct = default
        )
        {
            var cacheKey = $"{CACHE_PERGUNTA_PREFIX}{request.PerguntaId}";

            try
            {
                var payload = await _cache.GetOrSetAsync<object>(
                    cacheKey,
                    async token =>
                    {
                        var pergunta = await _ctx
                            .Perguntas.AsNoTracking()
                            .Where(p => p.PerguntaId == request.PerguntaId)
                            .Select(p => new
                            {
                                p.PerguntaId,
                                p.Texto,
                                p.TipoPerguntaId,
                                p.Ordem,
                                TotalRespostas = p.Respostas.Count,
                                Opcoes = p
                                    .OpcoesPergunta.OrderBy(o => o.Ordem)
                                    .Select(o => new
                                    {
                                        o.OpcaoId,
                                        o.Texto,
                                        o.Ordem,
                                    })
                                    .ToList(),
                                RespostasDiscursivas = p
                                    .Respostas.Where(r => !string.IsNullOrEmpty(r.Texto))
                                    .Select(r => new { r.Texto, r.DataResposta })
                                    .ToList(),
                            })
                            .FirstOrDefaultAsync(token);

                        if (pergunta is null)
                            throw new InvalidOperationException(
                                "NOT_FOUND:Pergunta não encontrada"
                            );

                        var opcoesComContagem = await CalcularContagemOpcoesAsync(
                            request.PerguntaId,
                            token
                        );

                        var resultado = new
                        {
                            pergunta = new
                            {
                                id = pergunta.PerguntaId,
                                texto = pergunta.Texto,
                                tipo = pergunta.TipoPerguntaId,
                                ordem = pergunta.Ordem,
                                totalRespostas = pergunta.TotalRespostas,
                            },
                            opcoes = pergunta
                                .Opcoes.Select(opcao => new
                                {
                                    id = opcao.OpcaoId,
                                    texto = opcao.Texto,
                                    ordem = opcao.Ordem,
                                    totalRespostas = opcoesComContagem.GetValueOrDefault(
                                        opcao.OpcaoId,
                                        0
                                    ),
                                    percentual = pergunta.TotalRespostas > 0
                                        ? (double)
                                            opcoesComContagem.GetValueOrDefault(opcao.OpcaoId, 0)
                                            * 100.0
                                            / pergunta.TotalRespostas
                                        : 0.0,
                                })
                                .ToList(),
                            respostasDiscursivas = pergunta.RespostasDiscursivas,
                            estatisticas = new
                            {
                                totalRespostas = pergunta.TotalRespostas,
                                totalOpcoes = pergunta.Opcoes.Count,
                                opcaoMaisEscolhida = pergunta
                                    .Opcoes.OrderByDescending(o =>
                                        opcoesComContagem.GetValueOrDefault(o.OpcaoId, 0)
                                    )
                                    .Select(o => new
                                    {
                                        o.Texto,
                                        total = opcoesComContagem.GetValueOrDefault(o.OpcaoId, 0),
                                    })
                                    .FirstOrDefault(),
                            },
                        };

                        return (object)resultado;
                    },
                    absoluteExpiration: TimeSpan.FromMinutes(15),
                    ct: ct
                );

                return ServiceResult<object>.Ok(payload);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("NOT_FOUND"))
            {
                return ServiceResult<object>.Fail("NOT_FOUND", ex.Message.Split(':', 2).Last());
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail(
                    "ERROR",
                    $"Erro ao obter estatísticas: {ex.Message}"
                );
            }
        }

        // ====================== EXPORTAR RESULTADOS ======================
        public async Task<ServiceResult<ExportResult>> ExportarResultadosAsync(
            EstatisticasPesquisaRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Where(p => p.PesquisaId == request.PesquisaId)
                    .Select(p => new { p.Titulo, p.Descricao })
                    .FirstOrDefaultAsync(ct);

                if (pesquisa is null)
                    return ServiceResult<ExportResult>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                var exportResult = new ExportResult
                {
                    Data = System.Text.Encoding.UTF8.GetBytes(
                        $"Exportação da pesquisa: {pesquisa.Titulo}"
                    ),
                    ContentType = "application/pdf",
                    FileName =
                        $"resultados_pesquisa_{request.PesquisaId}_{DateTime.UtcNow:yyyyMMdd}.pdf",
                };

                return ServiceResult<ExportResult>.Ok(exportResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportResult>.Fail(
                    "ERROR",
                    $"Erro ao exportar resultados: {ex.Message}"
                );
            }
        }

        // ====================== GRÁFICOS ======================
        public async Task<ServiceResult<object>> ObterGraficosAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            var cacheKey = $"{CACHE_GRAFICOS_PREFIX}{pesquisaId}";

            try
            {
                var payload = await _cache.GetOrSetAsync<object>(
                    cacheKey,
                    async token =>
                    {
                        var dadosPerguntas = await _ctx
                            .Perguntas.AsNoTracking()
                            .Where(p => p.PesquisaId == pesquisaId)
                            .Select(p => new
                            {
                                p.PerguntaId,
                                p.Texto,
                                p.TipoPerguntaId,
                                TotalRespostas = p.Respostas.Count,
                                Opcoes = p
                                    .OpcoesPergunta.Select(o => new
                                    {
                                        o.OpcaoId,
                                        o.Texto,
                                        o.Ordem,
                                    })
                                    .ToList(),
                            })
                            .ToListAsync(token);

                        var tasks = dadosPerguntas.Select(async pergunta =>
                        {
                            var contagemOpcoes = await CalcularContagemOpcoesAsync(
                                pergunta.PerguntaId,
                                token
                            );
                            return new
                            {
                                perguntaId = pergunta.PerguntaId,
                                texto = pergunta.Texto,
                                tipo = pergunta.TipoPerguntaId,
                                totalRespostas = pergunta.TotalRespostas,
                                dados = pergunta
                                    .Opcoes.OrderBy(o => o.Ordem)
                                    .Select(o => new
                                    {
                                        label = o.Texto,
                                        value = contagemOpcoes.GetValueOrDefault(o.OpcaoId, 0),
                                        percentual = pergunta.TotalRespostas > 0
                                            ? (double)contagemOpcoes.GetValueOrDefault(o.OpcaoId, 0)
                                                * 100.0
                                                / pergunta.TotalRespostas
                                            : 0.0,
                                    })
                                    .ToList(),
                            };
                        });

                        var resultados = await Task.WhenAll(tasks);
                        return (object)new { graficos = resultados };
                    },
                    absoluteExpiration: TimeSpan.FromMinutes(20),
                    ct: ct
                );

                return ServiceResult<object>.Ok(payload);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail("ERROR", $"Erro ao obter gráficos: {ex.Message}");
            }
        }

        // ====================== DASHBOARD ======================
        public async Task<ServiceResult<DashboardData>> ObterDashboardAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            var cacheKey = $"{CACHE_DASHBOARD_PREFIX}{loginId}";

            try
            {
                var payload = await _cache.GetOrSetAsync<DashboardData>(
                    cacheKey,
                    async token =>
                    {
                        var totalPesquisasTask = _ctx
                            .Pesquisas.AsNoTracking()
                            .Where(p => p.LoginId == loginId)
                            .CountAsync(token);

                        var totalRespostasTask = _ctx
                            .Respostas.AsNoTracking()
                            .Where(r => r.Pergunta.Pesquisa.LoginId == loginId)
                            .CountAsync(token);

                        var pesquisasAtivasTask = _ctx
                            .Pesquisas.AsNoTracking()
                            .Where(p => p.LoginId == loginId && p.Ativa)
                            .CountAsync(token);

                        var pesquisasInterativasTask = _ctx
                            .Pesquisas.AsNoTracking()
                            .Where(p => p.LoginId == loginId && p.IsInterativa)
                            .CountAsync(token);

                        var pesquisasPopularesTask = _ctx
                            .Pesquisas.AsNoTracking()
                            .Where(p => p.LoginId == loginId)
                            .Select(p => new
                            {
                                p.PesquisaId,
                                p.Titulo,
                                p.DataCriacao,
                                TotalRespostas = p.Perguntas.SelectMany(pg => pg.Respostas).Count(),
                            })
                            .OrderByDescending(p => p.TotalRespostas)
                            .Take(5)
                            .ToListAsync(token);

                        await Task.WhenAll(
                            totalPesquisasTask,
                            totalRespostasTask,
                            pesquisasAtivasTask,
                            pesquisasInterativasTask,
                            pesquisasPopularesTask
                        );

                        return new DashboardData
                        {
                            TotalPesquisas = await totalPesquisasTask,
                            TotalRespostas = await totalRespostasTask,
                            PesquisasAtivas = await pesquisasAtivasTask,
                            PesquisasInterativas = await pesquisasInterativasTask,
                            PesquisasPopulares = (await pesquisasPopularesTask)
                                .Select(p => new PesquisaPopular
                                {
                                    PesquisaId = p.PesquisaId,
                                    Titulo = p.Titulo,
                                    DataCriacao = p.DataCriacao,
                                    TotalRespostas = p.TotalRespostas,
                                })
                                .ToList(),
                            EstatisticasMensais = await CalcularEstatisticasMensaisAsync(
                                loginId,
                                token
                            ),
                        };
                    },
                    absoluteExpiration: TimeSpan.FromMinutes(10),
                    ct: ct
                );

                return ServiceResult<DashboardData>.Ok(payload);
            }
            catch (Exception ex)
            {
                return ServiceResult<DashboardData>.Fail(
                    "ERROR",
                    $"Erro ao obter dashboard: {ex.Message}"
                );
            }
        }

        // ====================== RELATÓRIO COMPLETO ======================
        public async Task<ServiceResult<object>> ObterRelatorioCompletoAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta)
                    .Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.Respostas)
                    .FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);

                if (pesquisa is null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                var perguntasComEstatisticas = await Task.WhenAll(
                    pesquisa
                        .Perguntas.OrderBy(p => p.Ordem)
                        .Select(async p =>
                        {
                            var contagemOpcoes = await CalcularContagemOpcoesAsync(
                                p.PerguntaId,
                                ct
                            );
                            var totalRespostas = p.Respostas.Count;

                            return new
                            {
                                perguntaId = p.PerguntaId,
                                texto = p.Texto,
                                tipo = p.TipoPerguntaId,
                                ordem = p.Ordem,
                                totalRespostas,
                                opcoes = p
                                    .OpcoesPergunta.OrderBy(o => o.Ordem)
                                    .Select(o => new
                                    {
                                        opcaoId = o.OpcaoId,
                                        texto = o.Texto,
                                        ordem = o.Ordem,
                                        totalVotos = contagemOpcoes.GetValueOrDefault(o.OpcaoId, 0),
                                        percentual = totalRespostas > 0
                                            ? (double)contagemOpcoes.GetValueOrDefault(o.OpcaoId, 0)
                                                * 100.0
                                                / totalRespostas
                                            : 0.0,
                                    })
                                    .ToList(),
                            };
                        })
                );

                var resultadoFinal = new
                {
                    pesquisaId = pesquisa.PesquisaId,
                    titulo = pesquisa.Titulo,
                    descricao = pesquisa.Descricao,
                    dataCriacao = pesquisa.DataCriacao,
                    totalRespostas = pesquisa.Perguntas.SelectMany(p => p.Respostas).Count(),
                    perguntas = perguntasComEstatisticas,
                };

                return ServiceResult<object>.Ok(resultadoFinal);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail(
                    "ERROR",
                    $"Erro ao obter relatório completo: {ex.Message}"
                );
            }
        }

        // ====================== PRIVADOS ======================

        private async Task<Dictionary<int, int>> CalcularContagemOpcoesAsync(
            int perguntaId,
            CancellationToken ct
        )
        {
            // Se houver tabela de junção RespostasOpcoes, adapte a projeção.
            var contagem = await _ctx
                .Respostas.AsNoTracking()
                .Where(r => r.PerguntaId == perguntaId)
                .SelectMany(r => r.Opcao ?? new List<OpcoesPergunta>())
                .GroupBy(o => o.OpcaoId)
                .Select(g => new { OpcaoId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return contagem.ToDictionary(x => x.OpcaoId, x => x.Count);
        }

        private async Task<object> CalcularEstatisticasAsync(int pesquisaId, CancellationToken ct)
        {
            var estatisticas = await _ctx
                .Perguntas.AsNoTracking()
                .Where(p => p.PesquisaId == pesquisaId)
                .Select(p => new { p.PerguntaId, TotalRespostas = p.Respostas.Count })
                .ToListAsync(ct);

            return new
            {
                totalPerguntas = estatisticas.Count,
                totalRespostas = estatisticas.Sum(e => e.TotalRespostas),
                mediaRespostasPorPergunta = estatisticas.Count > 0
                    ? estatisticas.Average(e => e.TotalRespostas)
                    : 0.0,
                perguntaMaisRespondida = estatisticas
                    .OrderByDescending(e => e.TotalRespostas)
                    .FirstOrDefault(),
            };
        }

        private async Task<List<EstatisticaMensal>> CalcularEstatisticasMensaisAsync(
            int loginId,
            CancellationToken ct
        )
        {
            var estatisticas = await _ctx
                .Pesquisas.AsNoTracking()
                .Where(p => p.LoginId == loginId)
                .GroupBy(p => new { p.DataCriacao.Year, p.DataCriacao.Month })
                .Select(g => new EstatisticaMensal
                {
                    Mes = $"{g.Key.Year}-{g.Key.Month:D2}",
                    PesquisasCriadas = g.Count(),
                    RespostasRecebidas = g.SelectMany(p => p.Perguntas)
                        .SelectMany(pg => pg.Respostas)
                        .Count(),
                })
                .OrderByDescending(e => e.Mes)
                .Take(12)
                .ToListAsync(ct);

            return estatisticas;
        }
    }
}
