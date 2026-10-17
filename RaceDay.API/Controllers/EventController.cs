
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                return int.TryParse(value, out var id) ? id : null;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var item = await _context.Events
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.EventId == id);

            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(
            [FromBody] EventRequest request)
        {
            if (!Valid(request))
                return BadRequest(new { message = "Invalid event details." });

            if (CurrentUserId is not int userId)
                return Unauthorized();

            var organiser = await _context.Organisers
                .SingleOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Forbid();

            var item = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Event_Name = request.EventName.Trim(),
                Event_Date = request.EventDate.Date,
                Event_Location = request.EventLocation.Trim(),
                Event_Type = request.EventType.Trim(),
                Description = request.Description?.Trim(),
                Registration_Deadline = request.RegistrationDeadline.Date,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = item.EventId },
                item);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id, [FromBody] EventRequest request)
        {
            if (!Valid(request))
                return BadRequest(new { message = "Invalid event details." });

            if (CurrentUserId is not int userId)
                return Unauthorized();

            var item = await _context.Events
                .SingleOrDefaultAsync(e => e.EventId == id);

            if (item == null)
                return NotFound();

            var ownsEvent = await _context.Organisers.AnyAsync(
                o => o.OrganiserId == item.OrganiserId &&
                     o.UserId == userId);

            if (!ownsEvent)
                return Forbid();

            item.Event_Name = request.EventName.Trim();
            item.Event_Date = request.EventDate.Date;
            item.Event_Location = request.EventLocation.Trim();
            item.Event_Type = request.EventType.Trim();
            item.Description = request.Description?.Trim();
            item.Registration_Deadline = request.RegistrationDeadline.Date;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var item = await _context.Events
                .SingleOrDefaultAsync(e => e.EventId == id);

            if (item == null)
                return NotFound();

            var ownsEvent = await _context.Organisers.AnyAsync(
                o => o.OrganiserId == item.OrganiserId &&
                     o.UserId == userId);

            if (!ownsEvent)
                return Forbid();

            _context.Events.Remove(item);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The event has related records and cannot be deleted."
                });
            }

            return Ok(new { message = "Event deleted successfully." });
        }

        private static bool Valid(EventRequest r)
        {
            return !string.IsNullOrWhiteSpace(r.EventName) &&
                   r.EventName.Trim().Length <= 100 &&
                   !string.IsNullOrWhiteSpace(r.EventLocation) &&
                   r.EventLocation.Trim().Length <= 100 &&
                   !string.IsNullOrWhiteSpace(r.EventType) &&
                   r.EventType.Trim().Length <= 50 &&
                   (r.Description?.Length ?? 0) <= 255 &&
                   r.EventDate != default &&
                   r.RegistrationDeadline != default &&
                   r.RegistrationDeadline.Date <= r.EventDate.Date;
        }
    }

    public class EventRequest
    {
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime RegistrationDeadline { get; set; }
    }
}
