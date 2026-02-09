using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DigitalWallet.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace DigitalWallet.Application.Services
{
    public class RedisCachingService : ICachingService
    {
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _defaultExpiration;

        public RedisCachingService(IDistributedCache cache)
        {
            _cache = cache;
            _defaultExpiration = TimeSpan.FromMinutes(5);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedData = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedData))
                return default;

            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
            };

            var serializedData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedData, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            // Note: Redis pattern matching requires direct Redis connection
            // For now, we'll implement a simple approach
            // In production, use StackExchange.Redis directly for pattern operations

            // This is a simplified version - in production, you'd use SCAN command
            // with the pattern to find and delete matching keys
            await Task.CompletedTask; // Placeholder
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var value = await _cache.GetStringAsync(key);
            return !string.IsNullOrEmpty(value);
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var cachedValue = await GetAsync<T>(key);

            if (cachedValue != null)
                return cachedValue;

            var value = await factory();
            await SetAsync(key, value, expiration);

            return value;
        }
    }
}
