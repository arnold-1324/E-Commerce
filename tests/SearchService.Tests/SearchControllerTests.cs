using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using SearchService;

namespace SearchService.Tests
{
    public class SearchControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SearchControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData("/?q=alpha&page=1&size=10", 200)]
        [InlineData("/?q=&page=0&size=0", 400)]
        public async Task SearchEndpoint_ReturnsExpectedStatus(string url, int expectedStatus)
        {
            var response = await _client.GetAsync($"/api/search{url}");
            Assert.Equal(expectedStatus, (int)response.StatusCode);
        }
    }
}