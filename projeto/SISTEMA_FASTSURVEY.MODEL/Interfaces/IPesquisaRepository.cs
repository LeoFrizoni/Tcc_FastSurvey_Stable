#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories;

public interface IPesquisaRepository : IRepository<Pesquisas>
{
    Task<Pesquisas?> GetDetalheAsync(
        int pesquisaId,
        bool incluirPerguntas = true,
        bool incluirAnexos = false,
        CancellationToken ct = default);

    Task<List<Pesquisas>> ListarPorLoginAsync(
        int loginId,
        int? pastaId = null,
        int? tipoPesquisaId = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);
}
