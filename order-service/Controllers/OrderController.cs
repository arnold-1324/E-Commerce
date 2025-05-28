using Microsoft.AspNetCore.Mvc;

namespace Order.Controllers
{
    [ApiController]
    [Route("api/order/health")]
    public class OrderController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("order service is healthy 🚀");
    }
}
