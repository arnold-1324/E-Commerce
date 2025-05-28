using Microsoft.AspNetCore.Mvc;

namespace Wishlist.Controllers
{
    [ApiController]
    [Route("api/wishlist/health")]
    public class WishlistController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("wishlist service is healthy 🚀");
    }
}
