# Quickstart: Implement Spec-007 Client Search

This guide outlines a minimal path to implement Spec-007.

## Prereqs
- Spec-006 delivered (Client entity + CRUD + ClientDto)
- PostgreSQL with FTS extensions enabled

## Steps

1. Data & Indexes
- Add TSVECTOR column or expression index for (CompanyName, ContactName, Email)
- Add indexes: CreatedByUserId+DeletedAt, Email, CompanyName, City, State, StateCode

2. Application Layer (CQRS)
- Queries:
  - SearchClientsQuery + Handler (hybrid FTS/ILIKE, auth filter, stable tiebreakers)
  - GetClientSearchSuggestionsQuery + Handler (DISTINCT, prefix priority)
  - GetFilterOptionsQuery + Handler (distinct + counts, cached daily)
  - GetSavedSearchesQuery + Handler (current user; admin can pass userId)
- Commands:
  - SaveSearchFilterCommand + Handler
  - DeleteSavedSearchCommand + Handler (soft delete or IsActive=false)
- Validators (FluentValidation): per spec

3. Persistence
- Migration: SavedSearches table (columns per spec)
- Optional table for SearchHistory or application-level rolling retention (last 20 per user)

4. API Layer
- Controller: ClientsSearchController
  - Implement endpoints as per contracts/openapi.yaml
  - Stream export; enforce 10k row cap; default CSV

5. Frontend (scope only)
- List page with search input, debounce 300–500ms
- Filter sidebar facets from filter-options API
- Table with pagination and sorting
- Saved searches: list/delete, save current filter
- Export button with CSV/Excel

6. Observability
- Log ClientSearchExecuted with execution time, filters, result count

7. Tests
- Unit: queries/commands/validators
- Integration: search/filtering/saved searches/export

## Done Criteria
- All endpoints return expected payloads
- Performance targets met (P95 latency < 300ms @10k rows)
- Unit + integration tests ≥ 85% coverage
