
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
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? CurrentUserId
        {
            get
            {
                var id = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                return int.TryParse(id, out var userId)
                    ? userId
                    : null;
            }
        }

        // GET: api/Profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (user.Role == "Organiser")
            {
                var profile = await _context.Organisers
                    .AsNoTracking()
                    .Where(o => o.UserId == userId)
                    .Select(o => new
                    {
                        user.UserId,
                        user.Email,
                        user.Role,
                        o.FirstName,
                        o.LastName,
                        o.Phone,
                        o.OrganisationName
                    })
                    .SingleOrDefaultAsync();

                return profile == null
                    ? NotFound(new { message = "Organiser profile not found." })
                    : Ok(profile);
            }

            if (user.Role == "Participant")
            {
                var profile = await _context.Participants
                    .AsNoTracking()
                    .Where(p => p.UserId == userId)
                    .Select(p => new
                    {
                        user.UserId,
                        user.Email,
                        user.Role,
                        p.FirstName,
                        p.LastName,
                        p.Phone,
                        p.DateOfBirth
                    })
                    .SingleOrDefaultAsync();

                return profile == null
                    ? NotFound(new { message = "Participant profile not found." })
                    : Ok(profile);
            }

            return Forbid();
        }

        // PUT: api/Profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateProfileRequest request)
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var firstName = request.FirstName?.Trim();
            var lastName = request.LastName?.Trim();
            var phone = request.Phone?.Trim();

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest(new
                {
                    message = "First name, last name and phone are required."
                });
            }

            if (firstName.Length > 50 ||
                lastName.Length > 50 ||
                phone.Length > 20)
            {
                return BadRequest(new
                {
                    message = "A field exceeds its maximum allowed length."
                });
            }

            if (user.Role == "Organiser")
            {
                var profile = await _context.Organisers
                    .SingleOrDefaultAsync(o => o.UserId == userId);

                if (profile == null)
                    return NotFound(new { message = "Organiser profile not found." });

                var organisationName = request.OrganisationName?.Trim();

                if (string.IsNullOrWhiteSpace(organisationName))
                {
                    return BadRequest(new
                    {
                        message = "Organisation name is required."
                    });
                }

                if (organisationName.Length > 100)
                {
                    return BadRequest(new
                    {
                        message = "Organisation name cannot exceed 100 characters."
                    });
                }

                profile.FirstName = firstName;
                profile.LastName = lastName;
                profile.Phone = phone;
                profile.OrganisationName = organisationName;
            }
            else if (user.Role == "Participant")
            {
                var profile = await _context.Participants
                    .SingleOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                    return NotFound(new { message = "Participant profile not found." });

                if (request.DateOfBirth == null ||
                    request.DateOfBirth.Value.Date > DateTime.UtcNow.Date)
                {
                    return BadRequest(new
                    {
                        message = "A valid date of birth is required."
                    });
                }

                profile.FirstName = firstName;
                profile.LastName = lastName;
                profile.Phone = phone;
                profile.DateOfBirth = request.DateOfBirth.Value.Date;
            }
            else
            {
                return Forbid();
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }
    }

    public class UpdateProfileRequest
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? OrganisationName { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}
