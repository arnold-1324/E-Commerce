using Microsoft.AspNetCore.Mvc;

namespace Cart.Controllers
{
    [ApiController]
    [Route("api/cart/health")]
    public class CartController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("cart service is healthy 🚀");
    }
}
