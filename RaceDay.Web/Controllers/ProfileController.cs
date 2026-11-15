
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("JwtToken"))
                && (
                    HttpContext.Session.GetString("UserRole") == "Organiser"
                    || HttpContext.Session.GetString("UserRole") == "Participant"
                );
        }

        private IActionResult RoleDashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Organiser")
                return RedirectToAction("Dashboard", "Organiser");

            if (role == "Participant")
                return RedirectToAction("Dashboard", "Participant");

            return RedirectToAction("Login", "Auth");
        }

        // GET: Profile/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                var response = await client.GetAsync("api/profile/me");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to load your profile.";

                    return RoleDashboard();
                }

                var profile =
                    await response.Content.ReadFromJsonAsync<ProfileViewModel>(
                        _jsonOptions);

                if (profile == null)
                {
                    TempData["ErrorMessage"] =
                        "Your profile could not be found.";

                    return RoleDashboard();
                }

                return View(profile);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return RoleDashboard();
            }
        }

        // POST: Profile/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Organiser")
            {
                if (string.IsNullOrWhiteSpace(model.OrganisationName))
                {
                    ModelState.AddModelError(
                        nameof(model.OrganisationName),
                        "Organisation name is required.");
                }
            }
            else if (role == "Participant")
            {
                if (model.DateOfBirth == null ||
                    model.DateOfBirth.Value.Date > DateTime.Today)
                {
                    ModelState.AddModelError(
                        nameof(model.DateOfBirth),
                        "Please enter a valid date of birth.");
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var request = new
            {
                model.FirstName,
                model.LastName,
                model.Phone,
                model.OrganisationName,
                model.DateOfBirth
            };

            try
            {
                var response = await client.PutAsJsonAsync(
                    "api/profile/me", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Your profile was updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] =
                    "Unable to update your profile. Please check your details.";

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return View(model);
            }
        }

        // POST: Profile/UploadPicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile file)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a profile picture.";

                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                using var multipartContent = new MultipartFormDataContent();

                await using var fileStream = file.OpenReadStream();

                using var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType =
                    new MediaTypeHeaderValue(file.ContentType);

                multipartContent.Add(
                    streamContent,
                    "file",
                    Path.GetFileName(file.FileName));

                var response = await client.PostAsync(
                    "api/profile/picture",
                    multipartContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Your profile picture was uploaded successfully.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    TempData["ErrorMessage"] =
                        $"Unable to upload picture: {error}";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
