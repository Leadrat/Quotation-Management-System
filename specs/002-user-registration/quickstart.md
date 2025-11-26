# Quickstart: Spec-002 User Registration

This guide explains how to run and test the registration flows for Spec-002.

## Prerequisites
- .NET 8 SDK installed
- PostgreSQL accessible; set POSTGRES_CONNECTION
- Migration applied (use CRM.Migrator)

## Apply/Refresh Database
```
$env:POSTGRES_CONNECTION = "Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
dotnet run --project src/Backend/CRM.Migrator/CRM.Migrator.csproj -c Release
```

## Test Client Registration (API contract only)
- POST /api/v1/auth/register
- Body:
```
{
  "email": "test.client@example.com",
  "password": "TestPass@123",
  "firstName": "Test",
  "lastName": "Client"
}
```
- Expected: 201 Created
- Notes: Email verification is required before first login (handled in Spec-003)

## Test Admin Create User (API contract only)
- POST /api/v1/users (requires Admin token)
- Body:
```
{
  "email": "priya@crm.com",
  "password": "AdminCreate@123",
  "firstName": "Priya",
  "lastName": "Singh",
  "mobile": "+919876543210",
  "roleId": "FAE6CEDB-42FD-497B-85F6-F2B14ECA0079",
  "reportingManagerId": "EB4F2FCA-B9F6-46CE-BB6F-2EA0689ABE9F"
}
```
- Expected: 201 Created

## Notes
- Captcha is not required in Spec-002; rely on IP rate limiting.
- Disposable email domains are allowed in Spec-002.
- Email verification enforcement is required but token mechanics are defined in Spec-003.

## Curl examples

Client register:
```
curl -X POST "http://localhost:5000/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test.client@example.com",
    "password": "TestPass@123",
    "firstName": "Test",
    "lastName": "Client"
  }'
```

Admin create user:
```
curl -X POST "http://localhost:5000/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "X-Admin: true" \
  -d '{
    "email": "rep@crm.local",
    "password": "Rep@123456",
    "firstName": "Riya",
    "lastName": "Kapoor",
    "roleId": "D8B159E4-6891-4B3A-99DB-27C29B9D9D1A",
    "reportingManagerId": "EB4F2FCA-B9F6-46CE-BB6F-2EA0689ABE9F"
  }'
```

## Troubleshooting
- 409 on registration: email already exists (case-insensitive).
- 400 on registration: weak password, ensure it meets policy.
- 422 on admin create: role/manager validation failed.
