# Quickstart: Spec-010 Quotation Management

**Spec**: Spec-010  
**Last Updated**: 2025-11-15

## Prerequisites

- ✅ Backend solution built from `main` with Spec-009 (Quotation Entity) applied.
- ✅ PostgreSQL database running and accessible.
- ✅ Frontend Next.js app set up with TailAdmin template.
- ✅ Email service configured (FluentEmail with SMTP/InMemory provider).
- ✅ PDF generation library (QuestPDF) installed with Community license.
- ✅ NCrontab package installed for background job scheduling.

## Backend Setup

### 1. Install Dependencies

**✅ COMPLETED** - All dependencies already installed:

```bash
# Already in CRM.Application.csproj:
# - QuestPDF (2.20.1)
# - FluentEmail.Core (3.0.2)
# - FluentEmail.Smtp (3.0.2)
# - FluentEmail.Razor (3.0.2)

# Already in CRM.Infrastructure.csproj:
# - NCrontab (3.3.3)
```

### 2. Database Migration

**✅ COMPLETED** - Tables auto-created via `EnsureCreated()` in `Program.cs`:

```sql
-- Verify tables exist:
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('QuotationAccessLinks', 'QuotationStatusHistory', 'QuotationResponses');

-- Expected result:
-- QuotationAccessLinks
-- QuotationStatusHistory  
-- QuotationResponses
```

**Schema Details:**
- `QuotationAccessLinks`: Stores secure access tokens for client portal
- `QuotationStatusHistory`: Audit trail of all status changes
- `QuotationResponses`: Client responses (accept/reject/needs modification)

### 3. Configuration

**✅ COMPLETED** - Configuration registered in `Program.cs`:

Add these settings to `appsettings.json` (defaults shown):

```json
{
  "Email": {
    "Provider": "InMemory",  // or "Smtp" for production
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "From": "noreply@company.com",
    "FromName": "Your Company Name"
  },
  "QuotationManagement": {
    "AccessLinkExpirationDays": 90,
    "PdfCacheHours": 24,
    "BaseUrl": "https://crm.example.com"
  },
  "Company": {
    "StateCode": "27",
    "StateName": "Maharashtra"
  }
}
```

**Key Settings:**
- `Email.Provider`: "InMemory" (dev) or "Smtp" (production)
- `QuotationManagement.BaseUrl`: Used for generating access links
- `Company.StateCode`: For GST tax calculation (CGST/SGST vs IGST)

### 4. Application Wiring

**✅ COMPLETED** - All services registered in `Program.cs`:

```csharp
// QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// Services
builder.Services.AddScoped<IQuotationPdfGenerationService, QuotationPdfGenerationService>();
builder.Services.AddScoped<IQuotationEmailService, QuotationEmailService>();
builder.Services.AddScoped<IQuotationSendWorkflow, QuotationSendWorkflow>();
builder.Services.AddScoped<QuotationReminderService>();

// Command/Query Handlers (9 handlers total)
// Validators (8 validators total)

// FluentEmail with conditional SMTP
builder.Services.AddFluentEmail(emailFrom, emailFromName)
    .AddRazorRenderer()
    .AddSmtpSender(...) or .AddSingleton<ISender, NullSender>();

// Background Jobs (3 cron jobs)
builder.Services.AddHostedService<QuotationExpirationCheckJob>();
builder.Services.AddHostedService<UnviewedQuotationReminderJob>();
builder.Services.AddHostedService<PendingResponseFollowUpJob>();
```

### 5. Email Templates

**✅ COMPLETED** - HTML templates inline in `QuotationEmailService.cs`:
- `GenerateEmailBody()` - Main quotation email with PDF attachment
- `GenerateAcceptedNotificationBody()` - Sales rep notification for acceptance
- `GenerateRejectedNotificationBody()` - Sales rep notification for rejection
- `GenerateUnviewedReminderBody()` - Reminder for unviewed quotations
- `GeneratePendingResponseFollowUpBody()` - Follow-up for pending responses

## Frontend Setup

### 1. API Integration

**✅ COMPLETED** - Extended `QuotationsApi` and added `ClientPortalApi` in `src/Frontend/web/src/lib/api.ts`:

```typescript
export const QuotationsApi = {
  // Spec-009 methods...
  list, get, getByClient, create, update, delete,
  
  // Spec-010 new methods:
  send: (quotationId, payload) => apiFetch(`/quotations/${quotationId}/send`, { method: 'POST', ... }),
  resend: (quotationId, payload) => apiFetch(`/quotations/${quotationId}/resend`, { method: 'POST', ... }),
  downloadPdf: (quotationId) => window.open(`${API_BASE}/quotations/${quotationId}/download-pdf`),
  getStatusHistory: (quotationId) => apiFetch(`/quotations/${quotationId}/status-history`),
  getResponse: (quotationId) => apiFetch(`/quotations/${quotationId}/response`),
  getAccessLink: (quotationId) => apiFetch(`/quotations/${quotationId}/access-link`)
};

export const ClientPortalApi = {
  getQuotationByAccessToken: (quotationId, token) => apiFetch(`/client-portal/quotations/${quotationId}/${token}`),
  submitQuotationResponse: (quotationId, token, payload) => apiFetch(`/client-portal/quotations/${quotationId}/${token}/respond`, { method: 'POST', ... }),
  downloadPdf: (quotationId, token) => window.open(`${API_BASE}/client-portal/quotations/${quotationId}/${token}/download-pdf`)
};
```

### 2. Create Public Client Portal Route

**✅ COMPLETED** - Public route created (no authentication required):
- `src/Frontend/web/src/app/(public)/client-portal/quotations/[quotationId]/[token]/page.tsx`
- Displays quotation details, line items, tax breakdown
- CTA buttons: Accept, Request Modifications, Decline, Download PDF, Contact Sales
- Auto-tracks view on page load via `MarkQuotationAsViewedCommand`

### 3. Create Components

**✅ COMPLETED** - All components created in `src/Frontend/web/src/components/quotations/`:
- `SendQuotationModal.tsx` - Send/resend with email validation and CC/BCC
- `ClientResponseModal.tsx` - Public client response form
- `QuotationStatusTimeline.tsx` - Visual status history with timestamps
- `ClientResponseCard.tsx` - Displays client response with styling
- `QuotationStatusBadge.tsx` - Color-coded status badges
- `useToast.tsx` - Toast notification hook

## Testing

### Backend Unit Tests

**✅ COMPLETED** - 58 tests created:

```bash
cd tests/CRM.Tests
dotnet test --filter "Quotation"

# New tests in Phase 8:
# - QuotationPdfGenerationServiceTests.cs (8 tests)
# - QuotationEmailServiceTests.cs (9 tests)
# Plus existing Spec-009 tests (41 tests)
```

### Backend Integration Tests

**✅ COMPLETED** - 18 integration tests created:

```bash
cd tests/CRM.Tests.Integration
dotnet test --filter "Quotation"

# New tests in Phase 8:
# - SendQuotationEndpointTests.cs (3 tests)
# - ClientPortalEndpointTests.cs (4 tests)
# - QuotationStatusHistoryEndpointTests.cs (3 tests)
# - QuotationResponseEndpointTests.cs (4 tests)
# Plus existing QuotationEndpointTests.cs (5 tests from Spec-009)
```

### Frontend Tests

```bash
cd src/Frontend/web
npm test

# Component tests available:
# - All quotation components render without errors
# - API integration tests pass
# - Client portal public access works
```

## Verification Checklist

### Backend
- [x] Migration runs successfully (via EnsureCreated)
- [x] All three new tables created (QuotationAccessLinks, QuotationStatusHistory, QuotationResponses)
- [x] PDF generation service works (QuestPDF with Community license)
- [x] Email service configured (FluentEmail with SMTP/InMemory)
- [x] Send quotation endpoint returns 200
- [x] Access link generated correctly (256-bit secure token)
- [x] Status history recorded on send
- [x] Client portal endpoint works without auth ([AllowAnonymous])
- [x] Background jobs scheduled correctly (3 cron jobs running)

### Frontend
- [x] Send quotation modal opens and works
- [x] Status timeline displays correctly (with user names, timestamps, reasons)
- [x] Client portal page loads without login
- [x] Client can view quotation (auto-tracks view)
- [x] Client can submit response (accept/reject/modify)
- [x] Analytics page shows view count and telemetry
- [x] PDF download works (both authenticated and public)

### Integration
- [x] Send quotation → email composed → access link created → status changed to SENT
- [x] Client views → status changes to VIEWED → FirstViewedAt recorded
- [x] Client responds → quotation status updated → sales rep notified via email
- [x] Expiration job marks expired quotations (runs daily at midnight UTC)
- [x] Reminder jobs send follow-up emails (unviewed at 9 AM, pending at 3 PM UTC)

## Common Issues

### Issue: Email not sending
**Solution**: Check SMTP configuration, verify credentials, check firewall.

### Issue: PDF generation fails
**Solution**: Verify QuestPDF installed, check file permissions, verify quotation data.

### Issue: Access token not working
**Solution**: Verify token generation, check IsActive flag, verify ExpiresAt.

### Issue: Client portal 404
**Solution**: Verify route is public (no auth middleware), check token validation logic.

## Next Steps

After Spec-010 completion:
- Spec-011: Template Management (quotation templates)
- Spec-012: Approval Workflow (discount approvals)
- Spec-013: Payment Processing (mark as paid)

