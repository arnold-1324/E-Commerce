using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using SearchService.Services;
using SearchService.Models;
using SearchService.Repositories;

namespace SearchService.Tests
{
    public class SearchServiceManualTest
    {
        private readonly ITestOutputHelper _output;

        public SearchServiceManualTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task DebugIndexAndSearch()
        {
            // Setup in-memory repository and service
            var repo = new InMemorySearchRepository();
            var service = new SearchService.Services.SearchService(repo);

            // Index sample products
            var products = new List<Product>
            {
                new Product("1", "Alpha", "Desc A", 10m, new List<string>{"tag1"}),
                new Product("2", "Beta",  "Desc B", 20m, new List<string>{"tag2"}),
                new Product("3", "Gamma", "Desc G", 30m, new List<string>{"tag3"})
            };

            foreach (var p in products)
            {
                _output.WriteLine($"Indexing Product: Id={p.Id}, Name={p.Name}");
                await service.IndexAsync(p);
            }

            // Debug internal trie state (via search of empty prefix)
            var allResult = await service.SearchAsync("", 1, 10);
            _output.WriteLine($"Search all - TotalCount={allResult.TotalCount}");
            foreach (var item in allResult.Items)
            {
                _output.WriteLine($"Found: Id={item.Id}, Name={item.Name}");
            }

            // Search for partial prefix
            string prefix = "B";
            var prefixResult = await service.SearchAsync(prefix, 1, 10);
            _output.WriteLine($"Search prefix '{prefix}' - TotalCount={prefixResult.TotalCount}");
            foreach (var item in prefixResult.Items)
            {
                _output.WriteLine($"Found: Id={item.Id}, Name={item.Name}");
            }

            // Assertions to ensure expected behavior
            Assert.Equal(3, allResult.TotalCount);
            Assert.Contains(allResult.Items, prod => prod.Name == "Beta");
            Assert.Equal(1, prefixResult.TotalCount);
            Assert.Single(prefixResult.Items);
            Assert.Equal("Beta", prefixResult.Items.First().Name);
        }
    }
}
