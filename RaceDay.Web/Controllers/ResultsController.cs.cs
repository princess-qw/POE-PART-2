
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public ResultsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser()
        {
            return !string.IsNullOrEmpty(
                       HttpContext.Session.GetString("JwtToken"))
                   && HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        public async Task<IActionResult> Index(int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (eventId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid event selected.";
                return RedirectToAction("Index", "Event");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                var enrolmentResponse =
                    await client.GetAsync($"api/enrolments/event/{eventId}");

                if (!enrolmentResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to load enrolments for this event.";

                    return RedirectToAction("Index", "Event");
                }

                var enrolments =
                    await enrolmentResponse.Content
                        .ReadFromJsonAsync<List<EnrolmentViewModel>>(
                            _jsonOptions)
                    ?? new List<EnrolmentViewModel>();

                var resultsResponse =
                    await client.GetAsync($"api/results/event/{eventId}");

                if (!resultsResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to load results for this event.";

                    return RedirectToAction("Index", "Event");
                }

                var results =
                    await resultsResponse.Content
                        .ReadFromJsonAsync<List<ApiResult>>(_jsonOptions)
                    ?? new List<ApiResult>();

                var viewModels = enrolments.Select(enrolment =>
                {
                    var result = results.FirstOrDefault(
                        r => r.EnrolmentId == enrolment.EnrolmentId);

                    return new ResultsViewModel
                    {
                        EnrolmentId = enrolment.EnrolmentId,
                        ResultId = result?.ResultId ?? 0,

                        ParticipantName = enrolment.Participant == null
                            ? "Unknown"
                            : $"{enrolment.Participant.FirstName} {enrolment.Participant.LastName}",

                        CategoryName =
                            enrolment.Category?.CategoryName ?? "N/A",

                        DistanceKm =
                            enrolment.Category?.DistanceKm ?? 0,

                        BibNumber = enrolment.BibNumber,

                        FinishTime = result?.FinishTime,

                        Position = result?.Position,

                        ResultStatus =
                            result?.ResultStatus ?? "Pending"
                    };
                }).ToList();

                ViewBag.EventId = eventId;

                return View(viewModels);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Index", "Event");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(
            int eventId,
            int enrolmentId,
            string? finishTime,
            int? position,
            string resultStatus)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            TimeSpan? parsedFinishTime = null;

            if (!string.IsNullOrWhiteSpace(finishTime))
            {
                if (!TimeSpan.TryParse(finishTime, out var parsed) ||
                    parsed < TimeSpan.Zero ||
                    parsed >= TimeSpan.FromDays(1))
                {
                    TempData["ErrorMessage"] =
                        "Please enter a valid finish time.";

                    return RedirectToAction("Index", new { eventId });
                }

                parsedFinishTime = parsed;
            }

            if (position.HasValue && position.Value <= 0)
            {
                TempData["ErrorMessage"] =
                    "Position must be greater than zero.";

                return RedirectToAction("Index", new { eventId });
            }

            var allowedStatuses = new[]
            {

            "Completed", "DNF", "DNS"
            };

            if (!allowedStatuses.Contains(resultStatus))
            {
                TempData["ErrorMessage"] =
                    "Please select a valid result status.";

                return RedirectToAction("Index", new { eventId });
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                var request = new
                {
                    FinishTime = parsedFinishTime,
                    Position = position,
                    ResultStatus = resultStatus
                };

                var response = await client.PutAsJsonAsync(
                    $"api/results/enrolment/{enrolmentId}",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Result saved successfully.";
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();

                    TempData["ErrorMessage"] =
                        $"Unable to save result. API response: {errorMessage}";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";
            }

            return RedirectToAction("Index", new { eventId });
        }

        private class ApiResult
        {
            public int ResultId { get; set; }

            public int EnrolmentId { get; set; }

            public string? FinishTime { get; set; }

            public int? Position { get; set; }

            public string ResultStatus { get; set; } = "Pending";
        }
    }
}
