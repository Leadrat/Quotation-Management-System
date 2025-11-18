# Research: Spec-011 Quotation Template Management

**Spec**: Spec-011  
**Last Updated**: 2025-11-15

## Overview

This document captures research, design decisions, and technical considerations for implementing Quotation Template Management.

---

## Version History Strategy

### Decision: Linked List Structure

**Approach**: Use `PreviousVersionId` to create a linked list of versions.

**Pros**:
- Simple to implement
- Preserves all historical data
- Easy to traverse backwards
- No need for separate version table

**Cons**:
- Recursive queries needed for full history
- Can't easily query "all versions of all templates"

**Alternative Considered**: Separate `TemplateVersions` table with `TemplateId` and `VersionNumber`.

**Decision**: Use linked list for simplicity. If performance becomes an issue, can add a materialized view or denormalized version table later.

### Version Increment Strategy

**Decision**: Increment version on every update, even minor changes.

**Rationale**: 
- Provides complete audit trail
- Allows rollback to any previous state
- Simple to implement

**Alternative**: Only increment on "major" changes (configurable).

---

## Visibility Rules Implementation

### Public Templates
- `Visibility = 'Public'` AND `IsApproved = true`
- Visible to all authenticated users
- Used for company-wide standard templates

### Team Templates
- `Visibility = 'Team'` AND `OwnerRole` matches user's role
- Example: SalesRep creates template with `OwnerRole = 'SalesRep'`, all SalesReps can see it
- Used for role-specific templates

### Private Templates
- `Visibility = 'Private'`
- Only visible to owner
- Used for personal templates

### Query Optimization

**Decision**: Use partial indexes with `WHERE DeletedAt IS NULL` for better performance.

**Query Pattern**:
```sql
SELECT * FROM QuotationTemplates
WHERE DeletedAt IS NULL
  AND (
    (Visibility = 'Public' AND IsApproved = true)
    OR (Visibility = 'Team' AND OwnerRole = @UserRole)
    OR (Visibility = 'Private' AND OwnerUserId = @UserId)
  )
```

---

## Template Application Flow

### Decision: Return CreateQuotationRequest

**Approach**: `/apply` endpoint returns data in `CreateQuotationRequest` format.

**Rationale**:
- Frontend can directly use response to pre-fill quotation form
- No need for separate mapping logic
- Consistent with existing quotation creation flow

**Flow**:
1. User selects template in quotation creation page
2. Frontend calls `POST /quotation-templates/{id}/apply?clientId={clientId}`
3. Backend returns `CreateQuotationRequest` with:
   - Line items from template
   - Default discount (if set)
   - Default notes (if set)
   - Client ID (from query param)
   - Quotation date (today)
   - Valid until (today + default days)
4. Frontend pre-fills form with response data
5. User can modify before submitting

---

## Soft Delete Strategy

### Decision: Soft Delete with Restore

**Approach**: Set `DeletedAt` timestamp instead of hard delete.

**Pros**:
- Preserves audit trail
- Can restore if needed
- Maintains referential integrity
- Version history preserved

**Cons**:
- Requires filtering in all queries
- Slightly more complex queries

**Implementation**:
- All queries filter `WHERE DeletedAt IS NULL`
- Partial indexes improve performance
- Restore endpoint clears `DeletedAt`

---

## Usage Tracking

### Decision: Update on Every Apply

**Approach**: Increment `UsageCount` and update `LastUsedAt` on every template application.

**Rationale**:
- Simple to implement
- Accurate metrics
- Minimal performance impact (single UPDATE)

**Alternative Considered**: Batch updates or separate usage log table.

**Decision**: Direct update is sufficient for current scale. Can migrate to usage log table if needed for detailed analytics.

---

## Authorization Rules

### Sales Rep Permissions
- Create templates (any visibility)
- Edit own templates (unless approved and not owner)
- Delete own templates
- View: Public (approved), Team (same role), Private (own)
- Apply any visible template

### Admin Permissions
- All sales rep permissions
- Edit any template
- Delete any template
- Approve templates (makes them Public)
- View all templates (including deleted)
- View usage statistics

### Implementation Pattern

```csharp
private bool CanEdit(QuotationTemplate template, Guid userId, string role)
{
    if (template.IsDeleted()) return false;
    
    if (template.OwnerUserId == userId) return true;
    
    if (role == "Admin") return true;
    
    if (template.IsApproved && template.OwnerUserId != userId) return false;
    
    return false;
}
```

---

## Template Line Items

### Decision: Store in Separate Table

**Approach**: `QuotationTemplateLineItems` table with foreign key to `QuotationTemplates`.

**Rationale**:
- Normalized structure
- Easy to query and update
- Consistent with `QuotationLineItems` pattern
- Cascade delete when template deleted

**Alternative**: Store as JSON in template table.

**Decision**: Separate table for queryability and consistency.

---

## Name Uniqueness

### Decision: Unique Per Owner

**Approach**: Unique constraint on `(Name, OwnerUserId)` where `DeletedAt IS NULL`.

**Rationale**:
- Allows multiple users to have templates with same name
- Prevents duplicates for same user
- Partial unique index handles soft delete correctly

**SQL**:
```sql
CREATE UNIQUE INDEX IX_QuotationTemplates_Name_Owner_Active 
ON QuotationTemplates (Name, OwnerUserId) 
WHERE DeletedAt IS NULL;
```

---

## Performance Considerations

### Indexes
- Partial indexes for active templates (`WHERE DeletedAt IS NULL`)
- Composite indexes for common query patterns
- Index on `PreviousVersionId` for version history traversal

### Query Optimization
- Use `Include()` for eager loading line items
- Pagination for list queries
- Filter at database level, not in memory

### Caching
- Consider caching frequently used public templates
- Cache invalidation on template update/delete
- Not implemented in initial version (can add later)

---

## Frontend Component Architecture

### Decision: Reusable Components

**Approach**: Build small, focused components that can be composed.

**Components**:
- `TemplateListTable` - Data table with search/filter
- `TemplateForm` - Create/edit form
- `LineItemsEditor` - Add/remove/edit line items
- `TemplatePreview` - Formatted preview
- `ApplyTemplateModal` - Template selection modal
- `VersionHistoryTimeline` - Version history display

**Benefits**:
- Reusable across pages
- Easier to test
- Consistent UI/UX

---

## Integration with Quotation Creation

### Decision: Modal-Based Selection

**Approach**: "Apply Template" button opens modal with template list.

**Flow**:
1. User clicks "Apply Template" in quotation creation page
2. Modal opens with searchable/filterable template list
3. User selects template
4. Modal shows preview
5. User confirms
6. Form pre-fills with template data
7. User can modify before submitting

**Alternative**: Dropdown or inline selector.

**Decision**: Modal provides better UX for template selection and preview.

---

## Admin Approval Workflow

### Decision: Simple Approval Flag

**Approach**: Admin sets `IsApproved = true`, which makes template Public.

**Rationale**:
- Simple to implement
- Clear approval state
- Can add more complex workflow later if needed

**Future Enhancement**: Multi-stage approval, approval comments, rejection reasons.

---

## Testing Strategy

### Backend
- Unit tests for all commands/queries
- Integration tests for API endpoints
- Authorization tests for visibility rules
- Versioning tests for history preservation

### Frontend
- Component unit tests
- Form validation tests
- E2E tests for critical flows:
  - Create → Apply → Create Quotation
  - Update → Version History → Restore
  - Admin Approval → Template becomes Public

---

## Future Enhancements

### Template Categories/Tags
- Organize templates by category
- Filter by tags
- Tag-based search

### Template Variables
- Dynamic placeholders (e.g., `{ClientName}`, `{Date}`)
- Replaced when applying template

### Template Sharing
- Share templates between users
- Template marketplace

### Template Analytics
- Conversion rates (template → quotation → accepted)
- Most successful templates
- Template performance metrics

### Template Cloning with Variations
- Clone and modify in one step
- Template variants

---

## Security Considerations

### Authorization
- All endpoints require authentication
- Visibility rules enforced at query level
- Admin-only endpoints check role

### Data Validation
- Input validation on all endpoints
- SQL injection prevention (parameterized queries)
- XSS prevention (sanitize user input in frontend)

### Audit Trail
- Domain events for all template operations
- IP address logging (if needed)
- User action logging

---

## Migration Strategy

### Zero-Downtime Deployment
1. Deploy migration (creates new tables, no breaking changes)
2. Deploy application code
3. Existing functionality continues to work
4. New template features available immediately

### Rollback Plan
- Migration can be rolled back (drops tables)
- Application code rollback removes endpoints
- No data loss for existing quotations

---

## Performance Benchmarks

### Target Metrics
- List templates: < 200ms (with pagination)
- Get template by ID: < 50ms
- Apply template: < 100ms
- Create template: < 300ms
- Update template: < 400ms (includes version creation)

### Optimization Opportunities
- Cache public templates
- Materialized view for usage stats
- Background job for usage aggregation

---

**Last Updated**: 2025-11-15

