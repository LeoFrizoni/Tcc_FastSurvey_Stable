#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public interface ITokenRepository : IRepository<Tokens>
    {
        Task<Tokens?> ObterPorIdAsync(int id, CancellationToken ct = default);
        Task<Tokens?> ObterPorCodigoAsync(string token, CancellationToken ct = default);
        Task<List<Tokens>> ListarAsync(bool apenasAtivos, int? skip = null, int? take = null, CancellationToken ct = default);
    }
}
