# Quick Start Guide: Company Details Admin Configuration (Spec-022)

**Spec**: [spec.md](./spec.md)  
**Plan**: [plan.md](./plan.md)  
**Data Model**: [data-model.md](./data-model.md)  
**Research**: [research.md](./research.md)

## Overview

This guide provides step-by-step instructions for implementing the Company Details Admin Configuration feature. The implementation follows Clean Architecture with CQRS pattern, consistent with existing codebase patterns.

## Implementation Order

### Phase 1: Backend Foundation

#### Step 1: Create Domain Entities

**File**: `src/Backend/CRM.Domain/Entities/CompanyDetails.cs`

```csharp
using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities
{
    public class CompanyDetails
    {
        public Guid CompanyDetailsId { get; set; } = new Guid("00000000-0000-0000-0000-000000000001");
        public string? PanNumber { get; set; }
        public string? TanNumber { get; set; }
        public string? GstNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Website { get; set; }
        public string? LegalDisclaimer { get; set; }
        public string? LogoUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }

        // Navigation properties
        public User? UpdatedByUser { get; set; }
        public ICollection<BankDetails> BankDetails { get; set; } = new List<BankDetails>();
    }
}
```

**File**: `src/Backend/CRM.Domain/Entities/BankDetails.cs`

```csharp
using System;

namespace CRM.Domain.Entities
{
    public class BankDetails
    {
        public Guid BankDetailsId { get; set; }
        public Guid CompanyDetailsId { get; set; }
        public string Country { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? IfscCode { get; set; }
        public string? Iban { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }

        // Navigation properties
        public CompanyDetails? CompanyDetails { get; set; }
        public User? UpdatedByUser { get; set; }
    }
}
```

#### Step 2: Create Entity Configurations

**File**: `src/Backend/CRM.Infrastructure/EntityConfigurations/CompanyDetailsEntityConfiguration.cs`

```csharp
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class CompanyDetailsEntityConfiguration : IEntityTypeConfiguration<CompanyDetails>
    {
        public void Configure(EntityTypeBuilder<CompanyDetails> builder)
        {
            builder.ToTable("CompanyDetails");
            builder.HasKey(c => c.CompanyDetailsId);
            
            builder.Property(c => c.CompanyDetailsId)
                .HasDefaultValue(new Guid("00000000-0000-0000-0000-000000000001"));
            
            builder.Property(c => c.PanNumber).HasMaxLength(10);
            builder.Property(c => c.TanNumber).HasMaxLength(10);
            builder.Property(c => c.GstNumber).HasMaxLength(15);
            builder.Property(c => c.CompanyName).HasMaxLength(255);
            builder.Property(c => c.ContactEmail).HasMaxLength(255);
            builder.Property(c => c.LogoUrl).HasMaxLength(500);
            
            builder.HasOne(c => c.UpdatedByUser)
                .WithMany()
                .HasForeignKey(c => c.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(c => c.BankDetails)
                .WithOne(b => b.CompanyDetails)
                .HasForeignKey(b => b.CompanyDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasIndex(c => c.UpdatedAt);
        }
    }
}
```

**File**: `src/Backend/CRM.Infrastructure/EntityConfigurations/BankDetailsEntityConfiguration.cs`

```csharp
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class BankDetailsEntityConfiguration : IEntityTypeConfiguration<BankDetails>
    {
        public void Configure(EntityTypeBuilder<BankDetails> builder)
        {
            builder.ToTable("BankDetails");
            builder.HasKey(b => b.BankDetailsId);
            
            builder.Property(b => b.Country).HasMaxLength(50).IsRequired();
            builder.Property(b => b.AccountNumber).HasMaxLength(50).IsRequired();
            builder.Property(b => b.IfscCode).HasMaxLength(11);
            builder.Property(b => b.Iban).HasMaxLength(34);
            builder.Property(b => b.SwiftCode).HasMaxLength(11);
            builder.Property(b => b.BankName).HasMaxLength(255).IsRequired();
            
            builder.HasOne(b => b.CompanyDetails)
                .WithMany(c => c.BankDetails)
                .HasForeignKey(b => b.CompanyDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(b => b.UpdatedByUser)
                .WithMany()
                .HasForeignKey(b => b.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(b => b.CompanyDetailsId);
            builder.HasAlternateKey(b => new { b.CompanyDetailsId, b.Country });
        }
    }
}
```

#### Step 3: Create Migration

Run EF Core migration command:
```bash
dotnet ef migrations add CreateCompanyDetailsTables --project src/Backend/CRM.Infrastructure --startup-project src/Backend/CRM.Api
```

#### Step 4: Create DTOs

**File**: `src/Backend/CRM.Application/CompanyDetails/Dtos/CompanyDetailsDto.cs`

```csharp
using System;
using System.Collections.Generic;

namespace CRM.Application.CompanyDetails.Dtos
{
    public class CompanyDetailsDto
    {
        public Guid CompanyDetailsId { get; set; }
        public string? PanNumber { get; set; }
        public string? TanNumber { get; set; }
        public string? GstNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Website { get; set; }
        public string? LegalDisclaimer { get; set; }
        public string? LogoUrl { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<BankDetailsDto> BankDetails { get; set; } = new();
    }
}
```

**File**: `src/Backend/CRM.Application/CompanyDetails/Dtos/BankDetailsDto.cs`

```csharp
using System;

namespace CRM.Application.CompanyDetails.Dtos
{
    public class BankDetailsDto
    {
        public Guid BankDetailsId { get; set; }
        public string Country { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? IfscCode { get; set; }
        public string? Iban { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
    }
}
```

**File**: `src/Backend/CRM.Application/CompanyDetails/Dtos/UpdateCompanyDetailsRequest.cs`

```csharp
using System.Collections.Generic;

namespace CRM.Application.CompanyDetails.Dtos
{
    public class UpdateCompanyDetailsRequest
    {
        public string? PanNumber { get; set; }
        public string? TanNumber { get; set; }
        public string? GstNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Website { get; set; }
        public string? LegalDisclaimer { get; set; }
        public string? LogoUrl { get; set; }
        public List<BankDetailsDto> BankDetails { get; set; } = new();
    }
}
```

#### Step 5: Create Validators

**File**: `src/Backend/CRM.Application/CompanyDetails/Validators/TaxNumberValidators.cs`

```csharp
using FluentValidation;

namespace CRM.Application.CompanyDetails.Validators
{
    public static class TaxNumberValidators
    {
        public static IRuleBuilderOptions<T, string?> PanNumber<T>(IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$")
                .WithMessage("PAN number must be in format: ABCDE1234F (5 letters, 4 digits, 1 letter)")
                .When(x => !string.IsNullOrWhiteSpace(x));
        }

        public static IRuleBuilderOptions<T, string?> TanNumber<T>(IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Matches(@"^[A-Z]{4}[0-9]{5}[A-Z]{1}$")
                .WithMessage("TAN number must be in format: ABCD12345E (4 letters, 5 digits, 1 letter)")
                .When(x => !string.IsNullOrWhiteSpace(x));
        }

        public static IRuleBuilderOptions<T, string?> GstNumber<T>(IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$")
                .WithMessage("GST number must be in format: 27ABCDE1234F1Z5 (15 characters)")
                .When(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
```

**File**: `src/Backend/CRM.Application/CompanyDetails/Validators/UpdateCompanyDetailsRequestValidator.cs`

```csharp
using CRM.Application.CompanyDetails.Dtos;
using FluentValidation;

namespace CRM.Application.CompanyDetails.Validators
{
    public class UpdateCompanyDetailsRequestValidator : AbstractValidator<UpdateCompanyDetailsRequest>
    {
        public UpdateCompanyDetailsRequestValidator()
        {
            RuleFor(x => x.PanNumber)
                .TaxNumberValidators.PanNumber();

            RuleFor(x => x.TanNumber)
                .TaxNumberValidators.TANNumber();

            RuleFor(x => x.GstNumber)
                .TaxNumberValidators.GstNumber();

            RuleFor(x => x.ContactEmail)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

            RuleForEach(x => x.BankDetails)
                .SetValidator(new BankDetailsDtoValidator());
        }
    }

    public class BankDetailsDtoValidator : AbstractValidator<BankDetailsDto>
    {
        public BankDetailsDtoValidator()
        {
            RuleFor(x => x.Country)
                .NotEmpty()
                .Must(c => c == "India" || c == "Dubai")
                .WithMessage("Country must be 'India' or 'Dubai'");

            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.BankName)
                .NotEmpty()
                .MaximumLength(255);

            // India-specific validation
            When(x => x.Country == "India", () =>
            {
                RuleFor(x => x.IfscCode)
                    .NotEmpty()
                    .Matches(@"^[A-Z]{4}0[A-Z0-9]{6}$")
                    .WithMessage("IFSC code must be 11 characters: 4 letters, 1 zero, 6 alphanumeric");
            });

            // Dubai-specific validation
            When(x => x.Country == "Dubai", () =>
            {
                RuleFor(x => x.Iban)
                    .NotEmpty()
                    .MinimumLength(15)
                    .MaximumLength(34);
                
                RuleFor(x => x.SwiftCode)
                    .NotEmpty()
                    .Matches(@"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$")
                    .WithMessage("SWIFT code must be 8-11 characters");
            });
        }
    }
}
```

#### Step 6: Create Commands and Queries

**File**: `src/Backend/CRM.Application/CompanyDetails/Commands/UpdateCompanyDetailsCommand.cs`

```csharp
using System;
using CRM.Application.CompanyDetails.Dtos;

namespace CRM.Application.CompanyDetails.Commands
{
    public class UpdateCompanyDetailsCommand
    {
        public UpdateCompanyDetailsRequest Request { get; set; } = new();
        public Guid UpdatedBy { get; set; }
        public string? IpAddress { get; set; }
    }
}
```

**File**: `src/Backend/CRM.Application/CompanyDetails/Queries/GetCompanyDetailsQuery.cs`

```csharp
namespace CRM.Application.CompanyDetails.Queries
{
    public class GetCompanyDetailsQuery
    {
        // No parameters needed - singleton pattern
    }
}
```

#### Step 7: Create Handlers

**File**: `src/Backend/CRM.Application/CompanyDetails/Commands/Handlers/UpdateCompanyDetailsCommandHandler.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyDetails.Commands;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Infrastructure.Logging;

namespace CRM.Application.CompanyDetails.Commands.Handlers
{
    public class UpdateCompanyDetailsCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IAuditLogger _audit;

        public UpdateCompanyDetailsCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            IAuditLogger audit)
        {
            _db = db;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<CompanyDetailsDto> Handle(UpdateCompanyDetailsCommand command)
        {
            // Get or create company details (singleton)
            var companyDetails = await _db.CompanyDetails
                .FirstOrDefaultAsync();

            if (companyDetails == null)
            {
                // Create new record
                companyDetails = new Domain.Entities.CompanyDetails
                {
                    CompanyDetailsId = new Guid("00000000-0000-0000-0000-000000000001"),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _db.CompanyDetails.Add(companyDetails);
            }

            // Update properties
            _mapper.Map(command.Request, companyDetails);
            companyDetails.UpdatedAt = DateTimeOffset.UtcNow;
            companyDetails.UpdatedBy = command.UpdatedBy;

            // Update bank details
            var existingBankDetails = _db.BankDetails
                .Where(b => b.CompanyDetailsId == companyDetails.CompanyDetailsId)
                .ToList();

            foreach (var bankDetailDto in command.Request.BankDetails)
            {
                var existing = existingBankDetails
                    .FirstOrDefault(b => b.Country == bankDetailDto.Country);

                if (existing != null)
                {
                    // Update existing
                    _mapper.Map(bankDetailDto, existing);
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                    existing.UpdatedBy = command.UpdatedBy;
                }
                else
                {
                    // Create new
                    var newBankDetail = _mapper.Map<Domain.Entities.BankDetails>(bankDetailDto);
                    newBankDetail.CompanyDetailsId = companyDetails.CompanyDetailsId;
                    newBankDetail.CreatedAt = DateTimeOffset.UtcNow;
                    newBankDetail.UpdatedAt = DateTimeOffset.UtcNow;
                    newBankDetail.UpdatedBy = command.UpdatedBy;
                    _db.BankDetails.Add(newBankDetail);
                }
            }

            // Remove bank details not in request
            var countriesToKeep = command.Request.BankDetails.Select(b => b.Country).ToList();
            var toRemove = existingBankDetails
                .Where(b => !countriesToKeep.Contains(b.Country))
                .ToList();
            _db.BankDetails.RemoveRange(toRemove);

            await _db.SaveChangesAsync();

            // Audit log
            await _audit.LogAsync("company_details_update_success", new
            {
                CompanyDetailsId = companyDetails.CompanyDetailsId,
                UpdatedBy = command.UpdatedBy
            });

            // Return updated DTO
            return await GetCompanyDetailsDto(companyDetails.CompanyDetailsId);
        }

        private async Task<CompanyDetailsDto> GetCompanyDetailsDto(Guid companyDetailsId)
        {
            var companyDetails = await _db.CompanyDetails
                .Include(c => c.BankDetails)
                .FirstOrDefaultAsync(c => c.CompanyDetailsId == companyDetailsId);

            return _mapper.Map<CompanyDetailsDto>(companyDetails);
        }
    }
}
```

**File**: `src/Backend/CRM.Application/CompanyDetails/Queries/Handlers/GetCompanyDetailsQueryHandler.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyDetails.Queries;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyDetails.Queries.Handlers
{
    public class GetCompanyDetailsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetCompanyDetailsQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CompanyDetailsDto> Handle(GetCompanyDetailsQuery query)
        {
            var companyDetails = await _db.CompanyDetails
                .Include(c => c.BankDetails)
                .FirstOrDefaultAsync();

            if (companyDetails == null)
            {
                // Return empty DTO if not configured yet
                return new CompanyDetailsDto();
            }

            return _mapper.Map<CompanyDetailsDto>(companyDetails);
        }
    }
}
```

#### Step 8: Create AutoMapper Profile

**File**: `src/Backend/CRM.Application/Mapping/CompanyDetailsProfile.cs`

```csharp
using AutoMapper;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class CompanyDetailsProfile : Profile
    {
        public CompanyDetailsProfile()
        {
            CreateMap<CompanyDetails, CompanyDetailsDto>();
            CreateMap<UpdateCompanyDetailsRequest, CompanyDetails>()
                .ForMember(dest => dest.CompanyDetailsId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.BankDetails, opt => opt.Ignore());

            CreateMap<BankDetails, BankDetailsDto>();
            CreateMap<BankDetailsDto, BankDetails>()
                .ForMember(dest => dest.BankDetailsId, opt => opt.MapFrom(src => src.BankDetailsId == Guid.Empty ? Guid.NewGuid() : src.BankDetailsId))
                .ForMember(dest => dest.CompanyDetailsId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
        }
    }
}
```

#### Step 9: Create Controller

**File**: `src/Backend/CRM.Api/Controllers/CompanyDetailsController.cs`

```csharp
using System.Security.Claims;
using CRM.Application.CompanyDetails.Commands;
using CRM.Application.CompanyDetails.Commands.Handlers;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyDetails.Queries;
using CRM.Application.CompanyDetails.Queries.Handlers;
using CRM.Application.CompanyDetails.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/company-details")]
    [Authorize(Roles = "Admin")]
    public class CompanyDetailsController : ControllerBase
    {
        private readonly GetCompanyDetailsQueryHandler _getHandler;
        private readonly UpdateCompanyDetailsCommandHandler _updateHandler;

        public CompanyDetailsController(
            GetCompanyDetailsQueryHandler getHandler,
            UpdateCompanyDetailsCommandHandler updateHandler)
        {
            _getHandler = getHandler;
            _updateHandler = updateHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var query = new GetCompanyDetailsQuery();
            var result = await _getHandler.Handle(query);
            return Ok(new { success = true, data = result });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCompanyDetailsRequest request)
        {
            // Validate request
            var validator = new UpdateCompanyDetailsRequestValidator();
            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { success = false, errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            // Get user ID from JWT
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            // Get IP address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Create command
            var command = new UpdateCompanyDetailsCommand
            {
                Request = request,
                UpdatedBy = userId,
                IpAddress = ipAddress
            };

            // Execute handler
            var result = await _updateHandler.Handle(command);

            return Ok(new { success = true, message = "Company details updated successfully", data = result });
        }
    }
}
```

### Phase 2: Integration with Quotations

#### Step 10: Modify Quotation PDF Generation

Update `QuotationPdfGenerationService` to include company details. See research.md for details.

#### Step 11: Modify Quotation Email Service

Update `QuotationEmailService` to include company details in email templates. See research.md for details.

### Phase 3: Frontend Implementation

#### Step 12: Create Admin Configuration Page

**File**: `src/Frontend/web/src/app/(protected)/admin/company-details/page.tsx`

Create the admin configuration page with form sections for:
- Tax Information (PAN, TAN, GST)
- Bank Details (India and Dubai sections)
- Company Address and Contact
- Legal Disclaimers
- Logo Upload

See existing admin pages (e.g., `users/new/page.tsx`) for styling patterns.

## Testing Checklist

- [ ] Unit tests for tax number validation
- [ ] Unit tests for bank details validation
- [ ] Integration tests for GET /api/v1/company-details
- [ ] Integration tests for PUT /api/v1/company-details
- [ ] Integration tests for quotation PDF with company details
- [ ] Integration tests for quotation email with company details
- [ ] E2E test: Admin configures company details
- [ ] E2E test: Sales rep creates quotation, verifies company details appear

## Next Steps

1. Implement file upload service for logo
2. Add caching for company details retrieval
3. Add historical snapshot to quotation creation
4. Update quotation PDF generation service
5. Update quotation email service
6. Create frontend admin page

