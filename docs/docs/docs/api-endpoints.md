# RaceDay API Endpoints

## Overview

RaceDay API exposes HTTP endpoints for user authentication, event management and participant enrolments.

## Authentication

The API includes registration and login functionality. Consult the API controllers and Swagger documentation for the exact routes, request fields and response formats.

## Events

The Events API provides event-related functionality. The API has been checked successfully with a GET Events request returning HTTP 200.

Event creation, updates and deletion should be tested using an appropriately authorised Organiser account.

## User Profile

A profile request made without a token returned HTTP 401 during initial manual testing.

## Enrolments

Participant enrolment functionality must be verified against the enrolment controller and database.

## API Reference

Run the application and use its Swagger/OpenAPI page to inspect the actual routes and request schemas. Do not assume routes or HTTP methods that are not implemented by the application.
