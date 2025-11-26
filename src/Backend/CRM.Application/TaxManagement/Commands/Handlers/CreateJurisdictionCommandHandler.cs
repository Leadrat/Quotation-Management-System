using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Commands;
using CRM.Application.TaxManagement.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Commands.Handlers
{
    public class CreateJurisdictionCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateJurisdictionCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<JurisdictionDto> Handle(CreateJurisdictionCommand cmd)
        {
            // Validate country exists
            var country = await _db.Countries
                .FirstOrDefaultAsync(c => c.CountryId == cmd.CountryId && c.DeletedAt == null);
            if (country == null)
            {
                throw new InvalidOperationException($"Country with ID '{cmd.CountryId}' not found");
            }

            // Validate parent jurisdiction if provided
            if (cmd.ParentJurisdictionId.HasValue)
            {
                var parent = await _db.Jurisdictions
                    .FirstOrDefaultAsync(j => j.JurisdictionId == cmd.ParentJurisdictionId.Value && j.DeletedAt == null);
                if (parent == null)
                {
                    throw new InvalidOperationException($"Parent jurisdiction with ID '{cmd.ParentJurisdictionId.Value}' not found");
                }
                if (parent.CountryId != cmd.CountryId)
                {
                    throw new InvalidOperationException("Parent jurisdiction must belong to the same country");
                }
            }

            // Validate jurisdiction code uniqueness within parent
            if (!string.IsNullOrWhiteSpace(cmd.JurisdictionCode))
            {
                var codeUpper = cmd.JurisdictionCode.Trim().ToUpperInvariant();
                var exists = await _db.Jurisdictions.AnyAsync(j =>
                    j.CountryId == cmd.CountryId &&
                    j.ParentJurisdictionId == cmd.ParentJurisdictionId &&
                    j.JurisdictionCode != null && j.JurisdictionCode.ToUpper() == codeUpper &&
                    j.DeletedAt == null);
                if (exists)
                {
                    throw new InvalidOperationException($"Jurisdiction with code '{cmd.JurisdictionCode}' already exists in this parent");
                }
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new Jurisdiction
            {
                JurisdictionId = Guid.NewGuid(),
                CountryId = cmd.CountryId,
                ParentJurisdictionId = cmd.ParentJurisdictionId,
                JurisdictionName = cmd.JurisdictionName.Trim(),
                JurisdictionCode = string.IsNullOrWhiteSpace(cmd.JurisdictionCode) ? null : cmd.JurisdictionCode.Trim().ToUpperInvariant(),
                JurisdictionType = string.IsNullOrWhiteSpace(cmd.JurisdictionType) ? null : cmd.JurisdictionType.Trim(),
                IsActive = cmd.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Jurisdictions.Add(entity);
            await _db.SaveChangesAsync();

            // Log configuration change
            var taxLog = new TaxCalculationLog
            {
                LogId = Guid.NewGuid(),
                QuotationId = null,
                ActionType = TaxCalculationActionType.ConfigurationChange,
                CountryId = entity.CountryId,
                JurisdictionId = entity.JurisdictionId,
                CalculationDetails = JsonSerializer.Serialize(new
                {
                    Action = "JurisdictionCreated",
                    JurisdictionId = entity.JurisdictionId,
                    JurisdictionName = entity.JurisdictionName,
                    JurisdictionCode = entity.JurisdictionCode,
                    JurisdictionType = entity.JurisdictionType,
                    ParentJurisdictionId = entity.ParentJurisdictionId,
                    IsActive = entity.IsActive
                }),
                ChangedByUserId = cmd.CreatedByUserId,
                ChangedAt = DateTimeOffset.UtcNow
            };
            _db.TaxCalculationLogs.Add(taxLog);
            await _db.SaveChangesAsync();

            // Load with related data for mapping
            var jurisdiction = await _db.Jurisdictions
                .Include(j => j.Country)
                .Include(j => j.ParentJurisdiction)
                .FirstOrDefaultAsync(j => j.JurisdictionId == entity.JurisdictionId);

            var dto = _mapper.Map<JurisdictionDto>(jurisdiction);
            if (jurisdiction?.Country != null)
            {
                dto.CountryName = jurisdiction.Country.CountryName;
            }
            if (jurisdiction?.ParentJurisdiction != null)
            {
                dto.ParentJurisdictionName = jurisdiction.ParentJurisdiction.JurisdictionName;
            }

            return dto;
        }
    }
}

