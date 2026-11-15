
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class EnrolmentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public EnrolmentController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser()
        {
            return !string.IsNullOrEmpty(
                       HttpContext.Session.GetString("JwtToken"))
                   && HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        // View enrolments for a specific event
        public async Task<IActionResult> Index(int eventId)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (eventId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid event selected.";
                return RedirectToAction("Index", "Event");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                var response = await client.GetAsync(
                    $"api/enrolments/event/{eventId}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        response.StatusCode == System.Net.HttpStatusCode.Forbidden
                            ? "You are not authorised to view enrolments for this event."
                            : "Unable to load event enrolments.";

                    return RedirectToAction("Index", "Event");
                }

                var enrolments =
                    await response.Content.ReadFromJsonAsync<
                        List<EnrolmentViewModel>>(_jsonOptions)
                    ?? new List<EnrolmentViewModel>();

                ViewBag.EventId = eventId;

                return View(enrolments);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Index", "Event");
            }
        }
    }
}
