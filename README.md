# RaceDay API

## Project Overview

RaceDay API is an ASP.NET Core Web API designed to manage race events, participation categories, enrolments, and race results. It uses Entity Framework Core to interact with a SQL Server database and JWT bearer authentication to protect authorised endpoints.

## Technologies Used

- C#
- ASP.NET Core Web API (.NET 9)
- Entity Framework Core
- Microsoft SQL Server
- JWT bearer authentication
- Swagger / OpenAPI
- xUnit automated tests
- GitHub Actions

## Main Features

- User registration and login
- JWT-based authentication
- Protected profile endpoint
- Race event management
- Race category management
- Participant enrolments
- Race result management
- Role-based access control
- API documentation through Swagger
- Automated build and test workflow

## Prerequisites

- Visual Studio 2022
- .NET 9 SDK
- SQL Server and SQL Server Management Studio (SSMS)

## Database Configuration

1. Create or configure the `RaceDayDB` database in SQL Server.
2. Update the connection string in `appsettings.json` to match your SQL Server instance.
3. Configure the required Entity Framework Core migrations or database setup scripts.
4. Never commit real passwords, production connection strings, or JWT signing secrets.

## Running the API

1. Clone or download this repository.
2. Open the solution in Visual Studio 2022.
3. Restore the NuGet packages.
4. Configure the SQL Server connection string.
5. Apply the required database migrations or SQL scripts.
6. Set `RaceDay.API` as the startup project.
7. Run the application.
8. Open the Swagger URL displayed by the application.

## Authentication

Register or log in using the authentication endpoints. Copy the JWT access token returned by login and use the **Authorize** option in Swagger to test protected endpoints.

Endpoints that require authentication should reject requests without a valid token. Endpoints restricted by role should also reject users who do not have the required role.

## API Resources

| Resource | Purpose |
|---|---|
| Authentication | Register users and obtain access tokens |
| Profile | Retrieve the authenticated user's profile |
| Events | Manage race events |
| Categories | Manage race categories |
| Enrolments | Manage participant enrolments |
| Results | Manage race results |

Refer to Swagger/OpenAPI for the actual routes, request bodies, response codes, and authorisation requirements.

## Automated Testing

The test project is `RaceDay.Test`.

GitHub Actions automatically runs the configured workflow on pushes and pull requests to the `main` or `master` branch. The workflow restores dependencies, builds the solution, and runs the automated tests.

## GitHub Actions Evidence

Add a screenshot of the successful GitHub Actions workflow here.

`[Insert screenshot of successful workflow run]`

## API Testing Evidence

Add screenshots showing successful and unsuccessful API requests, including authentication and authorisation checks.

https://localhost:7230/swagger/index.html

## Demo Video

Add the link to the assignment demonstration video here.

`[Insert demo video link]`

## Author

RaceDay API — Programming Part 2 Assignment
