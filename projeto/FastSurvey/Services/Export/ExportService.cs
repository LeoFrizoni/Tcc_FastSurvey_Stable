using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Text;
using System.Text.Json;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;

namespace FASTSURVEY.Services.Export
{
    public class ExportService : IExportService
    {
        private readonly FastSurveyContext _ctx;

        public ExportService(FastSurveyContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<ServiceResult<ExportFileResult>> ExportarPesquisaPDFAsync(int pesquisaId, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .Include(p => p.Perguntas.OrderBy(pg => pg.Ordem))
                        .ThenInclude(pg => pg.Opcoespergunta.OrderBy(o => o.Ordem))
                    .Include(p => p.Login)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Pesquisaid == pesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<ExportFileResult>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Título
                document.Add(new Paragraph(pesquisa.Titulo)
                    .SetFontSize(20)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // Informações da pesquisa
                document.Add(new Paragraph($"Descrição: {pesquisa.Descricao}")
                    .SetFontSize(12)
                    .SetMarginBottom(10));

                document.Add(new Paragraph($"Autor: {pesquisa.Login?.Usuario ?? "N/A"}")
                    .SetFontSize(12)
                    .SetMarginBottom(10));

                document.Add(new Paragraph($"Data de Criação: {pesquisa.Datacriacao:dd/MM/yyyy HH:mm}")
                    .SetFontSize(12)
                    .SetMarginBottom(20));

                // Perguntas
                document.Add(new Paragraph("PERGUNTAS")
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginBottom(15));

                foreach (var pergunta in pesquisa.Perguntas)
                {
                    // Título da pergunta
                    document.Add(new Paragraph($"{pergunta.Ordem}. {pergunta.Texto}")
                        .SetFontSize(14)
                        .SetBold()
                        .SetMarginBottom(10));

                    // Tipo da pergunta
                    string tipoPergunta = pergunta.Tipoperguntaid switch
                    {
                        1 => "Discursiva",
                        2 => "Objetiva",
                        3 => "Multipla Escolha", // Baseado no nome real da tabela
                        _ => "Desconhecido"
                    };

                    document.Add(new Paragraph($"Tipo: {tipoPergunta}")
                        .SetFontSize(10)
                        .SetItalic()
                        .SetMarginBottom(5));

                    // Opções (se houver)
                    if (pergunta.Opcoespergunta.Any())
                    {
                        document.Add(new Paragraph("Opções:")
                            .SetFontSize(12)
                            .SetBold()
                            .SetMarginBottom(5));

                        foreach (var opcao in pergunta.Opcoespergunta)
                        {
                            var marcador = pergunta.Tipoperguntaid == 2 ? "○" : "☐";
                            if (opcao.Correta && pergunta.Temgabarito)
                                marcador = pergunta.Tipoperguntaid == 2 ? "●" : "☑";

                            document.Add(new Paragraph($"   {marcador} {opcao.Texto}")
                                .SetFontSize(11)
                                .SetMarginBottom(3));
                        }
                    }

                    document.Add(new Paragraph("")
                        .SetMarginBottom(15));
                }

                document.Close();

                var fileName = $"pesquisa_{pesquisaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var result = new ExportFileResult
                {
                    FileData = stream.ToArray(),
                    ContentType = "application/pdf",
                    FileName = fileName,
                    FileExtension = ".pdf",
                    FileSize = stream.Length
                };

                return ServiceResult<ExportFileResult>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportFileResult>.Fail("ERROR", $"Erro ao exportar PDF: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ExportFileResult>> ExportarResultadosPDFAsync(int pesquisaId, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .Include(p => p.Perguntas.OrderBy(pg => pg.Ordem))
                        .ThenInclude(pg => pg.Opcoespergunta.OrderBy(o => o.Ordem))
                    .Include(p => p.Perguntas)
                        .ThenInclude(pg => pg.Respostas)
                    .Include(p => p.Login)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Pesquisaid == pesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<ExportFileResult>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Cabeçalho
                document.Add(new Paragraph("RESULTADOS DA PESQUISA")
                    .SetFontSize(20)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10));

                document.Add(new Paragraph(pesquisa.Titulo)
                    .SetFontSize(16)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // Estatísticas gerais
                var totalRespostas = pesquisa.Perguntas.SelectMany(p => p.Respostas).Count();
                document.Add(new Paragraph("ESTATÍSTICAS GERAIS")
                    .SetFontSize(14)
                    .SetBold()
                    .SetMarginBottom(10));

                document.Add(new Paragraph($"Total de Respostas: {totalRespostas}")
                    .SetFontSize(12)
                    .SetMarginBottom(5));

                document.Add(new Paragraph($"Total de Perguntas: {pesquisa.Perguntas.Count}")
                    .SetFontSize(12)
                    .SetMarginBottom(15));

                // Resultados por pergunta
                document.Add(new Paragraph("RESULTADOS POR PERGUNTA")
                    .SetFontSize(14)
                    .SetBold()
                    .SetMarginBottom(15));

                foreach (var pergunta in pesquisa.Perguntas)
                {
                    document.Add(new Paragraph($"{pergunta.Ordem}. {pergunta.Texto}")
                        .SetFontSize(13)
                        .SetBold()
                        .SetMarginBottom(10));

                    var totalRespostasPergunta = pergunta.Respostas.Count;
                    document.Add(new Paragraph($"Total de respostas: {totalRespostasPergunta}")
                        .SetFontSize(11)
                        .SetMarginBottom(10));

                    // Resultados das opções
                    if (pergunta.Opcoespergunta.Any())
                    {
                        foreach (var opcao in pergunta.Opcoespergunta)
                        {
                            var respostasOpcao = _ctx.Set<Respostas>()
                                .Where(r => r.Perguntaid == pergunta.Perguntaid)
                                .SelectMany(r => r.Opcao)
                                .Count(o => o.Opcaoid == opcao.Opcaoid);

                            var percentual = totalRespostasPergunta > 0 
                                ? (respostasOpcao * 100.0 / totalRespostasPergunta) 
                                : 0;

                            document.Add(new Paragraph($"   • {opcao.Texto}: {respostasOpcao} ({percentual:F1}%)")
                                .SetFontSize(11)
                                .SetMarginBottom(3));
                        }
                    }

                    // Respostas discursivas
                    var respostasDiscursivas = pergunta.Respostas
                        .Where(r => !string.IsNullOrEmpty(r.Texto))
                        .Take(10) // Limitar para não ficar muito grande
                        .ToList();

                    if (respostasDiscursivas.Any())
                    {
                        document.Add(new Paragraph("Respostas discursivas (amostra):")
                            .SetFontSize(11)
                            .SetBold()
                            .SetMarginBottom(5));

                        foreach (var resposta in respostasDiscursivas)
                        {
                            document.Add(new Paragraph($"   \"{resposta.Texto}\"")
                                .SetFontSize(10)
                                .SetItalic()
                                .SetMarginBottom(3));
                        }
                    }

                    document.Add(new Paragraph("")
                        .SetMarginBottom(15));
                }

                document.Close();

                var fileName = $"resultados_pesquisa_{pesquisaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var result = new ExportFileResult
                {
                    FileData = stream.ToArray(),
                    ContentType = "application/pdf",
                    FileName = fileName,
                    FileExtension = ".pdf",
                    FileSize = stream.Length
                };

                return ServiceResult<ExportFileResult>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportFileResult>.Fail("ERROR", $"Erro ao exportar resultados PDF: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ExportFileResult>> ExportarResultadosExcelAsync(int pesquisaId, CancellationToken ct = default)
        {
            try
            {
                // TODO: Implementar com EPPlus ou similar
                // Por enquanto, exporta como JSON estruturado
                var dados = await ObterDadosExportacao(pesquisaId, ct);
                if (!dados.Success) return ServiceResult<ExportFileResult>.Fail(dados.Errors[0].Code, dados.Errors[0].Message);

                var json = JsonSerializer.Serialize(dados.Data, new JsonSerializerOptions { WriteIndented = true });
                var bytes = Encoding.UTF8.GetBytes(json);

                var fileName = $"resultados_pesquisa_{pesquisaId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var result = new ExportFileResult
                {
                    FileData = bytes,
                    ContentType = "application/json",
                    FileName = fileName,
                    FileExtension = ".json",
                    FileSize = bytes.Length
                };

                return ServiceResult<ExportFileResult>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportFileResult>.Fail("ERROR", $"Erro ao exportar Excel: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ExportFileResult>> ExportarRespostasCSVAsync(int pesquisaId, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .Include(p => p.Perguntas.OrderBy(pg => pg.Ordem))
                        .ThenInclude(pg => pg.Respostas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Pesquisaid == pesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<ExportFileResult>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                var csv = new StringBuilder();
                
                // Cabeçalho
                csv.AppendLine("Data Resposta,Pergunta,Resposta,Tipo Resposta");

                foreach (var pergunta in pesquisa.Perguntas)
                {
                    foreach (var resposta in pergunta.Respostas)
                    {
                        var linha = $"{resposta.Dataresposta:yyyy-MM-dd HH:mm:ss}," +
                                   $"\"{pergunta.Texto.Replace("\"", "\"\"")}\"," +
                                   $"\"{resposta.Texto?.Replace("\"", "\"\"") ?? ""}\"," +
                                   $"{(string.IsNullOrEmpty(resposta.Texto) ? "Opcoes" : "Discursiva")}";
                        csv.AppendLine(linha);
                    }
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"respostas_pesquisa_{pesquisaId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                var result = new ExportFileResult
                {
                    FileData = bytes,
                    ContentType = "text/csv",
                    FileName = fileName,
                    FileExtension = ".csv",
                    FileSize = bytes.Length
                };

                return ServiceResult<ExportFileResult>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportFileResult>.Fail("ERROR", $"Erro ao exportar CSV: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ExportFileResult>>> ExportarTodosFormatosAsync(int pesquisaId, CancellationToken ct = default)
        {
            try
            {
                var resultados = new List<ExportFileResult>();

                var pdfPesquisa = await ExportarPesquisaPDFAsync(pesquisaId, ct);
                if (pdfPesquisa.Success) resultados.Add(pdfPesquisa.Data!);

                var pdfResultados = await ExportarResultadosPDFAsync(pesquisaId, ct);
                if (pdfResultados.Success) resultados.Add(pdfResultados.Data!);

                var csv = await ExportarRespostasCSVAsync(pesquisaId, ct);
                if (csv.Success) resultados.Add(csv.Data!);

                var excel = await ExportarResultadosExcelAsync(pesquisaId, ct);
                if (excel.Success) resultados.Add(excel.Data!);

                return ServiceResult<List<ExportFileResult>>.Ok(resultados);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ExportFileResult>>.Fail("ERROR", $"Erro ao exportar todos os formatos: {ex.Message}");
            }
        }

        private async Task<ServiceResult<object>> ObterDadosExportacao(int pesquisaId, CancellationToken ct)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .Include(p => p.Perguntas.OrderBy(pg => pg.Ordem))
                        .ThenInclude(pg => pg.Opcoespergunta.OrderBy(o => o.Ordem))
                    .Include(p => p.Perguntas)
                        .ThenInclude(pg => pg.Respostas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Pesquisaid == pesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                var dados = new
                {
                    pesquisa = new
                    {
                        id = pesquisa.Pesquisaid,
                        titulo = pesquisa.Titulo,
                        descricao = pesquisa.Descricao,
                        dataCriacao = pesquisa.Datacriacao
                    },
                    perguntas = pesquisa.Perguntas.Select(p => new
                    {
                        id = p.Perguntaid,
                        texto = p.Texto,
                        ordem = p.Ordem,
                        tipo = p.Tipoperguntaid,
                        opcoes = p.Opcoespergunta.Select(o => new
                        {
                            id = o.Opcaoid,
                            texto = o.Texto,
                            ordem = o.Ordem,
                            correta = o.Correta
                        }).ToList(),
                        respostas = p.Respostas.Select(r => new
                        {
                            id = r.Respostaid,
                            texto = r.Texto,
                            dataResposta = r.Dataresposta
                        }).ToList()
                    }).ToList()
                };

                return ServiceResult<object>.Ok(dados);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail("ERROR", ex.Message);
            }
        }
    }
}
