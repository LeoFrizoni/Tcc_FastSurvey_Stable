#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public interface ITipoUsuarioRepository : IRepository<Tipousuario>
    {
        Task<List<Tipousuario>> ListarAsync(bool incluirDesabilitados = false, CancellationToken ct = default);
    }
}
