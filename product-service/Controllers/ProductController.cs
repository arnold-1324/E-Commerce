using Microsoft.AspNetCore.Mvc;
using ProductService.Services;
using ProductService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using product_service.Utilits;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {

        private readonly IProductService _productService;
        private readonly KafkaProducerService _producer;
        private static FenwickTree _fenwickTree;
        private static RestockHeap _restockHeap;
        private static Dictionary<string, int> _skuToIndex;
        private static List<string> _indexToSku;


        public ProductController(IProductService productService, KafkaProducerService producer)
        {
            InitializeDSA().Wait();
            _productService = productService;
            _producer = producer;
        }

        private async Task InitializeDSA()
        {
            var allProduct = await _productService.GetAsync();
            int n = allProduct.Count;

            _fenwickTree = new FenwickTree(n);
            _restockHeap = new RestockHeap();
            _skuToIndex = new Dictionary<string, int>();
            _indexToSku = new List<string>();

            for (int i = 0; i < n; i++)
            {
                var product = allProduct[i];
                string sku = product.ProductId;
                int stock = product.Stock;

                _skuToIndex[sku] = i;
                _indexToSku.Add(sku);
                _fenwickTree.Add(i, stock);
                _restockHeap.AddOrUpdate(sku, stock);
            }
        }

        [HttpGet("health")]
        public IActionResult Get() => Ok("product service is healthy 🚀");

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
                // log ex
                return StatusCode(500, "Something went wrong."+ex);
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound($"Product {id} not found");
                return Ok(product);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateProduct(Product product)
        {
            await _productService.CreateAsync(product);
             await _producer.ProduceProductEventAsync("ProductCreated", product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, Product updatedProduct)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            updatedProduct.ProductId = id; // ensure id consistency
            await _productService.UpdateAsync(id, updatedProduct);
            await _producer.ProduceProductEventAsync("ProductUpdated", updatedProduct);
            return Ok(updatedProduct); // return updated resource

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                var existing = await _productService.GetByIdAsync(id);
                if (existing == null)
                    return NotFound();
                await _producer.ProduceProductEventAsync("ProductDeleted", new { ProductId = id });

                await _productService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                // log ex
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        } 







    }

}
