
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CategoryController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser()
        {
            return !string.IsNullOrEmpty(
                       HttpContext.Session.GetString("JwtToken"))
                   && HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        // GET: /Category/Index/5
        [HttpGet]
        public async Task<IActionResult> Index(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (id <= 0)
                return BadRequest();

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.GetAsync(
                $"api/category/event/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = await GetApiError(response);
                return View(new List<CategoryViewModel>());
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            ViewBag.EventId = id;

            return View(categories ?? new List<CategoryViewModel>());
        }

        // GET: /Category/Create/5
        [HttpGet]
        public IActionResult Create(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (id <= 0)
                return BadRequest();

            return View(new CategoryViewModel
            {
                EventId = id
            });
        }

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var categoryData = new
            {
                model.CategoryName,
                model.DistanceKm,
                model.EntryFee,
                model.MaximumParticipation,
                model.StartPoint,
                model.EndPoint,
                model.ElevationGainM,
                model.MapUrl
            };

            var response = await client.PostAsJsonAsync(
                $"api/category/event/{model.EventId}",
                categoryData);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    await GetApiError(response));

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Category created successfully!";

            return RedirectToAction(
                nameof(Index),
                new { id = model.EventId });
        }

        // GET: /Category/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Get the category's event ID from the event category lists.
            // We first need a known event ID, so the Edit link passes it.
            var eventId = HttpContext.Request.Query["eventId"].ToString();

            if (!int.TryParse(eventId, out var parsedEventId) ||
                parsedEventId <= 0)
            {
                TempData["ErrorMessage"] =
                    "The event ID is missing.";

                return RedirectToAction("Index", "Event");
            }

            var response = await client.GetAsync(
                $"api/category/event/{parsedEventId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = await GetApiError(response);
                return RedirectToAction("Index", new { id = parsedEventId });
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            var category = categories?.FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (id != model.CategoryId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var categoryData = new
            {
                model.CategoryName,
                model.DistanceKm,
                model.EntryFee,
                model.MaximumParticipation,
                model.StartPoint,
                model.EndPoint,
                model.ElevationGainM,
                model.MapUrl
            };

            var response = await client.PutAsJsonAsync(
                $"api/category/{id}",
                categoryData);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    await GetApiError(response));

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Category updated successfully!";

            return RedirectToAction(
                nameof(Index),
                new { id = model.EventId });
        }

        // POST: /Category/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id, int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.DeleteAsync(
                $"api/category/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = await GetApiError(response);
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Category deleted successfully!";
            }

            return RedirectToAction(
                nameof(Index),
                new { id = eventId });
        }

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
