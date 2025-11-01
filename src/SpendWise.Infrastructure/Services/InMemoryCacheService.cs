using SpendWise.Application.Services;
using System.Collections.Concurrent;

namespace SpendWise.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var item = new CacheItem
        {
            Value = value,
            ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : DateTime.UtcNow.AddHours(1)
        };
        
        _cache[key] = item;
        
        // Clean expired items periodically
        CleanExpiredItems();
        
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var item))
        {
            if (DateTime.UtcNow > item.ExpiresAt)
            {
                _cache.TryRemove(key, out _);
                return Task.FromResult<T?>(null);
            }
            
            return Task.FromResult(item.Value as T);
        }
        
        return Task.FromResult<T?>(null);
    }

    public Task RemoveAsync(string key)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            if (DateTime.UtcNow > item.ExpiresAt)
            {
                _cache.TryRemove(key, out _);
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    private void CleanExpiredItems()
    {
        var expiredKeys = _cache
            .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiresAt)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }

    private class CacheItem
    {
        public object? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
