using Microsoft.AspNetCore.Mvc;
using ProductService.Services;
using ProductService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {

        private readonly IProductService _productService;
        private readonly KafkaProducerService _producer;
        public ProductController(IProductService productService, KafkaProducerService producer)
        {
            _productService = productService;
            _producer = producer;
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

            await _productService.UpdateAsync(id, updatedProduct);
            await _producer.ProduceProductEventAsync("ProductUpdated", updatedProduct);
            return NoContent();

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                var existing = await _productService.GetByIdAsync(id);
                if (existing == null)
                    return NotFound();
                await _producer.ProduceEventAsync("product-events", "ProductDeleted", new { ProductId = id });
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
