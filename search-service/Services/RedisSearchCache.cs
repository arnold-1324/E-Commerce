using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace SearchService.Services
{
    public class RedisSearchCache : ISearchCache
    {
        private readonly IDatabase _db;

        public RedisSearchCache(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<string?> GetCachedResultAsync(string query)
        {
            return await _db.StringGetAsync(query);
        }

        public async Task SetCachedResultAsync(string query, string resultJson, TimeSpan expiration)
        {
            await _db.StringSetAsync(query, resultJson, expiration);
        }
    }
}
