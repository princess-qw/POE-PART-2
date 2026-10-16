# RaceDay API Testing Guide

## Purpose

Testing verifies authentication, authorisation, event management and participant enrolment behaviour.

## Manual Checks Already Performed

- A Profile request without a token returned HTTP 401.
- A GET Events request returned HTTP 200.
- An Organiser account is available for testing.

## Required Automated Tests

- Registration with valid and invalid input
- Login with valid and invalid credentials
- Access to protected endpoints without authentication
- Event management by an Organiser
- Rejection of event management requests from unauthorised roles
- Participant enrolment
- Verification that enrolments are persisted

## Running Tests

Once a .NET test project is present in the repository, run the test suite with:

`dotnet test`

## Reporting Results

Record actual test results. Do not report a test as passing unless it has been executed successfully.
