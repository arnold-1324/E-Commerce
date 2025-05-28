using Microsoft.AspNetCore.Mvc;

namespace User.Controllers
{
    [ApiController]
    [Route("api/user/health")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("user service is healthy 🚀");
    }
}
