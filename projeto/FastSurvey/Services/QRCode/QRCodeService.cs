using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.QRCode
{
    public class QRCodeService : IQRCodeService
    {
        private readonly FastSurveyContext _ctx;
        private readonly IConfiguration _config;

        public QRCodeService(FastSurveyContext ctx, IConfiguration config)
        {
            _ctx = ctx;
            _config = config;
        }

        // ========= Helpers =========

        private static string BuildRespostaLink(IConfiguration config, int pesquisaId)
        {
            var baseUrl = config["Frontend:BaseUrl"] ?? "http://localhost:3000";
            // garante sem barra dupla
            baseUrl = baseUrl.TrimEnd('/');
            return $"{baseUrl}/responder/{pesquisaId}";
        }

        private static string CreateQrDataUri(
            string payload,
            QRCodeGenerator.ECCLevel ecc = QRCodeGenerator.ECCLevel.Q,
            int pixelsPerModule = 8
        )
        {
            // sem System.Drawing: usa PngByteQRCode
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, ecc);
            var pngQr = new PngByteQRCode(data);
            var bytes = pngQr.GetGraphic(pixelsPerModule);
            var b64 = Convert.ToBase64String(bytes);
            return $"data:image/png;base64,{b64}";
        }

        private static bool TryExtractPesquisaId(string qrCodeUrl, out int pesquisaId)
        {
            pesquisaId = 0;

            // tenta absoluta
            if (Uri.TryCreate(qrCodeUrl, UriKind.Absolute, out var abs))
            {
                var seg = abs.Segments.LastOrDefault()?.TrimEnd('/') ?? string.Empty;
                return int.TryParse(seg, out pesquisaId);
            }

            // tenta relativa
            if (Uri.TryCreate(qrCodeUrl, UriKind.Relative, out var rel))
            {
                var s = rel.OriginalString.TrimEnd('/');
                var last = s.Split('/').LastOrDefault() ?? string.Empty;
                return int.TryParse(last, out pesquisaId);
            }

            // fallback simples
            var tail = qrCodeUrl.TrimEnd('/').Split('/').LastOrDefault() ?? string.Empty;
            return int.TryParse(tail, out pesquisaId);
        }

        // ========= Métodos da interface =========

        public async Task<ServiceResult<QRCodeResponse>> GerarQRCodeAsync(
            QRCodeRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas.FirstOrDefaultAsync(
                    p => p.PesquisaId == request.PesquisaId,
                    ct
                );

                if (pesquisa is null)
                    return ServiceResult<QRCodeResponse>.Fail(
                        "NOT_FOUND",
                        "Pesquisa não encontrada"
                    );

                // vencimento
                if (
                    pesquisa.TemLimitadorTempo
                    && pesquisa.DataFechamento.HasValue
                    && pesquisa.DataFechamento < DateTime.UtcNow
                )
                    return ServiceResult<QRCodeResponse>.Fail("EXPIRED", "Pesquisa expirada");

                var linkResposta = BuildRespostaLink(_config, request.PesquisaId);
                var qrCodeBase64 = CreateQrDataUri(linkResposta);

                // atualiza URL associada na pesquisa se solicitado
                if (request.GerarNovo || string.IsNullOrEmpty(pesquisa.QRCodeUrl))
                {
                    pesquisa.QRCodeUrl = linkResposta;
                    await _ctx.SaveChangesAsync(ct);
                }

                var resp = new QRCodeResponse
                {
                    PesquisaId = pesquisa.PesquisaId,
                    QRCodeUrl = linkResposta,
                    QRCodeBase64 = qrCodeBase64,
                    LinkResposta = linkResposta,
                    GeradoEm = DateTime.UtcNow,
                    Expiracao = request.Expiracao ?? pesquisa.DataFechamento,
                    Ativo =
                        !pesquisa.TemLimitadorTempo
                        || !pesquisa.DataFechamento.HasValue
                        || pesquisa.DataFechamento > DateTime.UtcNow,
                    TituloPesquisa = pesquisa.Titulo,
                };

                return ServiceResult<QRCodeResponse>.Ok(resp);
            }
            catch (Exception ex)
            {
                return ServiceResult<QRCodeResponse>.Fail(
                    "ERROR",
                    $"Erro ao gerar QR Code: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<QRCodeResponse>> ObterQRCodePesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas.FirstOrDefaultAsync(
                    p => p.PesquisaId == pesquisaId,
                    ct
                );

                if (pesquisa is null)
                    return ServiceResult<QRCodeResponse>.Fail(
                        "NOT_FOUND",
                        "Pesquisa não encontrada"
                    );

                // gera se ainda não houver
                var link = string.IsNullOrEmpty(pesquisa.QRCodeUrl)
                    ? BuildRespostaLink(_config, pesquisaId)
                    : pesquisa.QRCodeUrl;

                if (string.IsNullOrEmpty(pesquisa.QRCodeUrl))
                {
                    pesquisa.QRCodeUrl = link;
                    await _ctx.SaveChangesAsync(ct);
                }

                var qrCodeBase64 = CreateQrDataUri(link);

                var resp = new QRCodeResponse
                {
                    PesquisaId = pesquisa.PesquisaId,
                    QRCodeUrl = link,
                    QRCodeBase64 = qrCodeBase64,
                    LinkResposta = link,
                    GeradoEm = DateTime.UtcNow,
                    Expiracao = pesquisa.DataFechamento,
                    Ativo =
                        !pesquisa.TemLimitadorTempo
                        || !pesquisa.DataFechamento.HasValue
                        || pesquisa.DataFechamento > DateTime.UtcNow,
                    TituloPesquisa = pesquisa.Titulo,
                };

                return ServiceResult<QRCodeResponse>.Ok(resp);
            }
            catch (Exception ex)
            {
                return ServiceResult<QRCodeResponse>.Fail(
                    "ERROR",
                    $"Erro ao obter QR Code: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> ValidarQRCodeAsync(
            string qrCodeUrl,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!TryExtractPesquisaId(qrCodeUrl, out var pesquisaId))
                    return ServiceResult<bool>.Fail("INVALID_URL", "URL do QR Code inválida");

                var pesquisa = await _ctx.Pesquisas.FirstOrDefaultAsync(
                    p => p.PesquisaId == pesquisaId,
                    ct
                );

                if (pesquisa is null)
                    return ServiceResult<bool>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                if (!pesquisa.Ativa)
                    return ServiceResult<bool>.Fail("INACTIVE", "Pesquisa inativa");

                if (
                    pesquisa.TemLimitadorTempo
                    && pesquisa.DataFechamento.HasValue
                    && pesquisa.DataFechamento < DateTime.UtcNow
                )
                    return ServiceResult<bool>.Fail("EXPIRED", "Pesquisa expirada");

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail("ERROR", $"Erro ao validar QR Code: {ex.Message}");
            }
        }

        public async Task<ServiceResult<QRCodeResponse>> AtualizarQRCodeAsync(
            int pesquisaId,
            QRCodeRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas.FirstOrDefaultAsync(
                    p => p.PesquisaId == pesquisaId,
                    ct
                );

                if (pesquisa is null)
                    return ServiceResult<QRCodeResponse>.Fail(
                        "NOT_FOUND",
                        "Pesquisa não encontrada"
                    );

                // atualiza expiração se enviada
                if (request.Expiracao.HasValue)
                {
                    pesquisa.DataFechamento = request.Expiracao;
                    pesquisa.TemLimitadorTempo = true;
                }

                // gera novo link se solicitado
                if (request.GerarNovo || string.IsNullOrEmpty(pesquisa.QRCodeUrl))
                    pesquisa.QRCodeUrl = BuildRespostaLink(_config, pesquisaId);

                await _ctx.SaveChangesAsync(ct);

                // retorna QR atualizado (gera imagem nova)
                var link = pesquisa.QRCodeUrl!;
                var qrCodeBase64 = CreateQrDataUri(link);

                var resp = new QRCodeResponse
                {
                    PesquisaId = pesquisa.PesquisaId,
                    QRCodeUrl = link,
                    QRCodeBase64 = qrCodeBase64,
                    LinkResposta = link,
                    GeradoEm = DateTime.UtcNow,
                    Expiracao = pesquisa.DataFechamento,
                    Ativo =
                        !pesquisa.TemLimitadorTempo
                        || !pesquisa.DataFechamento.HasValue
                        || pesquisa.DataFechamento > DateTime.UtcNow,
                    TituloPesquisa = pesquisa.Titulo,
                };

                return ServiceResult<QRCodeResponse>.Ok(resp);
            }
            catch (Exception ex)
            {
                return ServiceResult<QRCodeResponse>.Fail(
                    "ERROR",
                    $"Erro ao atualizar QR Code: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<List<QRCodeResponse>>> ListarQRCodesUsuarioAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisas = await _ctx
                    .Pesquisas.AsNoTracking()
                    .Where(p => p.LoginId == loginId)
                    .OrderByDescending(p => p.DataCriacao)
                    .ToListAsync(ct);

                var list = new List<QRCodeResponse>(pesquisas.Count);

                foreach (var p in pesquisas)
                {
                    // garante que tem URL; se não, monta agora sem persistir
                    var link = string.IsNullOrEmpty(p.QRCodeUrl)
                        ? BuildRespostaLink(_config, p.PesquisaId)
                        : p.QRCodeUrl;

                    var qrCodeBase64 = CreateQrDataUri(link);

                    list.Add(
                        new QRCodeResponse
                        {
                            PesquisaId = p.PesquisaId,
                            QRCodeUrl = link,
                            QRCodeBase64 = qrCodeBase64,
                            LinkResposta = link,
                            GeradoEm = DateTime.UtcNow,
                            Expiracao = p.DataFechamento,
                            Ativo =
                                !p.TemLimitadorTempo
                                || !p.DataFechamento.HasValue
                                || p.DataFechamento > DateTime.UtcNow,
                            TituloPesquisa = p.Titulo,
                        }
                    );
                }

                return ServiceResult<List<QRCodeResponse>>.Ok(list);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<QRCodeResponse>>.Fail(
                    "ERROR",
                    $"Erro ao listar QR Codes: {ex.Message}"
                );
            }
        }
    }
}
