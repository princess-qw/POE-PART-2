
# RaceDay API Roles and Permissions

## Organiser

Organisers are intended to manage events, including creating and updating event information where authorised by the API.

## Participant

Participants are intended to enrol in events and access permitted participant functionality.

## Authentication

Protected endpoints require valid authentication. Requests without valid credentials should be rejected.

## Authorisation

Role-protected endpoints must check that the authenticated user has the required role. Users with other roles must not be allowed to perform restricted operations.

## Verification

Automated tests should check permitted and forbidden requests for each role. Actual permissions must match the implemented API controllers.
