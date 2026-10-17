
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();
        private readonly IConfiguration _configuration;

        public AuthController(
            RaceDayDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var role = request.Role.Trim();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            var isOrganiser = role.Equals(
                "Organiser", StringComparison.OrdinalIgnoreCase);

            var isParticipant = role.Equals(
                "Participant", StringComparison.OrdinalIgnoreCase);

            if (!isOrganiser && !isParticipant)
            {
                return BadRequest(new
                {
                    message = "Role must be Organiser or Participant."
                });
            }

            if (isOrganiser &&
                string.IsNullOrWhiteSpace(request.OrganisationName))
            {
                return BadRequest(new
                {
                    message = "Organisation name is required for an Organiser."
                });
            }

            if (isParticipant && request.DateOfBirth is null)
            {
                return BadRequest(new
                {
                    message = "Date of birth is required for a Participant."
                });
            }

            if (request.DateOfBirth?.Date > DateTime.UtcNow.Date)
            {
                return BadRequest(new
                {
                    message = "Date of birth cannot be in the future."
                });
            }

            var user = new User
            {
                Email = email,
                Role = isOrganiser ? "Organiser" : "Participant",
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user, request.Password);

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (isOrganiser)
                {
                    _context.Organisers.Add(new Organiser
                    {
                        UserId = user.UserId,
                        FirstName = request.FirstName.Trim(),
                        LastName = request.LastName.Trim(),
                        Phone = request.Phone.Trim(),
                        OrganisationName = request.OrganisationName!.Trim(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    _context.Participants.Add(new Participant
                    {
                        UserId = user.UserId,
                        FirstName = request.FirstName.Trim(),
                        LastName = request.LastName.Trim(),
                        Phone = request.Phone.Trim(),
                        DateOfBirth = request.DateOfBirth!.Value.Date,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Created("/api/auth/register", new
                {
                    message = "Registration successful.",
                    userId = user.UserId,
                    email = user.Email,
                    role = user.Role
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (verification == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                return StatusCode(500, new
                {
                    message = "JWT configuration is incomplete."
                });
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                message = "Login successful.",
                token = tokenString,
                userId = user.UserId,
                email = user.Email,
                role = user.Role
            });
        }
    }
}
