using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IParticipanteSessaoRepository : IRepository<ParticipantesSessao>
    {
        Task<ParticipantesSessao?> ObterPorNomeESessaoAsync(string nome, string sessaoId);
        Task<IEnumerable<ParticipantesSessao>> ObterParticipantesAtivosAsync(string sessaoId);
        Task<int> ContarParticipantesAtivosAsync(string sessaoId);
        Task<IEnumerable<ParticipantesSessao>> ObterParticipantesPorSessaoAsync(string sessaoId);
        Task MarcarSaidaParticipantesAsync(string sessaoId);
    }
}
