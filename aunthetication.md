
# RaceDay API — Authentication Guide

## 1. Overview

The RaceDay API uses JSON Web Tokens (JWT) to authenticate users and protect authorised endpoints.

Users register and log in to obtain an access token. The token must be included in requests to protected endpoints.

## 2. Authentication Process

1. Register a new user using the registration endpoint.
2. Log in using the registered account credentials.
3. Retrieve the JWT access token returned by the login endpoint.
4. Open Swagger UI and select **Authorize**.
5. Enter the token using the format required by Swagger. For the configured Bearer authentication scheme, enter the token itself.
6. Execute requests to protected endpoints.

## 3. JWT Configuration

The API reads its JWT settings from application configuration.

The required configuration keys are:

- `Jwt:Key` — secret key used to sign and validate tokens.
- `Jwt:Issuer` — identifies the issuer of the token.
- `Jwt:Audience` — identifies the intended recipient of the token.

The signing key must contain at least 32 bytes.

Example configuration structure:

```json
{
  "Jwt": {
    "Key": "REPLACE_WITH_A_SECURE_SECRET_OF_AT_LEAST_32_BYTES",
    "Issuer": "RaceDayAPI",
    "Audience": "RaceDayUsers"
  }
}
```

**Security warning:** The values above are an example configuration structure. Use the same issuer and audience values that your API uses when generating tokens. Never commit a real JWT signing key, password, or other secret to GitHub.

For development, keep secrets in User Secrets or environment variables rather than committing them to `appsettings.json`.

## 4. Sending an Authenticated Request

Include the access token in the HTTP `Authorization` header:

```http
Authorization: Bearer YOUR_ACCESS_TOKEN
```

Example using cURL:

```bash
curl -X GET "https://localhost:YOUR_PORT/api/YOUR_PROTECTED_ENDPOINT" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

Replace `YOUR_PORT` and `YOUR_PROTECTED_ENDPOINT` with the values for your running API.

## 5. Using Swagger UI

1. Run the RaceDay API.
2. Open the Swagger UI URL shown by Visual Studio.
3. Expand the login endpoint and submit valid credentials.
4. Copy the JWT token returned by the API.
5. Click **Authorize**.
6. Paste the token and confirm.
7. Run a protected endpoint.

The API validates the token's signature, issuer, audience, and expiration.

## 6. Authentication Responses

| HTTP status | Meaning |
|---|---|
| `200 OK` | The request succeeded. |
| `400 Bad Request` | The request contains invalid input. |
| `401 Unauthorized` | Authentication is missing, invalid, or expired. |
| `403 Forbidden` | The user is authenticated but lacks the required permission. |
| `409 Conflict` | The request conflicts with existing data, such as attempting to register a duplicate account, if handled by the endpoint. |

Actual responses depend on the endpoint implementation.

## 7. Security Best Practices

- Keep JWT signing keys and user passwords secret.
- Use HTTPS when sending credentials or access tokens.
- Use short-lived access tokens where appropriate.
- Never store passwords in plain text.
- Validate user input before processing requests.
- Protect restricted endpoints with `[Authorize]`.
- Use role-based authorization where different user roles have different permissions.
- Never commit credentials, signing keys, or personal data to source control.

## 8. Troubleshooting

**401 Unauthorized**
- Confirm that a token was supplied.
- Check that the token has not expired.
- Verify the issuer, audience, and signing key configuration.
- Confirm that the request includes the `Bearer` authentication scheme.

**403 Forbidden**
- Check whether the authenticated user has the necessary role or permission.

**API fails during startup**
- Ensure the database connection string and required JWT configuration values are present.
- Confirm that the signing key is at least 32 bytes long.

## 9. Related Configuration

Authentication is configured in `Program.cs` using ASP.NET Core JWT Bearer authentication. Authorization is enabled through ASP.NET Core authorization services and middleware.

For endpoint-specific registration, login, and user-profile routes, refer to the controllers in the `RaceDay.API` project.
```


