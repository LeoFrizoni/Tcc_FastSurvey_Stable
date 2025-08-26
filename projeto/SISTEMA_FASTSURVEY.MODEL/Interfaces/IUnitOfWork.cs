#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        /// <summary>Persiste alterações pendentes no DbContext.</summary>
        Task<int> CommitAsync(CancellationToken ct = default);

        /// <summary>
        /// Inicia transação explícita (apenas se você precisa controlar manualmente).
        /// </summary>
        Task BeginTransactionAsync(CancellationToken ct = default);

        /// <summary>Confirma a transação aberta via BeginTransactionAsync.</summary>
        Task CommitTransactionAsync(CancellationToken ct = default);

        /// <summary>Desfaz a transação aberta via BeginTransactionAsync.</summary>
        Task RollbackTransactionAsync(CancellationToken ct = default);

        /// <summary>
        /// Executa uma função dentro de uma estratégia de execução resiliente (retry)
        /// e transação; ideal para operações multi-repositório.
        /// </summary>
        Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            bool useExecutionStrategy = true,
            CancellationToken ct = default
        );

        /// <summary>
        /// Versão com retorno.
        /// </summary>
        Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            bool useExecutionStrategy = true,
            CancellationToken ct = default
        );
    }
}
