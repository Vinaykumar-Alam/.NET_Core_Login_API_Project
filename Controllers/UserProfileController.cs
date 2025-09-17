using Flutter_Backed.Models;
using Microsoft.AspNetCore.Mvc;
using Flutter_Backed.Models;

namespace YourApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly UserProfile _sampleProfile = new UserProfile
        {
            Id = 1,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Role = "Employee"
        };

        [HttpGet]
        [Route("[action]")]
        public ActionResult<UserProfile> GetUserProfile()
        {
            return Ok(_sampleProfile);
        }
    }
}