#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IParticipanteSessaoRepository : IRepository<ParticipantesSessao>
    {
        Task<ParticipantesSessao?> ObterPorNomeESessaoAsync(
            string nome,
            string sessaoId,
            CancellationToken ct = default
        );
        Task<List<ParticipantesSessao>> ObterParticipantesAtivosAsync(
            string sessaoId,
            CancellationToken ct = default
        );
        Task<int> ContarParticipantesAtivosAsync(string sessaoId, CancellationToken ct = default);
        Task<List<ParticipantesSessao>> ObterParticipantesPorSessaoAsync(
            string sessaoId,
            CancellationToken ct = default
        );

        // Conveniências (sem SaveChanges aqui):
        Task<ParticipantesSessao> RegistrarEntradaAsync(
            string sessaoId,
            string nome,
            DateTime entrouEmUtc,
            CancellationToken ct = default
        );
        Task<int> MarcarSaidaParticipanteAsync(
            int participanteId,
            DateTime saiuEmUtc,
            CancellationToken ct = default
        );
        Task<int> MarcarSaidaParticipantesAsync(
            string sessaoId,
            DateTime saiuEmUtc,
            CancellationToken ct = default
        );

        // Paginação opcional
        Task<List<ParticipantesSessao>> ObterParticipantesPorSessaoAsync(
            string sessaoId,
            int skip,
            int take,
            CancellationToken ct = default
        );

        // Query de composição (read-only)
        IQueryable<ParticipantesSessao> QueryPorSessao(string sessaoId);
    }
}
