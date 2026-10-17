
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
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? UserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var id) ? id : null;

        // Participants can view their own enrolments.
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> MyEnrolments()
        {
            if (UserId is not int userId)
                return Unauthorized();

            var participant = await _context.Participants
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return NotFound(new { message = "Participant profile not found." });

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.ParticipantId == participant.ParticipantId)
                .Include(e => e.Category)
                .ToListAsync();

            return Ok(enrolments);
        }

        // Participants enrol themselves in a category.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] CreateEnrolmentRequest request)
        {
            if (UserId is not int userId)
                return Unauthorized();

            var participant = await _context.Participants
                .SingleOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return NotFound(new { message = "Participant profile not found." });

            var category = await _context.Categories
                .SingleOrDefaultAsync(c => c.CategoryId == request.CategoryId);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var eventItem = await _context.Events
                .SingleOrDefaultAsync(e => e.EventId == category.EventId);

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            if (eventItem.Registration_Deadline.Date < DateTime.UtcNow.Date)
                return BadRequest(new { message = "Registration deadline has passed." });

            var currentCount = await _context.Enrolments
                .CountAsync(e => e.CategoryId == category.CategoryId);

            if (currentCount >= category.MaximumParticipation)
                return Conflict(new { message = "This category is full." });

            var alreadyEnrolled = await _context.Enrolments.AnyAsync(e =>
                e.ParticipantId == participant.ParticipantId &&
                e.CategoryId == category.CategoryId);

            if (alreadyEnrolled)
                return Conflict(new { message = "You are already enrolled in this category." });

            if (string.IsNullOrWhiteSpace(request.BibNumber) ||
                request.BibNumber.Trim().Length > 20)
                return BadRequest(new { message = "A valid bib number is required." });

            var bib = request.BibNumber.Trim();

            if (await _context.Enrolments.AnyAsync(e => e.BibNumber == bib))
                return Conflict(new { message = "That bib number is already in use." });

            var enrolment = new Enrolment
            {
                ParticipantId = participant.ParticipantId,
                CategoryId = category.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Pending",
                BibNumber = bib,
                CreatedAt = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment successful.",
                enrolmentId = enrolment.EnrolmentId,
                enrolment.ParticipantId,
                enrolment.CategoryId,
                enrolment.BibNumber,
                enrolment.Status
            });
        }

        // Organisers can view enrolments for their own events.
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            if (UserId is not int userId)
                return Unauthorized();

            var ownsEvent = await _context.Events.AnyAsync(e =>
                e.EventId == eventId &&
                _context.Organisers.Any(o =>
                    o.OrganiserId == e.OrganiserId &&
                    o.UserId == userId));

            if (!ownsEvent)
                return Forbid();

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.Category != null &&
                            e.Category.EventId == eventId)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .ToListAsync();

            return Ok(enrolments);
        }
    }

    public class CreateEnrolmentRequest
    {
        public int CategoryId { get; set; }
        public string BibNumber { get; set; } = string.Empty;
    }
}
