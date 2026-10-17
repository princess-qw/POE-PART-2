
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("organiser")]
        [Authorize(Roles = "Organiser")]
        public IActionResult OrganiserOnly()
        {
            return Ok(new
            {
                message = "You are authorised as an Organiser."
            });
        }

        [HttpGet("participant")]
        [Authorize(Roles = "Participant")]
        public IActionResult ParticipantOnly()
        {
            return Ok(new
            {
                message = "You are authorised as a Participant."
            });
        }
    }
}
