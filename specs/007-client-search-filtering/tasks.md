# Tasks: Spec-007 Client Search, Filtering & Advanced Queries

Created: 2025-11-14

## Phase 1: Setup
- [ ] T001 Ensure PostgreSQL FTS extension enabled (ops note)
- [ ] T002 Add GIN FTS index for Client (CompanyName, ContactName, Email) in src/Backend/CRM.Infrastructure/Persistence/Migrations/<new>_AddClientFtsIndex.cs
- [ ] T003 Add composite/indexes (CreatedByUserId,DeletedAt; Email; CompanyName; City; State; StateCode) in src/Backend/CRM.Infrastructure/Persistence/Migrations/<new>_AddClientSearchIndexes.cs
- [ ] T004 Create SavedSearches table migration in src/Backend/CRM.Infrastructure/Persistence/Migrations/<new>_CreateSavedSearches.cs
- [ ] T005 Wire DbSet<SavedSearch> and model config in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs

## Phase 2: Foundational
- [ ] T006 Define DTOs: ClientDto (confirm fields), SavedSearchDto, FilterOptionsDto, PagedSearchResult in src/Backend/CRM.Application/Clients/DTOs/*.cs
- [ ] T007 Add constants/enums for SortBy in src/Backend/CRM.Shared/Constants/SortingConstants.cs
- [ ] T008 Add validators base utilities (date range helpers) in src/Backend/CRM.Application/Common/Validation/DateRangeValidators.cs
- [ ] T009 Add mapping profiles (Client -> ClientDto) in src/Backend/CRM.Application/Common/Mapping/MappingProfile.cs

## Phase 3: [US1] Client Search (Full-text + Filters)
- [ ] T010 [US1] Define SearchClientsQuery.cs in src/Backend/CRM.Application/Clients/Queries/SearchClientsQuery.cs
- [ ] T011 [P] [US1] Implement SearchClientsQueryValidator.cs in src/Backend/CRM.Application/Clients/Validation/SearchClientsQueryValidator.cs
- [ ] T012 [US1] Implement SearchClientsQueryHandler.cs (hybrid FTS/ILIKE; auth filter; stable tiebreakers) in src/Backend/CRM.Application/Clients/Queries/SearchClientsQueryHandler.cs
- [ ] T013 [US1] Add domain event ClientSearchExecuted in src/Backend/CRM.Domain/Events/ClientSearchExecuted.cs
- [ ] T014 [US1] Add optional handler LogSearchAnalyticsEventHandler in src/Backend/CRM.Application/Clients/Events/LogSearchAnalyticsEventHandler.cs
- [ ] T015 [US1] Expose GET /clients/search in src/Backend/CRM.Api/Controllers/ClientsSearchController.cs

## Phase 4: [US2] Autocomplete Suggestions
- [ ] T016 [US2] Define GetClientSearchSuggestionsQuery.cs in src/Backend/CRM.Application/Clients/Queries/GetClientSearchSuggestionsQuery.cs
- [ ] T017 [P] [US2] Implement GetClientSearchSuggestionsQueryValidator.cs in src/Backend/CRM.Application/Clients/Validation/GetClientSearchSuggestionsQueryValidator.cs
- [ ] T018 [US2] Implement GetClientSearchSuggestionsQueryHandler.cs in src/Backend/CRM.Application/Clients/Queries/GetClientSearchSuggestionsQueryHandler.cs
- [ ] T019 [US2] Expose GET /clients/search/suggestions in src/Backend/CRM.Api/Controllers/ClientsSearchController.cs

## Phase 5: [US3] Faceted Filter Options
- [ ] T020 [US3] Define GetFilterOptionsQuery.cs in src/Backend/CRM.Application/Clients/Queries/GetFilterOptionsQuery.cs
- [ ] T021 [US3] Implement GetFilterOptionsQueryHandler.cs (daily cache) in src/Backend/CRM.Application/Clients/Queries/GetFilterOptionsQueryHandler.cs
- [ ] T022 [US3] Expose GET /clients/search/filter-options in src/Backend/CRM.Api/Controllers/ClientsSearchController.cs

## Phase 6: [US4] Saved Searches
- [ ] T023 [US4] Create SavedSearch entity in src/Backend/CRM.Domain/Entities/SavedSearch.cs
- [ ] T024 [US4] Define SaveSearchFilterCommand.cs in src/Backend/CRM.Application/Clients/Commands/SaveSearchFilterCommand.cs
- [ ] T025 [P] [US4] Implement SaveSearchFilterRequestValidator.cs in src/Backend/CRM.Application/Clients/Validation/SaveSearchFilterRequestValidator.cs
- [ ] T026 [US4] Implement SaveSearchFilterCommandHandler.cs in src/Backend/CRM.Application/Clients/Commands/SaveSearchFilterCommandHandler.cs
- [ ] T027 [US4] Define GetSavedSearchesQuery.cs in src/Backend/CRM.Application/Clients/Queries/GetSavedSearchesQuery.cs
- [ ] T028 [US4] Implement GetSavedSearchesQueryHandler.cs in src/Backend/CRM.Application/Clients/Queries/GetSavedSearchesQueryHandler.cs
- [ ] T029 [US4] Define DeleteSavedSearchCommand.cs in src/Backend/CRM.Application/Clients/Commands/DeleteSavedSearchCommand.cs
- [ ] T030 [US4] Implement DeleteSavedSearchCommandHandler.cs in src/Backend/CRM.Application/Clients/Commands/DeleteSavedSearchCommandHandler.cs
- [ ] T031 [US4] Add events: SavedSearchCreated, SavedSearchDeleted in src/Backend/CRM.Domain/Events/*.cs
- [ ] T032 [US4] Expose POST /clients/search/save, GET /clients/search/saved, DELETE /clients/search/saved/{id} in src/Backend/CRM.Api/Controllers/ClientsSearchController.cs

## Phase 7: [US5] Export Results (CSV/Excel)
- [ ] T033 [US5] Define ExportClientsQuery.cs in src/Backend/CRM.Application/Clients/Queries/ExportClientsQuery.cs
- [ ] T034 [US5] Implement ExportClientsQueryHandler.cs (streaming, CSV default, 10k cap) in src/Backend/CRM.Application/Clients/Queries/ExportClientsQueryHandler.cs
- [ ] T035 [US5] Expose GET /clients/export in src/Backend/CRM.Api/Controllers/ClientsSearchController.cs

## Phase 8: Cross-cutting & Policy
- [ ] T036 Add rate limiting policies (search/suggest/export) in src/Backend/CRM.Api/Extensions/RateLimitingConfig.cs
- [ ] T037 Add authorization attributes/handlers (SalesRep vs Admin; userId param admin-only) in src/Backend/CRM.Api/Authorization/SearchPolicies.cs
- [ ] T038 Add caching infrastructure for filter options in src/Backend/CRM.Infrastructure/Caching/FilterOptionsCache.cs
- [ ] T039 Add configuration flags (export cap, cache TTL) in src/Backend/CRM.Api/appsettings.json
- [ ] T040 Update OpenAPI to match endpoints in specs/007-client-search-filtering/contracts/openapi.yaml

## Phase 9: Validation & QA
- [ ] T041 Manual validation script: seed sample clients and verify search timing/results in src/Backend/CRM.Api/Utilities/SampleDataSeeder.cs
- [ ] T042 Integration tests for search/filter/pagination in src/Backend/CRM.Tests.Integration/Clients/ClientSearchIntegrationTests.cs
- [ ] T043 Integration tests for saved searches in src/Backend/CRM.Tests.Integration/Clients/SavedSearchIntegrationTests.cs
- [ ] T044 Integration tests for export in src/Backend/CRM.Tests.Integration/Clients/ClientExportIntegrationTests.cs
- [ ] T045 Unit tests for validators in src/Backend/CRM.Tests.Unit/Clients/Validation/

## Dependencies & Order
- Phase 1 → Phase 2 → US1 → US2 → US3 → US4 → US5 → Cross-cutting → QA
- Parallel opportunities marked [P] within phases (validators, some endpoints)

## MVP Scope
- Complete US1 (search) end-to-end with validator, handler, endpoint, and indexes.
