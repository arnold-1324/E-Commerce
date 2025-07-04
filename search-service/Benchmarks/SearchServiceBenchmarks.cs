using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SearchService.Models;
using SearchService.Repositories;
using SearchService.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchService.Benchmarks
{
    [MemoryDiagnoser]
    public class SearchServiceBenchmarks
    {
        private readonly ISearchService _searchService;
        private readonly ISearchCache _searchCache;
        private readonly IElasticSearchRepository _esRepo;
        private readonly string _query = "Nike";

        public SearchServiceBenchmarks()
        {
            // Setup with in-memory or mock implementations for isolated benchmarking
            _esRepo = new InMemorySearchRepository();
            _searchCache = new InMemorySearchCache();
            _searchService = new Services.SearchService(_esRepo, _searchCache, null);

            // Seed with sample data
            for (int i = 0; i < 1000; i++)
            {
                var product = new Product
                {
                    ProductId = Guid.NewGuid().ToString(),
                    Name = $"Nike Product {i}",
                    Description = "Benchmark test product",
                    Price = 99.99,
                    Category = "Clothing",
                    Subcategory = "Sportswear",
                    Attributes = new Dictionary<string, string> { { "Color", "Red" } },
                    Stock = 10,
                    Brand = "Nike",
                    Rating = 4.5,
                    Tags = new List<string> { "Nike", "Sportswear" },
                    RelatedProducts = new List<string>(),
                    ImageUrl = ""
                };
                _esRepo.IndexAsync(product).Wait();
            }
        }

        [Benchmark]
        public async Task Search_Elasticsearch()
        {
            await _searchService.SearchAsync(_query, 1, 20);
        }

        [Benchmark]
        public async Task Search_Cache()
        {
            // Simulate cache hit
            await _searchCache.SetCachedResultAsync(_query, "{}", TimeSpan.FromMinutes(10));
            await _searchService.SearchAsync(_query, 1, 20);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SearchServiceBenchmarks>();
        }
    }
}
