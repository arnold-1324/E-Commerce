using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Repositories
{
    public interface ISearchRepository
    {

        Task AddOrUpdateAsync(Product product);
        Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<string> ids);
        Task<int> CountAsync();
    }
}