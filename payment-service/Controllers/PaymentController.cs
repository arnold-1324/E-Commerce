using Microsoft.AspNetCore.Mvc;

namespace Payment.Controllers
{
    [ApiController]
    [Route("api/payment/health")]
    public class PaymentController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("payment service is healthy 🚀");
    }
}
