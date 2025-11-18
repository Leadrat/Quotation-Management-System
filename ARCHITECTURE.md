# CRM Project Architecture Overview

This document provides an overview of the CRM Quotation Management System architecture, built using SPEC Kit in Windsurf. The project follows Clean Architecture principles with CQRS pattern.

## Project Structure

### Backend (.NET 8.0)

```
src/Backend/
├── CRM.Api/              # Web API layer (Controllers, Program.cs, Filters)
├── CRM.Application/      # Application layer (Commands, Queries, Handlers, DTOs, Validators)
├── CRM.Domain/           # Domain layer (Entities, Events)
├── CRM.Infrastructure/   # Infrastructure layer (Persistence, Auth, Logging, Notifications, Jobs)
├── CRM.Shared/           # Shared utilities (DTOs, Constants, Helpers, Exceptions, Validation)
└── CRM.Migrator/         # Database migration tool
```

### Frontend (Next.js 16)

```
src/Frontend/web/
├── src/
│   ├── app/              # Next.js app router pages
│   ├── components/       # React components
│   ├── lib/              # API client, session management
│   ├── store/            # Zustand state management
│   └── context/          # React contexts
```

## Completed Specs (7)

1. **Spec-001**: User Entity & DTO Specification
2. **Spec-002**: User Registration
3. **Spec-003**: User Authentication (JWT)
4. **Spec-004**: RBAC Foundation
5. **Spec-005**: User Profile & Password Management
6. **Spec-006**: Client Entity & CRUD Operations
7. **Spec-007**: Client Search, Filtering & Advanced Queries

## Key Architectural Patterns

### 1. Clean Architecture Layers

- **API Layer**: Controllers, middleware, filters
- **Application Layer**: Business logic, CQRS handlers, validation
- **Domain Layer**: Entities, domain events
- **Infrastructure Layer**: Database, external services, logging
- **Shared Layer**: Common DTOs, constants, utilities

### 2. CQRS (Command Query Responsibility Segregation)

**Commands** (write operations):
- Located in `CRM.Application/{Feature}/Commands/`
- Handlers in `Commands/Handlers/`
- Example: `CreateClientCommand`, `UpdateClientCommand`, `DeleteClientCommand`

**Queries** (read operations):
- Located in `CRM.Application/{Feature}/Queries/`
- Handlers in `Queries/Handlers/`
- Example: `GetAllClientsQuery`, `GetClientByIdQuery`, `SearchClientsQuery`

### 3. Database & Persistence

- **Database**: PostgreSQL
- **ORM**: Entity Framework Core 8.0
- **Primary Keys**: UUID/GUID
- **Soft Deletes**: `DeletedAt` pattern (nullable DateTimeOffset)
- **Extensions**: `citext` for case-insensitive email uniqueness
- **Full-Text Search**: PostgreSQL TSVECTOR with GIN indexes

**DbContext Pattern**:
- `AppDbContext` implements `IAppDbContext`
- Registered as scoped service
- Used via `IAppDbContext` interface in handlers

### 4. Validation

- **FluentValidation** for request validation
- Validators in `CRM.Application/{Feature}/Validators/`
- Constants in `CRM.Shared/Constants/ValidationConstants.cs`
- Validation rules enforced at:
  - Request level (FluentValidation)
  - Handler level (business rules)

### 5. Mapping

- **AutoMapper** for entity-to-DTO mapping
- Profiles in `CRM.Application/Mapping/`
- Example: `UserProfile`, `ClientProfile`, `RoleProfile`

### 6. Authentication & Authorization

- **JWT Bearer Authentication**
- Roles: `Admin`, `Manager`, `SalesRep`, `Client`
- Authorization via `[Authorize(Roles = "...")]` attributes
- Custom filters: `AdminOnlyAttribute`
- User ID extracted from JWT claims: `ClaimTypes.NameIdentifier` or `"sub"`

### 7. Error Handling

- Custom exceptions in `CRM.Shared/Exceptions/` and `CRM.Application/{Feature}/Exceptions/`
- Global exception handler in `Program.cs`
- Standard HTTP status codes:
  - `400`: Bad Request (validation)
  - `401`: Unauthorized
  - `403`: Forbidden
  - `404`: Not Found
  - `409`: Conflict (duplicate)
  - `422`: Unprocessable Entity (domain validation)
  - `500`: Internal Server Error

### 8. Domain Events

- Events defined in `CRM.Domain/Events/`
- Examples: `UserCreated`, `ClientCreated`, `UserProfileUpdated`
- Currently used for logging/audit (not full event bus yet)

### 9. Audit Logging

- `IAuditLogger` interface
- Implementation: `AuditLogger` in `CRM.Infrastructure/Logging/`
- Logs operations like `client_create_attempt`, `client_create_success`

### 10. Notifications

- Email queue: `IEmailQueue` interface
- In-memory channel-based queue
- Background service: `EmailQueueProcessor`
- Email messages: `EmailMessage` DTO

## Naming Conventions

### Backend (C#)

- **Entities**: PascalCase, singular (e.g., `User`, `Client`, `Role`)
- **DTOs**: PascalCase with suffix (e.g., `UserDto`, `ClientDto`)
- **Commands**: PascalCase with `Command` suffix (e.g., `CreateClientCommand`)
- **Queries**: PascalCase with `Query` suffix (e.g., `GetAllClientsQuery`)
- **Handlers**: PascalCase with `Handler` suffix (e.g., `CreateClientCommandHandler`)
- **Validators**: PascalCase with `Validator` suffix (e.g., `CreateClientRequestValidator`)
- **Controllers**: PascalCase with `Controller` suffix (e.g., `ClientsController`)
- **Properties**: PascalCase (e.g., `ClientId`, `CompanyName`)
- **Database Columns**: TitleCase (e.g., `ClientId`, `CompanyName`, `CreatedAt`)

### Frontend (TypeScript/React)

- **Components**: PascalCase (e.g., `ClientList.tsx`)
- **Pages**: `page.tsx` (Next.js App Router)
- **Hooks**: camelCase with `use` prefix (e.g., `useAuth`)
- **Utilities**: camelCase (e.g., `api.ts`, `session.ts`)

## Database Conventions

- **Table Names**: Plural, PascalCase (e.g., `Clients`, `Users`, `Roles`)
- **Primary Keys**: `{Entity}Id` (UUID)
- **Foreign Keys**: `{ReferencedEntity}Id` (e.g., `RoleId`, `CreatedByUserId`)
- **Timestamps**: `CreatedAt`, `UpdatedAt` (TIMESTAMPTZ, NOT NULL)
- **Soft Delete**: `DeletedAt` (TIMESTAMPTZ, NULLABLE)
- **Indexes**: Named `IX_{Table}_{Column(s)}`
- **Unique Constraints**: Named `UQ_{Table}_{Column(s)}`

## API Conventions

### Endpoints

- Base path: `/api/v1/{resource}`
- RESTful verbs: `GET`, `POST`, `PUT`, `DELETE`
- Response format:
  ```json
  {
    "success": true,
    "data": { ... },
    "message": "..." // optional
  }
  ```

### Pagination

- Query params: `pageNumber` (default: 1), `pageSize` (default: 10, max: 100)
- Response includes: `data`, `totalCount`, `pageNumber`, `pageSize`, `hasMore`

### Authorization

- JWT token in `Authorization: Bearer {token}` header
- Role-based access:
  - `SalesRep`: Own resources only
  - `Admin`: All resources
  - `Manager`: Team resources (if applicable)

## Request/Response Patterns

### Create Request
```csharp
public class CreateClientRequest
{
    public string CompanyName { get; set; }
    public string? ContactName { get; set; }
    public string Email { get; set; }
    // ... other fields
}
```

### Update Request
```csharp
public class UpdateClientRequest
{
    public string? CompanyName { get; set; } // Optional fields
    // ... other fields
}
```

### DTO
```csharp
public class ClientDto
{
    public Guid ClientId { get; set; }
    public string CompanyName { get; set; }
    // ... includes computed fields like DisplayName, CreatedByUserName
}
```

## Validation Rules

### Common Patterns

- **Email**: RFC 5322, case-insensitive, max 255 chars
- **Password**: Min 8 chars, uppercase, lowercase, digit, special char
- **Mobile**: E.164 format (`^\+[1-9]\d{1,14}$`)
- **Names**: 2-100 chars, letters/spaces/hyphens/apostrophes
- **GSTIN**: Format validation (if provided)
- **StateCode**: Validated against constants list

### Validation Constants

Located in `CRM.Shared/Constants/ValidationConstants.cs`:
- `MinPasswordLength = 8`
- `MaxPasswordLength = 128`
- `MinNameLength = 2`
- `MaxNameLength = 100`
- Regex patterns for password, mobile, phone code, names

## Testing Structure

```
tests/
├── CRM.Tests/              # Unit tests
│   ├── Application/
│   ├── Auth/
│   ├── Clients/
│   └── Users/
└── CRM.Tests.Integration/  # Integration tests
    ├── Auth/
    ├── Clients/
    └── Users/
```

## Environment Configuration

- `.env.local` and `.env` files supported
- `DotEnv.Load()` in `Program.cs`
- Connection string: `POSTGRES_CONNECTION` environment variable
- JWT settings: `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`

## Frontend Architecture

- **Framework**: Next.js 16 (App Router)
- **UI Library**: Tailwind CSS
- **State Management**: Zustand
- **API Client**: Custom `api.ts` with session management
- **Authentication**: JWT stored in cookies/session

## Development Workflow

1. **Spec Creation**: Each feature has a spec in `specs/{spec-number}-{feature-name}/`
   - `spec.md`: Feature specification
   - `data-model.md`: Database schema
   - `contracts/`: OpenAPI contracts
   - `tasks.md`: Implementation tasks
   - `research.md`: Research notes

2. **Implementation Order**:
   - Domain entities
   - DTOs and requests
   - Validators
   - Commands/Queries
   - Handlers
   - Controllers
   - Tests
   - Frontend (if applicable)

3. **Database Migrations**:
   - EF Core migrations in `CRM.Infrastructure/Migrations/`
   - Or use `EnsureCreated()` for development (as in `Program.cs`)

## Key Dependencies

### Backend
- `Microsoft.EntityFrameworkCore` (8.0.8)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.8)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.8)
- `AutoMapper`
- `FluentValidation`
- `BCrypt.Net-Next` (password hashing)

### Frontend
- `next` (16.0.2)
- `react` (19.2.0)
- `tailwindcss` (^4)
- `zustand` (^5.0.8)

## Security Practices

- Password hashing: BCrypt (cost factor 12)
- JWT with expiration and refresh tokens
- CORS configured for frontend origins
- Security headers (X-Content-Type-Options, X-Frame-Options, etc.)
- Rate limiting (commented out, can be enabled)
- Input validation at multiple layers
- SQL injection prevention via EF Core parameterized queries

## Next Steps for Development

When continuing development:

1. **Follow the spec structure**: Create new specs in `specs/` directory
2. **Maintain layer separation**: Keep business logic in Application, not in Controllers
3. **Use CQRS pattern**: Separate commands and queries
4. **Validate inputs**: Use FluentValidation for all requests
5. **Handle errors consistently**: Use custom exceptions and global handler
6. **Write tests**: Unit tests for handlers, integration tests for endpoints
7. **Update OpenAPI contracts**: Keep contracts in sync with implementation
8. **Follow naming conventions**: Consistent naming across the codebase
9. **Soft delete pattern**: Always use `DeletedAt IS NULL` for active records
10. **Audit logging**: Log important operations via `IAuditLogger`

## Common Patterns to Follow

### Controller Pattern
```csharp
[ApiController]
[Route("api/v1/{resource}")]
[Authorize(Roles = "...")]
public class ResourceController : ControllerBase
{
    // Extract user ID from JWT
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    
    // Validate request
    var validator = new CreateResourceRequestValidator();
    var result = validator.Validate(body);
    if (!result.IsValid) return BadRequest(...);
    
    // Create command/query
    var cmd = new CreateResourceCommand { ... };
    
    // Execute handler
    var handler = new CreateResourceCommandHandler(_db, _mapper);
    var dto = await handler.Handle(cmd);
    
    // Audit log
    await _audit.LogAsync("resource_create_success", new { ... });
    
    // Return response
    return StatusCode(201, new { success = true, data = dto });
}
```

### Handler Pattern
```csharp
public class CreateResourceCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;
    
    public async Task<ResourceDto> Handle(CreateResourceCommand cmd)
    {
        // Validation
        // Business logic
        // Create entity
        // Save changes
        // Map to DTO
        // Return
    }
}
```

This architecture provides a solid foundation for continuing development while maintaining consistency and scalability.

