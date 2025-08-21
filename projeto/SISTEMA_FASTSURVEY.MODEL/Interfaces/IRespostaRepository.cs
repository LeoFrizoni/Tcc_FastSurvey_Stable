#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public interface IRespostaRepository : IRepository<Respostas>
    {
        Task<List<Respostas>> ListarPorPerguntaAsync(
            int perguntaId,
            bool incluirOpcoes = true,
            bool incluirAnexos = true,
            int skip = 0,
            int take = 200,
            CancellationToken ct = default);
    }
}
