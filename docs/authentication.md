# RaceDay API Authentication

## Overview

RaceDay API uses JSON Web Tokens (JWT) to support authentication for protected endpoints.

## Logging In

A user submits their login credentials to the API's login endpoint. If the credentials are valid, the API can return an authentication token.

## Using the Token

For protected endpoints, the client sends the token in the HTTP Authorization header:

`Authorization: Bearer YOUR_TOKEN`

Replace `YOUR_TOKEN` with a valid token issued by the API.

## Unauthenticated Requests

Requests to protected endpoints without a valid token should be rejected. During initial API checks, the Profile endpoint returned HTTP 401 when accessed without a token.

## Role-Based Access

Some operations are restricted by user role. Organiser-only event management operations should reject users who do not have the required role.

Participant enrolment operations should follow the authorisation rules implemented by the API.

## Security Notes

- Never commit real passwords, signing keys, or access tokens to the repository.
- Use valid credentials when testing authentication.
- Verify protected endpoints with both authorised and unauthorised requests.
