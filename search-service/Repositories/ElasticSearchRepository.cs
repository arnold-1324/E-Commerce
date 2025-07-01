using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchService.Models;
using Nest;

namespace SearchService.Repositories
{
    public class ElasticSearchRepository : IElasticSearchRepository
    {
        private readonly IElasticClient _client;
        private const string IndexName = "products";

        public ElasticSearchRepository(IElasticClient client)
        {
            _client = client;
        }

        public async Task IndexAsync(Product product)
        {
            await _client.IndexDocumentAsync(product);
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(string query, int page, int size)
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                        .Field("name")
                        .Field("description")
                        .Field("tags")
                        .Field("category")
                        .Field("subcategory")
                        .Field("brand")
                        .Field("attributes")
                    )
                        .Query(query)
                    )
                )
                .From((page - 1) * size)
                .Size(size)
            );
            return response.Documents.ToList();
        }
    }
}
