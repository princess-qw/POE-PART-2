
# RaceDay API Endpoints

## Overview

RaceDay API provides functionality for authentication, event management and participant enrolments.

## Authentication

The API includes user registration and login. Refer to the implemented authentication controller and Swagger/OpenAPI documentation for the exact routes and request formats.

## User Profile

During manual testing, a Profile request without an authentication token returned HTTP 401.

## Events

During manual testing, a GET Events request returned HTTP 200.

Event creation, updating and deletion should be tested with an authorised Organiser account and with accounts that lack the required role.

## Enrolments

Participant enrolment functionality and database persistence must be verified against the implemented enrolment endpoint and database.

## API Reference

Use the application's Swagger/OpenAPI page to confirm actual routes, HTTP methods, parameters and response codes.
