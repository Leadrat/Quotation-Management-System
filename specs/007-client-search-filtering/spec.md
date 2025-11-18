# Spec-007: Client Search, Filtering & Advanced Queries

- Project: CRM Quotation Management System
- Group: Client Management (Group 2 of 11)
- Priority: MEDIUM (Phase 1, after Spec 6)
- Dependencies: Spec-006 (Client Entity & CRUD Operations)
- Related Specs: Spec-008 (ClientHistory), Spec-009 (QuotationEntity)

## Overview
This spec defines advanced search, filtering, and query capabilities for the Client entity. It enables Sales Representatives to quickly find clients by company name, email, GST number, location, or custom criteria. This improves productivity by reducing manual scrolling and searching, especially when dealing with large client databases. Includes full-text search, faceted filtering, sorting, and saved search filters.

## Key Features
- Full-text search by company name, contact name, email
- Advanced filters (location, GST, creation date, last updated)
- Faceted filtering (by state, city, created date range)
- Sorting (by name, creation date, update date, alphabetical)
- Pagination with configurable page size
- Search suggestions/autocomplete
- Saved search filters (personal quick filters)
- Export search results (CSV, Excel)
- Search history tracking (last N searches)

## JTBD Alignment
- Persona: Sales Representative
- Primary JTBD: "I want to find the right client quickly without scrolling through hundreds of records"
- Expected Time: Find client in <30 seconds with search/filter
- Success Metric: "First result is almost always what I'm looking for"

## Business Value
- Increases sales rep productivity (faster client lookup)
- Reduces user frustration (clear, relevant results)
- Enables data-driven filtering (find similar clients)
- Supports large-scale client databases (100s or 1000s of records)
- Improves reporting capability (filtered client exports)

---

## Search Query Specifications

### [Query 1] SearchClientsQuery (Full-text search + filters)
- Namespace: `CRM.Application.Clients.Queries`
- Purpose: Complex search across clients with multiple filters

Inputs:
- `SearchTerm` (string, optional)
- `City` (string, optional)
- `State` (string, optional)
- `StateCode` (string, optional)
- `Gstin` (string, optional)
- `CreatedByUserId` (Guid, optional, internal) — API surface uses `userId` (admin-only) and maps to this field
- `CreatedDateFrom` (DateTime, optional)
- `CreatedDateTo` (DateTime, optional)
- `UpdatedDateFrom` (DateTime, optional)
- `UpdatedDateTo` (DateTime, optional)
- `SortBy` (string, default `CreatedAtDesc`): `NameAsc|NameDesc|CreatedAtDesc|CreatedAtAsc|UpdatedAtDesc|EmailAsc`
- `PageNumber` (int, default 1)
- `PageSize` (int, default 10, max 100)
- `IncludeDeleted` (bool, default false)

Output:
- `PagedSearchResult<ClientDto>` => `Data`, `TotalCount`, `PageNumber`, `PageSize`, `HasMore`

Handler:
- `SearchClientsQueryHandler : IQueryHandler<SearchClientsQuery, PagedSearchResult<ClientDto>>`

Implementation Logic:
1. Base: `FROM Clients WHERE DeletedAt IS NULL` (unless `IncludeDeleted`)
2. Authorization:
   - Non-admin: filter `CreatedByUserId = CurrentUserId`
   - Admin: if API provided `userId`, map to `CreatedByUserId = userId`
3. Filters:
   - `SearchTerm`: `(CompanyName ILIKE %term% OR ContactName ILIKE %term% OR Email ILIKE %term%)`
   - `City`: `City ILIKE %city%`
   - `State`: `State ILIKE %state%`
   - `StateCode`: `StateCode = @StateCode`
   - `Gstin`: `Gstin LIKE %gstin%`
   - `CreatedDateFrom/To`, `UpdatedDateFrom/To` ranges
4. Sorting per `SortBy`; apply stable tiebreakers: `CreatedAt DESC`, then `ClientId ASC` for determinism
5. Count total
6. Paginate (Skip/Take)
7. Map to `ClientDto`
8. Return paged result

Performance:
- Compound indexes for common combos
- Limit `PageSize` to 100
- Suggestion caches hourly

### [Query 2] GetClientSearchSuggestionsQuery (Autocomplete)
- Namespace: `CRM.Application.Clients.Queries`
- Purpose: Provide search suggestions/autocomplete as user types

Inputs:
- `SearchTerm` (string, required, min 2)
- `MaxSuggestions` (int, default 10)
- `Type` (enum): `CompanyName|Email|City|ContactName`

Output:
- `List<string>` suggestions

Handler:
- `GetClientSearchSuggestionsQueryHandler : IQueryHandler<GetClientSearchSuggestionsQuery, List<string>>`

Implementation:
1. Validate term length ≥2
2. Query distinct values by `Type` with `ILIKE %term%`
3. Relevance: prefix match first, then partial
4. Limit to `MaxSuggestions`

Performance: index on searchable columns; hourly cache/materialized view

### [Query 3] GetFilterOptionsQuery (Faceted filtering)
- Namespace: `CRM.Application.Clients.Queries`
- Purpose: Get available filter options for UI dropdowns

Output (`FilterOptionsDto`):
- `States`: `[ { State, Count } ]`
- `Cities`: `[ { City, Count } ]` (top 20)
- `CreatedDateRanges`: predefined ranges
- `StateCodes`: all valid codes

Handler: `GetFilterOptionsQueryHandler`

Implementation:
- Distinct with counts for states/cities (exclude `DeletedAt != NULL`)
- Predefined ranges: Last 7/30/90 days, This year
- Load codes from constants; cache daily

### [Query 4] GetSavedSearchesQuery
- Namespace: `CRM.Application.Clients.Queries`
- Purpose: Retrieve user's saved search filters
- Input: `UserId` (Guid)
- Output: `List<SavedSearchDto>` ordered by `CreatedAt DESC`
- Handler: `GetSavedSearchesQueryHandler`

Implementation:
- Query `SavedSearches` where `UserId` and `IsActive=true`

---

## Database Table: SavedSearches (For saved filters)
- `SavedSearchId` (UUID, PK)
- `UserId` (UUID, FK -> Users.UserId)
- `SearchName` (VARCHAR(255), required)
- `FilterCriteria` (JSONB, required)
- `SortBy` (VARCHAR(50), optional)
- `IsActive` (BOOLEAN, default true)
- `CreatedAt` (TIMESTAMPTZ, NOT NULL)
- `UpdatedAt` (TIMESTAMPTZ, NOT NULL)

Indexes:
- PK(SavedSearchId)
- IDX(UserId)
- IDX(IsActive)

---

## Command Specifications

### [Command 1] SaveSearchFilterCommand
- Namespace: `CRM.Application.Clients.Commands`
- Purpose: Save current search/filter combination for quick reuse
- Properties: `SearchName`, `FilterCriteria`, `SortBy`, `UserId`
- Handler: `SaveSearchFilterCommandHandler : ICommandHandler<SaveSearchFilterCommand, SavedSearchDto>`

Implementation:
1. Validate `SearchName` ≤255 and not empty
2. Validate `FilterCriteria` non-empty
3. Optional duplicate name check (warn but allow)
4. Persist entity and emit `SavedSearchCreated`

### [Command 2] DeleteSavedSearchCommand
- Namespace: `CRM.Application.Clients.Commands`
- Purpose: Remove saved search filter
- Properties: `SavedSearchId`, `UserId`
- Handler: `DeleteSavedSearchCommandHandler`

Implementation:
1. Load entity
2. Verify ownership (or admin)
3. Soft delete or `IsActive=false`
4. Emit `SavedSearchDeleted`

---

## API Endpoints

### [Endpoint 1] GET /api/v1/clients/search
Auth: Required. Roles: SalesRep, Admin
Query params: as per SearchClientsQuery
Response: Paged list with metadata and `searchExecutedIn`
Errors: 400 for invalid filters

### [Endpoint 2] GET /api/v1/clients/search/suggestions
Auth: Required. Roles: SalesRep, Admin
Query: `term`, `type`, `maxSuggestions`

### [Endpoint 3] GET /api/v1/clients/search/filter-options
Auth: Required. Roles: SalesRep, Admin

### [Endpoint 4] POST /api/v1/clients/search/save
Auth: Required. Roles: SalesRep, Admin

### [Endpoint 5] GET /api/v1/clients/search/saved
Auth: Required. Roles: SalesRep, Admin
Behavior: Returns the current user's saved searches. Admins may optionally pass `userId` to view another user's saved searches; if omitted, defaults to current user.

### [Endpoint 6] DELETE /api/v1/clients/search/saved/{savedSearchId}
Auth: Required. Owner or Admin

### [Endpoint 7] GET /api/v1/clients/export
Auth: Required. Roles: SalesRep, Admin
Query: same filter params + `format=csv|excel`
Response: file download (streamed). Default format is CSV. Hard cap at 10,000 rows per export; larger result sets require additional filters or multiple exports.

---

## Validation Rules (FluentValidation)

### SearchClientsQueryValidator
- `SearchTerm`: MaxLength(255)
- `PageSize`: 1..100
- `PageNumber`: >0
- Date ranges: `CreatedDateFrom <= CreatedDateTo`, `UpdatedDateFrom <= UpdatedDateTo`

### GetClientSearchSuggestionsQueryValidator
- `SearchTerm`: required, 2..255
- `MaxSuggestions`: 1..50

---

## Performance Optimization
- Indexing: `CreatedByUserId, DeletedAt`, `CompanyName`, `Email`, `City`, `State`, `StateCode`
- Full‑text index on `CompanyName + ContactName + Email` (PostgreSQL TSVECTOR, GIN)
- Hybrid search strategy: prefer FTS for normal terms; fallback to ILIKE for very short terms (<3 chars) or when FTS yields zero results.
- Query optimizations: projections, limit, lazy navs
- Caching: filter options (daily), suggestions (hourly)
- Export: stream results; enforce 10k row cap to protect resources
- Rate limiting: search=30/min/user; suggestions=60/min/user; export=10/min/user
- Search history storage minimal (last 20 per user); no background purge required beyond rolling retention.

## Domain Events
- `ClientSearchExecuted`
- `SavedSearchCreated`
- `SavedSearchDeleted`

## Frontend (scope-only)
- Client list with search box (autocomplete), filter sidebar, sortable paginated results, saved searches, export.
- Debounce 300–500ms, loading skeleton, highlight matches, remember last filters.

## Functional Requirements
- Full-text search works across name, contact, email
- When `SearchTerm` length < 3, or when FTS returns zero matches, the system MUST fallback to case‑insensitive partial matching (ILIKE) and return results accordingly.
- Filters: state, city, GST, date range
- Sorting by name/date/email with stable tiebreakers: `CreatedAt DESC`, then `ClientId ASC`
- Pagination with configurable size
- Autocomplete suggestions while typing
- Facet options API
- Save, list, delete saved searches (owner or admin)
- Saved searches are private per user by default; Admins can view any user's saved searches and delete if necessary.
- Track search history: retain last 20 searches per user; only the owner can view their history.
- Export CSV/Excel per current filters
- Authorization enforced (SalesRep only own clients; Admin can filter by user)
- Deleted clients excluded by default

## Security
- Users can only search their own clients (unless admin)
- Users can only delete own saved searches (unless admin)
- Search history is private to the owner; admins do not see user history by default.
- Audit trail logs search operations (optional)
- Input validation prevents injection attacks
- Rate limiting prevents API abuse
- Find a specific client in < 30s with search/filter
- P95 search endpoint latency < 300ms @ 10k records
- Export returns in < 5s for 5k filtered rows (streamed)
- 0 security violations in auth/authorization
- ≥ 90% of searches yield expected client in top 3 results

## Assumptions
- PostgreSQL used with TSVECTOR full-text support
- Hybrid FTS + ILIKE fallback accepted for relevance and resilience
- Spec-006 CRUD and ClientDto delivered
- Role enforcement per Spec-004; built-in roles immutable
- Timezone UTC for date filters

## Edge Cases
- Empty results → clear message, suggestions
- Very short search terms (<3 chars) → use ILIKE fallback to avoid poor FTS behavior
- Overlapping filters → still return valid (empty) without error
- Invalid date ranges → 400 with field errors
- Large page size > 100 → 400

## Out of Scope
- ML-based ranking, sharing saved searches (Phase 2+)

## Delivery
- Queries/Commands, DTOs, Validators, Migration for SavedSearches, Controller endpoints, Unit + Integration tests (>85% coverage)

## Clarifications

### Session 2025-11-14
- Q: Full‑text search approach and fallback? → A: Hybrid – Prefer PostgreSQL FTS (TSVECTOR + GIN). Fallback to ILIKE when term length < 3 or when FTS yields zero results.
- Q: Export default and size cap? → A: Default CSV; cap 10k rows per export.
- Q: Saved search ownership/visibility? → A: Private per user; Admin can view all (optional `userId` on GET /saved).
- Q: Sorting tiebreakers? → A: After primary SortBy, use `CreatedAt DESC`, then `ClientId ASC`.
- Q: Search history retention/visibility? → A: Keep last 20 searches per user; visible only to the user.
