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
        private readonly ISearchRepository _repo;
        private readonly Trie _trie = new();

        public SearchService(ISearchRepository repo)
        {
            _repo = repo;
        }

        public async Task IndexAsync(Product product)
        {
            await _repo.AddOrUpdateAsync(product);
            _trie.Insert(product.Name, product.Id);
        }

        public async Task<SearchResult> SearchAsync(string query, int page, int size)
        {
            var ids = _trie.Search(query);
            var products = await _repo.GetByIdsAsync(ids);

            var sorted = products.OrderBy(p => p.Name).ToList();

            var paged = sorted
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return new SearchResult
            {
                TotalCount = sorted.Count,
                Page = page,
                Size = size,
                Items = paged.AsReadOnly()
            };
        }
    }
}
