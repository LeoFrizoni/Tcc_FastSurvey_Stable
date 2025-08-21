#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories;

public interface ITipoPerguntaRepository : IRepository<Tipopergunta>
{
    Task<List<Tipopergunta>> ListarAtivosAsync(CancellationToken ct = default);
}
