# Requirements Checklist: Spec-011 Quotation Template Management

**Spec**: Spec-011  
**Last Updated**: 2025-11-15

## Functional Requirements

### FR1: Template Creation
- [ ] Sales rep can create new quotation template
- [ ] Template must have unique name (per owner)
- [ ] Template must have at least one line item
- [ ] Template can have description, notes, default discount
- [ ] Template can be set to Public, Team, or Private visibility
- [ ] Template version starts at 1
- [ ] Template owner and creation timestamp recorded

### FR2: Template Editing
- [ ] Owner or Admin can edit template
- [ ] Editing creates new version (version increments)
- [ ] Previous version preserved (linked via PreviousVersionId)
- [ ] Can update name, description, line items, notes, discount
- [ ] Cannot edit if template is deleted
- [ ] Cannot edit approved template if not owner (unless admin)

### FR3: Template Application
- [ ] Sales rep can apply template when creating quotation
- [ ] Only visible templates shown (based on visibility rules)
- [ ] Applying template pre-fills quotation form
- [ ] Usage count increments on apply
- [ ] Last used timestamp updates on apply
- [ ] Can preview template before applying

### FR4: Template Visibility
- [ ] Public templates: Visible to all if approved
- [ ] Team templates: Visible to users with same role as owner
- [ ] Private templates: Visible only to owner
- [ ] Admin can see all templates
- [ ] Visibility rules enforced at query level

### FR5: Template Approval
- [ ] Admin can approve templates
- [ ] Approved templates marked as Public
- [ ] Approval timestamp and approver recorded
- [ ] Approved templates visible to all sales reps
- [ ] Approval notification (future enhancement)

### FR6: Template Version History
- [ ] Can view all versions of a template
- [ ] Version history shows who changed what and when
- [ ] Can restore previous version
- [ ] Version history preserved even after deletion
- [ ] Versions linked via PreviousVersionId

### FR7: Template Deletion
- [ ] Owner or Admin can delete template
- [ ] Deletion is soft delete (sets DeletedAt)
- [ ] Deleted templates hidden from normal listing
- [ ] Can restore deleted templates
- [ ] Version history preserved after deletion

### FR8: Template Search and Filter
- [ ] Can search templates by name or description
- [ ] Can filter by visibility (My, Team, Public)
- [ ] Can filter by approval status
- [ ] Can filter by owner
- [ ] Results paginated
- [ ] Can sort by name, last updated, usage count

### FR9: Template Usage Statistics
- [ ] Admin can view usage statistics
- [ ] Shows total templates, total usage
- [ ] Shows most used templates
- [ ] Shows templates by visibility breakdown
- [ ] Shows templates by role breakdown
- [ ] Can export statistics as CSV/Excel

### FR10: Template Cloning
- [ ] Can create new template based on existing
- [ ] New template has new ID and version 1
- [ ] Original template unchanged
- [ ] New template owned by current user

---

## Non-Functional Requirements

### NFR1: Performance
- [ ] List templates: < 200ms response time
- [ ] Get template by ID: < 50ms response time
- [ ] Apply template: < 100ms response time
- [ ] Create template: < 300ms response time
- [ ] Update template: < 400ms response time

### NFR2: Security
- [ ] All endpoints require authentication
- [ ] Authorization rules enforced
- [ ] Input validation on all endpoints
- [ ] SQL injection prevention
- [ ] XSS prevention

### NFR3: Scalability
- [ ] Supports 1000+ templates per organization
- [ ] Pagination for large result sets
- [ ] Indexes optimized for common queries
- [ ] Can handle 100+ template applications per day

### NFR4: Usability
- [ ] Mobile responsive design
- [ ] Accessible (WCAG 2.1 AA)
- [ ] Clear error messages
- [ ] Loading states for async operations
- [ ] Toast notifications for user feedback

### NFR5: Reliability
- [ ] Version history never lost
- [ ] Soft delete preserves data
- [ ] Transaction support for multi-step operations
- [ ] Error handling and logging

### NFR6: Maintainability
- [ ] Clean code architecture (CQRS)
- [ ] Comprehensive test coverage (90%+)
- [ ] Documentation complete
- [ ] Code follows existing patterns

---

## User Interface Requirements

### UI1: Templates List Page
- [ ] Displays all available templates in table/grid
- [ ] Search box for name/description
- [ ] Filter dropdown (My, Team, Public)
- [ ] Pagination controls
- [ ] Actions: View, Apply, Edit, Delete, Clone
- [ ] Badge for approved templates
- [ ] Mobile responsive (cards on mobile)

### UI2: Create/Edit Template Form
- [ ] Form fields: Name, Description, Visibility
- [ ] Line items editor (add/remove/edit)
- [ ] Auto-calculation of line item amounts
- [ ] Notes/terms text area
- [ ] Default discount input
- [ ] Preview button
- [ ] Save, Save-as-New, Cancel buttons
- [ ] Form validation with error messages

### UI3: Apply Template Flow
- [ ] "Apply Template" button in quotation creation
- [ ] Modal opens with template list
- [ ] Search and filter in modal
- [ ] Preview before applying
- [ ] Pre-fills quotation form on apply
- [ ] User can modify before submitting

### UI4: Version History View
- [ ] Timeline or table of versions
- [ ] Shows version number, date, user
- [ ] Restore previous version button
- [ ] Compare versions (future enhancement)

### UI5: Template Preview
- [ ] Modal shows formatted quotation preview
- [ ] Includes all line items, totals, tax breakdown
- [ ] Matches actual quotation PDF format

### UI6: Admin Approval Page
- [ ] List of unapproved templates
- [ ] Template details and preview
- [ ] Approve/Reject buttons
- [ ] Version history link

### UI7: Usage Statistics Page
- [ ] Cards with key metrics
- [ ] Charts (pie, bar) for breakdowns
- [ ] Table with detailed statistics
- [ ] Export to CSV/Excel button

---

## Integration Requirements

### INT1: Quotation Creation Integration
- [ ] Apply template button in quotation creation page
- [ ] Template selection modal
- [ ] Pre-fill quotation form with template data
- [ ] User can modify before submitting

### INT2: API Integration
- [ ] All 10 endpoints implemented
- [ ] Standardized response format
- [ ] Error handling
- [ ] Swagger documentation

### INT3: Database Integration
- [ ] Migration creates tables correctly
- [ ] Foreign keys and constraints enforced
- [ ] Indexes created for performance
- [ ] Soft delete filtering works

---

## Testing Requirements

### TEST1: Backend Unit Tests
- [ ] Create template command test
- [ ] Update template command test (versioning)
- [ ] Delete template command test (soft delete)
- [ ] Restore template command test
- [ ] Approve template command test
- [ ] Apply template command test (usage tracking)
- [ ] Get template by ID query test
- [ ] List templates query test (filtering, pagination)
- [ ] Version history query test
- [ ] Usage stats query test
- [ ] Authorization tests for all operations

### TEST2: Backend Integration Tests
- [ ] Create template endpoint test
- [ ] Update template endpoint test
- [ ] Delete template endpoint test
- [ ] Restore template endpoint test
- [ ] Apply template endpoint test
- [ ] Get versions endpoint test
- [ ] Approve template endpoint test (admin only)
- [ ] Usage stats endpoint test (admin only)
- [ ] Authorization tests for all endpoints

### TEST3: Frontend Component Tests
- [ ] TemplateListTable component test
- [ ] TemplateForm component test
- [ ] LineItemsEditor component test
- [ ] TemplatePreview component test
- [ ] ApplyTemplateModal component test
- [ ] VersionHistoryTimeline component test
- [ ] Form validation tests

### TEST4: Frontend E2E Tests
- [ ] Create template → Apply → Create quotation flow
- [ ] Update template → Version history → Restore flow
- [ ] Admin approval → Template becomes public flow
- [ ] Search and filter templates flow
- [ ] Delete and restore template flow

---

## Documentation Requirements

### DOC1: API Documentation
- [ ] Swagger/OpenAPI specification complete
- [ ] All endpoints documented
- [ ] Request/response schemas documented
- [ ] Authorization requirements documented
- [ ] Example requests/responses provided

### DOC2: Code Documentation
- [ ] XML comments on all public methods
- [ ] README for template management module
- [ ] Architecture decision records (if needed)

### DOC3: User Documentation
- [ ] Quickstart guide
- [ ] User guide for sales reps
- [ ] Admin guide for approval workflow
- [ ] FAQ section

---

## Acceptance Criteria Summary

### Backend
- ✅ All 10 API endpoints working
- ✅ Versioning preserves history
- ✅ Authorization rules enforced
- ✅ Usage tracking accurate
- ✅ 90%+ test coverage

### Frontend
- ✅ All 7 pages functional
- ✅ Templates usable in quotation creation
- ✅ All forms/lists responsive
- ✅ User-friendly with feedback and error handling
- ✅ Accessible (WCAG 2.1 AA)

### Integration
- ✅ Template application works in quotation creation
- ✅ Version history accessible
- ✅ Admin approval workflow functional
- ✅ Usage statistics accurate

---

**Last Updated**: 2025-11-15

