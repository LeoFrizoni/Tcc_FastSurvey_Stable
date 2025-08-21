#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FastSurveyContext _context;

        public UnitOfWork(FastSurveyContext context)
        {
            _context = context;
        }

        public async Task<int> CommitAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
