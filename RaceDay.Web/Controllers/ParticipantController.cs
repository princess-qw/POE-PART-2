
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsParticipant()
        {
            return !string.IsNullOrEmpty(
                       HttpContext.Session.GetString("JwtToken"))
                   && HttpContext.Session.GetString("UserRole") == "Participant";
        }

        // Participant Dashboard
        public IActionResult Dashboard()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // Display categories belonging to a selected event
        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.GetAsync(
                $"api/category/event/{eventId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    await GetApiError(response);

                return RedirectToAction("Index", "Event");
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<ParticipantCategoryViewModel>>();

            ViewBag.EventId = eventId;

            return View(categories ??
                new List<ParticipantCategoryViewModel>());
        }

        // Submit a Participant enrolment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(
            int eventId,
            int categoryId,
            string bibNumber)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(bibNumber))
            {
                TempData["ErrorMessage"] =
                    "Please enter your bib number.";

                return RedirectToAction(
                    nameof(Categories),
                    new { eventId });
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var enrolmentData = new
            {
                CategoryId = categoryId,
                BibNumber = bibNumber.Trim()
            };

            var response = await client.PostAsJsonAsync(
                "api/enrolments", enrolmentData);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    await GetApiError(response);

                return RedirectToAction(
                    nameof(Categories),
                    new { eventId });
            }

            TempData["SuccessMessage"] =
                "You have successfully enrolled in this event!";

            return RedirectToAction(nameof(MyEnrolments));
        }

        // Display the Participant's own enrolments
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.GetAsync("api/enrolments/my");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    await GetApiError(response);

                return View(new List<MyEnrolmentViewModel>());
            }

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<MyEnrolmentViewModel>>();

            return View(enrolments ??
                new List<MyEnrolmentViewModel>());
        }

        // Display the Participant's own results
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.GetAsync("api/results/my");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    await GetApiError(response);

                return View(new List<ParticipantResultViewModel>());
            }

            var results = await response.Content
                .ReadFromJsonAsync<List<ParticipantResultViewModel>>();

            return View(results ??
                new List<ParticipantResultViewModel>());
        }

        // Read an error message returned by the API
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
}
