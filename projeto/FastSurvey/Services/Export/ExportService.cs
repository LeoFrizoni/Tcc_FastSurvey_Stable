#nullable enable
using System.Text;
using System.Text.Json;
using FASTSURVEY.Services.Result;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Export
{
    public sealed class ExportService : IExportService
    {
        private readonly FastSurveyContext _ctx;

        public ExportService(FastSurveyContext ctx) => _ctx = ctx;

        // =============================
        // PDF - Layout da Pesquisa
        // =============================
        public async Task<ServiceResult<ExportFileResult>> ExportarPesquisaPDFAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx
                    .Pesquisas.Where(p => p.PesquisaId == pesquisaId)
                    .Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta)
                    .Include(p => p.Login)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);

                if (pesquisa is null)
                    return ServiceResult<ExportFileResult>.Fail(
                        "NOT_FOUND",
                        "Pesquisa não encontrada"
                    );

                using var stream = new MemoryStream();
                using var writer = new PdfWriter(stream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // Metadados & margem
                pdf.GetDocumentInfo()
                    .SetTitle(pesquisa.Titulo ?? $"Pesquisa {pesquisa.PesquisaId}");
                pdf.GetDocumentInfo().SetAuthor(pesquisa.Login?.Usuario ?? "FastSurvey");
                document.SetMargins(36, 36, 36, 36);

                // Título
                document.Add(
                    new Paragraph(pesquisa.Titulo ?? $"Pesquisa {pesquisa.PesquisaId}")
                        .SetFontSize(20)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20)
                );

                // Informações da pesquisa
                if (!string.IsNullOrWhiteSpace(pesquisa.Descricao))
                {
                    document.Add(
                        new Paragraph($"Descrição: {pesquisa.Descricao}")
                            .SetFontSize(12)
                            .SetMarginBottom(8)
                    );
                }

                document.Add(
                    new Paragraph($"Autor: {pesquisa.Login?.Usuario ?? "N/A"}")
                        .SetFontSize(12)
                        .SetMarginBottom(8)
                );

                document.Add(
                    new Paragraph($"Data de Criação: {pesquisa.DataCriacao:dd/MM/yyyy HH:mm}")
                        .SetFontSize(12)
                        .SetMarginBottom(16)
                );

                // Perguntas
                document.Add(
                    new Paragraph("PERGUNTAS").SetFontSize(16).SetBold().SetMarginBottom(12)
                );

                foreach (var pergunta in pesquisa.Perguntas.OrderBy(pg => pg.Ordem))
                {
                    // Cabeçalho da pergunta
                    var tituloPergunta = $"{pergunta.Ordem}. {pergunta.Texto}";
                    document.Add(
                        new Paragraph(tituloPergunta).SetFontSize(14).SetBold().SetMarginBottom(6)
                    );

                    // Tipo
                    document.Add(
                        new Paragraph($"Tipo: {MapTipoPergunta(pergunta.TipoPerguntaId)}")
                            .SetFontSize(10)
                            .SetItalic()
                            .SetMarginBottom(4)
                    );

                    // Opções
                    var opcoes =
                        pergunta.OpcoesPergunta?.OrderBy(o => o.Ordem).ToList()
                        ?? new List<OpcoesPergunta>();
                    if (opcoes.Count > 0)
                    {
                        document.Add(
                            new Paragraph("Opções:").SetFontSize(12).SetBold().SetMarginBottom(4)
                        );

                        foreach (var opcao in opcoes)
                        {
                            var marcador = pergunta.TipoPerguntaId == 2 ? "○" : "☐";
                            if (pergunta.TemGabarito && opcao.Correta)
                                marcador = pergunta.TipoPerguntaId == 2 ? "●" : "☑";

                            document.Add(
                                new Paragraph($"   {marcador} {opcao.Texto}")
                                    .SetFontSize(11)
                                    .SetMarginBottom(2)
                            );
                        }
                    }

                    document.Add(new Paragraph("").SetMarginBottom(10));
                }

                document.Close();

                var bytes = stream.ToArray();
                var fileName = $"pesquisa_{pesquisaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                return ServiceResult<ExportFileResult>.Ok(
                    new ExportFileResult
                    {
                        FileData = bytes,
                        ContentType = "application/pdf",
                        FileName = fileName,
                        FileExtension = ".pdf",
                        FileSize = bytes.LongLength,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportFileResult>.Fail(
                    "ERROR",
                    $"Erro ao exportar PDF: {ex.Message}"
                );
            }
        }

        // =============================
        // PDF - Resultados da Pesquisa
        // =============================
        public async Task<ServiceResult<ExportFileResult>> ExportarResultadosPDFAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                // Carrega tudo necessário (perguntas, respostas e opções marcadas)
                var pesquisa = await _ctx
                    .Pesquisas.Where(p => p.PesquisaId == pesquisaId)
                    .Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta)
                    .Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.Respostas)
                    .ThenInclude(r => r.Opcao) // navegação many-to-many (ajuste se o nome for diferente)
                    .Include(p => p.Login)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);

                if (pesquisa is null)
                    return ServiceResult<ExportFileResult>.Fail(
                        "NOT_FOUND",
                        "Pesquisa não encontrada"
                    );

                using var stream = new MemoryStream();
                using var writer = new PdfWriter(stream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // Metadados & margem
                pdf.GetDocumentInfo()
                    .SetTitle(
                        $"Resultados - {pesquisa.Titulo ?? $"Pesquisa {pesquisa.PesquisaId}"}"
                    );
                pdf.GetDocumentInfo().SetAuthor(pesquisa.Login?.Usuario ?? "FastSurvey");
                document.SetMargins(36, 36, 36, 36);

                // Cabeçalho
                document.Add(
                    new Paragraph("RESULTADOS DA PESQUISA")
                        .SetFontSize(20)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(8)
                );

                document.Add(
                    new Paragraph(pesquisa.Titulo ?? $"Pesquisa {pesquisa.PesquisaId}")
                        .SetFontSize(16)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(16)
                );

                // Estatísticas gerais (baseadas em respostas por pergunta)
                var totalRespostas = pesquisa.Perguntas.SelectMany(p => p.Respostas).Count();
                document.Add(
                    new Paragraph("ESTATÍSTICAS GERAIS")
                        .SetFontSize(14)
                        .SetBold()
                        .SetMarginBottom(8)
                );

                document.Add(
                    new Paragraph($"Total de Respostas: {totalRespostas}")
                        .SetFontSize(12)
                        .SetMarginBottom(2)
                );

                document.Add(
                    new Paragraph($"Total de Perguntas: {pesquisa.Perguntas.Count}")
                        .SetFontSize(12)
                        .SetMarginBottom(12)
                );

                // Resultados por pergunta
                document.Add(
                    new Paragraph("RESULTADOS POR PERGUNTA")
                        .SetFontSize(14)
                        .SetBold()
                        .SetMarginBottom(10)
                );

                foreach (var pergunta in pesquisa.Perguntas.OrderBy(p => p.Ordem))
                {
                    document.Add(
                        new Paragraph($"{pergunta.Ordem}. {pergunta.Texto}")
                            .SetFontSize(13)
                            .SetBold()
                            .SetMarginBottom(6)
                    );

                    var totalRespostasPergunta = pergunta.Respostas.Count;
                    document.Add(
                        new Paragraph($"Total de respostas: {totalRespostasPergunta}")
                            .SetFontSize(11)
                            .SetMarginBottom(6)
                    );

                    // Contagem por opção (sem N+1; já está carregado em memória)
                    var contagemPorOpcao = new Dictionary<int, int>();
                    foreach (var r in pergunta.Respostas)
                    {
                        if (r.Opcao is null)
                            continue;
                        foreach (var op in r.Opcao) // coleção de OpcoesPergunta
                        {
                            var id = op.OpcaoId;
                            contagemPorOpcao[id] = contagemPorOpcao.GetValueOrDefault(id) + 1;
                        }
                    }

                    // Resultados das opções
                    var opcoes =
                        pergunta.OpcoesPergunta?.OrderBy(o => o.Ordem).ToList()
                        ?? new List<OpcoesPergunta>();
                    if (opcoes.Count > 0)
                    {
                        foreach (var opcao in opcoes)
                        {
                            contagemPorOpcao.TryGetValue(opcao.OpcaoId, out var respostasOpcao);
                            var percentual =
                                totalRespostasPergunta > 0
                                    ? (respostasOpcao * 100.0 / totalRespostasPergunta)
                                    : 0.0;

                            document.Add(
                                new Paragraph(
                                    $"   • {opcao.Texto}: {respostasOpcao} ({percentual:F1}%)"
                                )
                                    .SetFontSize(11)
                                    .SetMarginBottom(2)
                            );
                        }
                    }

                    // Respostas discursivas (amostra)
                    var respostasDiscursivas = pergunta
                        .Respostas.Where(r => !string.IsNullOrWhiteSpace(r.Texto))
                        .Take(10)
                        .ToList();

                    if (respostasDiscursivas.Count > 0)
                    {
                        document.Add(
                            new Paragraph("Respostas discursivas (amostra):")
                                .SetFontSize(11)
                                .SetBold()
                                .SetMarginBottom(4)
                        );

                        foreach (var resposta in respostasDiscursivas)
                        {
                            document.Add(
                                new Paragraph($"   \"{resposta.Texto}\"")
                                    .SetFontSize(10)
                                    .SetItalic()
                                    .SetMarginBottom(2)
                            );
                        }
                    }

                    document.Add(new Paragraph("").SetMarginBottom(10));
                }

                document.Close();

                var bytes = stream.ToArray();
                var fileName =
                    $"resultados_pesquisa_{pesquisaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                return ServiceResult<ExportFileResult>.Ok(
                    new ExportFileResult
                    {
                        FileData = bytes,
                        ContentType = "application/pdf",
                        FileName = fileName,
                        FileExtension = ".pdf",
                        FileSize = bytes.LongLength,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportFileResult>.Fail(
                    "ERROR",
                    $"Erro ao exportar resultados PDF: {ex.Message}"
                );
            }
        }

        // =============================
        // Placeholder: "Excel" como JSON
        // =============================
        public async Task<ServiceResult<ExportFileResult>> ExportarResultadosExcelAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                var dados = await ObterDadosExportacao(pesquisaId, ct);
                if (!dados.Success)
                {
                    var msg =
                        dados.Errors?.FirstOrDefault() ?? "Falha ao obter dados para exportação.";
                    return ServiceResult<ExportFileResult>.Fail("ERROR", msg);
                }

                var json = JsonSerializer.Serialize(
                    dados.Data,
                    new JsonSerializerOptions { WriteIndented = true }
                );
                var bytes = Encoding.UTF8.GetBytes(json);

                var fileName =
                    $"resultados_pesquisa_{pesquisaId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                return ServiceResult<ExportFileResult>.Ok(
                    new ExportFileResult
                    {
                        FileData = bytes,
                        ContentType = "application/json",
                        FileName = fileName,
                        FileExtension = ".json",
                        FileSize = bytes.LongLength,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportFileResult>.Fail(
                    "ERROR",
                    $"Erro ao exportar Excel: {ex.Message}"
                );
            }
        }

        // =============================
        // CSV - Respostas (discursivas + opções)
        // =============================
        public async Task<ServiceResult<ExportFileResult>> ExportarRespostasCSVAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx
                    .Pesquisas.Where(p => p.PesquisaId == pesquisaId)
                    .Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.Respostas)
                    .ThenInclude(r => r.Opcao) // coleção many-to-many
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);

                if (pesquisa is null)
                    return ServiceResult<ExportFileResult>.Fail(
                        "NOT_FOUND",
                        "Pesquisa não encontrada"
                    );

                var sb = new StringBuilder();

                // Cabeçalho
                sb.AppendLine(
                    "Data Resposta,Pergunta,Resposta Discursiva,Opcoes Marcadas,Tipo Resposta"
                );

                foreach (var pergunta in pesquisa.Perguntas.OrderBy(p => p.Ordem))
                {
                    foreach (var resposta in pergunta.Respostas)
                    {
                        var data = resposta.DataResposta.ToString("yyyy-MM-dd HH:mm:ss");
                        var perguntaTxt = CsvEscape(pergunta.Texto);
                        var respTxt = CsvEscape(resposta.Texto ?? "");

                        string opcoesMarcadas = "";
                        if (resposta.Opcao is not null && resposta.Opcao.Count > 0)
                        {
                            var textos = resposta
                                .Opcao.Select(o => o?.Texto ?? "")
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .Select(CsvEscape);
                            opcoesMarcadas = string.Join(";", textos);
                        }

                        var tipo =
                            string.IsNullOrEmpty(resposta.Texto)
                            && !string.IsNullOrEmpty(opcoesMarcadas)
                                ? "Opcoes"
                                : (string.IsNullOrEmpty(resposta.Texto) ? "Vazia" : "Discursiva");

                        sb.AppendLine(
                            $"{data},\"{perguntaTxt}\",\"{respTxt}\",\"{opcoesMarcadas}\",{tipo}"
                        );
                    }
                }

                // UTF-8 BOM para Excel
                var bom = new byte[] { 0xEF, 0xBB, 0xBF };
                var payload = Encoding.UTF8.GetBytes(sb.ToString());
                var bytes = bom.Concat(payload).ToArray();

                var fileName =
                    $"respostas_pesquisa_{pesquisaId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return ServiceResult<ExportFileResult>.Ok(
                    new ExportFileResult
                    {
                        FileData = bytes,
                        ContentType = "text/csv",
                        FileName = fileName,
                        FileExtension = ".csv",
                        FileSize = bytes.LongLength,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<ExportFileResult>.Fail(
                    "ERROR",
                    $"Erro ao exportar CSV: {ex.Message}"
                );
            }
        }

        // =============================
        // Todos os formatos
        // =============================
        public async Task<ServiceResult<List<ExportFileResult>>> ExportarTodosFormatosAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                var resultados = new List<ExportFileResult>();

                var pdfPesquisa = await ExportarPesquisaPDFAsync(pesquisaId, ct);
                if (pdfPesquisa.Success && pdfPesquisa.Data is not null)
                    resultados.Add(pdfPesquisa.Data);

                var pdfResultados = await ExportarResultadosPDFAsync(pesquisaId, ct);
                if (pdfResultados.Success && pdfResultados.Data is not null)
                    resultados.Add(pdfResultados.Data);

                var csv = await ExportarRespostasCSVAsync(pesquisaId, ct);
                if (csv.Success && csv.Data is not null)
                    resultados.Add(csv.Data);

                var excel = await ExportarResultadosExcelAsync(pesquisaId, ct);
                if (excel.Success && excel.Data is not null)
                    resultados.Add(excel.Data);

                return ServiceResult<List<ExportFileResult>>.Ok(resultados);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ExportFileResult>>.Fail(
                    "ERROR",
                    $"Erro ao exportar todos os formatos: {ex.Message}"
                );
            }
        }

        // =============================
        // Helpers
        // =============================
        private async Task<ServiceResult<object>> ObterDadosExportacao(
            int pesquisaId,
            CancellationToken ct
        )
        {
            try
            {
                var pesquisa = await _ctx
                    .Pesquisas.Where(p => p.PesquisaId == pesquisaId)
                    .Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta)
                    .Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.Respostas)
                    .ThenInclude(r => r.Opcao)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);

                if (pesquisa is null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                var dados = new
                {
                    pesquisa = new
                    {
                        id = pesquisa.PesquisaId,
                        titulo = pesquisa.Titulo,
                        descricao = pesquisa.Descricao,
                        dataCriacao = pesquisa.DataCriacao,
                    },
                    perguntas = pesquisa
                        .Perguntas.OrderBy(p => p.Ordem)
                        .Select(p => new
                        {
                            id = p.PerguntaId,
                            texto = p.Texto,
                            ordem = p.Ordem,
                            tipo = p.TipoPerguntaId,
                            opcoes = (p.OpcoesPergunta ?? new List<OpcoesPergunta>())
                                .OrderBy(o => o.Ordem)
                                .Select(o => new
                                {
                                    id = o.OpcaoId,
                                    texto = o.Texto,
                                    ordem = o.Ordem,
                                    correta = o.Correta,
                                })
                                .ToList(),
                            respostas = p
                                .Respostas.Select(r => new
                                {
                                    id = r.RespostaId,
                                    texto = r.Texto,
                                    dataResposta = r.DataResposta,
                                    opcoesMarcadas = (r.Opcao ?? new List<OpcoesPergunta>())
                                        .Select(o => o.Texto)
                                        .ToList(),
                                })
                                .ToList(),
                        })
                        .ToList(),
                };

                return ServiceResult<object>.Ok(dados);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail("ERROR", ex.Message);
            }
        }

        private static string MapTipoPergunta(int tipoId) =>
            tipoId switch
            {
                1 => "Discursiva",
                2 => "Objetiva",
                3 => "Múltipla Escolha",
                _ => "Desconhecido",
            };

        private static string CsvEscape(string? s) =>
            string.IsNullOrEmpty(s) ? "" : s.Replace("\"", "\"\"");
    }
}
