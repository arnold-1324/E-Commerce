using System.Collections.Generic;
using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Repositories
{
    public interface IElasticSearchRepository
    {
        Task IndexAsync(Product product);
        Task<IReadOnlyList<Product>> SearchAsync(string query, int page, int size);
    }
}
