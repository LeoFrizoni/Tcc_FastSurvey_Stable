using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Text.Json;

namespace FASTSURVEY.Services.Resultados
{
    public class ResultadosService : IResultadosService
    {
        private readonly FastSurveyContext _ctx;

        public ResultadosService(FastSurveyContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<ServiceResult<object>> ObterResultadosPesquisaAsync(EstatisticasPesquisaRequest request, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .Include(p => p.Perguntas)
                        .ThenInclude(pg => pg.Opcoespergunta)
                    .Include(p => p.Perguntas)
                        .ThenInclude(pg => pg.Respostas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Pesquisaid == request.PesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                var resultado = new
                {
                    pesquisa = new
                    {
                        id = pesquisa.Pesquisaid,
                        titulo = pesquisa.Titulo,
                        descricao = pesquisa.Descricao,
                        dataCriacao = pesquisa.Datacriacao,
                        totalRespostas = pesquisa.Perguntas.SelectMany(p => p.Respostas).Count()
                    },
                    perguntas = pesquisa.Perguntas.OrderBy(p => p.Ordem).Select(pergunta => new
                    {
                        id = pergunta.Perguntaid,
                        texto = pergunta.Texto,
                        tipo = pergunta.Tipoperguntaid,
                        totalRespostas = pergunta.Respostas.Count,
                        opcoes = pergunta.Opcoespergunta.OrderBy(o => o.Ordem).Select(opcao => new
                        {
                            id = opcao.Opcaoid,
                            texto = opcao.Texto,
                            totalRespostas = _ctx.Set<Respostas>()
                                .Where(r => r.Perguntaid == pergunta.Perguntaid)
                                .SelectMany(r => r.Opcao)
                                .Count(o => o.Opcaoid == opcao.Opcaoid),
                            percentual = CalcularPercentual(pergunta.Perguntaid, opcao.Opcaoid)
                        }).ToList(),
                        respostasDiscursivas = pergunta.Respostas
                            .Where(r => !string.IsNullOrEmpty(r.Texto))
                            .Select(r => new
                            {
                                texto = r.Texto,
                                dataResposta = r.Dataresposta
                            }).ToList()
                    }).ToList(),
                    estatisticas = new
                    {
                        totalPerguntas = pesquisa.Perguntas.Count,
                        totalRespostas = pesquisa.Perguntas.SelectMany(p => p.Respostas).Count(),
                        mediaRespostasPorPergunta = pesquisa.Perguntas.Count > 0 
                            ? pesquisa.Perguntas.Average(p => p.Respostas.Count) 
                            : 0,
                        perguntaMaisRespondida = pesquisa.Perguntas
                            .OrderByDescending(p => p.Respostas.Count)
                            .Select(p => new { p.Texto, total = p.Respostas.Count })
                            .FirstOrDefault()
                    }
                };

                return ServiceResult<object>.Ok(resultado);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail("ERROR", $"Erro ao obter resultados: {ex.Message}");
            }
        }

        public async Task<ServiceResult<object>> ObterEstatisticasPerguntaAsync(EstatisticasPerguntaRequest request, CancellationToken ct = default)
        {
            try
            {
                var pergunta = await _ctx.Perguntas
                    .Include(p => p.Opcoespergunta)
                    .Include(p => p.Respostas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Perguntaid == request.PerguntaId, ct);

                if (pergunta == null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Pergunta não encontrada");

                var totalRespostas = pergunta.Respostas.Count;
                
                var estatisticas = new
                {
                    pergunta = new
                    {
                        id = pergunta.Perguntaid,
                        texto = pergunta.Texto,
                        tipo = pergunta.Tipoperguntaid,
                        totalRespostas
                    },
                    opcoes = pergunta.Opcoespergunta.OrderBy(o => o.Ordem).Select(opcao => 
                    {
                        var respostasOpcao = _ctx.Set<Respostas>()
                            .Where(r => r.Perguntaid == pergunta.Perguntaid)
                            .SelectMany(r => r.Opcao)
                            .Count(o => o.Opcaoid == opcao.Opcaoid);
                        
                        return new
                        {
                            id = opcao.Opcaoid,
                            texto = opcao.Texto,
                            totalRespostas = respostasOpcao,
                            percentual = totalRespostas > 0 ? (respostasOpcao * 100.0 / totalRespostas) : 0,
                            correta = opcao.Correta
                        };
                    }).ToList(),
                    grafico = request.IncluirGraficos ? GerarDadosGrafico(pergunta, request.TipoGrafico) : null
                };

                return ServiceResult<object>.Ok(estatisticas);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail("ERROR", $"Erro ao obter estatísticas: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ExportResult>> ExportarResultadosAsync(EstatisticasPesquisaRequest request, CancellationToken ct = default)
        {
            try
            {
                var resultados = await ObterResultadosPesquisaAsync(request, ct);
                if (!resultados.Success)
                    return ServiceResult<ExportResult>.Fail("ERROR", "Erro ao obter dados para exportação");

                byte[] dados;
                string contentType;
                string fileName;

                switch (request.Formato.ToLower())
                {
                    case "pdf":
                        dados = await GerarPDFAsync(resultados.Data);
                        contentType = "application/pdf";
                        fileName = $"resultados_pesquisa_{request.PesquisaId}_{DateTime.Now:yyyyMMdd}.pdf";
                        break;
                    
                    case "excel":
                        dados = await GerarExcelAsync(resultados.Data);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileName = $"resultados_pesquisa_{request.PesquisaId}_{DateTime.Now:yyyyMMdd}.xlsx";
                        break;
                    
                    default:
                        var json = JsonSerializer.Serialize(resultados.Data, new JsonSerializerOptions { WriteIndented = true });
                        dados = System.Text.Encoding.UTF8.GetBytes(json);
                        contentType = "application/json";
                        fileName = $"resultados_pesquisa_{request.PesquisaId}_{DateTime.Now:yyyyMMdd}.json";
                        break;
                }

                return ServiceResult<ExportResult>.Ok(new ExportResult
                {
                    Data = dados,
                    ContentType = contentType,
                    FileName = fileName
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportResult>.Fail("ERROR", $"Erro ao exportar resultados: {ex.Message}");
            }
        }

        public async Task<ServiceResult<object>> ObterGraficosAsync(int pesquisaId, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .Include(p => p.Perguntas)
                        .ThenInclude(pg => pg.Opcoespergunta)
                    .Include(p => p.Perguntas)
                        .ThenInclude(pg => pg.Respostas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Pesquisaid == pesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                var graficos = pesquisa.Perguntas
                    .Where(p => p.Opcoespergunta.Any()) // Apenas perguntas com opções
                    .OrderBy(p => p.Ordem)
                    .Select(pergunta => new
                    {
                        perguntaId = pergunta.Perguntaid,
                        titulo = pergunta.Texto,
                        tipo = pergunta.Tipoperguntaid == 2 ? "pie" : "bar", // Objetiva = pie, Multipla = bar
                        dados = pergunta.Opcoespergunta.OrderBy(o => o.Ordem).Select(opcao =>
                        {
                            var respostasOpcao = _ctx.Set<Respostas>()
                                .Where(r => r.Perguntaid == pergunta.Perguntaid)
                                .SelectMany(r => r.Opcao)
                                .Count(o => o.Opcaoid == opcao.Opcaoid);
                            
                            return new
                            {
                                label = opcao.Texto,
                                value = respostasOpcao,
                                color = GerarCorGrafico(opcao.Opcaoid)
                            };
                        }).ToList()
                    }).ToList();

                return ServiceResult<object>.Ok(new { graficos });
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail("ERROR", $"Erro ao obter gráficos: {ex.Message}");
            }
        }

        public async Task<ServiceResult<DashboardData>> ObterDashboardAsync(int loginId, CancellationToken ct = default)
        {
            try
            {
                var totalPesquisas = await _ctx.Pesquisas.CountAsync(p => p.Loginid == loginId, ct);
                var totalRespostas = await _ctx.Pesquisas
                    .Where(p => p.Loginid == loginId)
                    .SelectMany(p => p.Perguntas)
                    .SelectMany(pg => pg.Respostas)
                    .CountAsync(ct);

                var pesquisasAtivas = await _ctx.Pesquisas
                    .CountAsync(p => p.Loginid == loginId && 
                        (p.Datafechamento == null || p.Datafechamento > DateTime.UtcNow), ct);

                var pesquisasInterativas = await _ctx.Pesquisas
                    .CountAsync(p => p.Loginid == loginId && p.Isinterativa == true, ct);

                var pesquisasPopulares = await _ctx.Pesquisas
                    .Where(p => p.Loginid == loginId)
                    .Select(p => new PesquisaPopular
                    {
                        PesquisaId = p.Pesquisaid,
                        Titulo = p.Titulo,
                        TotalRespostas = p.Perguntas.SelectMany(pg => pg.Respostas).Count(),
                        DataCriacao = p.Datacriacao
                    })
                    .OrderByDescending(p => p.TotalRespostas)
                    .Take(5)
                    .ToListAsync(ct);

                var dashboard = new DashboardData
                {
                    TotalPesquisas = totalPesquisas,
                    TotalRespostas = totalRespostas,
                    PesquisasAtivas = pesquisasAtivas,
                    PesquisasInterativas = pesquisasInterativas,
                    PesquisasPopulares = pesquisasPopulares
                };

                return ServiceResult<DashboardData>.Ok(dashboard);
            }
            catch (Exception ex)
            {
                return ServiceResult<DashboardData>.Fail("ERROR", $"Erro ao obter dashboard: {ex.Message}");
            }
        }

        // Métodos auxiliares
        private double CalcularPercentual(int perguntaId, int opcaoId)
        {
            var totalRespostas = _ctx.Respostas.Count(r => r.Perguntaid == perguntaId);
            if (totalRespostas == 0) return 0;

            var respostasOpcao = _ctx.Set<Respostas>()
                .Where(r => r.Perguntaid == perguntaId)
                .SelectMany(r => r.Opcao)
                .Count(o => o.Opcaoid == opcaoId);

            return respostasOpcao * 100.0 / totalRespostas;
        }

        private object GerarDadosGrafico(Perguntas pergunta, string? tipoGrafico)
        {
            var tipo = tipoGrafico ?? (pergunta.Tipoperguntaid == 2 ? "pie" : "bar"); // Objetiva = pie, Multipla = bar
            
            return new
            {
                type = tipo,
                labels = pergunta.Opcoespergunta.OrderBy(o => o.Ordem).Select(o => o.Texto).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        data = pergunta.Opcoespergunta.OrderBy(o => o.Ordem).Select(opcao =>
                            _ctx.Set<Respostas>()
                                .Where(r => r.Perguntaid == pergunta.Perguntaid)
                                .SelectMany(r => r.Opcao)
                                .Count(o => o.Opcaoid == opcao.Opcaoid)
                        ).ToArray(),
                        backgroundColor = pergunta.Opcoespergunta.OrderBy(o => o.Ordem)
                            .Select(o => GerarCorGrafico(o.Opcaoid)).ToArray()
                    }
                }
            };
        }

        private string GerarCorGrafico(int opcaoId)
        {
            var cores = new[]
            {
                "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
                "#FF9F40", "#FF6384", "#C9CBCF", "#4BC0C0", "#FF6384"
            };
            return cores[opcaoId % cores.Length];
        }

        private async Task<byte[]> GerarPDFAsync(object dados)
        {
            // TODO: Implementar geração de PDF usando uma biblioteca como iText ou similar
            // Por enquanto, retorna JSON como bytes
            var json = JsonSerializer.Serialize(dados, new JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        private async Task<byte[]> GerarExcelAsync(object dados)
        {
            // TODO: Implementar geração de Excel usando uma biblioteca como EPPlus
            // Por enquanto, retorna JSON como bytes
            var json = JsonSerializer.Serialize(dados, new JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
}
