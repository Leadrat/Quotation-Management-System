# Quickstart: Spec-011 Quotation Template Management

**Spec**: Spec-011  
**Last Updated**: 2025-11-15

## Prerequisites

- ✅ Backend solution with Spec-009 (Quotation Entity) and Spec-010 (Quotation Management) applied
- ✅ PostgreSQL database running and accessible
- ✅ Frontend Next.js app set up with TailAdmin template
- ✅ User authentication and authorization working (JWT, RBAC)

## Backend Setup

### 1. Database Migration

Run the migration to create new tables:

```bash
cd src/Backend/CRM.Infrastructure
dotnet ef migrations add CreateQuotationTemplatesTables --startup-project ../CRM.Api
dotnet ef database update --startup-project ../CRM.Api
```

**Verify:**
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('QuotationTemplates', 'QuotationTemplateLineItems');
```

### 2. Application Wiring

Ensure in `Program.cs`:
- New command/query handlers registered
- Validators registered
- AutoMapper profiles registered
- Controller registered

### 3. Configuration

No additional configuration required (uses existing QuotationSettings if needed).

## Frontend Setup

### 1. API Integration

Extend `QuotationsApi` in `src/Frontend/web/src/lib/api.ts`:

```typescript
export const TemplatesApi = {
  list: (params) => apiFetch(`/quotation-templates?${queryString}`),
  get: (templateId) => apiFetch(`/quotation-templates/${templateId}`),
  create: (payload) => apiFetch(`/quotation-templates`, { method: 'POST', ... }),
  update: (templateId, payload) => apiFetch(`/quotation-templates/${templateId}`, { method: 'PUT', ... }),
  delete: (templateId) => apiFetch(`/quotation-templates/${templateId}`, { method: 'DELETE' }),
  restore: (templateId) => apiFetch(`/quotation-templates/${templateId}/restore`, { method: 'POST' }),
  apply: (templateId, clientId) => apiFetch(`/quotation-templates/${templateId}/apply?clientId=${clientId}`, { method: 'POST' }),
  getVersions: (templateId) => apiFetch(`/quotation-templates/${templateId}/versions`),
  approve: (templateId) => apiFetch(`/quotation-templates/${templateId}/approve`, { method: 'POST' }),
  getUsageStats: () => apiFetch(`/quotation-templates/usage-stats`)
};
```

### 2. Create Pages

Create pages:
- `/templates` - Templates list
- `/templates/create` - Create template
- `/templates/[id]/edit` - Edit template
- `/templates/[id]/versions` - Version history
- `/admin/templates/pending` - Approval queue
- `/admin/templates/stats` - Usage statistics

### 3. Create Components

Create reusable components:
- `TemplateListTable.tsx`
- `TemplateForm.tsx`
- `LineItemsEditor.tsx`
- `TemplatePreview.tsx`
- `ApplyTemplateModal.tsx`
- `VersionHistoryTimeline.tsx`
- `AdminApprovalActions.tsx`
- `UsageStatsWidgets.tsx`

## Testing

### Backend Tests

```bash
cd tests/CRM.Tests
dotnet test --filter "QuotationTemplate"
```

### Frontend Tests

```bash
cd src/Frontend/web
npm test -- templates
```

## Verification Checklist

### Backend
- [ ] Migration runs successfully
- [ ] All two new tables created
- [ ] Create template endpoint returns 201
- [ ] Update template increments version
- [ ] Apply template returns CreateQuotationRequest
- [ ] Version history query works
- [ ] Approval endpoint works (admin only)
- [ ] Usage stats endpoint works (admin only)

### Frontend
- [ ] Templates list page loads
- [ ] Create template form works
- [ ] Edit template form works
- [ ] Apply template modal works in quotation creation
- [ ] Version history displays correctly
- [ ] Admin approval page works
- [ ] Usage stats page works
- [ ] All pages mobile responsive

### Integration
- [ ] Create template → Apply to quotation → Quotation created successfully
- [ ] Update template → Version increments → History preserved
- [ ] Admin approves template → Template visible to all sales reps
- [ ] Delete template → Template hidden → Restore works

## Common Issues

### Issue: Template not visible to sales rep
**Solution**: Check visibility rules (Public/Team/Private) and approval status.

### Issue: Version history not showing
**Solution**: Verify PreviousVersionId links are set correctly on update.

### Issue: Apply template fails
**Solution**: Check template visibility, ensure template not deleted, verify client exists.

### Issue: Approval not working
**Solution**: Verify user has Admin role, check IsApproved flag.

## Next Steps

After Spec-011 completion:
- Spec-012: Approval Workflow (enhance template approval)
- Spec-013: Notification System (notify on template approval)
- Spec-014: Template Categories/Tags

---

**Last Updated**: 2025-11-15

