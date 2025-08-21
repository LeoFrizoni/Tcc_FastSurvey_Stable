#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories;

public interface ITipoPesquisaRepository : IRepository<Tipopesquisa>
{
    Task<List<Tipopesquisa>> ListarAtivosAsync(CancellationToken ct = default);
}
