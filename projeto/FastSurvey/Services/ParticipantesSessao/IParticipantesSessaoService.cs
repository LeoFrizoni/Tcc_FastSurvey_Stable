using FASTSURVEY.Dtos.ParticipantesSessao;

namespace FASTSURVEY.Services.ParticipantesSessao
{
    public interface IParticipantesSessaoService
    {
        Task<ParticipanteResponse?> ObterPorNomeESessaoAsync(string nome, string sessaoId, CancellationToken ct = default);
        Task<List<ParticipanteResponse>> ObterParticipantesAtivosAsync(string sessaoId, CancellationToken ct = default);
        Task<int> ContarParticipantesAtivosAsync(string sessaoId, CancellationToken ct = default);
        Task<List<ParticipanteResponse>> ObterParticipantesPorSessaoAsync(string sessaoId, CancellationToken ct = default);
        Task<ParticipanteResponse> RegistrarEntradaAsync(ParticipanteRequest request, CancellationToken ct = default);
        Task<bool> MarcarSaidaAsync(int participanteId, CancellationToken ct = default);
        Task<bool> MarcarSaidaTodosAsync(string sessaoId, CancellationToken ct = default);
        Task<List<ParticipanteRankingResponse>> ObterRankingAsync(string sessaoId, CancellationToken ct = default);
        Task<ParticipanteResponse?> ObterPorIdAsync(int participanteId, CancellationToken ct = default);
    }
}
