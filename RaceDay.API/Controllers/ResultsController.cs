
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var id) ? id : null;

        // Organisers record or update a participant's result.
        [HttpPut("enrolment/{enrolmentId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> RecordResult(
            int enrolmentId, [FromBody] ResultRequest request)
        {
            if (request.Position is <= 0)
                return BadRequest(new
                {
                    message = "Position must be positive."
                });

            if (request.FinishTime.HasValue &&
                (request.FinishTime.Value < TimeSpan.Zero ||
                 request.FinishTime.Value >= TimeSpan.FromDays(1)))
                return BadRequest(new
                {
                    message = "Finish time must be between 00:00:00 and 23:59:59."
                });

            var allowedStatuses = new[] { "Completed", "DNF", "DNS" };

            if (string.IsNullOrWhiteSpace(request.ResultStatus) ||
                !allowedStatuses.Contains(
                    request.ResultStatus.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Result status must be Completed, DNF or DNS."
                });
            }

            if (CurrentUserId is not int userId)
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .SingleOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment == null || enrolment.Category == null)
                return NotFound(new
                {
                    message = "Enrolment not found."
                });

            var ownsEvent = await _context.Events.AnyAsync(e =>
                e.EventId == enrolment.Category.EventId &&
                _context.Organisers.Any(o =>
                    o.OrganiserId == e.OrganiserId &&
                    o.UserId == userId));

            if (!ownsEvent)
                return Forbid();

            var result = await _context.Results
                .SingleOrDefaultAsync(r => r.EnrolmentId == enrolmentId);

            if (result == null)
            {
                result = new Result
                {
                    EnrolmentId = enrolmentId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Results.Add(result);
            }

            result.FinishTime = request.FinishTime;
            result.Position = request.Position;
            result.ResultStatus = request.ResultStatus.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result saved successfully.",
                result.ResultId,
                result.EnrolmentId,
                result.FinishTime,
                result.Position,
                result.ResultStatus
            });
        }

        // Participants can view their own race history.
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var participant = await _context.Participants
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return NotFound(new
                {
                    message = "Participant profile not found."
                });

            var results = await (
                from result in _context.Results.AsNoTracking()
                join enrolment in _context.Enrolments
                    on result.EnrolmentId equals enrolment.EnrolmentId
                join category in _context.Categories
                    on enrolment.CategoryId equals category.CategoryId
                join raceEvent in _context.Events
                    on category.EventId equals raceEvent.EventId
                where enrolment.ParticipantId == participant.ParticipantId
                select new
                {
                    result.ResultId,
                    result.EnrolmentId,
                    EventName = raceEvent.Event_Name,
                    EventDate = raceEvent.Event_Date,
                    CategoryName = category.CategoryName,
                    result.FinishTime,
                    result.Position,
                    result.ResultStatus,

                    TotalFinishers = _context.Results.Count(otherResult =>
                        otherResult.ResultStatus == "Completed" &&
                        _context.Enrolments.Any(otherEnrolment =>
                            otherEnrolment.EnrolmentId == otherResult.EnrolmentId &&
                            otherEnrolment.CategoryId == category.CategoryId))
                })
                .ToListAsync();

            return Ok(results);
        }

        // Organisers can view results for events they own.
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var ownsEvent = await _context.Events.AnyAsync(e =>
                e.EventId == eventId &&
                _context.Organisers.Any(o =>
                    o.OrganiserId == e.OrganiserId &&
                    o.UserId == userId));

            if (!ownsEvent)
                return Forbid();

            var results = await _context.Results
                .AsNoTracking()
                .Where(r => _context.Enrolments.Any(en =>
                    en.EnrolmentId == r.EnrolmentId &&
                    en.Category != null &&
                    en.Category.EventId == eventId))
                .ToListAsync();

            return Ok(results);
        }
    }

    public class ResultRequest
    {
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string ResultStatus { get; set; } = "Completed";
    }
}
