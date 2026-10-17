
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaceDay.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services.AddControllers();

// Configure the SQL Server database.
var connectionString =
    builder.Configuration.GetConnectionString("RaceDayConnection")
    ?? throw new InvalidOperationException(
        "Database connection string 'RaceDayConnection' is missing.");

builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(connectionString));

// Load and validate JWT configuration.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

var signingKey = Encoding.UTF8.GetBytes(jwtKey);

if (signingKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT key must be at least 32 bytes long.");
}

// Configure JWT authentication and token validation.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(signingKey),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Register authorization policies and role checks used by controllers.
builder.Services.AddAuthorization();

// Configure Swagger/OpenAPI and JWT bearer authorization.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description =
            "API for managing race events, categories, enrolments, and results."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Enable Swagger in development environments.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authenticate requests before evaluating authorization requirements.
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes.
app.MapControllers();

app.Run();

// Expose Program to the integration test project.
public partial class Program { }
