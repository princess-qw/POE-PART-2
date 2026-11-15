
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Dashboard()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                var response = await client.GetAsync(
                    "api/event/organiser/dashboard");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        "Unable to load your dashboard information.";

                    return View(new OrganiserDashboardViewModel());
                }

                var dashboard = await response.Content
                    .ReadFromJsonAsync<OrganiserDashboardViewModel>();

                return View(dashboard ?? new OrganiserDashboardViewModel());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to connect to RaceDay API. Please try again.";

                return View(new OrganiserDashboardViewModel());
            }
        }
    }
}
