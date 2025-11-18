# Quickstart: Spec-008 Client History & Activity Log

## Prerequisites
- Backend solution built from `main` with Specs 001–007 applied.
- PostgreSQL 14+ with `citext`, `pg_trgm`, and `unaccent` extensions enabled.
- Feature branch `008-client-history` checked out.

## 1. Database Prep
```bash
dotnet ef migrations add AddClientHistoryTables --project src/Backend/CRM.Infrastructure --startup-project src/Backend/CRM.Api
dotnet ef database update --project src/Backend/CRM.Infrastructure --startup-project src/Backend/CRM.Api
```
- Verify new tables: `ClientHistories`, `SuspiciousActivityFlags`.
- Confirm indexes: `(ClientId, CreatedAt)`, `(ActorUserId, CreatedAt)`, JSONB GIN on metadata.

## 2. Application Wiring
1. **Domain & Application**
   - Add `ClientHistory` entity + configurations.
   - Publish history events from existing CRUD handlers (Specs 006–007).
   - Implement MediatR handlers for new queries/commands:
     - `GetClientHistoryQuery`, `GetClientTimelineQuery`, `GetUserActivityQuery`
     - `GetSuspiciousActivityQuery`, `ExportClientHistoryQuery`, `RestoreClientCommand`
2. **Infrastructure**
   - Repositories/specifications for paginated queries.
   - Background job (Hangfire/Quartz) for suspicious correlation every 5 minutes.
3. **API**
   - Add `ClientHistoryController` (or extend `ClientsController`) with endpoints per OpenAPI contract.
   - Stream CSV responses using `FileCallbackResult`.

## 3. Configuration
- `appsettings.json`
  - `History:RetentionYears = 7`
  - `History:RestoreWindowDays = 30`
  - `History:DefaultPageSize = 20` / `History:MaxPageSize = 100`
  - `History:ExportRowLimit = 5000`
  - `SuspiciousActivity:InlineThreshold = 7`
  - `SuspiciousActivity:RapidChangeThresholdPerHour = 10`
  - `SuspiciousActivity:OddHoursStart = "22:00"` / `OddHoursEnd = "05:00"`
  - `SuspiciousActivity:AllowedIpCidrs = ["10.0.0.0/8","192.168.0.0/16"]`
  - `SuspiciousActivity:BatchJobCron = "*/5 * * * *"`
- Register caching (IMemoryCache) for timeline summaries if needed.

## 4. Testing Checklist
- **Unit**: History factory, diff builder, validators (restore reason, export filters).
- **Integration**: Timeline, restore, export, suspicious activity endpoints.
- **Performance**: Load 10k history rows; ensure timeline API p95 ≤ 5s.

## 5. Verification Steps
1. Seed sample data via `DbSeeder.SeedHistoryDemo()` (add temp helper).
2. `GET /api/v1/clients/{id}/history`
3. `POST /api/v1/clients/{id}/restore` with Admin token
4. `GET /api/v1/admin/suspicious-activity?minScore=7`
5. `GET /api/v1/clients/history/export?clientIds={id}&format=csv`

## 6. QA Notes
- Ensure SalesReps cannot access other reps’ history.
- Attempt restore after 31 days → expect 400 + unchanged row.
- Disable access logging flag → verify Accessed events disappear.
- Confirm CSV export respects 5k row cap with friendly message when exceeded.

