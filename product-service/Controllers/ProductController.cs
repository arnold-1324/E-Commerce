using Microsoft.AspNetCore.Mvc;

namespace Product.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult Get() => Ok("product service is healthy 🚀");


        [HttpGet]
        public IActionResult GetAllProducts()
        { 
         

        }

    }


   
}
