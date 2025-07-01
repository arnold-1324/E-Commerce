using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchService.Repositories;
using SearchService.Models;
using SearchService.Utils;
using Nest;

namespace SearchService.Services
{
    public class SearchService : ISearchService
    {
        private readonly IElasticSearchRepository _repo;
        private readonly IElasticClient _elasticClient;

        public SearchService(IElasticSearchRepository repo, IElasticClient elasticClient)
        {
            _repo = repo;
            _elasticClient = elasticClient;
        }

        public async Task IndexAsync(Product product)
        {
            await _repo.IndexAsync(product);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var searchResponse = await _elasticClient.SearchAsync<Product>(s => s
                .Index("products")
                .Size(10000) 
                .Query(q => q.MatchAll())
            );

            return searchResponse.Documents.ToList();
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
