# Research & Technical Decisions: Company Details Admin Configuration (Spec-022)

**Spec**: Spec-022  
**Date**: 2025-01-27

## Database Design: Singleton Pattern

### Decision: Use Singleton Entity with Single Record Constraint

**Rationale:**
- Company details represent a single company/organization configuration
- Only one set of company details should exist in the system
- Simpler than key-value store approach (SystemSettings table)
- Direct foreign key relationships possible if needed in future
- Better type safety and validation at database level

**Implementation:**
- `CompanyDetails` table with a single row (enforced via application logic or database constraint)
- Use a constant ID (e.g., `00000000-0000-0000-0000-000000000001`) or auto-increment with CHECK constraint
- Application layer ensures only one record exists (upsert pattern)

**Alternatives Considered:**
- **SystemSettings key-value approach**: More flexible but less type-safe, harder to query
- **Multiple tables**: Over-engineered for singleton data
- **JSONB column**: Less structured, harder to validate and index

## Tax Identification Number Validation

### Decision: Use Regex-Based Validation with Indian Tax Regulations

**PAN (Permanent Account Number):**
- Format: `^[A-Z]{5}[0-9]{4}[A-Z]{1}$`
- Example: `ABCDE1234F`
- 10 characters: 5 letters, 4 digits, 1 letter

**TAN (Tax Deduction and Collection Account Number):**
- Format: `^[A-Z]{4}[0-9]{5}[A-Z]{1}$`
- Example: `ABCD12345E`
- 10 characters: 4 letters, 5 digits, 1 letter

**GST (Goods and Services Tax Identification Number):**
- Format: `^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$`
- Example: `27ABCDE1234F1Z5`
- 15 characters: 2-digit state code, 10-character PAN, 1 check digit, 1 'Z', 1 alphanumeric

**Implementation:**
- Create `TaxNumberValidators.cs` in `CRM.Application/CompanyDetails/Validators/`
- Use FluentValidation with custom validators
- Validation rules applied at request validation level

## Bank Details: Country-Specific Fields

### Decision: Separate BankDetails Table with Country-Specific Columns

**Rationale:**
- Different countries require different banking fields
- India: Account Number, IFSC Code, Bank Name, Branch Name
- Dubai: Account Number, IBAN, SWIFT Code, Bank Name, Branch Name
- Future countries may have different requirements
- Flexible schema allows adding new countries without schema changes

**Schema Design:**
- `BankDetails` table with:
  - `BankDetailsId` (PK, UUID)
  - `CompanyDetailsId` (FK to CompanyDetails)
  - `Country` (enum or string: "India", "Dubai")
  - `AccountNumber` (required)
  - `IfscCode` (nullable, for India)
  - `Iban` (nullable, for Dubai)
  - `SwiftCode` (nullable, for Dubai)
  - `BankName` (required)
  - `BranchName` (nullable)
  - `CreatedAt`, `UpdatedAt`, `UpdatedBy`

**Indexes:**
- `IX_BankDetails_CompanyDetailsId_Country` (unique constraint: one bank detail per country per company)

## Logo Storage Strategy

### Decision: Store Logo in File System with URL Reference in Database

**Rationale:**
- Logos are binary files, not suitable for database storage
- File system storage is simpler than cloud storage for MVP
- Database stores file path/URL only
- Can migrate to cloud storage (S3, Azure Blob) later without schema changes

**Implementation:**
- Create `FileStorageService` in `CRM.Infrastructure/Services/`
- Store logos in `wwwroot/uploads/company/logo/` directory
- Generate unique filename: `{guid}.{extension}`
- Validate file type: PNG, JPG, JPEG, SVG, WEBP
- Validate file size: max 5MB
- Database stores relative path: `/uploads/company/logo/{guid}.{extension}`
- API endpoint serves files: `/api/v1/files/company/logo/{filename}`

**Alternatives Considered:**
- **Database BLOB storage**: Not recommended for large files, impacts database performance
- **Cloud storage (S3/Azure Blob)**: Better for production but adds complexity for MVP
- **Base64 encoding in database**: Inefficient, increases database size significantly

## Historical Accuracy in Quotations

### Decision: Store Company Details Snapshot in Quotation Entity (Optional JSONB Column)

**Rationale:**
- Quotations created before company details update should preserve original details
- Two approaches:
  1. **Snapshot approach**: Store company details JSON in Quotation table
  2. **Timestamp approach**: Store company details version timestamp, query historical data

**Decision: Snapshot Approach (Simpler)**
- Add optional `CompanyDetailsSnapshot` JSONB column to `Quotations` table
- When quotation is created, serialize current company details to JSON and store
- When viewing quotation, use snapshot if available, otherwise use current company details
- Simpler queries, no need for versioning system
- Trade-off: Slightly larger table, but quotations are not high-volume updates

**Implementation:**
- Add `CompanyDetailsSnapshot` JSONB column to Quotations table
- Store snapshot in `CreateQuotationCommandHandler` before saving quotation
- Use snapshot in `QuotationPdfGenerationService` when generating PDFs
- Migration: Add column as nullable, existing quotations will use current company details

## Integration with Quotation PDF Generation

### Decision: Inject Company Details Service into PDF Generation Service

**Rationale:**
- PDF generation service already exists (`QuotationPdfGenerationService`)
- Company details should be retrieved and included in PDF header/footer
- Use dependency injection to provide `ICompanyDetailsService`
- Service retrieves company details (or uses quotation snapshot if available)

**Implementation:**
- Modify `QuotationPdfGenerationService` constructor to accept `ICompanyDetailsService`
- In `GenerateQuotationPdf` method:
  - Check if quotation has `CompanyDetailsSnapshot`, use if available
  - Otherwise, retrieve current company details via service
  - Include company logo, address, contact info in PDF header
  - Include PAN, TAN, GST in PDF header or footer
  - Include country-specific bank details in PDF footer based on client country

## Email Template Integration

### Decision: Use FluentEmail Razor Templates with Company Details Model

**Rationale:**
- FluentEmail already in use for quotation emails
- Razor templates support dynamic content
- Company details can be passed as model to template
- Consistent with existing email template approach

**Implementation:**
- Modify `QuotationEmailService` to include company details in email model
- Update email Razor template to include:
  - Company logo (as embedded image or URL)
  - Company address and contact information
  - Company branding colors (if applicable)
- PDF attachment already includes company details (from PDF generation)

## Validation Rules Summary

### Required Fields
- PAN Number (if applicable)
- TAN Number (if applicable)
- GST Number (optional, if applicable)
- At least one bank detail (India or Dubai)
- Company address
- Company contact information (email or phone)

### Optional Fields
- Legal disclaimers
- Company logo
- Additional bank details for other countries

### Validation Rules
- PAN: 10 characters, format `^[A-Z]{5}[0-9]{4}[A-Z]{1}$`
- TAN: 10 characters, format `^[A-Z]{4}[0-9]{5}[A-Z]{1}$`
- GST: 15 characters, format `^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$`
- IFSC Code: 11 characters, alphanumeric
- IBAN: 15-34 characters, alphanumeric (country-specific format)
- SWIFT Code: 8-11 characters, alphanumeric
- Logo: Max 5MB, formats: PNG, JPG, JPEG, SVG, WEBP

## Performance Considerations

### Caching Strategy
- Company details are read frequently but updated rarely
- Cache company details in memory with 5-minute expiration
- Invalidate cache on update
- Use `IMemoryCache` in `ICompanyDetailsService` implementation

### Database Queries
- Company details retrieval: Single record lookup (very fast with PK)
- Bank details: Filtered by CompanyDetailsId and Country (indexed)
- Quotation PDF generation: Include company details in single query or use cached data

## Security Considerations

### File Upload Security
- Validate file type (MIME type and extension)
- Validate file size (max 5MB)
- Sanitize filename (remove special characters)
- Store files outside web root or use secure file serving endpoint
- Scan uploaded files for malware (future enhancement)

### Authorization
- All company details endpoints require Admin role
- Use `[Authorize(Roles = "Admin")]` attribute on controller
- Frontend route protection: Check role before rendering page

## Audit Logging

### Decision: Log All Company Details Changes

**Implementation:**
- Use existing `IAuditLogger` interface
- Log events:
  - `company_details_update_attempt`
  - `company_details_update_success`
  - `company_details_update_failure`
- Include changed fields in audit log
- Include user ID and timestamp

## Error Handling

### Edge Cases
- Company details not configured: Show warning in quotation creation, allow creation with placeholders
- Invalid tax number format: Show clear validation error message
- Logo upload failure: Show error, allow retry
- Bank details missing for client country: Show warning, use default or empty bank details
- Concurrent updates: Use optimistic concurrency (row version/timestamp)

## Testing Strategy

### Unit Tests
- Tax number validation (PAN, TAN, GST formats)
- Bank details validation (IFSC, IBAN, SWIFT formats)
- Company details service logic
- Logo upload validation

### Integration Tests
- Company details API endpoints (GET, PUT)
- Quotation PDF generation with company details
- Email template rendering with company details
- File upload and retrieval

### E2E Tests
- Admin configures company details
- Sales rep creates quotation, verifies company details appear
- Admin updates company details, verifies new quotations reflect changes
- Historical quotation preserves original company details

