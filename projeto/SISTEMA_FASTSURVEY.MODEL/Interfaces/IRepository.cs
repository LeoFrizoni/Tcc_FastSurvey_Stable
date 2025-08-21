#nullable enable
using System.Linq.Expressions;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces;

/// <summary>
/// Contrato genérico de repositório, assíncrono e cancelável.
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>Obtém uma entidade pela chave inteira (Id).</summary>
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Obtém a primeira entidade que atende ao predicado (ou null).</summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>Verifica existência de registros pelo predicado.</summary>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>Conta registros (com predicado opcional).</summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    /// <summary>Lista registros com filtro opcional e paginação.</summary>
    Task<List<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    /// <summary>Lista registros com filtro, ordenação e paginação.</summary>
    Task<List<T>> ListAsync<TOrder>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, TOrder>> orderBy,
        bool descending = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    /// <summary>Página de registros + total (útil para paginação no front).</summary>
    Task<(List<T> Items, int Total)> PageAsync<TOrder>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, TOrder>> orderBy,
        bool descending = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    /// <summary>Inclui uma entidade.</summary>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>Inclui várias entidades.</summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>Atualiza uma entidade.</summary>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>Remove uma entidade.</summary>
    Task RemoveAsync(T entity, CancellationToken ct = default);

    /// <summary>Remove várias entidades.</summary>
    Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>Persiste alterações no contexto.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
