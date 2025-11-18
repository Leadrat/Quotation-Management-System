# Data Model: Spec-007 Client Search

Created: 2025-11-14

## Entities

### Client (existing from Spec-006)
- ClientId (uuid, PK)
- CompanyName (text, indexed, FTS)
- ContactName (text, indexed, FTS)
- Email (text, indexed, FTS)
- Mobile (text)
- Gstin (text, indexed)
- State (text, indexed)
- City (text, indexed)
- StateCode (text, indexed)
- CreatedAt (timestamptz, indexed)
- UpdatedAt (timestamptz)
- CreatedByUserId (uuid, indexed)
- DeletedAt (timestamptz, nullable, indexed)

Indexes:
- IDX_Client_Search_FTS GIN(tsvector(CompanyName, ContactName, Email))
- IDX_Client_State
- IDX_Client_City
- IDX_Client_StateCode
- IDX_Client_CreatedBy_DeletedAt (CreatedByUserId, DeletedAt)
- IDX_Client_Email
- IDX_Client_CompanyName

### SavedSearch
- SavedSearchId (uuid, PK)
- UserId (uuid, FK Users.UserId, indexed)
- SearchName (varchar(255), required)
- FilterCriteria (jsonb, required)
- SortBy (varchar(50), nullable)
- IsActive (boolean, default true, indexed)
- CreatedAt (timestamptz, not null)
- UpdatedAt (timestamptz, not null)

Constraints:
- SearchName length <= 255
- FilterCriteria must be non-empty JSON

### SearchHistory (lightweight, optional table or log)
- SearchHistoryId (uuid, PK)
- UserId (uuid, indexed)
- SearchTerm (text)
- Filters (jsonb)
- ResultCount (int)
- ExecutionTimeMs (int)
- CreatedAt (timestamptz)

Retention:
- Keep last 20 per user (application-enforced rolling retention)

## DTOs

### ClientDto (existing)
- clientId, companyName, contactName, email, mobile, gstin, state, city, createdByUserName, createdAt, updatedAt, displayName

### SavedSearchDto
- savedSearchId, searchName, filterCriteria, sortBy, createdAt

### FilterOptionsDto
- states: [{ state, count }]
- cities: [{ city, count }]
- createdDateRanges: [{ label, from, to }]
- stateCodes: [{ code, name }]

### PagedSearchResult<T>
- data: T[]
- totalCount: number
- pageNumber: number
- pageSize: number
- hasMore: boolean

## Validation
- SearchTerm max 255
- PageSize 1..100; PageNumber >0
- Date ranges From <= To for Created/Updated

## Notes
- Timezone UTC for all temporal filters
- API `userId` mapped to internal `CreatedByUserId`
