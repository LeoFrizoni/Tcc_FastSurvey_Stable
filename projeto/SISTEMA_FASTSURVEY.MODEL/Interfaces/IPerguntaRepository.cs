#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using PerguntaEntity = SISTEMA_FASTSURVEY.MODEL.Models.Perguntas;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public interface IPerguntaRepository : IRepository<PerguntaEntity>
    {
        Task<List<PerguntaEntity>> ListarPorPesquisaAsync(
            int pesquisaId,
            bool incluirOpcoes = true,
            bool incluirAnexos = true,
            CancellationToken ct = default);
    }
}
