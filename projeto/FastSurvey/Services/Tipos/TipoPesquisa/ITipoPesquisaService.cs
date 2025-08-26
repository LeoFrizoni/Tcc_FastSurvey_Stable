// FASTSURVEY/Services/Tipos/ITipoPesquisaService.cs
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Tipos;

namespace FASTSURVEY.Services.Tipos
{
    public interface ITipoPesquisaService
    {
        Task<IReadOnlyList<TipoPesquisaCatalogDto>> ListarAsync(
            bool incluirDesabilitados = false,
            CancellationToken ct = default
        );
    }
}
