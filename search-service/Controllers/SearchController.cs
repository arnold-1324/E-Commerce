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
        private readonly TrieAutocompleteService _autocompleteService;

        public SearchController(ISearchService searchService, ISearchCache searchCache, ILogger<SearchController> logger, TrieAutocompleteService autocompleteService)
        {
            _searchService = searchService;
            _searchCache = searchCache;
            _logger = logger;
            _autocompleteService = autocompleteService;
        }

        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] Product product)
        {
            if (product is null || string.IsNullOrWhiteSpace(product.ProductId))
                return BadRequest("Product or Product.ProductId cannot be null.");

            await _searchService.IndexAsync(product);
            _autocompleteService.Insert(product.Name);
            return Ok(new { Message = "Product indexed", ProductId = product.ProductId });
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
            if (elasticResult.TotalCount > 0 && elasticResult.Items.Any())
            {
                var json = JsonSerializer.Serialize(elasticResult);
                await _searchCache.SetCachedResultAsync(q, json, TimeSpan.FromMinutes(10));
            }
            return Ok(elasticResult);
        }

        [HttpGet("health")]
        public IActionResult Health() => Ok("Search service is healthy 🚀");

        [HttpGet("autocomplete")]
        public IActionResult GetAutocompleteSuggestions([FromQuery] string prefix)
        {
            _logger.LogInformation("Received autocomplete request for prefix: {Prefix}", prefix.ToLowerInvariant());
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return ok([]);
            }
            var results = _autocompleteService.GetSuggestions(prefix.ToLowerInvariant());
            return Ok(results);
        }

        [HttpGet("trie-words")]
        public IActionResult GetAllTrieWords()
        {
            var words = _autocompleteService.GetAllWords();
            return Ok(words);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return BadRequest("ProductId cannot be null or empty.");
            }
            var product = await _searchCache.GetProductFromSkuLookupAsync(productId);
            if (product == null)
                return NotFound($"Product with ID {productId} not found");

            return Ok(product);
        }
    }
}