using Microsoft.AspNetCore.Mvc;
using SearchService.Models;
using SearchService.Services;
using System.Text.Json;
namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ISearchCache _searchCache;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ISearchCache searchCache, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _searchCache = searchCache;
            _logger = logger;
        }

        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] Product product)
        {
            if (product is null || string.IsNullOrWhiteSpace(product.Id))
                return BadRequest("Product or Product.Id cannot be null.");

            await _searchService.IndexAsync(product);
            return Ok(new { Message = "Product indexed", ProductId = product.Id });
        }

        [HttpGet("query")]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20)
        {
            // Try cache first, then fallback to Elasticsearch
            var cached = await _searchCache.GetCachedResultAsync(q);
            if (cached != null)
            {
                var deserialized = JsonSerializer.Deserialize<SearchResult>(cached);
                return Ok(deserialized);
            }
            var elasticResult = await _searchService.SearchAsync(q, page, size);
            var json = JsonSerializer.Serialize(elasticResult);
            await _searchCache.SetCachedResultAsync(q, json, TimeSpan.FromMinutes(10));
            return Ok(elasticResult);
        }

        [HttpGet("health")]
        public IActionResult Health() => Ok("Search service is healthy 🚀");

        
    }
}