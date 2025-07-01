using System;
using System.Threading.Tasks;
using SearchService.Models; 

namespace SearchService.Services
{
    public interface ISearchCache
    {
        Task SetProductAsync(Product product, TimeSpan? expiry = null);
        Task<Product?> GetProductAsync(string productId);
        Task RemoveCachedSearchResultsContainingProductAsync(string productId);

        Task<string?> GetCachedResultAsync(string query);
        Task SetCachedResultAsync(string query, string resultJson, TimeSpan? expiry = null);

        Task<bool> IsHealthyAsync();
        Task ClearAllAsync();
    }
}