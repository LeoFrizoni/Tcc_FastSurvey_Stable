// FASTSURVEY/Services/Tipos/ITipoPerguntaService.cs
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Tipos;

namespace FASTSURVEY.Services.Tipos
{
    public interface ITipoPerguntaService
    {
        /// Lista tipos de pergunta. Quando incluirDesabilitados=false, retorna apenas ativos.
        Task<IReadOnlyList<TipoPerguntaCatalogDto>> ListarAsync(
            bool incluirDesabilitados = false,
            CancellationToken ct = default
        );
    }
}
