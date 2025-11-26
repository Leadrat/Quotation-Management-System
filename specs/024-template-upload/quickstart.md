# Quickstart: Spec-001 Document Template Upload & Conversion

**Spec**: Spec-001  
**Last Updated**: 2025-01-27

## Prerequisites

- ✅ Backend solution built from `main` with Spec-011 (Quotation Template Management) applied
- ✅ PostgreSQL database running and accessible
- ✅ Frontend Next.js app set up with TailAdmin template
- ✅ File storage service configured (FileStorageServiceAdapter)
- ✅ Company details configured (Spec-022: Company Details Admin Configuration)

## Backend Setup

### 1. Install Dependencies

**Required NuGet Packages:**

```bash
# Add to CRM.Application.csproj:
dotnet add package DocumentFormat.OpenXml --version 3.0.0

# Add to CRM.Infrastructure.csproj:
dotnet add package PdfSharpCore --version 1.3.0
# Optional for OCR (scanned PDFs):
dotnet add package Tesseract --version 5.2.0
```

**Package Details:**
- **DocumentFormat.OpenXml**: Microsoft library for Word document manipulation (.docx)
- **PdfSharpCore**: .NET port of PdfSharp for PDF text extraction
- **Tesseract**: OCR library for scanned PDFs (optional, for future enhancement)

### 2. Database Migration

**Run migration script from `data-model.md`:**

```sql
-- Add new columns to QuotationTemplates
ALTER TABLE QuotationTemplates
ADD COLUMN IsFileBased BOOLEAN NOT NULL DEFAULT false,
ADD COLUMN TemplateFilePath VARCHAR(500) NULL,
ADD COLUMN TemplateType VARCHAR(50) NULL,
ADD COLUMN OriginalFileName VARCHAR(255) NULL,
ADD COLUMN FileSizeBytes BIGINT NULL,
ADD COLUMN ProcessingStatus VARCHAR(50) NULL,
ADD COLUMN ProcessingErrorMessage TEXT NULL;

-- Create TemplatePlaceholders table
CREATE TABLE TemplatePlaceholders (
    PlaceholderId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TemplateId UUID NOT NULL,
    PlaceholderName VARCHAR(100) NOT NULL,
    PlaceholderType VARCHAR(50) NOT NULL,
    OriginalText TEXT NULL,
    PositionInDocument INTEGER NULL,
    IsManuallyAdded BOOLEAN NOT NULL DEFAULT false,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_TemplatePlaceholders_Template
        FOREIGN KEY (TemplateId) REFERENCES QuotationTemplates(TemplateId) ON DELETE CASCADE
);

-- Create indexes (see data-model.md for full script)
```

**Or use EF Core migration:**

```bash
cd src/Backend/CRM.Infrastructure
dotnet ef migrations add AddDocumentTemplateSupport
dotnet ef database update
```

### 3. Configuration

**Add file upload settings to `appsettings.json`:**

```json
{
  "FileUpload": {
    "MaxFileSizeBytes": 52428800,
    "AllowedExtensions": [".pdf", ".doc", ".docx"],
    "UploadPath": "wwwroot/uploads/templates",
    "TemplatePath": "wwwroot/templates"
  },
  "DocumentProcessing": {
    "MaxProcessingTimeSeconds": 300,
    "EnableAsyncProcessing": true,
    "AsyncProcessingThresholdPages": 10
  }
}
```

### 4. Register Services

**Update `src/Backend/CRM.Api/Program.cs`:**

```csharp
// Register document processing services
services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
services.AddScoped<IPlaceholderIdentificationService, PlaceholderIdentificationService>();
services.AddScoped<ITemplateConversionService, TemplateConversionService>();
services.AddScoped<IPdfParserService, PdfParserService>();
services.AddScoped<IWordDocumentService, WordDocumentService>();

// Configure file upload limits
services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
});
```

## Frontend Setup

### 1. Install Dependencies

**No additional dependencies required** - uses existing React, Next.js, and Tailwind CSS setup.

### 2. Create Upload Page

**Create `src/Frontend/web/src/app/(protected)/templates/upload/page.tsx`:**

```typescript
// Template upload page component
// See implementation in tasks.md
```

### 3. Create Components

**Create components in `src/Frontend/web/src/components/templates/`:**

- `DocumentUploader.tsx`: File upload component with drag-and-drop
- `TemplatePreview.tsx`: Preview converted template with placeholders
- `PlaceholderEditor.tsx`: Edit/manage placeholders interface

## Testing

### 1. Unit Tests

**Create test files:**

```bash
# Backend unit tests
tests/CRM.Tests.Unit/DocumentTemplates/
├── DocumentProcessingServiceTests.cs
├── PlaceholderIdentificationServiceTests.cs
└── TemplateConversionServiceTests.cs
```

### 2. Integration Tests

**Create integration test:**

```bash
tests/CRM.Tests.Integration/DocumentTemplates/
└── DocumentTemplateIntegrationTests.cs
```

### 3. Test Documents

**Prepare test documents:**

- Simple Word document with company details
- Complex Word document with tables and images
- PDF with text content
- PDF with scanned content (for OCR testing)

## API Testing

### Upload Document

```bash
curl -X POST "http://localhost:5000/api/v1/document-templates/upload" \
  -H "Authorization: Bearer {token}" \
  -F "file=@sample-quotation.docx" \
  -F "templateType=Quotation" \
  -F "name=Sample Quotation Template" \
  -F "description=Test template"
```

### Convert Document

```bash
curl -X POST "http://localhost:5000/api/v1/document-templates/{templateId}/convert" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"autoIdentify": true}'
```

### Get Template Preview

```bash
curl -X GET "http://localhost:5000/api/v1/document-templates/{templateId}/preview" \
  -H "Authorization: Bearer {token}"
```

### Download Template

```bash
curl -X GET "http://localhost:5000/api/v1/document-templates/{templateId}/download" \
  -H "Authorization: Bearer {token}" \
  -o template.docx
```

## Development Workflow

1. **Upload Document**: User uploads PDF or Word document via frontend
2. **Process Document**: Backend extracts text and identifies company details
3. **Convert to Template**: Replace identified text with placeholders, preserve formatting
4. **Preview**: User reviews converted template and placeholders
5. **Edit Placeholders**: User can manually add/edit/remove placeholders
6. **Save Template**: Save template with metadata (name, description, visibility)

## Common Issues

### Issue: File Upload Fails

**Solution**: Check file size (max 50MB) and file type (.pdf, .doc, .docx only)

### Issue: Placeholder Identification Inaccurate

**Solution**: Use manual placeholder editing in preview interface. Improve identification patterns based on feedback.

### Issue: Formatting Lost in Conversion

**Solution**: Ensure DocumentFormat.OpenXml is properly handling Run and Paragraph properties. Check document structure preservation.

### Issue: Large Documents Timeout

**Solution**: Enable async processing for documents >10 pages. Check `DocumentProcessing:AsyncProcessingThresholdPages` setting.

## Next Steps

1. Implement document processing services (see `tasks.md`)
2. Create frontend upload interface
3. Test with various document formats
4. Iterate on placeholder identification accuracy
5. Add OCR support for scanned PDFs (future enhancement)

