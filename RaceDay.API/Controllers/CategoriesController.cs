
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
    [Authorize(Roles = "Organiser,Participant")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? UserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var id) ? id : null;

        private Task<bool> OwnsEvent(int eventId, int userId) =>
            _context.Events.AnyAsync(e =>
                e.EventId == eventId &&
                _context.Organisers.Any(o =>
                    o.OrganiserId == e.OrganiserId &&
                    o.UserId == userId));

        [HttpGet]
        public async Task<IActionResult> GetCategories(
            [FromQuery] int? eventId)
        {
            var query = _context.Categories.AsNoTracking();

            if (eventId.HasValue)
                query = query.Where(c => c.EventId == eventId.Value);

            return Ok(await query.ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.CategoryId == id);

            return category == null ? NotFound() : Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryRequest request)
        {
            if (!Valid(request))
                return BadRequest(new { message = "Invalid category details." });

            if (UserId is not int userId)
                return Unauthorized();

            if (!await OwnsEvent(request.EventId, userId))
                return Forbid();

            var category = new Category
            {
                EventId = request.EventId,
                CategoryName = request.CategoryName.Trim(),
                DistanceKm = request.DistanceKm,
                EntryFee = request.EntryFee,
                MaximumParticipation = request.MaximumParticipation,
                StartPoint = request.StartPoint.Trim(),
                EndPoint = request.EndPoint.Trim(),
                ElevationGainM = request.ElevationGainM,
                MapUrl = request.MapUrl?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory),
                new { id = category.CategoryId }, category);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id, [FromBody] CategoryRequest request)
        {
            if (!Valid(request))
                return BadRequest(new { message = "Invalid category details." });

            if (UserId is not int userId)
                return Unauthorized();

            var category = await _context.Categories
                .SingleOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            if (!await OwnsEvent(category.EventId, userId) ||
                !await OwnsEvent(request.EventId, userId))
                return Forbid();

            category.EventId = request.EventId;
            category.CategoryName = request.CategoryName.Trim();
            category.DistanceKm = request.DistanceKm;
            category.EntryFee = request.EntryFee;
            category.MaximumParticipation = request.MaximumParticipation;
            category.StartPoint = request.StartPoint.Trim();
            category.EndPoint = request.EndPoint.Trim();
            category.ElevationGainM = request.ElevationGainM;
            category.MapUrl = request.MapUrl?.Trim();

            await _context.SaveChangesAsync();
            return Ok(new { message = "Category updated." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (UserId is not int userId)
                return Unauthorized();

            var category = await _context.Categories
                .SingleOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            if (!await OwnsEvent(category.EventId, userId))
                return Forbid();

            _context.Categories.Remove(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This category has related enrolments and cannot be deleted."
                });
            }

            return Ok(new { message = "Category deleted." });
        }

        private static bool Valid(CategoryRequest r) =>
            r.EventId > 0 &&
            !string.IsNullOrWhiteSpace(r.CategoryName) &&
            r.CategoryName.Trim().Length <= 100 &&
            r.DistanceKm > 0 &&
            r.EntryFee >= 0 &&
            r.MaximumParticipation > 0 &&
            !string.IsNullOrWhiteSpace(r.StartPoint) &&
            r.StartPoint.Trim().Length <= 100 &&
            !string.IsNullOrWhiteSpace(r.EndPoint) &&
            r.EndPoint.Trim().Length <= 100 &&
               r.ElevationGainM >= 0 &&
            (!r.ElevationGainM.HasValue || r.ElevationGainM >= 0);
    }

    public class CategoryRequest
    {
        public int EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; }
        public int MaximumParticipation { get; set; }
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
        public int? ElevationGainM { get; set; }
        public string? MapUrl { get; set; }
    }
}
