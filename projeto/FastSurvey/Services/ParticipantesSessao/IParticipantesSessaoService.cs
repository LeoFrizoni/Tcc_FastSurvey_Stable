using FASTSURVEY.Dtos.ParticipantesSessao;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.ParticipantesSessao
{
    public interface IParticipantesSessaoService
    {
        Task<ParticipanteResponse> ObterParticipanteAsync(int participanteId, CancellationToken ct = default);
        Task<List<ParticipanteResponse>> ObterParticipantesAtivosAsync(string sessaoId, CancellationToken ct = default);
        Task<int> ContarParticipantesAtivosAsync(string sessaoId, CancellationToken ct = default);
        Task<List<ParticipanteResponse>> ObterParticipantesPorSessaoAsync(string sessaoId, CancellationToken ct = default);
        Task<ParticipanteResponse> RegistrarEntradaAsync(ParticipanteRequest request, CancellationToken ct = default);
        Task<bool> MarcarSaidaAsync(int participanteId, CancellationToken ct = default);
        Task<bool> AtualizarParticipanteAsync(int participanteId, ParticipanteUpdateRequest request, CancellationToken ct = default);
        Task<bool> RemoverParticipanteAsync(int participanteId, CancellationToken ct = default);
        Task<List<ParticipanteResponse>> ObterParticipantesPorPeriodoAsync(string sessaoId, DateTime inicio, DateTime fim, CancellationToken ct = default);
        Task<ServiceResult<bool>> AtivarParticipanteAsync(int participanteId, CancellationToken ct = default);
        Task<ServiceResult<bool>> DesativarParticipanteAsync(int participanteId, CancellationToken ct = default);
    }
}
