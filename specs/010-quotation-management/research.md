# Research & Technical Decisions: Quotation Management (Spec-010)

**Spec**: Spec-010  
**Date**: 2025-11-15

## PDF Generation Library Selection

### Decision: Use QuestPDF

**Rationale:**
- **QuestPDF** is a modern, open-source .NET library with fluent API
- No external dependencies (pure C#)
- Good performance for generating PDFs
- Supports complex layouts (tables, headers, footers)
- Active maintenance and community support
- Free and open-source (MIT license)

**Alternatives Considered:**
- **iTextSharp**: Commercial license required for commercial use
- **SelectPdf**: Commercial license required
- **PuppeteerSharp**: Requires Chrome/Chromium, heavier dependency

**Implementation:**
```csharp
// Example structure
public class QuotationPdfGenerationService
{
    public byte[] GenerateQuotationPdf(Quotation quotation)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Header().Element(Header);
                page.Content().Element(Content);
                page.Footer().Element(Footer);
            });
        }).GeneratePdf();
    }
}
```

## Email Service Integration

### Decision: Use FluentEmail with SMTP

**Rationale:**
- **FluentEmail** provides clean API for email composition
- Supports multiple providers (SMTP, SendGrid, SES)
- Easy template integration (Razor, Liquid)
- Can switch providers without code changes
- Good error handling and retry logic

**Configuration:**
- Primary: SMTP (for development/testing)
- Production: SendGrid or AWS SES (configurable)
- Templates: HTML email templates stored in `Infrastructure/Notifications/Templates/`

**Implementation:**
```csharp
public class QuotationEmailService
{
    public async Task SendQuotationEmailAsync(
        Quotation quotation,
        string recipientEmail,
        byte[] pdfAttachment,
        string accessLink)
    {
        var email = Email
            .From("noreply@company.com")
            .To(recipientEmail)
            .Subject($"Quotation {quotation.QuotationNumber}")
            .Body(htmlTemplate)
            .Attach(new Attachment(pdfAttachment, "quotation.pdf"))
            .SendAsync();
    }
}
```

## Access Token Generation

### Decision: Use Cryptographically Secure Random Generator

**Rationale:**
- Security: Must be unguessable to prevent unauthorized access
- Length: 32+ characters provides sufficient entropy
- Format: Base64URL encoding for URL-safe tokens

**Implementation:**
```csharp
public static string GenerateAccessToken()
{
    var bytes = new byte[32];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(bytes);
    }
    return Convert.ToBase64UrlString(bytes);
}
```

**Security Considerations:**
- Never expose full token in logs
- Store hashed version if possible (but need plain text for URL)
- Implement rate limiting on token validation endpoints
- Expire tokens after reasonable time (30-90 days)

## Status Transition Rules

### Decision: Enforce Status Lifecycle Strictly

**Valid Transitions:**
- DRAFT → SENT (via SendQuotationCommand)
- SENT → VIEWED (automatic on first view)
- VIEWED → ACCEPTED (via client response)
- VIEWED → REJECTED (via client response)
- VIEWED → NEEDS_MODIFICATION (via client response, then back to SENT)
- Any → EXPIRED (automatic via background job)
- DRAFT → CANCELLED (via delete)

**Invalid Transitions:**
- SENT → DRAFT (cannot undo sending)
- ACCEPTED → REJECTED (final state)
- REJECTED → ACCEPTED (final state)
- EXPIRED → any other status (except resend creates new link)

**Implementation:**
- Status validation in command handlers
- QuotationStatusHistory records all transitions
- Domain events emitted for each transition

## Client Portal Security

### Decision: Token-Based Authorization (No Authentication)

**Rationale:**
- Clients should not need to create accounts
- Secure token provides sufficient authorization
- Simpler user experience
- Token can be revoked if needed (IsActive flag)

**Security Measures:**
1. Token must be cryptographically secure
2. Token validated on every request
3. Expired tokens rejected
4. Revoked tokens (IsActive=false) rejected
5. Rate limiting on public endpoints
6. IP tracking for fraud detection
7. No sensitive data exposed (no internal notes, creator info)

## Background Job Scheduling

### Decision: Use Quartz.NET

**Rationale:**
- Already used in project (SuspiciousActivityAggregationJob)
- Supports cron expressions
- Reliable job execution
- Can persist job state
- Good error handling and retry logic

**Jobs:**
1. **QuotationExpirationCheckJob**: Daily at midnight
2. **UnviewedQuotationReminderJob**: Daily at 9 AM
3. **PendingResponseFollowUpJob**: Daily at 3 PM

## Email Template Strategy

### Decision: HTML Templates with Razor Syntax

**Rationale:**
- Professional appearance
- Can include dynamic data
- Supports branding (logo, colors)
- Easy to maintain and update

**Template Location:**
- `src/Backend/CRM.Infrastructure/Notifications/Templates/QuotationSentEmail.html`
- `src/Backend/CRM.Infrastructure/Notifications/Templates/QuotationAcceptedNotification.html`
- `src/Backend/CRM.Infrastructure/Notifications/Templates/QuotationRejectedNotification.html`
- `src/Backend/CRM.Infrastructure/Notifications/Templates/QuotationExpirationReminder.html`

**Template Variables:**
- `{QuotationNumber}`, `{ClientName}`, `{TotalAmount}`, `{ValidUntil}`, `{AccessLink}`, `{CompanyName}`, etc.

## PDF Caching Strategy

### Decision: Cache PDF for 24 Hours

**Rationale:**
- PDF generation is CPU-intensive
- Quotations rarely change after sending
- 24-hour cache balances freshness vs performance
- Can invalidate cache if quotation updated

**Implementation:**
- Use `IMemoryCache` with 24-hour expiration
- Cache key: `quotation-pdf-{quotationId}`
- Invalidate on quotation update
- Regenerate if cache miss

## Access Link Expiration

### Decision: Default to 90 Days, Configurable

**Rationale:**
- Balances security (not too long) with usability (not too short)
- Can be customized per quotation if needed
- Background job can extend expiration if needed

**Configuration:**
- Default: 90 days from creation
- Can be set per quotation (ExpiresAt field)
- Can be null (never expires) for special cases

## Notification Strategy

### Decision: Immediate Email Notifications

**Rationale:**
- Sales reps need immediate feedback
- Email is reliable and doesn't require app open
- Can add in-app notifications later (Spec-013)

**Notifications:**
1. Quotation sent → Confirmation to sales rep
2. Client viewed → Optional notification (can be disabled)
3. Client responded → Immediate notification to sales rep
4. Quotation expired → Daily digest or immediate

## Performance Considerations

1. **PDF Generation**: Cache for 24 hours, generate async if possible
2. **Email Sending**: Use background queue to avoid blocking
3. **Access Link Validation**: Index on AccessToken for fast lookup
4. **Status History**: Index on (QuotationId, ChangedAt) for timeline queries
5. **View Tracking**: Batch updates if multiple views in short time

## Future Enhancements

1. **Real-time Notifications**: WebSocket or SignalR for instant updates
2. **Email Tracking**: Track email opens and link clicks
3. **Quotation Templates**: Pre-defined templates for common quotations
4. **Bulk Operations**: Send multiple quotations at once
5. **Client Portal Dashboard**: Show all quotations for a client
6. **Digital Signatures**: Integrate e-signature for acceptance

