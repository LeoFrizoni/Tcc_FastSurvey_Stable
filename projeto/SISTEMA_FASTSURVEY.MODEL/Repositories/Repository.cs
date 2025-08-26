#nullable enable
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class Repository<T> : IRepository<T>, IDisposable
        where T : class
    {
        protected readonly FastSurveyContext _context;
        protected readonly DbSet<T> _set;

        public Repository(FastSurveyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _set = _context.Set<T>();
        }

        // ------ Leitura ------
        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _set.FindAsync(new object?[] { id }, ct);

        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken ct = default
        ) => await _set.AsNoTracking().FirstOrDefaultAsync(predicate, ct);

        public async Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken ct = default
        ) => await _set.AsNoTracking().AnyAsync(predicate, ct);

        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken ct = default
        ) =>
            predicate is null
                ? await _set.AsNoTracking().CountAsync(ct)
                : await _set.AsNoTracking().CountAsync(predicate, ct);

        public async Task<List<T>> ListAsync(
            Expression<Func<T, bool>>? predicate = null,
            int skip = 0,
            int take = 50,
            CancellationToken ct = default
        )
        {
            if (take <= 0)
                take = 50;
            if (skip < 0)
                skip = 0;

            IQueryable<T> q = _set.AsNoTracking();
            if (predicate is not null)
                q = q.Where(predicate);
            if (skip > 0)
                q = q.Skip(skip);
            q = q.Take(take);
            return await q.ToListAsync(ct);
        }

        public async Task<List<T>> ListAsync<TOrder>(
            Expression<Func<T, bool>>? predicate,
            Expression<Func<T, TOrder>> orderBy,
            bool descending = false,
            int skip = 0,
            int take = 50,
            CancellationToken ct = default
        )
        {
            if (take <= 0)
                take = 50;
            if (skip < 0)
                skip = 0;

            IQueryable<T> q = _set.AsNoTracking();
            if (predicate is not null)
                q = q.Where(predicate);
            q = descending ? q.OrderByDescending(orderBy) : q.OrderBy(orderBy);
            if (skip > 0)
                q = q.Skip(skip);
            q = q.Take(take);
            return await q.ToListAsync(ct);
        }

        public async Task<(List<T> Items, int Total)> PageAsync<TOrder>(
            Expression<Func<T, bool>>? predicate,
            Expression<Func<T, TOrder>> orderBy,
            bool descending = false,
            int skip = 0,
            int take = 50,
            CancellationToken ct = default
        )
        {
            if (take <= 0)
                take = 50;
            if (skip < 0)
                skip = 0;

            IQueryable<T> q = _set.AsNoTracking();
            if (predicate is not null)
                q = q.Where(predicate);

            int total = await q.CountAsync(ct);

            q = descending ? q.OrderByDescending(orderBy) : q.OrderBy(orderBy);
            if (skip > 0)
                q = q.Skip(skip);
            q = q.Take(take);

            var items = await q.ToListAsync(ct);
            return (items, total);
        }

        // ------ Escrita (sem SaveChanges) ------
        public async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));
            await _set.AddAsync(entity, ct);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            if (entities is null)
                throw new ArgumentNullException(nameof(entities));
            await _set.AddRangeAsync(entities, ct);
        }

        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));
            _set.Update(entity);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(T entity, CancellationToken ct = default)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));
            _set.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            if (entities is null)
                throw new ArgumentNullException(nameof(entities));
            _set.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            _context.SaveChangesAsync(ct); // permanece para compatibilidade com a interface

        public void Dispose() { /* DbContext é Scoped e descartado pelo DI */
        }
    }
}
