using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace SearchService.Services
{
    public class RedisSearchCache : ISearchCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisSearchCache> _logger;
        private IDatabase _db;

        public RedisSearchCache(IConnectionMultiplexer redis, ILogger<RedisSearchCache> logger)
        {
            _redis = redis;
            _logger = logger;
            _db = redis.GetDatabase();
            
            // Setup event handlers for connection issues
            _redis.ConnectionFailed += (sender, args) => 
                _logger.LogWarning("Redis connection failed: {FailureType}", args.FailureType);
            
            _redis.ConnectionRestored += (sender, args) => 
                _logger.LogInformation("Redis connection restored");
        }

        public async Task<string?> GetCachedResultAsync(string query)
        {
            try
            {
                return await _db.StringGetAsync(query);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error during GET for {Query}", query);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Redis error during GET for {Query}", query);
                return null;
            }
        }

        public async Task SetCachedResultAsync(string query, string resultJson, TimeSpan expiration)
        {
            try
            {
                await _db.StringSetAsync(query, resultJson, expiration);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error during SET for {Query}", query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Redis error during SET for {Query}", query);
            }
        }
    }
}