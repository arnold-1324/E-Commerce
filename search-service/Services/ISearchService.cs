using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Services
{
    public interface ISearchService
    {
        Task IndexAsync(Product product);
        Task<List<Product>> GetAllProductsAsync();
        Task<SearchResult> SearchAsync(string query, int page, int size);
        
        Task<SearchResult> SmartSearchAsync(string query, int page, int size, List<string>? filterIds = null);

    }
}
