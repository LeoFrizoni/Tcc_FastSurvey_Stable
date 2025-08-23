#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Tipos
{
    public interface ITipoPesquisaService
    {
        Task<List<Tipopesquisa>> ListarAsync(bool incluirDesabilitados = false, CancellationToken ct = default);
    }
}
