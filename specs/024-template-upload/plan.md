# Implementation Plan: Document Template Upload & Conversion

**Branch**: `001-template-upload` | **Date**: 2025-01-27 | **Spec**: [`specs/001-template-upload/spec.md`](./spec.md)  
**Input**: Feature specification and artifacts under `specs/001-template-upload/`

## Summary

Enable Admin users to upload PDF or Word documents that Sales Reps can apply during quotation creation. When a Sales Rep applies an uploaded document, the system automatically converts it to a template, identifies placeholders, and populates the quotation form with company details, bank details, and client information. Upon quotation generation, the system creates a final document matching the original uploaded document structure with all placeholders replaced by actual data. The final quotation is available for client viewing and PDF download, maintaining the exact layout, formatting, and design of the original document. Implementation builds on existing Clean Architecture backend (.NET 8, EF Core, MediatR) and Next.js frontend, integrating with quotation management (Spec-009, Spec-010), template management (Spec-011), company details (Spec-022), and client data (Spec-006).

## Technical Context

**Language/Version**: C# 12 / .NET 8 Web API (Backend), TypeScript / Next.js 16 (Frontend)  
**Primary Dependencies**: ASP.NET Core, MediatR, EF Core, AutoMapper, FluentValidation, Serilog (Backend); React Query, React Hook Form, Tailwind CSS, TailAdmin (Frontend); DocumentFormat.OpenXml (Word processing), PdfSharpCore (PDF parsing), QuestPDF (PDF generation for final quotations)  
**Storage**: PostgreSQL 14+ with UUID PKs, file storage for uploaded documents and generated quotations (local filesystem or cloud storage)  
**Testing**: xUnit + FluentAssertions (backend unit), WebApplicationFactory (integration), React Testing Library + Jest (frontend)  
**Target Platform**: Containerized Linux backend API (Kestrel) + Next.js frontend (Node.js)  
**Project Type**: Multi-project Clean Architecture backend + Next.js frontend mono-repo  
**Performance Goals**: Document template application and form population ≤10s, placeholder identification accuracy ≥85%, final quotation generation ≤5s, template conversion preserves 100% of formatting, final documents match original structure 100%  
**Constraints**: Maximum file size 50MB, support PDF and Word (.doc, .docx) formats only, preserve all document formatting/styles/layout, automatic conversion on template apply, real-time form population, client-accessible PDF downloads  
**Scale/Scope**: Hundreds of template uploads per month, thousands of quotation generations using templates, documents typically 1-50 pages, support for complex layouts (tables, images, QR codes, multi-column, headers/footers)

## Constitution Check

*GATE STATUS: PASS (all mandatory principles satisfied pre-design)*

| Principle | Evidence |
|-----------|----------|
| Clean Architecture Layers | Feature follows existing pattern: Domain entities, Application commands/queries, Infrastructure file processing, API controllers |
| CQRS Pattern | Upload/Convert as Commands, List/Get templates as Queries, using MediatR |
| Authorization | Admin and SalesRep roles enforced via existing RBAC (Spec-004) |
| Validation | FluentValidation for file uploads, file type/size validation |
| Error Handling | Standard exception handling with user-friendly error messages |
| File Storage | Integration with existing file storage service (FileStorageServiceAdapter) |

## Project Structure

### Documentation (this feature)

```text
specs/001-template-upload/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Backend/
├── CRM.Domain/
│   └── Entities/
│       └── DocumentTemplate.cs          # New entity for file-based templates
├── CRM.Application/
│   ├── DocumentTemplates/              # New feature module
│   │   ├── Commands/
│   │   │   ├── UploadDocumentCommand.cs
│   │   │   └── ApplyDocumentTemplateCommand.cs  # Apply template during quotation creation
│   │   ├── Queries/
│   │   │   ├── GetTemplateQuery.cs
│   │   │   └── ListTemplatesQuery.cs
│   │   ├── Dtos/
│   │   │   ├── DocumentTemplateDto.cs
│   │   │   ├── UploadDocumentRequest.cs
│   │   │   ├── TemplatePlaceholderDto.cs
│   │   │   ├── ApplyTemplateResponse.cs
│   │   │   └── PlaceholderMappingDto.cs
│   │   ├── Services/
│   │   │   ├── DocumentProcessingService.cs    # PDF/Word parsing
│   │   │   ├── PlaceholderIdentificationService.cs  # Company detail detection
│   │   │   ├── TemplateConversionService.cs     # Word template generation
│   │   │   └── PlaceholderMappingService.cs     # Map placeholders to form fields
│   │   └── Validators/
│   │       └── UploadDocumentRequestValidator.cs
│   └── Quotations/                     # Extend existing module
│       ├── Commands/
│       │   └── GenerateQuotationWithTemplateCommand.cs  # Generate final quotation
│       └── Services/
│           └── QuotationDocumentGenerationService.cs  # Generate final document matching template
├── CRM.Infrastructure/
│   └── Services/
│       └── DocumentProcessing/         # External library integrations
│           ├── PdfParserService.cs
│           └── WordDocumentService.cs
└── CRM.Api/
    └── Controllers/
        └── DocumentTemplatesController.cs

src/Frontend/web/
└── src/
    ├── app/
    │   ├── (protected)/
    │   │   ├── templates/
    │   │   │   └── upload/
    │   │   │       └── page.tsx          # Template upload page (Admin)
    │   │   └── quotations/
    │   │       └── create/
    │   │           └── page.tsx          # Quotation create page (extend existing)
    │   └── (public)/
    │       └── quotations/
    │           └── [quotationId]/
    │               └── page.tsx          # Client quotation view page
    └── components/
        ├── templates/
        │   ├── DocumentUploader.tsx
        │   ├── TemplateSelector.tsx      # Template selection in quotation form
        │   └── TemplatePreview.tsx
        └── quotations/
            ├── QuotationFormWithTemplate.tsx  # Extended form with template support
            ├── QuotationDocumentViewer.tsx    # Client-side document viewer
            └── QuotationPDFDownload.tsx       # PDF download component

tests/
├── CRM.Tests.Unit/
│   └── DocumentTemplates/
│       ├── DocumentProcessingServiceTests.cs
│       └── PlaceholderIdentificationServiceTests.cs
└── CRM.Tests.Integration/
    └── DocumentTemplates/
        └── DocumentTemplateIntegrationTests.cs
```

**Structure Decision**: Follows existing Clean Architecture pattern with feature module in Application layer. Document processing services in Infrastructure for external library integration. Quotation module extended to support template-based generation. Frontend extends existing quotation creation flow with template selection and application. Client-facing quotation view page added for document viewing and PDF download.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| External document processing libraries | PDF parsing and Word manipulation require specialized libraries | Manual parsing would be error-prone and not support complex document structures |
| File storage service | Need to store uploaded documents and generated quotations | In-memory storage insufficient for production, need persistence and retrieval |
| Real-time document processing | Need to convert and apply templates instantly during quotation creation | Pre-processing all documents would be inefficient and not support dynamic template selection |
| Complex placeholder mapping | Need to map document placeholders to form fields and data sources | Hard-coded mappings would not support flexible document structures |
| Document structure preservation | Must maintain exact layout, formatting, and design of original documents | Simplified templates would not meet brand consistency requirements |
