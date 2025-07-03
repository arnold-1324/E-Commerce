using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using SearchService.Models;
using Microsoft.Extensions.Logging;

namespace SearchService.Repositories
{
    public class ElasticSearchRepository : IElasticSearchRepository
    {
        private const string INDEX = "products";
        private readonly IElasticClient _client;
        private readonly ILogger<ElasticSearchRepository> _logger;

        public ElasticSearchRepository(IElasticClient client, ILogger<ElasticSearchRepository> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task IndexAsync(Product product)
        {
            var resp = await _client.IndexAsync(product, i => i
                .Index(INDEX)
                .Id(product.ProductId));
            if (!resp.IsValid)
                throw new Exception(resp.ServerError?.ToString() 
                    ?? "Elasticsearch indexing error");
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(string query, int page, int size)
        {
            var resp = await _client.SearchAsync<Product>(s => s
                .Index(INDEX)
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(p => p.Name)
                            .Field(p => p.Description)
                            .Field(p => p.Tags)
                            .Field(p => p.Category)
                            .Field(p => p.Subcategory)
                            .Field(p => p.Brand)
                            .Field(p => p.Attributes)
                        )
                        .Query(query)
                    )
                )
                .From((page - 1) * size)
                .Size(size)
            );
            if (!resp.IsValid)
                throw new Exception(resp.ServerError?.ToString() 
                    ?? "Elasticsearch search error");

           
            return resp.Documents.ToList();
        }
    }
}
