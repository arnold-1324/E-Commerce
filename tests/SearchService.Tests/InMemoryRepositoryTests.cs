using Xunit;
using SearchService.Repositories;
using SearchService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchService.Tests
{
    public class InMemorySearchRepositoryTests
    {
        [Fact]
        public async Task AddOrUpdate_StoresProduct()
        {
            var repo = new InMemorySearchRepository();
            var product = new Product("1", "Test", "Desc", 10m, new List<string>());
            await repo.AddOrUpdateAsync(product);
            var items = await repo.GetByIdsAsync(new[] { "1" });
            Assert.Single(items);
            Assert.Equal(product, items[0]);
        }

        [Fact]
        public async Task GetByIds_MultipleIds_ReturnsCorrectList()
        {
            var repo = new InMemorySearchRepository();
            var p1 = new Product("1", "A", "", 1m, new List<string>());
            var p2 = new Product("2", "B", "", 2m, new List<string>());
            await repo.AddOrUpdateAsync(p1);
            await repo.AddOrUpdateAsync(p2);

            var items = await repo.GetByIdsAsync(new[] { "1", "2", "3" });
            Assert.Equal(2, items.Count);
        }
    }
}