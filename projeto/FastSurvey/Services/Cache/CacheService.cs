#nullable enable
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FASTSURVEY.Services.Cache
{
    public sealed class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _options;

        // Índice de chaves para suportar RemoveByPattern
        private readonly ConcurrentDictionary<string, byte> _keys = new();

        // Coalescência de chamadas concorrentes por chave
        private readonly ConcurrentDictionary<string, Lazy<Task<object?>>> _inflight = new();

        public CacheService(IMemoryCache memoryCache, IOptions<CacheOptions> options)
        {
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult<T?>(default);

            if (ct.IsCancellationRequested)
                return Task.FromCanceled<T?>(ct);

            var ok = _memoryCache.TryGetValue(key, out var obj);
            return Task.FromResult(ok ? (T?)obj : default);
        }

        public Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CacheItemPriority priority = CacheItemPriority.Normal,
            long? size = null,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(key) || value is null)
                return Task.CompletedTask;

            if (ct.IsCancellationRequested)
                return Task.FromCanceled(ct);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    absoluteExpiration ?? _options.DefaultAbsoluteExpiration,
                SlidingExpiration = slidingExpiration ?? _options.DefaultSlidingExpiration,
                Priority = priority,
            };

            if (size.HasValue)
                options.Size = size.Value;

            options.RegisterPostEvictionCallback(
                (evictedKey, _, _, _) =>
                {
                    if (evictedKey is string s)
                        _keys.TryRemove(s, out _);
                }
            );

            _memoryCache.Set(key, value!, options);
            _keys[key] = 0;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.CompletedTask;

            if (ct.IsCancellationRequested)
                return Task.FromCanceled(ct);

            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return Task.CompletedTask;

            if (ct.IsCancellationRequested)
                return Task.FromCanceled(ct);

            var rx = new Regex(pattern, RegexOptions.Compiled);

            foreach (var k in _keys.Keys)
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled(ct);

                if (rx.IsMatch(k))
                {
                    _memoryCache.Remove(k);
                    _keys.TryRemove(k, out _);
                }
            }
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult(false);

            if (ct.IsCancellationRequested)
                return Task.FromCanceled<bool>(ct);

            var exists = _memoryCache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }

        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CacheItemPriority priority = CacheItemPriority.Normal,
            long? size = null,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // 1) Tenta pegar
            if (_memoryCache.TryGetValue(key, out var cachedObj) && cachedObj is T cachedT)
                return cachedT;

            // 2) Coalescer concorrência por chave
            var lazy = _inflight.GetOrAdd(
                key,
                _ => new Lazy<Task<object?>>(
                    async () =>
                    {
                        var value = await factory(ct).ConfigureAwait(false);
                        // Grava no cache
                        await SetAsync(
                                key,
                                value,
                                absoluteExpiration,
                                slidingExpiration,
                                priority,
                                size,
                                ct
                            )
                            .ConfigureAwait(false);
                        return (object?)value;
                    },
                    isThreadSafe: true
                )
            );

            try
            {
                var obj = await (lazy.Value ?? Task.FromResult<object?>(default)).ConfigureAwait(
                    false
                );
                return obj is T v ? v : default!;
            }
            finally
            {
                // Remove controle de voo para a chave atual
                _inflight.TryRemove(key, out _);
            }
        }
    }
}
