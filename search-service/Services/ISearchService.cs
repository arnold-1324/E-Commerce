using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Services
{
    public interface ISearchService
    {
        Task IndexAsync(Product product);
        Task<SearchResult> SearchAsync(string query, int page, int size);
    }
}
