#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Tipos
{
    public interface ITipoUsuarioService
    {
        Task<List<Tipousuario>> ListarAsync(bool incluirDesabilitados = false, CancellationToken ct = default);
    }
}
