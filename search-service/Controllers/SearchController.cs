using Microsoft.AspNetCore.Mvc;
using SearchService.Models;
using SearchService.Services;
using System.Text.Json;
namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ISearchCache _searchCache;
        public SearchController(ISearchService searchService,ISearchCache searchCache)
        {
            _searchService = searchService;
            _searchCache = searchCache;
        }

        [HttpGet("health")]
        public IActionResult Health() => Ok("Search service is healthy 🚀");


        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] Product product)
        {
            if (product is null || string.IsNullOrWhiteSpace(product.Id))
                return BadRequest("Product or Product.Id cannot be null.");

            await _searchService.IndexAsync(product);
            return Ok(new { Message = "Product indexed", ProductId = product.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Search(
     [FromQuery] string q,
     [FromQuery] int page = 1,
     [FromQuery] int size = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query parameter 'q' is required.");

            if (page < 1 || size < 1)
                return BadRequest("'page' and 'size' must be >= 1.");

            var cached = await _searchCache.GetCachedResultAsync(q);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                try
                {
                    var result = JsonSerializer.Deserialize<List<SearchItem>>(cached);
                    if (result != null)
                        return Ok(result);
                }
                catch (JsonException ex)
                {
                    // Optionally log: corrupted cache
                }
            }

            var searchResult = await _searchService.SearchAsync(q, page, size);
            //var searchItems = searchResult.Items.Select(p => new SearchItem
            //{
            //    Title = p.Name,
            //    Url = $"/products/{p.Id}"
            //}).ToList();

            var json = JsonSerializer.Serialize(searchResult);
            await _searchCache.SetCachedResultAsync(q, json, TimeSpan.FromMinutes(10));

            return Ok(searchResult);
        }
    }
}