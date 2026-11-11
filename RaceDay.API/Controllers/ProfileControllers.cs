
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
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize = 5 * 1024 * 1024;

        public ProfileController(
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
                        o.OrganisationName,
                        user.ProfilePictureUrl
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
                        p.DateOfBirth,
                        user.ProfilePictureUrl
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

        // POST: api/Profile/picture
        [HttpPost("picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(
            IFormFile file)
        {
            if (CurrentUserId is not int userId)
                return Unauthorized();

            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select an image."
                });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new
                {
                    message = "The image cannot exceed 5 MB."
                });
            }

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    message = "Only JPG, JPEG, PNG and WEBP images are allowed."
                });
            }

            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(
                file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(new
                {
                    message = "The uploaded file must be a valid image."
                });
            }

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var webRoot = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(
                    _environment.ContentRootPath, "wwwroot");

            var uploadFolder = Path.Combine(
                webRoot, "uploads", "profile-pictures");

            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            await using (var stream = new FileStream(
                filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePictureUrl =
                $"/uploads/profile-pictures/{fileName}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile picture uploaded successfully.",
                profilePictureUrl = user.ProfilePictureUrl
            });
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
