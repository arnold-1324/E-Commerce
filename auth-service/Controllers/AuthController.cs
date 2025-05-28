using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [ApiController]
    [Route("api/auth/health")]
    public class AuthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("auth service is healthy 🚀");
    }
}
