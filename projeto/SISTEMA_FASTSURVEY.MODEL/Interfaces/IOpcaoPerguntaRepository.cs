#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public interface IOpcaoPerguntaRepository : IRepository<Opcoespergunta>
    {
        Task<List<Opcoespergunta>> ListarPorPerguntaAsync(int perguntaId, CancellationToken ct = default);
        Task<List<Opcoespergunta>> ListarAtivasPorPerguntaAsync(int perguntaId, CancellationToken ct = default);
    }
}
