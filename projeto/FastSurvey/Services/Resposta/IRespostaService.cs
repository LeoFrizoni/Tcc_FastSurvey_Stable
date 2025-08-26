#nullable enable
using FASTSURVEY.Dtos.Respostas;
using RespostaModel = SISTEMA_FASTSURVEY.MODEL.Models.Respostas;

namespace FASTSURVEY.Services.Resposta
{
    public interface IRespostaService
    {
        Task<RespostaModel> CriarDiscursivaAsync(
            CriarRespostaDiscursivaRequest req,
            CancellationToken ct = default
        );
        Task<RespostaModel> CriarOpcoesAsync(
            CriarRespostaOpcoesRequest req,
            CancellationToken ct = default
        );

        // Novos
        Task<int> EnviarRespostaCompletaAsync(
            int pesquisaId,
            EnviarRespostaCompletaRequest request,
            CancellationToken ct = default
        );
        Task<RespostaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<RespostaDto>> ListarPorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
        Task<AnalyticsResponse?> ObterAnalyticsAsync(
            int pesquisaId,
            EstatisticasPesquisaRequest request,
            CancellationToken ct = default
        );
        Task<ValidacaoRespostaResponse> ValidarRespostaAsync(
            int pesquisaId,
            ValidarRespostaRequest request,
            CancellationToken ct = default
        );
        Task<int> ObterTotalRespostasAsync(int pesquisaId, CancellationToken ct = default);
        Task<bool> ExcluirAsync(int id, CancellationToken ct = default);
    }
}
