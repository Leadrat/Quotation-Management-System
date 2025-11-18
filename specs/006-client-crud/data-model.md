# Data Model: Client (Spec-006)

Created: 2025-11-13

## Entities

### Client
- ClientId: UUID (PK)
- CompanyName: varchar(255) NOT NULL
- ContactName: varchar(255) NULL
- Email: varchar(255) NOT NULL (stored lowercase)
- Mobile: varchar(20) NOT NULL (E.164)
- PhoneCode: varchar(5) NULL
- Gstin: varchar(15) NULL (required for India B2B)
- StateCode: varchar(2) NULL (India GST state code)
- Address: text NULL
- City: varchar(100) NULL
- State: varchar(100) NULL
- PinCode: varchar(10) NULL
- CreatedByUserId: UUID NOT NULL (FK → Users.UserId)
- CreatedAt: timestamptz NOT NULL (default now)
- UpdatedAt: timestamptz NOT NULL (default now; update on change)
- DeletedAt: timestamptz NULL (soft delete)

#### Relationships
- CreatedByUser (many-to-one) → Users(UserId)

#### Constraints & Validation
- CompanyName: 2..255 chars
- ContactName (if provided): 2..255 chars, letters/space/hyphen/apostrophe
- Email: RFC 5322, unique among active clients, stored lowercase
- Mobile: E.164 `^\+[1-9]\d{1,14}$`
- Gstin (if provided): `^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$`
- StateCode (if provided): `^[0-9]{2}$` and must be in constants list
- PinCode (if provided): India 6 digits or country format
- GSTIN Required: for India B2B; optional otherwise

#### Indexes
- PK(ClientId)
- UNIQUE PARTIAL on lower(Email) WHERE DeletedAt IS NULL
- INDEX(Gstin)
- INDEX(CreatedByUserId)
- INDEX(CreatedAt)
- INDEX(UpdatedAt)
- INDEX(DeletedAt)
- COMPOSITE INDEX(CreatedByUserId, DeletedAt)

## State Transitions
- Create → sets CreatedAt/UpdatedAt; DeletedAt = NULL
- Update → sets UpdatedAt
- Soft Delete → sets DeletedAt; entity excluded from reads

## Notes
- Email normalization applied on write; API returns email as stored (lowercase).
- All list/get queries must filter DeletedAt IS NULL by default.
