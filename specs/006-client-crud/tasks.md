# Tasks: Client Entity & CRUD Operations (Spec-006)

Created: 2025-11-13
Branch: 006-client-crud
Spec: specs/006-client-crud/spec.md

## Phase 1: Setup

- [X] T001 Ensure feature branch is active `006-client-crud`
- [X] T002 Add specs/006-client-crud/contracts/clients.openapi.yaml to API Swagger in src/Backend/CRM.Api/Program.cs
- [ ] T003 Add constitution gating checklist to PR template (reference Spec-006) in .github/PULL_REQUEST_TEMPLATE.md

## Phase 2: Foundational

- [X] T004 Create Client entity class in src/Backend/CRM.Domain/Entities/Client.cs
- [X] T005 Create EF configuration in src/Backend/CRM.Infrastructure/EntityConfigurations/ClientEntityConfiguration.cs
- [X] T006 Create StateCodeConstants in src/Backend/CRM.Application/Common/Validation/StateCodeConstants.cs
- [X] T007 Add migration CreateClients table in src/Backend/CRM.Infrastructure/Migrations/<timestamp>_CreateClients.cs
- [X] T008 Apply indexes (lower(email) partial unique, gstin, created/updated/deleted, owner+deleted) in migration file path T007
- [X] T009 Wire DbSet<Client> and OnModelCreating in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T010 [P] Add AutoMapper profile for Client→ClientDto in src/Backend/CRM.Application/Clients/Dtos/ClientDto.cs and mapping profile

## Phase 3: US1 Create client (P1)
Goal: Authenticated SalesRep/Admin can create a client with required fields; ownership set; validations applied.

- [X] T011 [US1] Define CreateClientCommand in src/Backend/CRM.Application/Clients/Commands/CreateClientCommand.cs
- [X] T012 [US1] Implement CreateClientCommandHandler in src/Backend/CRM.Application/Clients/Commands/Handlers/CreateClientCommandHandler.cs
- [X] T013 [US1] Implement CreateClientRequest (API DTO) in src/Backend/CRM.Application/Clients/Commands/CreateClientRequest.cs
- [X] T014 [US1] Implement CreateClientRequestValidator in src/Backend/CRM.Application/Clients/Validators/CreateClientRequestValidator.cs
- [X] T015 [US1] Implement ClientValidator (shared rules) in src/Backend/CRM.Application/Clients/Validators/ClientValidator.cs
- [X] T016 [US1] Add DuplicateEmailException in src/Backend/CRM.Application/Clients/Exceptions/DuplicateEmailException.cs
- [X] T017 [US1] Add domain event ClientCreated in src/Backend/CRM.Domain/Events/ClientCreated.cs
- [X] T018 [US1] Add POST /api/v1/clients to src/Backend/CRM.Api/Controllers/ClientsController.cs
- [ ] T019 [P] [US1] Unit tests: Create handler/validator in tests/CRM.Tests/Clients/CreateClientTests.cs
- [ ] T020 [P] [US1] Integration test: POST /clients success, duplicate email, invalid mobile in tests/CRM.Tests.Integration/Clients/CreateClientEndpointTests.cs

## Phase 4: US2 List own clients (P1)
Goal: SalesRep lists only own active clients; paginated and sorted by CreatedAt DESC; Admin sees all.

- [ ] T021 [US2] Define GetAllClientsQuery (+PagedResult) in src/Backend/CRM.Application/Clients/Queries/GetAllClientsQuery.cs
- [X] T022 [US2] Implement GetAllClientsQueryHandler in src/Backend/CRM.Application/Clients/Queries/Handlers/GetAllClientsQueryHandler.cs
- [X] T023 [US2] Add GET /api/v1/clients to ClientsController in src/Backend/CRM.Api/Controllers/ClientsController.cs
- [ ] T024 [P] [US2] Unit tests: query handler pagination/ownership in tests/CRM.Tests/Clients/GetAllClientsQueryTests.cs
- [ ] T025 [P] [US2] Integration test: GET /clients owner vs admin, pagination clamp in tests/CRM.Tests.Integration/Clients/ListClientsEndpointTests.cs

## Phase 5: US3 Get client details (P2)
Goal: Owner or Admin can view details; non-owner SalesRep forbidden; soft-deleted returns 404.

- [ ] T026 [US3] Define GetClientByIdQuery in src/Backend/CRM.Application/Clients/Queries/GetClientByIdQuery.cs
- [X] T027 [US3] Implement GetClientByIdQueryHandler in src/Backend/CRM.Application/Clients/Queries/Handlers/GetClientByIdQueryHandler.cs
- [X] T028 [US3] Add GET /api/v1/clients/{clientId} to ClientsController in src/Backend/CRM.Api/Controllers/ClientsController.cs
- [X] T029 [US3] Add ClientNotFoundException in src/Backend/CRM.Application/Clients/Exceptions/ClientNotFoundException.cs
- [ ] T030 [P] [US3] Unit tests: owner/admin/soft-deleted in tests/CRM.Tests/Clients/GetClientByIdQueryTests.cs
- [ ] T031 [P] [US3] Integration test: GET by id (200 owner/admin, 403 non-owner, 404 deleted) in tests/CRM.Tests.Integration/Clients/GetClientByIdEndpointTests.cs

## Phase 6: US4 Update client (P2)
Goal: Owner or Admin updates fields; uniqueness/validation enforced; UpdatedAt set.

- [ ] T032 [US4] Define UpdateClientCommand in src/Backend/CRM.Application/Clients/Commands/UpdateClientCommand.cs
- [X] T033 [US4] Implement UpdateClientCommandHandler in src/Backend/CRM.Application/Clients/Commands/Handlers/UpdateClientCommandHandler.cs
- [X] T034 [US4] Implement UpdateClientRequest (API DTO) in src/Backend/CRM.Application/Clients/Commands/UpdateClientRequest.cs
- [X] T035 [US4] Add InvalidStateCodeException in src/Backend/CRM.Application/Clients/Exceptions/InvalidStateCodeException.cs
- [X] T036 [US4] Add domain event ClientUpdated in src/Backend/CRM.Domain/Events/ClientUpdated.cs
- [X] T037 [US4] Add PUT /api/v1/clients/{clientId} to ClientsController in src/Backend/CRM.Api/Controllers/ClientsController.cs
- [ ] T038 [P] [US4] Unit tests: handler/validator (duplicate email, GSTIN, state code) in tests/CRM.Tests/Clients/UpdateClientTests.cs
- [ ] T039 [P] [US4] Integration test: PUT update success, 403 non-owner, 409 duplicate in tests/CRM.Tests.Integration/Clients/UpdateClientEndpointTests.cs

## Phase 7: US5 Soft delete client (P2)
Goal: Owner or Admin soft deletes; DeletedAt set; excluded from lists and get-by-id.

- [ ] T040 [US5] Define DeleteClientCommand (+DeleteResult) in src/Backend/CRM.Application/Clients/Commands/DeleteClientCommand.cs
- [X] T041 [US5] Implement DeleteClientCommandHandler in src/Backend/CRM.Application/Clients/Commands/Handlers/DeleteClientCommandHandler.cs
- [X] T042 [US5] Add domain event ClientDeleted in src/Backend/CRM.Domain/Events/ClientDeleted.cs
- [X] T043 [US5] Add DELETE /api/v1/clients/{clientId} to ClientsController in src/Backend/CRM.Api/Controllers/ClientsController.cs
- [ ] T044 [P] [US5] Unit tests: delete handler (owner/admin, idempotency) in tests/CRM.Tests/Clients/DeleteClientTests.cs
- [ ] T045 [P] [US5] Integration test: DELETE success, 403 non-owner, 404 nonexistent in tests/CRM.Tests.Integration/Clients/DeleteClientEndpointTests.cs

## Phase 8: US6 Admin oversight (P3)
Goal: Admin can perform all CRUD regardless of ownership with RBAC enforced.

- [ ] T046 [US6] Ensure [Authorize(Roles="SalesRep,Admin")] is applied to all endpoints in src/Backend/CRM.Api/Controllers/ClientsController.cs
- [ ] T047 [P] [US6] Integration test: Admin CRUD flows across endpoints in tests/CRM.Tests.Integration/Clients/AdminOversightEndpointTests.cs

## Final Phase: Polish & Cross-Cutting

- [ ] T048 [P] Add Serilog structured logs for client CRUD and authorization denials across handlers and controller
- [ ] T049 [P] Swagger: ensure tags, summaries, response codes align to contracts in src/Backend/CRM.Api/Program.cs and XML comments
- [ ] T050 [P] Update quickstart with auth/token instructions in specs/006-client-crud/quickstart.md
- [ ] T051 Add audit logging event handlers for ClientCreated/Updated/Deleted in src/Backend/CRM.Application/Clients/Events/*.cs
- [ ] T052 [P] Add pagination clamp tests to list endpoint integration tests path T025
- [ ] T053 Review DTO mapping for DisplayName, CreatedByUserName in src/Backend/CRM.Application/Clients/Dtos/ClientDto.cs
- [ ] T054 [P] Add README snippet linking clients.openapi.yaml in specs/006-client-crud/contracts/clients.openapi.yaml

## Dependencies & Order

- Phase 1 → Phase 2 → US1 (P1) → US2 (P1) → US3/US4/US5 (P2, parallel where marked) → US6 (P3) → Final
- Parallel examples: T019/T020, T024/T025, T030/T031, T038/T039, T044/T045, T048/T049/T050/T052/T054

## Implementation Strategy (MVP-first)

- MVP = US1 (Create) + minimal mapping + POST endpoint + unit/integration tests
- Next = US2 (List) paginated + clamp tests
- Then parallelize US3/US4/US5; finalize with US6 and polish
