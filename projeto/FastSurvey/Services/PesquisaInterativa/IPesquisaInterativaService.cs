#nullable enable
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public interface IPesquisaInterativaService
    {
        Task<ServiceResult<SessaoInterativaResponse>> IniciarSessaoAsync(
            PesquisaInterativaRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<ParticipanteResponse>> EntrarSessaoAsync(
            EntrarSessaoRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<SessaoInterativaResponse>> ObterSessaoAsync(
            string codigo,
            CancellationToken ct = default
        );
        Task<ServiceResult<RespostaInterativaResponse>> ResponderPerguntaAsync(
            RespostaInterativaRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<object>> ObterResultadosTempoRealAsync(
            string sessaoId,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> FinalizarSessaoAsync(
            FinalizarSessaoRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<object>> ObterStatusSessaoAsync(
            string sessaoId,
            CancellationToken ct = default
        );
        Task<ServiceResult<List<ParticipanteResponse>>> ObterParticipantesAsync(
            string sessaoId,
            CancellationToken ct = default
        );
        Task<ServiceResult<PerguntaAtualResponse>> AvancarPerguntaAsync(
            AvancarPerguntaRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<PerguntaAtualResponse>> VoltarPerguntaAsync(
            VoltarPerguntaRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<PerguntaAtualResponse>> IrParaPerguntaAsync(
            IrParaPerguntaRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> AtivarPerguntaAsync(
            AtivarPerguntaRequest request,
            CancellationToken ct = default
        );
    }
}
