#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories;

public interface ILoginRepository : IRepository<Login>
{
    Task<Login?> ObterPorEmailAsync(string email, CancellationToken ct = default);
}
