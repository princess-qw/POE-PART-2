
using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay.API.Data;
using System.Net;
using Xunit;

public class ApiSmokeTests : IClassFixture<RaceDayApiFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(RaceDayApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnauthenticatedProfileRequest_IsRejected()
    {
        var response = await _client.GetAsync("/api/Profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EventsEndpoint_IsReachable()
    {
        var response = await _client.GetAsync("/api/Events");

        Assert.True(
            response.IsSuccessStatusCode,
            $"Expected success but received {(int)response.StatusCode}.");
    }
}
