using FASTSURVEY.Dtos.Respostas;
using RespostaModel = SISTEMA_FASTSURVEY.MODEL.Models.Respostas;

namespace FASTSURVEY.Services.Resposta
{
    public interface IRespostaService
    {
        Task<RespostaModel> CriarDiscursivaAsync(CriarRespostaDiscursivaRequest req, CancellationToken ct = default);
        Task<RespostaModel> CriarOpcoesAsync(CriarRespostaOpcoesRequest req, CancellationToken ct = default);
    }
}
