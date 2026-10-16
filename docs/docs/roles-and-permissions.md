# RaceDay API Roles and Permissions

## Overview

RaceDay API uses authentication and role-based authorisation to control access to protected operations.

## Organiser

The Organiser role is intended for event management operations, such as creating, updating and managing events, subject to the permissions implemented by the API.

## Participant

The Participant role is intended for users who participate in events and enrol in available events, subject to the permissions implemented by the API.

## Access Rules

- Protected endpoints should reject unauthenticated requests.
- Organiser-only endpoints should reject users without the Organiser role.
- Participant-only endpoints should reject users without the Participant role.
- Public endpoints should remain available without authentication where configured.

## Verification

Automated tests must confirm that permitted operations succeed and restricted operations are denied. The actual rules must match the API implementation.
