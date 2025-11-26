# Research & Technical Decisions: Document Template Upload & Conversion

**Spec**: Spec-001  
**Date**: 2025-01-27

## Document Processing Library Selection

### Decision: Use DocumentFormat.OpenXml for Word Documents

**Rationale:**
- **DocumentFormat.OpenXml** is the official Microsoft library for working with Office Open XML formats (.docx)
- Pure .NET library, no external dependencies
- Full control over document structure, formatting, and content
- Supports reading and writing Word documents with complete formatting preservation
- Active maintenance and Microsoft support
- Free and open-source (MIT license)
- Can manipulate styles, fonts, colors, tables, images, and all document elements

**Alternatives Considered:**
- **Aspose.Words**: Commercial license required, expensive for production use
- **NPOI**: Primarily for older .doc format, limited .docx support
- **DocX**: Simpler API but less control over complex formatting

**Implementation:**
```csharp
// Example structure
public class WordDocumentService
{
    public WordprocessingDocument OpenDocument(string filePath)
    {
        return WordprocessingDocument.Open(filePath, true);
    }
    
    public void ReplaceTextWithPlaceholder(WordprocessingDocument doc, string originalText, string placeholder)
    {
        // Find and replace text while preserving formatting
    }
}
```

### Decision: Use iText7 or PdfSharpCore for PDF Processing

**Rationale:**
- **PdfSharpCore** is a .NET port of PdfSharp, open-source and free
- Good for text extraction and basic PDF manipulation
- **iText7** (Community Edition) is more powerful but has AGPL license (requires open-source project)
- For text extraction and basic operations, PdfSharpCore is sufficient
- For complex PDF manipulation, consider iText7 Community Edition or commercial license

**Alternatives Considered:**
- **PdfPig**: Good for text extraction but limited manipulation capabilities
- **QuestPDF**: Good for PDF generation but not for parsing/manipulation
- **Tesseract OCR**: Needed only if PDFs are scanned images

**Implementation:**
```csharp
// Example structure for PDF text extraction
public class PdfParserService
{
    public string ExtractText(string pdfPath)
    {
        // Extract text from PDF for analysis
    }
    
    public List<TextElement> ExtractTextWithFormatting(string pdfPath)
    {
        // Extract text with position and formatting information
    }
}
```

**Decision**: Use **PdfSharpCore** for PDF text extraction and basic operations. For scanned PDFs requiring OCR, integrate **Tesseract.NET** wrapper.

## Placeholder Identification Strategy

### Decision: Pattern-Based Identification with Machine Learning Enhancement (Future)

**Rationale:**
- **Phase 1**: Use regex patterns and keyword matching for common company detail formats
- **Pattern Recognition**: Identify common patterns like "Company Name:", "Address:", "GSTIN:", etc.
- **Context Analysis**: Distinguish between sender (user's company) and recipient (customer) based on document structure (headers, "Bill To" sections, etc.)
- **Future Enhancement**: Consider ML-based NER (Named Entity Recognition) for better accuracy

**Identification Patterns:**
1. **Company Name**: Patterns like "Company:", "Name:", "Business Name:", followed by capitalized text
2. **Address**: Multi-line patterns with street, city, state, postal code
3. **Postal/Pin Code**: Numeric patterns (6 digits for India, various formats for other countries)
4. **Bank Details**: Keywords like "Account Number", "IFSC", "IBAN", "SWIFT", "Bank Name"
5. **Tax Identifiers**: 
   - PAN: 10 alphanumeric characters (A-Z, 0-9)
   - TAN: 10 alphanumeric characters
   - GSTIN: 15 alphanumeric characters with specific format
6. **Country**: Common country names or country codes

**Implementation:**
```csharp
public class PlaceholderIdentificationService
{
    public List<IdentifiedPlaceholder> IdentifyCompanyDetails(string documentText)
    {
        var placeholders = new List<IdentifiedPlaceholder>();
        
        // Identify company name
        placeholders.AddRange(IdentifyCompanyName(documentText));
        
        // Identify address components
        placeholders.AddRange(IdentifyAddress(documentText));
        
        // Identify bank details
        placeholders.AddRange(IdentifyBankDetails(documentText));
        
        // Identify tax identifiers
        placeholders.AddRange(IdentifyTaxIdentifiers(documentText));
        
        return placeholders;
    }
    
    private bool IsCompanySection(string text, int position)
    {
        // Determine if text belongs to company (sender) or customer section
        // Based on context: headers, "From:", "Bill To:", document structure
    }
}
```

## Word Template Placeholder Format

### Decision: Use Mustache-Style Placeholders `{{PlaceholderName}}`

**Rationale:**
- **Standard Format**: `{{PlaceholderName}}` is widely recognized and easy to identify
- **Compatible**: Works with common templating engines (Mustache, Handlebars)
- **Editable**: Users can easily edit placeholder names in Word
- **Distinct**: Unlikely to conflict with actual document content
- **Case-Sensitive**: Use PascalCase for placeholder names (e.g., `{{CompanyName}}`, `{{CustomerAddress}}`)

**Placeholder Naming Convention:**
- Company (user's company): `{{CompanyName}}`, `{{CompanyAddress}}`, `{{CompanyCity}}`, etc.
- Customer: `{{CustomerCompanyName}}`, `{{CustomerAddress}}`, `{{CustomerCity}}`, etc.
- Bank Details: `{{BankAccountNumber}}`, `{{BankIFSC}}`, `{{BankIBAN}}`, `{{BankSWIFT}}`, etc.
- Tax Identifiers: `{{CompanyPAN}}`, `{{CompanyTAN}}`, `{{CompanyGST}}`, `{{CustomerGSTIN}}`

**Implementation:**
```csharp
public class TemplateConversionService
{
    public void ReplaceWithPlaceholder(WordprocessingDocument doc, IdentifiedPlaceholder placeholder)
    {
        // Find text in document
        // Replace with {{PlaceholderName}} format
        // Preserve all formatting (font, size, color, style)
    }
}
```

## Format Preservation Strategy

### Decision: Preserve All Formatting at Character Level

**Rationale:**
- Word documents store formatting at multiple levels: document, paragraph, run (character)
- Must preserve Run properties (font, size, color, bold, italic, underline)
- Must preserve Paragraph properties (alignment, spacing, indentation)
- Must preserve Section properties (margins, headers, footers)
- Must preserve Table structures and formatting
- Must preserve Images and other embedded content

**Implementation Approach:**
1. When replacing text with placeholder, identify the Run element containing the text
2. Replace text content within the Run while keeping all Run properties
3. If text spans multiple Runs, merge Runs or apply placeholder to each Run maintaining individual formatting
4. Preserve paragraph and section properties unchanged

**Challenges:**
- Text may span multiple Run elements (each with different formatting)
- Need to handle complex scenarios where formatting changes mid-word
- Tables and images require special handling

## Integration with Existing Template Management

### Decision: Extend QuotationTemplates Table with File-Based Template Support

**Rationale:**
- Existing `QuotationTemplates` table (Spec-011) stores structured templates (line items, etc.)
- Need to support file-based templates alongside structured templates
- Add `IsFileBased` boolean flag to distinguish template types
- Add `TemplateFilePath` to store reference to Word template file
- Add `TemplateType` enum: "Quotation" or "ProformaInvoice"
- Reuse existing visibility, approval, versioning mechanisms

**Database Schema Extension:**
```sql
ALTER TABLE QuotationTemplates
ADD COLUMN IsFileBased BOOLEAN NOT NULL DEFAULT false,
ADD COLUMN TemplateFilePath VARCHAR(500) NULL,
ADD COLUMN TemplateType VARCHAR(50) NULL; -- 'Quotation' or 'ProformaInvoice'

-- Index for file-based templates
CREATE INDEX IX_QuotationTemplates_IsFileBased_TemplateType
ON QuotationTemplates (IsFileBased, TemplateType)
WHERE DeletedAt IS NULL;
```

## File Storage Strategy

### Decision: Use Existing FileStorageServiceAdapter

**Rationale:**
- Project already has `FileStorageServiceAdapter` in `CRM.Api/Adapters/`
- Supports local filesystem and can be extended for cloud storage (S3, Azure Blob)
- Consistent with existing file storage patterns
- Store uploaded documents in `wwwroot/uploads/templates/`
- Store converted templates in `wwwroot/templates/`
- Use relative paths in database, full paths resolved by storage service

**File Organization:**
```
wwwroot/
├── uploads/
│   └── templates/
│       └── {userId}/
│           └── {templateId}/
│               └── original.{pdf|docx}
└── templates/
    └── {templateId}/
        └── template.docx
```

## Performance Considerations

### Decision: Async Processing for Large Documents

**Rationale:**
- Document processing can be CPU-intensive for large files
- Should not block API request thread
- Use background job processing for documents >10 pages
- Return job ID immediately, poll for completion status
- For smaller documents (<10 pages), process synchronously with timeout

**Implementation:**
- Use Hangfire or similar background job framework (if available)
- Or use Task.Run for async processing with progress tracking
- Store processing status in database (Processing, Completed, Failed)

## Error Handling

### Decision: Graceful Degradation with Manual Correction

**Rationale:**
- Automatic identification may not be 100% accurate
- Always provide preview interface for user review
- Allow manual correction of placeholders
- Log identification failures for pattern improvement
- Provide clear error messages for unsupported formats or corrupted files

## Security Considerations

### Decision: Validate and Sanitize Uploaded Files

**Rationale:**
- File uploads are security-sensitive
- Validate file type (MIME type, file extension, magic bytes)
- Limit file size (50MB maximum)
- Scan for malicious content if possible
- Store files with restricted permissions
- Use secure file names (UUID-based) to prevent path traversal

**Implementation:**
- Validate file extension and MIME type
- Check file size before processing
- Use UUID for file names
- Store files outside web root or with restricted access
- Implement virus scanning if available

## Testing Strategy

### Decision: Comprehensive Test Coverage

**Rationale:**
- Document processing is complex and error-prone
- Need unit tests for identification logic
- Need integration tests for end-to-end conversion
- Need test documents with various formats and edge cases

**Test Documents Needed:**
- Simple Word document with company details
- Complex Word document with tables, images, multiple sections
- PDF with text (not scanned)
- PDF with scanned content (requires OCR)
- Documents with mixed formatting
- Documents with multiple company sections
- Documents already containing placeholders

