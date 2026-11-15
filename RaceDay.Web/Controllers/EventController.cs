
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Check that the logged-in user is an Organiser
        private bool IsOrganiser()
        {
            return !string.IsNullOrEmpty(
                       HttpContext.Session.GetString("JwtToken"))
                   && HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        // Check that the logged-in user is a Participant
        private bool IsParticipant()
        {
            return !string.IsNullOrEmpty(
                       HttpContext.Session.GetString("JwtToken"))
                   && HttpContext.Session.GetString("UserRole") == "Participant";
        }

        // Validate an optional banner image
        private void ValidateBannerFile(EventViewModel model)
        {
            if (model.BannerFile == null || model.BannerFile.Length == 0)
                return;

            if (model.BannerFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.BannerFile),
                    "The banner image must not exceed 5 MB.");
                return;
            }

            var extension = Path.GetExtension(
                model.BannerFile.FileName).ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var allowedContentTypes = new[]
            {
                "image/jpeg", "image/png", "image/webp"
            };

            if (!allowedExtensions.Contains(extension) ||
                !allowedContentTypes.Contains(
                    model.BannerFile.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError(
                    nameof(model.BannerFile),
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }
        }

        // Upload a banner image to the API
        private static async Task<HttpResponseMessage> UploadBannerAsync(
            HttpClient client,
            int eventId,
            IFormFile bannerFile)
        {
            using var form = new MultipartFormDataContent();
            using var fileStream = bannerFile.OpenReadStream();
            using var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(bannerFile.ContentType);

            form.Add(
                fileContent,
                "file",
                Path.GetFileName(bannerFile.FileName));

            return await client.PostAsync(
                $"api/event/{eventId}/banner",
                form);
        }

        // GET: /Event/Index
        // Both Organisers and Participants can browse events
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser() && !IsParticipant())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.GetAsync("api/event");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    await GetApiError(response);

                return View(new List<EventViewModel>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            ViewBag.UserRole =
                HttpContext.Session.GetString("UserRole");

            return View(events ?? new List<EventViewModel>());
        }

        // GET: /Event/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // POST: /Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (model.Registration_Deadline.Date > model.Event_Date.Date)
            {
                ModelState.AddModelError(
                    nameof(model.Registration_Deadline),
                    "Registration deadline cannot be after the event date.");
            }

            ValidateBannerFile(model);

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var eventData = new
            {
                EventName = model.Event_Name,
                EventDate = model.Event_Date,
                EventLocation = model.Event_Location,
                EventType = model.Event_Type,
                Description = model.Description,
                RegistrationDeadline = model.Registration_Deadline
            };

            var response = await client.PostAsJsonAsync(
                "api/event", eventData);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    await GetApiError(response));

                return View(model);
            }

            // Upload the banner after the event has been created
            if (model.BannerFile != null && model.BannerFile.Length > 0)
            {
                var createdEvent = await response.Content
                    .ReadFromJsonAsync<EventViewModel>();

                if (createdEvent == null || createdEvent.EventId <= 0)
                {
                    TempData["ErrorMessage"] =
                        "The event was created, but its banner could not be uploaded because the event ID could not be read.";

                    return RedirectToAction(nameof(Index));
                }

                using var bannerResponse = await UploadBannerAsync(
                    client,
                    createdEvent.EventId,
                    model.BannerFile);

                if (!bannerResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "The event was created, but the banner upload failed: "
                        + await GetApiError(bannerResponse);

                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["SuccessMessage"] =
                "Event created successfully!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.GetAsync($"api/event/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    await GetApiError(response);

                return RedirectToAction(nameof(Index));
            }

            var eventItem = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // POST: /Event/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, EventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (id != model.EventId)
                return BadRequest();

            if (model.Registration_Deadline.Date > model.Event_Date.Date)
            {
                ModelState.AddModelError(
                    nameof(model.Registration_Deadline),
                    "Registration deadline cannot be after the event date.");
            }

            ValidateBannerFile(model);

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var eventData = new
            {
                EventName = model.Event_Name,
                EventDate = model.Event_Date,
                EventLocation = model.Event_Location,
                EventType = model.Event_Type,
                Description = model.Description,
                RegistrationDeadline = model.Registration_Deadline
            };

            var response = await client.PutAsJsonAsync(
                $"api/event/{id}", eventData);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    await GetApiError(response));

                return View(model);
            }

            // Replace the banner only if a new image was selected
            if (model.BannerFile != null && model.BannerFile.Length > 0)
            {
                using var bannerResponse = await UploadBannerAsync(
                    client,
                    id,
                    model.BannerFile);

                if (!bannerResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "The event details were updated, but the banner upload failed: "
                        + await GetApiError(bannerResponse);

                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["SuccessMessage"] =
                "Event updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Event/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.DeleteAsync($"api/event/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    await GetApiError(response);

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Event deleted successfully!";

            return RedirectToAction(nameof(Index));
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
