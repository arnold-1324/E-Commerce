using Xunit;
using SearchService.Models;
using SearchService.Repositories;
using SearchService.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchService.Tests
{
    public class SearchServiceTests
    {
        private readonly ISearchService _searchService;
        private readonly ISearchCache _searchCache;
        private readonly IElasticSearchRepository _esRepo;

        public SearchServiceTests()
        {
            _esRepo = new InMemorySearchRepository();
            _searchCache = new InMemorySearchCache();
            _searchService = new Services.SearchService(_esRepo, _searchCache, null);
        }

        [Fact]
        public async Task IndexAndSearch_Product_Works()
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid().ToString(),
                Name = "Test Product",
                Description = "A test product",
                Price = 10.0,
                Category = "Test",
                Subcategory = "TestSub",
                Attributes = new Dictionary<string, string> { { "Color", "Blue" } },
                Stock = 5,
                Brand = "TestBrand",
                Rating = 4.0,
                Tags = new List<string> { "Test", "Blue" },
                RelatedProducts = new List<string>(),
                ImageUrl = ""
            };
            await _esRepo.IndexAsync(product);
            var result = await _searchService.SearchAsync("Test Product", 1, 10);
            Assert.NotNull(result);
            Assert.Contains(result.Items, p => p.Name == "Test Product");
        }

        [Fact]
        public async Task Cache_Works()
        {
            var product = new Product { ProductId = "cache1", Name = "Cache Test" };
            await _searchCache.SetProductAsync(product);
            var cached = await _searchCache.GetProductAsync("cache1");
            Assert.NotNull(cached);
            Assert.Equal("Cache Test", cached.Name);
        }

        [Fact]
        public async Task Autocomplete_Works()
        {
            var trie = new TrieAutocompleteService();
            trie.Insert("Nike Air");
            trie.Insert("Nike Zoom");
            var results = trie.GetSuggestions("Nike");
            Assert.Contains("Nike Air", results);
            Assert.Contains("Nike Zoom", results);
        }
    }
}
