# Research: Country-Specific Identifiers & Bank Details (Spec-023)

**Date**: 2025-01-27  
**Spec**: [spec.md](./spec.md)  
**Plan**: [plan.md](./plan.md)

## Research Topics

This document consolidates research findings for implementing country-specific company identifiers and bank details management.

---

## 1. Data Model Approach for Country-Specific Values

### Question
How should we store company identifier values and bank details that vary by country (different fields per country)?

### Options Evaluated

#### Option A: EAV (Entity-Attribute-Value) Pattern
**Approach**: Store values in a generic key-value table
- `CompanyIdentifierValues` table: CompanyDetailsId, CountryId, IdentifierTypeId, Value
- `CompanyBankDetailValues` table: CompanyDetailsId, CountryId, BankFieldTypeId, Value

**Pros**:
- Highly flexible - supports unlimited field types without schema changes
- Easy to add new identifier/bank field types per country
- Normalized relational structure

**Cons**:
- More complex queries (joins across multiple tables)
- Potential performance issues for lookups
- Type safety challenges (all values stored as strings)
- More complex validation logic

#### Option B: JSONB Column
**Approach**: Store country-specific values as JSONB in CompanyDetails and BankDetails tables
- `CompanyDetails.IdentifierValues` JSONB: `{ "countryId": { "pan": "ABCDE1234F", "vat": "..." } }`
- `BankDetails.FieldValues` JSONB: `{ "ifsc": "HDFC0001234", "accountNumber": "..." }`

**Pros**:
- Simple schema - no additional tables
- Fast reads (single column query)
- Flexible structure (can store nested data)
- PostgreSQL JSONB has excellent indexing and query support

**Cons**:
- Validation must be done in application code (no database-level constraints)
- Less type safety
- Querying nested JSON can be complex
- Migration complexity if structure changes

#### Option C: Hybrid Normalized + JSONB
**Approach**: Master configuration in normalized tables, values in JSONB
- Master config: Normalized tables (IdentifierType, CountryIdentifierConfiguration)
- Values: JSONB columns in CompanyDetails/BankDetails

**Pros**:
- Best of both worlds: structured config, flexible values
- Easier queries for configuration lookups
- Flexible value storage

**Cons**:
- More complex architecture
- Still has JSONB validation challenges

#### Option D: Fully Normalized with Country-Specific Tables
**Approach**: Separate tables per country or pivot table with all possible fields
- `CompanyIdentifierValues`: All fields nullable, one row per country
- Or separate tables: `CompanyIdentifiersIndia`, `CompanyIdentifiersDubai`, etc.

**Pros**:
- Strong type safety
- Database-level constraints and validation
- Standard SQL queries

**Cons**:
- Requires schema changes for each new country
- Many nullable columns or many tables
- Not extensible without code changes
- Does not meet requirement "extend to new countries without code changes"

### Decision

**Chosen**: **Option C - Hybrid Normalized + JSONB**

**Rationale**:
1. **Master Configuration (Normalized)**: Store identifier types, bank field types, and country configurations in normalized relational tables for:
   - Efficient queries and lookups
   - Referential integrity
   - Configuration management
   - Audit trail

2. **Company Values (JSONB)**: Store actual company identifier and bank detail values as JSONB for:
   - Flexibility to store different fields per country without schema changes
   - Fast reads (single column query per country)
   - Supports requirement "extend to new countries without code changes"
   - PostgreSQL JSONB provides excellent indexing and query capabilities

3. **Validation**: Implement validation rules from master configuration tables and apply them to JSONB values in application code using FluentValidation

**Implementation**:
```sql
-- Master Configuration (Normalized)
IdentifierTypes (IdentifierTypeId, Name, Description)
CountryIdentifierConfigurations (CountryId, IdentifierTypeId, IsRequired, ValidationRegex, DisplayName, HelpText, MinLength, MaxLength)
BankFieldTypes (BankFieldTypeId, Name, Description)
CountryBankFieldConfigurations (CountryId, BankFieldTypeId, IsRequired, ValidationRegex, DisplayName, HelpText, MinLength, MaxLength)

-- Company Values (JSONB)
CompanyDetails.IdentifierValues JSONB
CompanyBankDetails.FieldValues JSONB
```

**Alternatives Considered**: Option A (EAV) was rejected due to query complexity. Option B (Pure JSONB) was rejected due to lack of structured configuration. Option D was rejected as it violates the extensibility requirement.

---

## 2. Dynamic Form Field Rendering

### Question
How should the frontend dynamically render form fields based on country-specific configuration?

### Options Evaluated

#### Option A: Server-Side Field Configuration Endpoint
**Approach**: Backend API returns field configuration for a country
- Endpoint: `GET /api/countries/{countryId}/identifier-configurations`
- Returns: Array of field definitions with validation rules, display names, help text

**Pros**:
- Single source of truth (backend configuration)
- Client receives complete field definitions including validation rules
- Supports real-time validation on frontend

**Cons**:
- Requires API call on country change
- Slight delay in form rendering

#### Option B: Client-Side Field Configuration Caching
**Approach**: Load all country configurations on page load, cache in frontend state

**Pros**:
- Instant form updates on country change
- No API calls during editing

**Cons**:
- Larger initial payload
- Configuration changes require page refresh

#### Option C: Hybrid - Load on Demand with Caching
**Approach**: Load configuration on first country selection, cache for session

**Pros**:
- Best of both: fast after first load, fresh data
- Reasonable initial payload

**Cons**:
- Slight delay on first country selection

### Decision

**Chosen**: **Option C - Hybrid (Load on Demand with Caching)**

**Rationale**:
1. Initial page load is fast (no configuration data loaded)
2. First country selection triggers API call (acceptable delay <500ms)
3. Subsequent country changes use cached configuration (instant)
4. Configuration can be refreshed if needed (admin config changes)

**Implementation**:
- Frontend stores country configurations in Zustand store
- On country selection: Check cache → If missing, fetch from API → Cache → Render fields
- Config refresh: Admin can trigger refresh after configuration changes

---

## 3. Validation Rules Storage and Execution

### Question
How should validation rules be stored and executed for country-specific fields?

### Options Evaluated

#### Option A: Store Regex Patterns in Database
**Approach**: Store regex patterns, min/max length, required flag in CountryIdentifierConfiguration table

**Pros**:
- Configuration-driven validation
- Easy to update rules without code changes
- Single source of truth

**Cons**:
- Regex compilation overhead
- Complex patterns may be hard to maintain

#### Option B: Validation Rule IDs Referencing Code
**Approach**: Store validation rule IDs, actual logic in code

**Pros**:
- Type-safe validation logic
- Complex validation logic possible

**Cons**:
- Requires code changes for new validation rules
- Violates requirement "extend without code changes"

#### Option C: Hybrid - Regex + Application Validation
**Approach**: Store regex patterns for format validation, additional application-level validation for complex rules

**Pros**:
- Simple format validation via regex
- Complex validation in code where needed
- Configuration-driven for common cases

**Cons**:
- Two layers of validation (acceptable complexity)

### Decision

**Chosen**: **Option A - Store Regex Patterns in Database**

**Rationale**:
1. Meets requirement "extend without code changes"
2. Most identifier and bank field validations are format-based (regex sufficient)
3. PostgreSQL regex support is excellent
4. For complex validations, can add custom validation functions in FluentValidation (extensible)

**Implementation**:
- Store `ValidationRegex`, `MinLength`, `MaxLength`, `IsRequired` in configuration tables
- Frontend: Validate on blur/change using JavaScript regex
- Backend: Validate using FluentValidation with regex pattern from database
- Complex validation: Add custom FluentValidation validators when needed

---

## 4. Performance Considerations

### Question
How to ensure fast lookups and form rendering for country-specific fields?

### Findings

#### Database Indexing
- **Index on CountryId** in CountryIdentifierConfiguration and CountryBankFieldConfiguration tables
- **GIN index on JSONB columns** for CompanyDetails.IdentifierValues and BankDetails.FieldValues
- **Composite index** on (CountryId, IdentifierTypeId) and (CountryId, BankFieldTypeId)

#### Caching Strategy
- **Configuration Cache**: Cache country configurations in memory (Redis or in-memory cache)
  - TTL: 1 hour (configurations change infrequently)
  - Invalidate on configuration update
- **Company Details Cache**: Cache company identifier and bank values per country
  - TTL: 15 minutes (values change occasionally)
  - Invalidate on company details update

#### Query Optimization
- **Batch Loading**: Load all configurations for a country in single query
- **Selective JSONB Queries**: Use PostgreSQL JSONB operators (`->`, `->>`, `@>`) for efficient filtering

### Decision

**Approach**:
1. Database indexes as specified above
2. In-memory caching for country configurations (IMemoryCache in ASP.NET Core)
3. Cache invalidation on configuration/company details updates
4. Efficient JSONB queries using PostgreSQL operators

---

## 5. Migration from Existing Company Details (Spec-022)

### Question
How to migrate existing hardcoded CompanyDetails (PAN, TAN, GST) to country-specific model?

### Approach

#### Migration Strategy
1. **Create master configuration tables** (IdentifierType, CountryIdentifierConfiguration, etc.)
2. **Seed initial data**: Create PAN, TAN, GST identifier types and configure for India
3. **Migrate existing values**: 
   - Extract PAN, TAN, GST from existing CompanyDetails table
   - Convert to JSONB format: `{ "pan": "...", "tan": "...", "gst": "..." }`
   - Store in CompanyDetails.IdentifierValues JSONB column
4. **Deprecate old columns**: Keep old columns temporarily, mark as deprecated, remove in future migration
5. **Update application code**: Use new JSONB-based values, fallback to old columns during transition

### Decision

**Chosen**: Phased migration approach

**Rationale**:
1. Zero downtime - existing functionality continues to work
2. Backward compatible during transition
3. Clear migration path with rollback option

---

## Summary of Decisions

| Topic | Decision | Rationale |
|-------|----------|-----------|
| Data Model | Hybrid: Normalized config + JSONB values | Flexibility + Queryability |
| Form Rendering | Load on demand with caching | Performance + Freshness |
| Validation Rules | Regex patterns in database | Extensibility + Configuration-driven |
| Performance | Indexes + In-memory caching | Fast lookups + Efficient queries |
| Migration | Phased approach | Zero downtime + Backward compatible |

---

## Open Questions Resolved

✅ All "NEEDS CLARIFICATION" markers resolved  
✅ Data model approach selected  
✅ Form rendering strategy defined  
✅ Validation approach determined  
✅ Performance strategy planned  
✅ Migration approach defined

---

## References

- PostgreSQL JSONB Documentation: https://www.postgresql.org/docs/current/datatype-json.html
- Entity Framework Core JSONB Support: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns
- FluentValidation Documentation: https://docs.fluentvalidation.net/
- Next.js Dynamic Forms Best Practices: https://nextjs.org/docs

