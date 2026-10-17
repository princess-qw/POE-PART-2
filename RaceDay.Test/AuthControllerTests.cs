
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using Xunit;

public class AuthControllerTests
{
    private static RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings =>
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new RaceDayDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Key"] =
                        "RaceDayAutomatedTestSigningKey32Bytes!",
                    ["Jwt:Issuer"] = "RaceDayTestIssuer",
                    ["Jwt:Audience"] = "RaceDayTestAudience"
                })
            .Build();
    }

    private static RegisterRequest CreateParticipantRequest(
        string email = "participant@example.com")
    {
        return new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Phone = "0712345678",
            Email = email,
            Password = "TestPassword123!",
            Role = "Participant",
            DateOfBirth = new DateTime(2000, 1, 1)
        };
    }

    [Fact]
    public async Task Register_ValidParticipant_ReturnsCreated()
    {
        await using var context = CreateContext();

        var controller = new AuthController(
            context, CreateConfiguration());

        var request = CreateParticipantRequest();

        var result = await controller.Register(request);

        Assert.IsType<CreatedResult>(result);

        var user = await context.Users.SingleAsync();

        Assert.Equal(
            "participant@example.com", user.Email);

        Assert.Equal("Participant", user.Role);

        Assert.NotEqual(
            request.Password, user.PasswordHash);

        Assert.Single(await context.Participants.ToListAsync());
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        await using var context = CreateContext();

        var controller = new AuthController(
            context, CreateConfiguration());

        var request = CreateParticipantRequest();

        await controller.Register(request);

        var duplicateResult =
            await controller.Register(request);

        Assert.IsType<ConflictObjectResult>(duplicateResult);

        Assert.Single(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        await using var context = CreateContext();

        var controller = new AuthController(
            context, CreateConfiguration());

        var request = CreateParticipantRequest();
        request.Role = "Administrator";

        var result = await controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result);

        Assert.Empty(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        await using var context = CreateContext();

        var configuration = CreateConfiguration();

        var controller = new AuthController(
            context, configuration);

        var registration = CreateParticipantRequest();

        await controller.Register(registration);

        var loginRequest = new LoginRequest
        {
            Email = registration.Email,
            Password = registration.Password
        };

        var result = await controller.Login(loginRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(okResult.Value);

        var tokenProperty = okResult.Value.GetType()
            .GetProperty("token");

        Assert.NotNull(tokenProperty);

        var token = tokenProperty!.GetValue(okResult.Value) as string;

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        await using var context = CreateContext();

        var controller = new AuthController(
            context, CreateConfiguration());

        var registration = CreateParticipantRequest();

        await controller.Register(registration);

        var loginRequest = new LoginRequest
        {
            Email = registration.Email,
            Password = "WrongPassword123!"
        };

        var result = await controller.Login(loginRequest);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        await using var context = CreateContext();

        var controller = new AuthController(
            context, CreateConfiguration());

        var request = new LoginRequest
        {
            Email = "unknown@example.com",
            Password = "TestPassword123!"
        };

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
