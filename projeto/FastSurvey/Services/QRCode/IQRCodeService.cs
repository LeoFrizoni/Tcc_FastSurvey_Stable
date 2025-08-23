using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.QRCode
{
    public interface IQRCodeService
    {
        Task<ServiceResult<QRCodeResponse>> GerarQRCodeAsync(QRCodeRequest request, CancellationToken ct = default);
        Task<ServiceResult<QRCodeResponse>> ObterQRCodePesquisaAsync(int pesquisaId, CancellationToken ct = default);
        Task<ServiceResult<bool>> ValidarQRCodeAsync(string qrCodeUrl, CancellationToken ct = default);
        Task<ServiceResult<QRCodeResponse>> AtualizarQRCodeAsync(int pesquisaId, QRCodeRequest request, CancellationToken ct = default);
        Task<ServiceResult<List<QRCodeResponse>>> ListarQRCodesUsuarioAsync(int loginId, CancellationToken ct = default);
    }

    public class QRCodeResponse
    {
        public int PesquisaId { get; set; }
        public string QRCodeUrl { get; set; } = string.Empty;
        public string QRCodeBase64 { get; set; } = string.Empty;
        public string LinkResposta { get; set; } = string.Empty;
        public DateTime GeradoEm { get; set; }
        public DateTime? Expiracao { get; set; }
        public bool Ativo { get; set; }
        public string TituloPesquisa { get; set; } = string.Empty;
    }
}
