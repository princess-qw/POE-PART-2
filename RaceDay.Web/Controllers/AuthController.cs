
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync(
                "api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    await GetApiError(response));

                return View(model);
            }

            var result = await response.Content
                .ReadFromJsonAsync<AuthResponse>();

            if (result == null || string.IsNullOrWhiteSpace(result.Token))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Login failed. The API did not return a valid token.");

                return View(model);
            }

            // Store authentication information in Session
            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("UserId", result.UserId.ToString());
            HttpContext.Session.SetString("UserEmail", result.Email ?? model.Email);
            HttpContext.Session.SetString("UserRole", result.Role ?? "");

            // Redirect according to the user's role
            if (result.Role == "Organiser")
            {
                return RedirectToAction(
                    "Dashboard", "Organiser");
            }

            if (result.Role == "Participant")
            {
                return RedirectToAction(
                    "Dashboard", "Participant");
            }

            ModelState.AddModelError(
                string.Empty,
                "Your account has an unrecognised role.");

            HttpContext.Session.Clear();

            return View(model);
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Role == "Organiser" &&
                string.IsNullOrWhiteSpace(model.OrganisationName))
            {
                ModelState.AddModelError(
                    nameof(model.OrganisationName),
                    "Organisation name is required for Organisers.");
            }

            if (model.Role == "Participant" &&
                model.DateOfBirth == null)
            {
                ModelState.AddModelError(
                    nameof(model.DateOfBirth),
                    "Date of birth is required for Participants.");
            }

            if (model.Role == "Participant" &&
                model.DateOfBirth > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.DateOfBirth),
                    "Date of birth cannot be in the future.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync(
                "api/auth/register", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    await GetApiError(response));

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Registration successful! Please log in.";

            return RedirectToAction("Login");
        }

        // Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        // Read error messages returned by the API
        private static async Task<string> GetApiError(
            HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    using var document = JsonDocument.Parse(content);

                    if (document.RootElement.TryGetProperty(
                        "message", out var message))
                    {
                        return message.GetString()
                            ?? "The request failed.";
                    }

                    if (document.RootElement.TryGetProperty(
                        "title", out var title))
                    {
                        return title.GetString()
                            ?? "The request failed.";
                    }
                }
                catch (JsonException)
                {
                    return content;
                }
            }

            return $"The request failed. Status: {(int)response.StatusCode}";
        }
    }

    // Matches the API authentication response
    public class AuthResponse
    {
        public string? Message { get; set; }
        public string? Token { get; set; }
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
