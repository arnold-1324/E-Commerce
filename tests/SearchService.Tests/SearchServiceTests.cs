using Xunit;
using Moq;
using SearchService.Services;
using SearchService.Repositories;
using SearchService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SearchService.Tests
{
    public class SearchServiceTests
    {
        [Fact]
        public async Task IndexAndSearch_ReturnsPagedResults()
        {
            var repoMock = new Mock<ISearchRepository>();
            repoMock.Setup(x => x.AddOrUpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            repoMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
                    .ReturnsAsync((IEnumerable<string> ids) => ids.Select(id => new Product(id, id, "", 0m, new List<string>())).ToList().AsReadOnly());

            var service = new SearchService.Services.SearchService(repoMock.Object);
            await service.IndexAsync(new Product("1", "Alpha", "", 0m, new List<string>()));
            await service.IndexAsync(new Product("2", "Beta", "", 0m, new List<string>()));

            var result = await service.SearchAsync("A", 1, 1);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal("Alpha", result.Items[0].Name);
        }
    }
}
