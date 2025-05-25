using BenchmarkDotNet.Attributes;
using SearchService.Models;
using SearchService.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SearchService.Tests
{
    public class SearchServiceBenchmark
    {
        private SearchService.Services.SearchService _service;

        [GlobalSetup]
        public void Setup()
        {
            var repo = new InMemorySearchRepository();
            _service = new SearchService.Services.SearchService(repo);

            for (int i = 0; i < 1000; i++)
            {
                var p = new Product(i.ToString(), "Product" + i, string.Empty, 0m, new List<string>());
                _service.IndexAsync(p).Wait();
            }
        }

        [Benchmark]
        public async Task BenchmarkSearchLatency()
        {
            await _service.SearchAsync("Prod", 1, 10);
        }
    }
}
