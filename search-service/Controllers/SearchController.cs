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

        private readonly IProductIndexService _productIndexService;
        private readonly ILogger<SearchController> _logger;
        private readonly TrieAutocompleteService _autocompleteService;

        public SearchController(ISearchService searchService, ISearchCache searchCache, IProductIndexService priceIndexService, ILogger<SearchController> logger, TrieAutocompleteService autocompleteService)
        {
            _searchService = searchService;
            _searchCache = searchCache;
            _logger = logger;
            _autocompleteService = autocompleteService;
            _productIndexService = priceIndexService;
        }

        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] Product product)
        {
            if (product is null || string.IsNullOrWhiteSpace(product.ProductId))
                return BadRequest("Product or Product.ProductId cannot be null.");

            await _searchService.IndexAsync(product);
            _autocompleteService.Insert(product.Name);
            // Add product to price index for price range filtering
            _productIndexService.AddProduct(product.Price, product.ProductId);
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


        [HttpGet("smart-search")]
        public async Task<IActionResult> SmartSearch(
    [FromQuery] string q,
    [FromQuery] int page = 1,
    [FromQuery] int size = 20,
    [FromQuery] double? minPrice = null,
    [FromQuery] double? maxPrice = null
)
        {
            // Step 1: Filter product IDs based on price range
            List<string>? priceFilteredIds = null;

            if (minPrice.HasValue && maxPrice.HasValue)
            {
                priceFilteredIds = _productIndexService.GetProductIdsInPriceRange(minPrice.Value, maxPrice.Value);
                _logger.LogInformation("Filtered {Count} product IDs in price range [{MinPrice}, {MaxPrice}]", priceFilteredIds.Count, minPrice, maxPrice);
                if (priceFilteredIds == null || !priceFilteredIds.Any())
                    return Ok(new SearchResult { TotalCount = 0, Page = page, Size = size, Items = new List<Product>() });
            }

            // Step 2: Perform search using search service
            var results = await _searchService.SmartSearchAsync(q, page, size, priceFilteredIds);

            return Ok(results);
        }


        [HttpGet("health")]
        public IActionResult Health() => Ok("Search service is healthy 🚀");

        [HttpGet("autocomplete")]
        public IActionResult GetAutocompleteSuggestions([FromQuery] string prefix)
        {
            // Handle empty/whitespace prefix
            if (string.IsNullOrWhiteSpace(prefix))
            {
                _logger.LogInformation("Empty prefix request - returning default suggestions");
                var defaults = _autocompleteService.GetDefaultSuggestions();
                return Ok(defaults != null ? defaults : new List<string>());
            }

            // Process non-empty prefix
            var cleanPrefix = prefix.Trim().ToLowerInvariant();
            _logger.LogInformation("Processing prefix: '{Prefix}'", cleanPrefix);

            var results = _autocompleteService.GetSuggestions(cleanPrefix);
            return Ok(results != null ? results : new List<string>());
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
            if (!Guid.TryParse(productId, out _))
            {
                return BadRequest(Guid.Empty.ToString());
            }
            var product = await _searchCache.GetProductFromSkuLookupAsync(productId);
            if (product == null)
                return NotFound($"Product with ID {productId} not found");

            return Ok(product);
        }
    }
}