#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FastSurveyContext _context;
        private IDbContextTransaction? _currentTx;

        public UnitOfWork(FastSurveyContext context)
        {
            _context = context;
        }

        public async Task<int> CommitAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_currentTx is not null)
                return; // já existe transação aberta

            _currentTx = await _context.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (_currentTx is null)
                return;

            await _context.SaveChangesAsync(ct);
            await _currentTx.CommitAsync(ct);
            await _currentTx.DisposeAsync();
            _currentTx = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (_currentTx is null)
                return;

            await _currentTx.RollbackAsync(ct);
            await _currentTx.DisposeAsync();
            _currentTx = null;
        }

        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            bool useExecutionStrategy = true,
            CancellationToken ct = default
        )
        {
            if (useExecutionStrategy)
            {
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _context.Database.BeginTransactionAsync(ct);
                    await action(ct);
                    await _context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                });
            }
            else
            {
                await using var tx = await _context.Database.BeginTransactionAsync(ct);
                await action(ct);
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
        }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            bool useExecutionStrategy = true,
            CancellationToken ct = default
        )
        {
            if (useExecutionStrategy)
            {
                var strategy = _context.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _context.Database.BeginTransactionAsync(ct);
                    var result = await action(ct);
                    await _context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    return result;
                });
            }
            else
            {
                await using var tx = await _context.Database.BeginTransactionAsync(ct);
                var result = await action(ct);
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return result;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_currentTx is not null)
            {
                await _currentTx.DisposeAsync();
                _currentTx = null;
            }
            await _context.DisposeAsync();
        }
    }
}
