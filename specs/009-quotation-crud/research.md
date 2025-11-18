# Research & Technical Decisions: Quotation Entity & CRUD Operations (Spec-009)

**Spec**: Spec-009  
**Date**: 2025-11-15

## Key Technical Decisions

### 1. Tax Calculation Strategy

**Decision**: Calculate tax automatically based on client location (intra-state vs inter-state) at quotation creation/update time.

**Rationale**:
- Ensures tax compliance and accuracy
- Reduces manual calculation errors
- Aligns with GST regulations (CGST/SGST for intra-state, IGST for inter-state)
- Tax calculation is deterministic and can be validated

**Implementation**:
- Tax calculation service (`TaxCalculationService`) determines client state code
- Compares with company state code (from system settings)
- Applies appropriate tax rates (18% total, split as 9%+9% for intra-state or 18% IGST for inter-state)
- Tax calculated on (SubTotal - DiscountAmount)

**Alternatives Considered**:
- Manual tax entry: Rejected - too error-prone and non-compliant
- Tax as line item: Rejected - tax is applied to total, not per item
- Configurable tax rates per client: Deferred to Phase 2

### 2. Quotation Number Generation

**Decision**: Auto-generate unique quotation numbers using configurable format (e.g., QT-YYYY-NNNNNN).

**Rationale**:
- Ensures uniqueness and traceability
- Human-readable format for easy reference
- Immutable after creation (audit compliance)
- Format configurable via settings for different companies

**Implementation**:
- `QuotationNumberGenerator` service generates numbers
- Format: `QT-{Year}-{SequentialNumber}` (e.g., QT-2025-001234)
- Sequential number retrieved from database sequence or max+1 with retry on collision
- Stored in database with UNIQUE constraint

**Alternatives Considered**:
- UUID only: Rejected - not human-readable
- Manual entry: Rejected - risk of duplicates
- Date-based only: Rejected - insufficient for high volume

### 3. Status Lifecycle Management

**Decision**: Enforce strict status transitions with business rules (DRAFT → SENT → VIEWED → ACCEPTED/REJECTED).

**Rationale**:
- Prevents invalid state transitions
- Ensures data integrity
- Only DRAFT quotations can be edited/deleted
- Clear audit trail of quotation progression

**Implementation**:
- Status enum with validation
- Business logic in command handlers prevents invalid transitions
- Status changes logged in audit trail (future: Spec-010)
- Expired status set by background job (future)

**Alternatives Considered**:
- Free-form status: Rejected - too flexible, no validation
- State machine library: Considered but overkill for 7 states

### 4. Line Items Storage

**Decision**: Store line items as separate table (`QuotationLineItems`) with foreign key relationship.

**Rationale**:
- Normalized design (follows database best practices)
- Supports unlimited line items per quotation
- Easy to query and update individual items
- Maintains referential integrity with CASCADE DELETE

**Implementation**:
- One-to-many relationship (Quotation → LineItems)
- SequenceNumber for ordering
- Amount auto-calculated (Quantity × UnitRate)
- Line items loaded eagerly with quotation to avoid N+1 queries

**Alternatives Considered**:
- JSONB array in Quotations table: Rejected - harder to query and validate
- Single table with denormalized items: Rejected - violates normalization

### 5. Discount Calculation

**Decision**: Support percentage-based discount (0-100%) applied to subtotal before tax.

**Rationale**:
- Most common discount model in business
- Simple to understand and calculate
- Percentage allows flexible discount amounts
- Applied before tax (standard accounting practice)

**Implementation**:
- `DiscountPercentage` stored (0-100)
- `DiscountAmount` calculated: `SubTotal × (DiscountPercentage / 100)`
- Validation: DiscountAmount cannot exceed SubTotal
- Tax calculated on (SubTotal - DiscountAmount)

**Alternatives Considered**:
- Fixed amount discount: Deferred to Phase 2 (can add both)
- Per-line-item discount: Deferred to Phase 2 (complexity)

### 6. Authorization Model

**Decision**: SalesReps see only own quotations; Admins see all quotations.

**Rationale**:
- Follows existing RBAC pattern (Spec-004, Spec-006)
- Protects sensitive pricing information
- Admins need visibility for reporting and management
- Consistent with client ownership model

**Implementation**:
- Authorization checks in query handlers
- Filter by `CreatedByUserId` for SalesReps
- No filter for Admins
- Enforced at API endpoint level with `[Authorize]` attributes

**Alternatives Considered**:
- Shared quotations: Deferred to Phase 2 (team collaboration)
- Client-based access: Rejected - quotations are user-owned

### 7. Soft Delete Strategy

**Decision**: Soft delete quotations by setting status to CANCELLED (preserve audit trail).

**Rationale**:
- Maintains audit trail for compliance
- Allows recovery if needed
- Preserves relationships (line items, history)
- Consistent with client soft delete pattern (Spec-006)

**Implementation**:
- Status changed to CANCELLED (not physical delete)
- Quotation remains in database
- Filtered out from active lists (WHERE Status != 'CANCELLED')
- Can be viewed in history/reports

**Alternatives Considered**:
- Physical delete: Rejected - loses audit trail
- DeletedAt timestamp: Considered but status-based simpler for this use case

### 8. Real-Time Tax Calculation (Frontend)

**Decision**: Calculate tax in real-time on frontend as user types (mirrors backend logic).

**Rationale**:
- Immediate feedback improves UX
- Reduces form submission errors
- Users can see totals before saving
- Tax calculation is deterministic (same logic as backend)

**Implementation**:
- JavaScript/TypeScript tax calculation service
- Mirrors backend `TaxCalculationService` logic
- Updates on: client selection, line item changes, discount changes
- Debounced for performance (300ms)

**Alternatives Considered**:
- Server-side only: Rejected - poor UX, requires round-trip
- Estimated calculation: Rejected - must match backend exactly

## Performance Considerations

### Database Indexes
- Compound indexes on `(ClientId, Status)` and `(CreatedByUserId, Status, CreatedAt DESC)` for common queries
- Index on `QuotationNumber` for fast lookups
- Index on `ValidUntil` for expiration checks

### Query Optimization
- Eager load line items with quotation (avoid N+1)
- Pagination for large lists (default 10, max 100)
- Filter at database level (not in-memory)

### Caching Strategy
- Cache quotation number sequence (high-volume scenarios)
- Cache tax calculation results for reporting (future)
- No caching for real-time quotation data (must be current)

## Security Considerations

### Authorization
- JWT-based authentication required
- Role-based access control (SalesRep vs Admin)
- Ownership validation in all operations

### Data Validation
- FluentValidation for all inputs
- SQL injection prevention (parameterized queries)
- XSS prevention (sanitize user inputs in frontend)

### Audit Trail
- All operations logged (create, update, delete)
- Track who made changes and when
- Immutable quotation numbers for traceability

## Future Enhancements (Out of Scope)

- Quotation templates (Spec-011)
- Email sending (Spec-010)
- Approval workflows (Spec-012)
- PDF generation (Spec-010)
- Multi-currency support
- Recurring quotations
- Electronic signatures

