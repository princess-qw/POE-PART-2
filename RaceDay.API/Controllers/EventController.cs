
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
        private readonly IWebHostEnvironment _environment;

        public EventController(
            RaceDayDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

        [HttpPost("{id:int}/banner")]
        [Authorize(Roles = "Organiser")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadBanner(
            int id,
            IFormFile file)
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please select an image." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Image must not exceed 5 MB." });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new
                {
                    message = "Only JPG, JPEG, PNG and WEBP images are allowed."
                });

            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(new { message = "Invalid image content type." });

            var item = await _context.Events
                .SingleOrDefaultAsync(e => e.EventId == id);

            if (item == null)
                return NotFound(new { message = "Event not found." });

            var ownsEvent = await _context.Organisers.AnyAsync(
                o => o.OrganiserId == item.OrganiserId &&
                     o.UserId == userId);

            if (!ownsEvent)
                return Forbid();

            var uploadFolder = Path.Combine(
                _environment.WebRootPath ??
                Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "uploads",
                "events");

            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            try
            {
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var oldBanner = item.BannerImage;

                item.BannerImage = $"/uploads/events/{fileName}";

                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(oldBanner))
                {
                    var oldFileName = Path.GetFileName(oldBanner);
                    var oldFilePath = Path.Combine(uploadFolder, oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                return Ok(new
                {
                    message = "Event banner uploaded successfully.",
                    bannerImage = item.BannerImage
                });
            }
            catch
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                throw;
            }
        }

        // ORGANISER DASHBOARD SUMMARY
        [HttpGet("organiser/dashboard")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetOrganiserDashboard()
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var organiser = await _context.Organisers
                .SingleOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Forbid();

            var events = await _context.Events
                .AsNoTracking()
                .Where(e => e.OrganiserId == organiser.OrganiserId)
                .Select(e => new
                {
                    EventId = e.EventId,
                    EventName = e.Event_Name,
                    EventDate = e.Event_Date,
                    EventLocation = e.Event_Location,
                    EventType = e.Event_Type,
                    BannerImage = e.BannerImage,

                    TotalEnrolments = _context.Enrolments.Count(enrolment =>
                        _context.Categories.Any(category =>
                            category.CategoryId == enrolment.CategoryId &&
                            category.EventId == e.EventId))
                })
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            var today = DateTime.Today;

            return Ok(new
            {
                TotalEvents = events.Count,

                UpcomingEvents = events.Count(e =>
                    e.EventDate.Date >= today),

                TotalEnrolments = events.Sum(e =>
                    e.TotalEnrolments),

                Events = events
            });
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
