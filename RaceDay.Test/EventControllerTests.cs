
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

public class EventControllerTests
{
    private static RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
    }

    private static EventController CreateController(
        RaceDayDbContext context,
        int userId)
    {
        var controller = new EventController(context);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString()),
                new Claim(ClaimTypes.Role, "Organiser")
            },
            "TestAuthentication");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }

    private static Organiser CreateOrganiser(int userId)
    {
        return new Organiser
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "Organiser",
            Phone = "0712345678",
            OrganisationName = "RaceDay Test Club",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static EventRequest CreateValidRequest()
    {
        return new EventRequest
        {
            EventName = "Community Run",
            EventDate = DateTime.Today.AddDays(30),
            EventLocation = "Johannesburg",
            EventType = "Running",
            Description = "Automated test event",
            RegistrationDeadline = DateTime.Today.AddDays(20)
        };
    }

    private static async Task<Organiser> SeedOrganiser(
        RaceDayDbContext context,
        int userId)
    {
        var organiser = CreateOrganiser(userId);

        context.Organisers.Add(organiser);
        await context.SaveChangesAsync();

        return organiser;
    }

    private static async Task<Event> SeedEvent(
        RaceDayDbContext context,
        int organiserId)
    {
        var eventItem = new Event
        {
            OrganiserId = organiserId,
            Event_Name = "Original Event",
            Event_Date = DateTime.Today.AddDays(30),
            Event_Location = "Johannesburg",
            Event_Type = "Running",
            Description = "Original description",
            Registration_Deadline = DateTime.Today.AddDays(20),
            CreatedAt = DateTime.UtcNow
        };

        context.Events.Add(eventItem);
        await context.SaveChangesAsync();

        return eventItem;
    }

    [Fact]
    public async Task CreateEvent_ValidRequest_ReturnsCreatedAndSavesEvent()
    {
        await using var context = CreateContext();

        var organiser = await SeedOrganiser(context, 101);
        var controller = CreateController(context, organiser.UserId);

        var result = await controller.CreateEvent(CreateValidRequest());

        Assert.IsType<CreatedAtActionResult>(result);

        var savedEvent = await context.Events.SingleAsync();

        Assert.Equal(organiser.OrganiserId, savedEvent.OrganiserId);
        Assert.Equal("Community Run", savedEvent.Event_Name);
        Assert.Equal("Johannesburg", savedEvent.Event_Location);
    }

    [Fact]
    public async Task UpdateEvent_OwnEvent_ReturnsOkAndSavesChanges()
    {
        await using var context = CreateContext();

        var organiser = await SeedOrganiser(context, 101);
        var eventItem = await SeedEvent(
            context, organiser.OrganiserId);

        var controller = CreateController(context, organiser.UserId);
        var request = CreateValidRequest();
        request.EventName = "Updated Community Run";

        var result = await controller.UpdateEvent(
            eventItem.EventId, request);

        Assert.IsType<OkObjectResult>(result);

        var updatedEvent = await context.Events
            .SingleAsync(e => e.EventId == eventItem.EventId);

        Assert.Equal("Updated Community Run", updatedEvent.Event_Name);
    }

    [Fact]
    public async Task UpdateEvent_EventOwnedByAnotherOrganiser_ReturnsForbid()
    {
        await using var context = CreateContext();

        var owner = await SeedOrganiser(context, 101);
        var eventItem = await SeedEvent(
            context, owner.OrganiserId);

        var otherOrganiserController = CreateController(context, 202);

        var result = await otherOrganiserController.UpdateEvent(
            eventItem.EventId, CreateValidRequest());

        Assert.IsType<ForbidResult>(result);

        var unchangedEvent = await context.Events
            .SingleAsync(e => e.EventId == eventItem.EventId);

        Assert.Equal("Original Event", unchangedEvent.Event_Name);
    }

    [Fact]
    public async Task DeleteEvent_EventOwnedByAnotherOrganiser_ReturnsForbid()
    {
        await using var context = CreateContext();

        var owner = await SeedOrganiser(context, 101);
        var eventItem = await SeedEvent(
            context, owner.OrganiserId);

        var otherOrganiserController = CreateController(context, 202);

        var result = await otherOrganiserController.DeleteEvent(
            eventItem.EventId);

        Assert.IsType<ForbidResult>(result);

        Assert.Single(await context.Events.ToListAsync());
    }

    [Fact]
    public async Task CreateEvent_InvalidRequest_ReturnsBadRequest()
    {
        await using var context = CreateContext();

        var organiser = await SeedOrganiser(context, 101);
        var controller = CreateController(context, organiser.UserId);

        var request = CreateValidRequest();
        request.EventName = string.Empty;

        var result = await controller.CreateEvent(request);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(await context.Events.ToListAsync());
    }
}
