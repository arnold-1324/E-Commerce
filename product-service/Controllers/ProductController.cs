using Microsoft.AspNetCore.Mvc;
using product_service.Utilits;
using ProductService.Models;
using ProductService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;            // your Mongo-backed service
        private readonly KafkaProducerService _producer;            // your Kafka producer (for events)

        // these are initialized in constructor synchronously (so DI + startup ordering is safe)
        private FenwickTree _fenwickTree;
        private RestockHeap _restockHeap;
        private Dictionary<string, int> _skuToIndex;
        private List<string> _indexToSku;

        // lock for concurrent updates (simple approach)
        private readonly object _sync = new();

        public ProductController(IProductService productService, KafkaProducerService producer)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));

            // synchronous initialization on startup (blocks until initial load complete).
            // For production, move to IHostedService if you want async non-blocking startup.
            var allProducts = _productService.GetAsync().GetAwaiter().GetResult() ?? new List<Product>();

            _skuToIndex = new Dictionary<string, int>();
            _indexToSku = new List<string>();

            int count = Math.Max(1, allProducts.Count);
            _fenwickTree = new FenwickTree(count);
            _restockHeap = new RestockHeap();

            int idx = 0;
            foreach (var p in allProducts)
            {
                var sku = p?.ProductId ?? string.Empty;
                if (string.IsNullOrEmpty(sku))
                {
                    idx++;
                    continue;
                }

                _skuToIndex[sku] = idx;
                _indexToSku.Add(sku);
                // adding stock as delta from 0
                try { _fenwickTree.Add(idx, p.Stock); } catch { /* fallback if out of range */ }
                _restockHeap.AddOrUpdate(sku, p.Stock);
                idx++;
            }
        }

        // Health
        [HttpGet("health")]
        public IActionResult Health() => Ok("product service is healthy 🚀");

        // Get all products (existing)
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // Get single product by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null) return NotFound($"Product {id} not found");
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Create product (existing)
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (product == null) return BadRequest("product body required");
            await _productService.CreateAsync(product);
            await _producer.ProduceProductEventAsync("ProductCreated", product);

            // update in-memory structures (attempt)
            lock (_sync)
            {
                if (!_skuToIndex.ContainsKey(product.ProductId))
                {
                    int newIdx = _indexToSku.Count;
                    _skuToIndex[product.ProductId] = newIdx;
                    _indexToSku.Add(product.ProductId);
                    try { _fenwickTree.Add(newIdx, product.Stock); } catch { /* ignore */ }
                    _restockHeap.AddOrUpdate(product.ProductId, product.Stock);
                }
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
        }

        // Update product (existing)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product updatedProduct)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            updatedProduct.ProductId = id;
            await _productService.UpdateAsync(id, updatedProduct);
            await _producer.ProduceProductEventAsync("ProductUpdated", updatedProduct);

            // update in-memory structures
            lock (_sync)
            {
                if (_skuToIndex.TryGetValue(id, out int idx))
                {
                    int current = _fenwickTree.RangeQuery(idx, idx);
                    int delta = updatedProduct.Stock - current;
                    if (delta != 0) _fenwickTree.Add(idx, delta);
                    _restockHeap.AddOrUpdate(id, updatedProduct.Stock);
                }
                else
                {
                    // If not present, add as new index (best-effort)
                    int newIdx = _indexToSku.Count;
                    _skuToIndex[id] = newIdx;
                    _indexToSku.Add(id);
                    try { _fenwickTree.Add(newIdx, updatedProduct.Stock); } catch { }
                    _restockHeap.AddOrUpdate(id, updatedProduct.Stock);
                }
            }

            return Ok(updatedProduct);
        }

        // Delete product (existing)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _producer.ProduceProductEventAsync("ProductDeleted", new { ProductId = id });
            await _productService.DeleteAsync(id);

            lock (_sync)
            {
                // mark stock 0 in fenwick and update heap
                if (_skuToIndex.TryGetValue(id, out int idx))
                {
                    int current = _fenwickTree.RangeQuery(idx, idx);
                    if (current != 0) _fenwickTree.Add(idx, -current);
                    _restockHeap.AddOrUpdate(id, 0);
                }
            }

            return NoContent();
        }

        // GET single SKU stock (Fenwick backed)
        [HttpGet("{sku}/stock")]
        public IActionResult GetStock(string sku)
        {
            if (string.IsNullOrEmpty(sku)) return BadRequest("sku required");
            if (!_skuToIndex.ContainsKey(sku)) return NotFound();

            int idx = _skuToIndex[sku];
            int stock = _fenwickTree.RangeQuery(idx, idx);
            return Ok(new { SKU = sku, Stock = stock });
        }

        // GET range stock
        [HttpGet("stock-range")]
        public IActionResult GetStockRange([FromQuery] string start, [FromQuery] string end)
        {
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end)) return BadRequest("start & end required");
            if (!_skuToIndex.ContainsKey(start) || !_skuToIndex.ContainsKey(end)) return NotFound();

            int left = _skuToIndex[start];
            int right = _skuToIndex[end];
            if (left > right) (left, right) = (right, left);

            int total = _fenwickTree.RangeQuery(left, right);
            return Ok(new { StartSKU = start, EndSKU = end, TotalStock = total });
        }

        // POST update-stock (DB + Fenwick + Heap + produce event)
        [HttpPost("{sku}/update-stock")]
        public async Task<IActionResult> UpdateStock(string sku, [FromBody] int newStock)
        {
            if (string.IsNullOrEmpty(sku)) return BadRequest("sku required");
            var existing = await _productService.GetByIdAsync(sku);
            if (existing == null) return NotFound();

            // update DB
           // await _productService.UpdateStockAsync(sku, newStock);

            // update in-memory structures
            lock (_sync)
            {
                if (_skuToIndex.TryGetValue(sku, out int idx))
                {
                    int cur = _fenwickTree.RangeQuery(idx, idx);
                    int delta = newStock - cur;
                    if (delta != 0) _fenwickTree.Add(idx, delta);
                    _restockHeap.AddOrUpdate(sku, newStock);
                }
                else
                {
                    // add new index if not present
                    int newIdx = _indexToSku.Count;
                    _skuToIndex[sku] = newIdx;
                    _indexToSku.Add(sku);
                    try { _fenwickTree.Add(newIdx, newStock); } catch { }
                    _restockHeap.AddOrUpdate(sku, newStock);
                }
            }

            // produce stock-updated event
            await _producer.ProduceProductEventAsync("ProductStockUpdated", new { ProductId = sku, Stock = newStock });

            return Ok(new { SKU = sku, Stock = newStock });
        }

        // GET top-K lowest stock
        [HttpGet("restock/top/{k}")]
        public IActionResult GetTopKRestock(int k)
        {
            if (k <= 0) return BadRequest("k must be > 0");

            lock (_sync)
            {
                var temp = new RestockHeap();
                foreach (var it in _restockHeap.Heap)
                    temp.AddOrUpdate(it.SKU, it.Stock);

                var result = new List<RestockItem>();
                for (int i = 0; i < k; i++)
                {
                    var item = temp.PopLowestStock();
                    if (item == null) break;
                    result.Add(item);
                }
                return Ok(result);
            }
        }

       
    }
}
