#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IParticipanteSessaoRepository : IRepository<ParticipantesSessao>
    {
        Task<ParticipantesSessao?> ObterParticipanteAsync(
            int participanteId,
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
        Task<List<ParticipantesSessao>> ObterParticipantesPorPeriodoAsync(
            string sessaoId,
            DateTime inicio,
            DateTime fim,
            CancellationToken ct = default
        );

        // Conveniências (sem SaveChanges aqui):
        Task<ParticipantesSessao> RegistrarEntradaAsync(
            string sessaoId,
            string nome,
            DateTime entrouEmUtc,
            CancellationToken ct = default
        );
        Task<bool> MarcarSaidaParticipanteAsync(
            int participanteId,
            DateTime saiuEmUtc,
            CancellationToken ct = default
        );
        Task<bool> AtualizarParticipanteAsync(
            int participanteId,
            string nome,
            CancellationToken ct = default
        );
        Task<bool> RemoverParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        );
        Task<bool> AtivarParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        );
        Task<bool> DesativarParticipanteAsync(
            int participanteId,
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
