// FASTSURVEY/Services/Tipos/ITipoUsuarioService.cs
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Tipos;

namespace FASTSURVEY.Services.Tipos
{
    public interface ITipoUsuarioService
    {
        Task<IReadOnlyList<TipoUsuarioCatalogDto>> ListarAsync(CancellationToken ct = default);
    }
}
