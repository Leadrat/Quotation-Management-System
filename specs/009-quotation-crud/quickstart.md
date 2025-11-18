# Quickstart: Spec-009 Quotation Entity & CRUD Operations

**Spec**: Spec-009  
**Last Updated**: 2025-11-15

## Prerequisites

- Backend solution built from `main` with Specs 001-008 applied.
- PostgreSQL database running and accessible.
- Frontend Next.js app set up with TailAdmin template.
- JWT authentication working (Spec-003).
- Client CRUD operations working (Spec-006).

## Backend Setup

### 1. Database Migration

Run the migration to create `Quotations` and `QuotationLineItems` tables:

```bash
cd src/Backend/CRM.Infrastructure
dotnet ef migrations add CreateQuotationsTables --startup-project ../CRM.Api
dotnet ef database update --startup-project ../CRM.Api
```

**Verify**:
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('Quotations', 'QuotationLineItems');
```

### 2. Configuration

Add quotation settings to `appsettings.json`:

```json
{
  "Quotations": {
    "NumberFormat": "QT-{Year}-{Sequence}",
    "DefaultValidDays": 30,
    "TaxRate": 18.0,
    "MaxLineItems": 100
  },
  "Company": {
    "StateCode": "27",
    "StateName": "Maharashtra"
  }
}
```

### 3. Application Wiring

Ensure in `Program.cs`:
- MediatR registered
- AutoMapper profiles registered (including `QuotationProfile`)
- FluentValidation validators registered
- Authorization policies configured

### 4. Seed Data (Optional)

Create test quotations via `DbSeeder` or API:

```csharp
// Example: Create quotation via API
POST /api/v1/quotations
{
  "clientId": "...",
  "quotationDate": "2025-11-15",
  "validUntil": "2025-12-15",
  "discountPercentage": 10.0,
  "notes": "Test quotation",
  "lineItems": [
    {
      "itemName": "Cloud Storage 1TB/month",
      "description": "Monthly subscription",
      "quantity": 10,
      "unitRate": 5000.00
    }
  ]
}
```

## Frontend Setup

### 1. API Integration

Add `QuotationsApi` methods to `src/Frontend/web/src/lib/api.ts`:

```typescript
export const QuotationsApi = {
  list: (params) => apiFetch('/api/v1/quotations', { params }),
  get: (id) => apiFetch(`/api/v1/quotations/${id}`),
  create: (payload) => apiFetch('/api/v1/quotations', { method: 'POST', body: JSON.stringify(payload) }),
  update: (id, payload) => apiFetch(`/api/v1/quotations/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  delete: (id) => apiFetch(`/api/v1/quotations/${id}`, { method: 'DELETE' })
};
```

### 2. Create Pages

Create quotation pages in `src/Frontend/web/src/app/(protected)/quotations/`:
- `page.tsx` - List page
- `create/page.tsx` - Create form
- `[id]/edit/page.tsx` - Edit form
- `[id]/view/page.tsx` - View quotation
- `[id]/timeline/page.tsx` - Status timeline

### 3. Create Components

Create reusable components in `src/Frontend/web/src/components/quotations/`:
- `QuotationTable.tsx`
- `QuotationForm.tsx`
- `QuotationViewer.tsx`
- `LineItemRepeater.tsx`
- `TaxCalculationPreview.tsx`
- `ClientSelector.tsx`

### 4. Tax Calculation Service

Create `src/Frontend/web/src/utils/taxCalculator.ts`:

```typescript
export function calculateTax(
  subtotal: number,
  discountAmount: number,
  isIntraState: boolean
): { cgst: number; sgst: number; igst: number; total: number } {
  const taxableAmount = subtotal - discountAmount;
  const taxRate = 0.18; // 18%
  
  if (isIntraState) {
    const cgst = taxableAmount * 0.09;
    const sgst = taxableAmount * 0.09;
    return { cgst, sgst, igst: 0, total: cgst + sgst };
  } else {
    const igst = taxableAmount * taxRate;
    return { cgst: 0, sgst: 0, igst, total: igst };
  }
}
```

## Testing

### Backend Unit Tests

```bash
cd tests/CRM.Tests
dotnet test --filter "FullyQualifiedName~Quotation"
```

### Backend Integration Tests

```bash
cd tests/CRM.Tests.Integration
dotnet test --filter "FullyQualifiedName~Quotation"
```

### Frontend Tests

```bash
cd src/Frontend/web
npm test -- quotations
```

### E2E Tests

```bash
cd src/Frontend/web
npm run test:e2e -- quotations
```

## Verification Checklist

### Backend
- [ ] Migration runs successfully
- [ ] Quotation entity created with all properties
- [ ] Line items entity created with foreign key
- [ ] Tax calculation service works (intra-state and inter-state)
- [ ] Quotation number generator creates unique numbers
- [ ] Create quotation endpoint returns 201 with correct totals
- [ ] Update quotation (draft only) works
- [ ] Delete quotation (draft only) works
- [ ] Authorization: SalesRep sees only own quotations
- [ ] Authorization: Admin sees all quotations
- [ ] Filter by status, client, date works
- [ ] Pagination works

### Frontend
- [ ] Quotation list page loads and displays quotations
- [ ] Create form validates inputs
- [ ] Tax calculation updates in real-time
- [ ] Line items can be added/removed
- [ ] Edit form loads existing quotation
- [ ] View page displays quotation like PDF
- [ ] Status badges color-coded correctly
- [ ] Mobile responsive
- [ ] Error handling works

### Integration
- [ ] Create quotation from frontend saves to database
- [ ] Tax amounts match between frontend and backend
- [ ] Authorization enforced (cannot access other user's quotations)
- [ ] Status transitions work correctly

## Common Issues

### Issue: Tax calculation incorrect
**Solution**: Verify client state code matches company state code for intra-state calculation.

### Issue: Quotation number collision
**Solution**: Check `QuotationNumberGenerator` retry logic and database sequence.

### Issue: Cannot edit quotation
**Solution**: Verify quotation status is DRAFT (only draft quotations editable).

### Issue: Frontend tax not updating
**Solution**: Check `useTaxCalculation` hook and debounce timing (300ms).

### Issue: Line items not saving
**Solution**: Verify foreign key relationship and CASCADE DELETE not interfering.

## Next Steps

After Spec-009 completion:
- Spec-010: Quotation Management (send, track, change status)
- Spec-011: Template Management (quotation templates)
- Spec-012: Approval Workflow (discount approvals)

