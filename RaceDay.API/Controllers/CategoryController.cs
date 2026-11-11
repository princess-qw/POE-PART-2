
using System.ComponentModel.DataAnnotations;
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
    public class CategoryController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoryController(RaceDayDbContext context)
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

        // Check whether the logged-in organiser owns the event
        private async Task<bool> OwnsEventAsync(
            int eventId, int userId)
        {
            return await _context.Events.AnyAsync(e =>
                e.EventId == eventId &&
                _context.Organisers.Any(o =>
                    o.OrganiserId == e.OrganiserId &&
                    o.UserId == userId));
        }

        // GET: api/category/event/5
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/category/event/5
        [HttpPost("event/{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(
            int eventId,
            [FromBody] CategoryRequest request)
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            if (!await OwnsEventAsync(eventId, userId))
                return Forbid();

            var category = new Category
            {
                EventId = eventId,
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

            return Ok(new
            {
                message = "Category created successfully.",
                category
            });
        }

        // PUT: api/category/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] CategoryRequest request)
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var category = await _context.Categories
                .SingleOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (!await OwnsEventAsync(category.EventId, userId))
                return Forbid();

            category.CategoryName = request.CategoryName.Trim();
            category.DistanceKm = request.DistanceKm;
            category.EntryFee = request.EntryFee;
            category.MaximumParticipation = request.MaximumParticipation;
            category.StartPoint = request.StartPoint.Trim();
            category.EndPoint = request.EndPoint.Trim();
            category.ElevationGainM = request.ElevationGainM;
            category.MapUrl = request.MapUrl?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully."
            });
        }

        // DELETE: api/category/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var category = await _context.Categories
                .SingleOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (!await OwnsEventAsync(category.EventId, userId))
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

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }

    public class CategoryRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "1000.00")]
        public decimal DistanceKm { get; set; }

        [Range(typeof(decimal), "0.00", "99999999.99")]
        public decimal EntryFee { get; set; }

        [Range(1, 1000000)]
        public int MaximumParticipation { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string StartPoint { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string EndPoint { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int? ElevationGainM { get; set; }

        [StringLength(255)]
        [Url]
        public string? MapUrl { get; set; }
    }
}
