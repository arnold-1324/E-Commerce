using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using SearchService.Models;
using SearchService.Services;
using Microsoft.Extensions.Logging;

namespace SearchService.Repositories
{
    public class ElasticSearchRepository : IElasticSearchRepository
    {
        private const string INDEX = "products";
        private readonly IElasticClient _client;

        private readonly ISearchCache _searchCache;
        private readonly ILogger<ElasticSearchRepository> _logger;

        public ElasticSearchRepository(IElasticClient client, ILogger<ElasticSearchRepository> logger, ISearchCache searchCache)
        {
            _client = client;
            _logger = logger;
            _searchCache = searchCache;
        }

        public async Task IndexAsync(Product product)
        {
            var resp = await _client.IndexAsync(product, i => i
                .Index(INDEX)
                .Id(product.ProductId));
            if (!resp.IsValid)
                throw new Exception(resp.ServerError?.ToString()
                    ?? "Elasticsearch indexing error");


            await _searchCache.SetProductInSkuLookupAsync(product);
            _logger.LogInformation("✅ Indexed and cached SKU lookup for ProductId: {ProductId}", product.ProductId);
        }

        public async Task<SearchResult> SmartSearchAsync(string query, int page, int size, List<string>? filterIds = null)
        {
            var searchDescriptor = new SearchDescriptor<Product>()
                .Index("products")
                .From((page - 1) * size)
                .Size(size);

            // 1. Query
            if (!string.IsNullOrWhiteSpace(query))
            {
                searchDescriptor = searchDescriptor.Query(q => q
                    .Bool(b => b
                        .Must(mu => mu
                            .MultiMatch(m => m
                                .Fields(f => f
                                    .Field(p => p.Name)
                                    .Field(p => p.Description)
                                    .Field(p => p.Tags)
                                )
                                .Query(query)
                            )
                        )
                    )
                );
            }

            // 2. Filter by ProductId (from price index)
            if (filterIds != null && filterIds.Any())
            {
                searchDescriptor = searchDescriptor.PostFilter(f => f
                    .Ids(i => i.Values(filterIds))
                );
            }

            var response = await _client.SearchAsync<Product>(searchDescriptor);

            if (!response.IsValid)
                throw new Exception(response.ServerError?.ToString() ?? "ElasticSearch error");
            
            return new SearchResult
            {
                TotalCount = (int)response.Total,
                Page = page,
                Size = size,
                Items = response.Documents.ToList()
            };
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
