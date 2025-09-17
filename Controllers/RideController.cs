using Flutter_Backed.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Flutter_Backed.Models;

namespace YourApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RidesController : ControllerBase
    {
        private readonly List<Ride> _sampleRides = new List<Ride>
        {
            new Ride { Id = 1, Title = "Airport", Time = "Today, 5:30 PM", Status = "Confirmed", Color = "#4CAF50" },
            new Ride { Id = 2, Title = "CFO Office", Time = "Tomorrow, 10:00 AM", Status = "Pending", Color = "#FF9800" },
            new Ride { Id = 3, Title = "Hotel", Time = "Friday, 3:00 PM", Status = "Completed", Color = "#9C27B0" }
        };

        [HttpGet]
        [Route("[action]")]
        public ActionResult<IEnumerable<Ride>> GetRides()
        {
            return Ok(_sampleRides);
        }

        [HttpGet("{id}")]
        public ActionResult<Ride> GetRide(int id)
        {
            var ride = _sampleRides.Find(r => r.Id == id);
            if (ride == null)
            {
                return NotFound();
            }
            return Ok(ride);
        }
    }
}