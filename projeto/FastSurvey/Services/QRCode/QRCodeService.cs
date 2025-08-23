using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;

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

        public async Task<ServiceResult<QRCodeResponse>> GerarQRCodeAsync(QRCodeRequest request, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .FirstOrDefaultAsync(p => p.Pesquisaid == request.PesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<QRCodeResponse>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                // Verificar se a pesquisa está ativa
                if (pesquisa.Temlimitadortempo && pesquisa.Datafechamento.HasValue && pesquisa.Datafechamento < DateTime.UtcNow)
                    return ServiceResult<QRCodeResponse>.Fail("EXPIRED", "Pesquisa expirada");

                var baseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:3000";
                var linkResposta = $"{baseUrl}/responder/{request.PesquisaId}";

                // Gerar QR Code
                // TODO: Fix QR Code generation
                // var qrGenerator = new QRCodeGenerator();
                // var qrCodeData = qrGenerator.CreateQrCode(linkResposta, QRCodeGenerator.ECCLevel.Q);
                // var qrCode = new QRCoder.QRCodeGenerator.QRCode(qrCodeData);
                // 
                // using var qrCodeImage = qrCode.GetGraphic(20);
                var qrCodeBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=="; // Placeholder

                // Atualizar URL do QR Code na pesquisa se solicitado
                if (request.GerarNovo || string.IsNullOrEmpty(pesquisa.Qrcodeurl))
                {
                    pesquisa.Qrcodeurl = linkResposta;
                    await _ctx.SaveChangesAsync(ct);
                }

                var response = new QRCodeResponse
                {
                    PesquisaId = pesquisa.Pesquisaid,
                    QRCodeUrl = linkResposta,
                    QRCodeBase64 = qrCodeBase64,
                    LinkResposta = linkResposta,
                    GeradoEm = DateTime.UtcNow,
                    Expiracao = request.Expiracao ?? pesquisa.Datafechamento,
                    Ativo = !pesquisa.Temlimitadortempo || !pesquisa.Datafechamento.HasValue || pesquisa.Datafechamento > DateTime.UtcNow,
                    TituloPesquisa = pesquisa.Titulo
                };

                return ServiceResult<QRCodeResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<QRCodeResponse>.Fail("ERROR", $"Erro ao gerar QR Code: {ex.Message}");
            }
        }

        public async Task<ServiceResult<QRCodeResponse>> ObterQRCodePesquisaAsync(int pesquisaId, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .FirstOrDefaultAsync(p => p.Pesquisaid == pesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<QRCodeResponse>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                // Se não tem QR Code, gerar um novo
                if (string.IsNullOrEmpty(pesquisa.Qrcodeurl))
                {
                    return await GerarQRCodeAsync(new QRCodeRequest { PesquisaId = pesquisaId, GerarNovo = true }, ct);
                }

                // TODO: Fix QR Code generation
                // var qrGenerator = new QRCodeGenerator();
                // var qrCodeData = qrGenerator.CreateQrCode(pesquisa.Qrcodeurl, QRCodeGenerator.ECCLevel.Q);
                // var qrCode = new QRCoder.QRCode(qrCodeData);
                // 
                // using var qrCodeImage = qrCode.GetGraphic(20);
                var qrCodeBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=="; // Placeholder

                var response = new QRCodeResponse
                {
                    PesquisaId = pesquisa.Pesquisaid,
                    QRCodeUrl = pesquisa.Qrcodeurl,
                    QRCodeBase64 = qrCodeBase64,
                    LinkResposta = pesquisa.Qrcodeurl,
                    GeradoEm = pesquisa.Datacriacao,
                    Expiracao = pesquisa.Datafechamento,
                    Ativo = !pesquisa.Temlimitadortempo || !pesquisa.Datafechamento.HasValue || pesquisa.Datafechamento > DateTime.UtcNow,
                    TituloPesquisa = pesquisa.Titulo
                };

                return ServiceResult<QRCodeResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<QRCodeResponse>.Fail("ERROR", $"Erro ao obter QR Code: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ValidarQRCodeAsync(string qrCodeUrl, CancellationToken ct = default)
        {
            try
            {
                // Extrair ID da pesquisa da URL
                var uri = new Uri(qrCodeUrl);
                var segments = uri.Segments;
                if (segments.Length < 2 || !int.TryParse(segments[^1], out int pesquisaId))
                    return ServiceResult<bool>.Fail("INVALID_URL", "URL do QR Code inválida");

                var pesquisa = await _ctx.Pesquisas
                    .FirstOrDefaultAsync(p => p.Pesquisaid == pesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<bool>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                // Verificar se está ativa
                if (!pesquisa.Ativa)
                    return ServiceResult<bool>.Fail("INACTIVE", "Pesquisa inativa");

                // Verificar expiração
                if (pesquisa.Temlimitadortempo && pesquisa.Datafechamento.HasValue && pesquisa.Datafechamento < DateTime.UtcNow)
                    return ServiceResult<bool>.Fail("EXPIRED", "Pesquisa expirada");

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail("ERROR", $"Erro ao validar QR Code: {ex.Message}");
            }
        }

        public async Task<ServiceResult<QRCodeResponse>> AtualizarQRCodeAsync(int pesquisaId, QRCodeRequest request, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .FirstOrDefaultAsync(p => p.Pesquisaid == pesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<QRCodeResponse>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                // Atualizar configurações da pesquisa se necessário
                if (request.Expiracao.HasValue)
                {
                    pesquisa.Datafechamento = request.Expiracao;
                    pesquisa.Temlimitadortempo = true;
                }

                await _ctx.SaveChangesAsync(ct);

                // Gerar novo QR Code
                return await GerarQRCodeAsync(new QRCodeRequest 
                { 
                    PesquisaId = pesquisaId, 
                    GerarNovo = true,
                    Expiracao = request.Expiracao 
                }, ct);
            }
            catch (Exception ex)
            {
                return ServiceResult<QRCodeResponse>.Fail("ERROR", $"Erro ao atualizar QR Code: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<QRCodeResponse>>> ListarQRCodesUsuarioAsync(int loginId, CancellationToken ct = default)
        {
            try
            {
                var pesquisas = await _ctx.Pesquisas
                    .Where(p => p.Loginid == loginId && !string.IsNullOrEmpty(p.Qrcodeurl))
                    .OrderByDescending(p => p.Datacriacao)
                    .ToListAsync(ct);

                var qrCodes = new List<QRCodeResponse>();

                foreach (var pesquisa in pesquisas)
                {
                    try
                    {
                        // TODO: Fix QR Code generation
                        // var qrGenerator = new QRCodeGenerator();
                        // var qrCodeData = qrGenerator.CreateQrCode(pesquisa.Qrcodeurl, QRCodeGenerator.ECCLevel.Q);
                        // var qrCode = new QRCoder.QRCode(qrCodeData);
                        // 
                        // using var qrCodeImage = qrCode.GetGraphic(10); // Menor para listagem
                        var qrCodeBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=="; // Placeholder

                        qrCodes.Add(new QRCodeResponse
                        {
                            PesquisaId = pesquisa.Pesquisaid,
                            QRCodeUrl = pesquisa.Qrcodeurl,
                            QRCodeBase64 = qrCodeBase64,
                            LinkResposta = pesquisa.Qrcodeurl,
                            GeradoEm = pesquisa.Datacriacao,
                            Expiracao = pesquisa.Datafechamento,
                            Ativo = !pesquisa.Temlimitadortempo || !pesquisa.Datafechamento.HasValue || pesquisa.Datafechamento > DateTime.UtcNow,
                            TituloPesquisa = pesquisa.Titulo
                        });
                    }
                    catch
                    {
                        // Se falhar ao gerar QR Code para uma pesquisa específica, pula ela
                        continue;
                    }
                }

                return ServiceResult<List<QRCodeResponse>>.Ok(qrCodes);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<QRCodeResponse>>.Fail("ERROR", $"Erro ao listar QR Codes: {ex.Message}");
            }
        }

        private string ConvertToBase64(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            var bytes = stream.ToArray();
            return Convert.ToBase64String(bytes);
        }
    }
}
