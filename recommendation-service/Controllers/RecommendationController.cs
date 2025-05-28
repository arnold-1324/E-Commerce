using Microsoft.AspNetCore.Mvc;

namespace Recommendation.Controllers
{
    [ApiController]
    [Route("api/recommendation/health")]
    public class RecommendationController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("recommendation service is healthy 🚀");
    }
}
