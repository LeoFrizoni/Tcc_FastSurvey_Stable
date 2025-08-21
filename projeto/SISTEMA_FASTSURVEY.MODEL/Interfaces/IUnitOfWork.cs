#nullable enable
namespace SISTEMA_FASTSURVEY.MODEL.Repositories;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
