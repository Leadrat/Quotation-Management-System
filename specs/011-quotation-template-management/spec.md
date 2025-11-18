# Spec-011: Quotation Template Management

**Spec Number**: 011  
**Spec Name**: Quotation Template Management (Create, Edit, Apply)  
**Group**: Quotation Management (Group 3 of 11)  
**Priority**: HIGH (Phase 1, after Spec 10)  
**Status**: Draft

## Dependencies

- **Spec-009**: Quotation Entity & CRUD Operations
- **Spec-010**: Quotation Management (Send, Track, Respond)
- **Spec-006**: Client Entity

## Related Specs

- **Spec-012**: Approval Workflow (for template approval)
- **Spec-013**: Notification System (for template approval notifications)

---

## Overview

This specification defines the Quotation Template Management system, enabling the creation, updating, and application of reusable quotation templates. Templates predefine sets of line items, tax rules, discount defaults, and notes, allowing sales reps to generate quotations rapidly and consistently, reducing manual entry and errors.

### Key Features

- Create new quotation templates with line items, default tax/discount, and terms
- Edit and update templates with version history (can revert to previous version)
- Apply template to quotation creation (pre-fills form)
- Preview template before applying
- Save-as-new (clone existing template with changes)
- Admin can approve and publish templates for all users (global), or restrict to own use
- Template visibility: Public (all sales reps), Team (by role), Private (only creator)
- Search, filter, and sort templates by name, creator, last updated
- Delete/Archive (soft-delete) templates (can restore)
- Template usage metrics: Times used, last used
- Audit trail: Who created/edited/applied template and when

---

## Business Value

- Reduces repetitive work for sales team; increases productivity
- Ensures all quotations use standardized language and pricing
- Prevents errors (taxes, discounts, notes always correct)
- Enforces compliance with legal and branding requirements
- Provides quick updates to all sales when pricing or terms change
- Powerful for organizations with many products/services or fast-changing offerings

---

## User Stories

### US1: Create Quotation Template
**As a** Sales Representative  
**I want to** create reusable quotation templates with predefined line items and settings  
**So that** I can quickly generate consistent quotations without repetitive data entry

**Acceptance Criteria:**
- Can create template with name, description, visibility setting
- Must include at least one line item
- Can set default discount percentage and notes/terms
- Template saved with owner information
- Template version starts at 1

### US2: Edit Quotation Template
**As a** Sales Representative or Admin  
**I want to** edit existing templates  
**So that** I can update pricing, terms, or line items as business needs change

**Acceptance Criteria:**
- Can update template name, description, line items, notes
- Version increments automatically on update
- Previous version preserved for rollback
- Owner or Admin can edit (visibility rules apply)
- Cannot edit if template is deleted/archived

### US3: Apply Template to Quotation
**As a** Sales Representative  
**I want to** apply a template when creating a quotation  
**So that** the quotation form is pre-filled with template data, saving time

**Acceptance Criteria:**
- Can browse/search available templates (based on visibility)
- Selecting template pre-fills quotation form
- Can preview template before applying
- Usage count increments on apply
- LastUsedAt timestamp updated

### US4: Clone Template (Save-as-New)
**As a** Sales Representative  
**I want to** clone an existing template with modifications  
**So that** I can create variations without affecting the original

**Acceptance Criteria:**
- Can create new template based on existing one
- New template has new ID and version 1
- Original template unchanged
- New template owned by current user

### US5: Approve Template (Admin)
**As an** Admin  
**I want to** approve templates for global use  
**So that** all sales reps can use standardized, company-approved templates

**Acceptance Criteria:**
- Admin can approve/reject templates
- Approved templates marked as global/public
- Approval timestamp and approver recorded
- Approved templates visible to all sales reps

### US6: View Template Version History
**As a** Sales Representative or Admin  
**I want to** view version history of a template  
**So that** I can see changes over time and rollback if needed

**Acceptance Criteria:**
- Can view list of all versions
- Shows who changed what and when
- Can restore previous version
- Version history preserved even after deletion

### US7: Search and Filter Templates
**As a** Sales Representative  
**I want to** search and filter templates  
**So that** I can quickly find the template I need

**Acceptance Criteria:**
- Can search by name or description
- Can filter by visibility (My, Team, Public)
- Can filter by approval status
- Results paginated
- Sort by name, last updated, usage count

### US8: Soft Delete and Restore Template
**As a** Sales Representative or Admin  
**I want to** archive templates instead of permanently deleting  
**So that** I can restore them later if needed

**Acceptance Criteria:**
- Delete sets DeletedAt timestamp (soft delete)
- Deleted templates hidden from normal listing
- Can restore deleted templates
- Restore clears DeletedAt timestamp

### US9: View Template Usage Statistics
**As an** Admin  
**I want to** view template usage statistics  
**So that** I can identify popular templates and optimize the template library

**Acceptance Criteria:**
- Dashboard shows most used templates
- Usage count and last used date visible
- Can filter by team/user
- Export statistics as CSV/Excel

### US10: Preview Template
**As a** Sales Representative  
**I want to** preview how a template will look as a quotation  
**So that** I can verify it's correct before applying

**Acceptance Criteria:**
- Preview shows formatted quotation view
- Includes all line items, totals, tax breakdown
- Matches actual quotation PDF format
- Can preview before applying or from template list

---

## Technical Requirements

### Database Schema

See `data-model.md` for detailed schema.

**Tables:**
- `QuotationTemplates` (16 columns)
- `QuotationTemplateLineItems` (9 columns)

### API Endpoints

1. `POST /api/v1/quotation-templates` - Create template
2. `PUT /api/v1/quotation-templates/{templateId}` - Update template
3. `GET /api/v1/quotation-templates` - List/search/filter templates
4. `GET /api/v1/quotation-templates/{templateId}` - Get template details
5. `DELETE /api/v1/quotation-templates/{templateId}` - Soft delete
6. `POST /api/v1/quotation-templates/{templateId}/restore` - Restore deleted
7. `POST /api/v1/quotation-templates/{templateId}/apply` - Apply to quotation
8. `GET /api/v1/quotation-templates/{templateId}/versions` - Version history
9. `POST /api/v1/quotation-templates/{templateId}/approve` - Approve (admin)
10. `GET /api/v1/quotation-templates/usage-stats` - Usage statistics (admin)

### Authorization Rules

- **Sales Rep**: Can create/edit/delete own templates; can view Public, Team, and own Private templates
- **Admin**: Can view/edit/approve all templates; can see usage statistics

### Validation Rules

- Template name: 3-100 characters, unique per user
- Description: 0-255 characters
- Line items: Minimum 1 required
- Quantity/UnitRate: Must be > 0
- Notes/Terms: Maximum 2000 characters
- Visibility: Must be one of: Public, Team, Private

---

## Frontend Requirements

### Sales Rep Pages

- **SR-P21**: Templates List (`/dashboard/templates`)
- **SR-P22**: Create/Edit Template (`/dashboard/templates/create`, `/edit/{templateId}`)
- **SR-P23**: Apply Template Flow (integrated in Create Quotation)
- **SR-P24**: Template Version History View
- **SR-P25**: Template Preview Modal

### Admin Pages

- **A-P17**: Template Approval Queue (`/admin/templates/pending`)
- **A-P18**: Template Usage Stats (`/admin/templates/stats`)

### Components

- TemplateListTable
- TemplateForm
- LineItemsEditor
- TemplatePreview
- ApplyTemplateModal
- VersionHistoryTimeline
- AdminApprovalActions
- UsageStatsWidgets

---

## Testing Requirements

### Backend Tests

- Create/update/delete/restore operations
- Versioning and rollback
- Approval workflow
- Authorization and visibility rules
- Usage tracking
- Search and filtering

### Frontend Tests

- Template CRUD operations
- Apply template flow
- Version history
- Admin approval
- Search and filtering
- Mobile responsiveness

---

## Acceptance Criteria

### Backend
- ✅ Create/update/approve/apply/clone/delete/restore templates works
- ✅ Versioning works, no data loss
- ✅ Owner/visibility security rules enforced
- ✅ Usage metrics always current

### Frontend
- ✅ All flows visible to user, no "backend only" features
- ✅ Templates usable by sales reps when creating quotations
- ✅ All forms/lists are responsive
- ✅ User-friendly with feedback and error handling

---

## Implementation Notes

- Templates store default values but can be overridden when applying
- Version history uses PreviousVersionId for linked list structure
- Soft delete preserves data for audit and restore
- Approval workflow requires admin role
- Usage tracking updates on every apply operation
- Template preview uses same formatting as quotation PDF

---

**Last Updated**: 2025-11-15  
**Version**: 1.0

