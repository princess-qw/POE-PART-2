
# Security Policy

## Reporting a Security Issue

Do not publish passwords, access tokens, database credentials, or JWT
signing keys in issues or public discussions.

Report suspected vulnerabilities privately to the repository owner.

## Protecting Secrets

- Never commit real credentials or JWT signing keys.
- Keep development secrets out of tracked configuration files.
- Use environment variables or .NET User Secrets for local development.
- Replace any secret that has accidentally been exposed.
- Use HTTPS when transmitting credentials and bearer tokens.

## Authentication

The RaceDay API uses JWT bearer authentication.

Protected endpoints should validate tokens and enforce the relevant
authorization requirements.

## Database Security

- Keep database credentials private.
- Grant the application only the database permissions it needs.
- Validate incoming data before processing it.
- Avoid returning internal exception details to API clients.
