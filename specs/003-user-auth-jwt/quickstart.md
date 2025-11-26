# Quickstart: Spec-003 Authentication & JWT

This guide shows how to configure JwtSettings, run the API, and test login, refresh, and logout.

## Prerequisites
- .NET 8 SDK
- Database with Users table seeded (Spec-001/002 complete)
- Environment variables:
  - POSTGRES_CONNECTION
  - JWT__SECRET (≥32 chars)
  - JWT__ISSUER=crm.system
  - JWT__AUDIENCE=crm.api
  - JWT__ACCESS_TOKEN_EXP=3600
  - JWT__REFRESH_TOKEN_EXP=2592000

## Run API
```
$env:POSTGRES_CONNECTION = "Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
$env:JWT__SECRET = "change-me-32+chars-secret" 
$env:JWT__ISSUER = "crm.system"
$env:JWT__AUDIENCE = "crm.api"
$env:JWT__ACCESS_TOKEN_EXP = "3600"
$env:JWT__REFRESH_TOKEN_EXP = "2592000"
dotnet run --project src/Backend/CRM.Api/CRM.Api.csproj -c Release
```

## Login
```
curl -X POST "http://localhost:5000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "priya.singh@crm.com",
    "password": "SecurePass@123"
  }'
```
- Response: 200 with accessToken, refreshToken (or refresh cookie if configured)

## Refresh token
- Cookie flow (browser): send request with refresh cookie automatically
- JSON fallback:
```
curl -X POST "http://localhost:5000/api/v1/auth/refresh-token" \
  -H "Content-Type: application/json" \
  -d '{ "refreshToken": "eyJ..." }'
```
- Response: 200 with new accessToken (and rotated refreshToken if returned)

## Logout
```
curl -X POST "http://localhost:5000/api/v1/auth/logout" \
  -H "Authorization: Bearer {accessToken}"
```
- Response: 200

## Notes
- Access token accepted via Authorization header (Bearer) or HttpOnly cookie.
- If using cookie for access token on state-changing routes, enable CSRF protections.
- Refresh token uses HttpOnly, Secure, SameSite=None cookie (preferred); JSON fallback allowed for non-browser clients.
