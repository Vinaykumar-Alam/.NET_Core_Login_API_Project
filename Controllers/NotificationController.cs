using Flutter_Backed.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace YourApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly List<Notification> _sampleNotifications = new List<Notification>
        {
            new Notification { Id = 1, Message = "Your ride to Airport is confirmed!", Time = DateTime.Now.AddHours(-2), IsRead = false },
            new Notification { Id = 2, Message = "Payment for Client Office ride is pending.", Time = DateTime.Now.AddHours(-1), IsRead = true }
        };

        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]
        [Route("[action]")]
        public ActionResult<IEnumerable<Notification>> GetNotifications()
        {
            return Ok(_sampleNotifications);
        }
    }
}