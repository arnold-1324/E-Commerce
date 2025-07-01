using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Services
{
    public class RedisSearchCache : ISearchCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisSearchCache> _logger;
        private readonly IDatabase _db;
        private readonly TimeSpan _defaultExpiry;

        public RedisSearchCache(
            IConnectionMultiplexer redis, 
            ILogger<RedisSearchCache> logger,
            IConfiguration configuration)
        {
            _redis = redis;
            _logger = logger;
            _db = redis.GetDatabase();
            _defaultExpiry = TimeSpan.FromMinutes(configuration.GetValue("Redis:CacheExpiryMinutes", 30));
            SetupConnectionHandlers();
        }

        private void SetupConnectionHandlers()
        {
            _redis.ConnectionFailed += (_, e) => 
                _logger.LogWarning("Redis connection failed: {FailureType}", e.FailureType);
            
            _redis.ConnectionRestored += (_, e) => 
                _logger.LogInformation("Redis connection restored");
        }

        /* Product Cache Operations */
        public async Task SetProductAsync(Product product, TimeSpan? expiry = null)
        {
            await SetAsync($"product:{product.ProductId}", product, expiry);
        }

        public async Task<Product?> GetProductAsync(string productId)
        {
            var key = $"product:{productId}";
            try
            {
                var json = await _db.StringGetAsync(key);
                return json.HasValue 
                    ? JsonSerializer.Deserialize<Product>(json!) 
                    : null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize product {ProductId}", productId);
                await _db.KeyDeleteAsync(key);
                return null;
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error getting product {ProductId}", productId);
                return null;
            }
        }

        public async Task RemoveProductAsync(string productId)
        {
            try
            {
                await RemoveAsync($"product:{productId}");
                _logger.LogInformation("Removed product {ProductId} from Redis cache", productId);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error removing product {ProductId}", productId);
            }
        }

        /* Search Query Cache Operations */
        public async Task<string?> GetCachedResultAsync(string query)
        {
            try
            {
                return await _db.StringGetAsync($"search:{query}");
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error getting query {Query}", query);
                return null;
            }
        }

        public async Task SetCachedResultAsync(string query, string resultJson, TimeSpan? expiry = null)
        {
            try
            {
                await _db.StringSetAsync(
                    $"search:{query}", 
                    resultJson, 
                    expiry ?? _defaultExpiry);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error caching query {Query}", query);
            }
        }

        
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                return await _db.PingAsync() != TimeSpan.Zero;
            }
            catch
            {
                return false;
            }
        }

        public async Task ClearAllAsync()
        {
            try
            {
                var endpoints = _redis.GetEndPoints();
                var server = _redis.GetServer(endpoints[0]);
                await server.FlushDatabaseAsync();
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Failed to clear Redis cache");
                throw;
            }
        }

        
        private async Task SetAsync(string key, object value, TimeSpan? expiry = null)
        {
            try
            {
                await _db.StringSetAsync(
                    key, 
                    JsonSerializer.Serialize(value), 
                    expiry ?? _defaultExpiry);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error setting {Key}", key);
                throw;
            }
        }

        public async Task RemoveCachedSearchResultsContainingProductAsync(string productId)
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints[0]);
            foreach (var key in server.Keys(pattern: "search:*"))
            {
                var json = await _db.StringGetAsync(key);
                if (json.HasValue && json.ToString().Contains(productId))
                {
                    await _db.KeyDeleteAsync(key);
                    _logger.LogInformation("Removed cached search result key {Key} containing deleted product {ProductId}", key, productId);
                }
            }
        }


        private async Task RemoveAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error removing {Key}", key);
                throw;
            }
        }
    }
}