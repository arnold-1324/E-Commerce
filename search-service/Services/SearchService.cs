using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchService.Repositories;
using SearchService.Models;
using SearchService.Utils;

namespace SearchService.Services
{
    public class SearchService : ISearchService
    {
        private readonly IElasticSearchRepository _repo;

        public SearchService(IElasticSearchRepository repo)
        {
            _repo = repo;
        }

        public async Task IndexAsync(Product product)
        {
            await _repo.IndexAsync(product);
        }

        public async Task<SearchResult> SearchAsync(string query, int page, int size)
        {
            var products = await _repo.SearchAsync(query, page, size);
            return new SearchResult
            {
                TotalCount = products.Count,
                Page = page,
                Size = size,
                Items = products
            };
        }
    }
}
