#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using AnexoEntity = SISTEMA_FASTSURVEY.MODEL.Models.Anexos;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public interface IAnexoRepository : IRepository<AnexoEntity>
    {
        Task<List<AnexoEntity>> ListarPorPesquisaAsync(int pesquisaId, CancellationToken ct = default);
        Task<List<AnexoEntity>> ListarPorPerguntaAsync(int perguntaId, CancellationToken ct = default);
    }
}
