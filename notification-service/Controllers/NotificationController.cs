using Microsoft.AspNetCore.Mvc;

namespace Notification.Controllers
{
    [ApiController]
    [Route("api/notification/health")]
    public class NotificationController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("notification service is healthy 🚀");
    }
}
