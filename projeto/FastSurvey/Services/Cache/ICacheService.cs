#nullable enable
using Microsoft.Extensions.Caching.Memory;

namespace FASTSURVEY.Services.Cache
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CacheItemPriority priority = CacheItemPriority.Normal,
            long? size = null,
            CancellationToken ct = default
        );
        Task RemoveAsync(string key, CancellationToken ct = default);
        Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);
        Task<bool> ExistsAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// Obtém a chave do cache ou avalia a factory de forma coalescida (evita chamadas duplicadas concorrentes).
        /// </summary>
        Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CacheItemPriority priority = CacheItemPriority.Normal,
            long? size = null,
            CancellationToken ct = default
        );
    }

    public sealed class CacheOptions
    {
        public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan ShortExpiration { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan LongExpiration { get; set; } = TimeSpan.FromHours(2);
    }
}
