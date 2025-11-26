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
    public class UpdateJurisdictionCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateJurisdictionCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<JurisdictionDto> Handle(UpdateJurisdictionCommand cmd)
        {
            var entity = await _db.Jurisdictions
                .Include(j => j.Country)
                .Include(j => j.ParentJurisdiction)
                .FirstOrDefaultAsync(j => j.JurisdictionId == cmd.JurisdictionId && j.DeletedAt == null);

            if (entity == null)
            {
                throw new InvalidOperationException($"Jurisdiction with ID '{cmd.JurisdictionId}' not found");
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
                if (parent.CountryId != entity.CountryId)
                {
                    throw new InvalidOperationException("Parent jurisdiction must belong to the same country");
                }
                // Prevent circular reference
                if (parent.JurisdictionId == cmd.JurisdictionId)
                {
                    throw new InvalidOperationException("Jurisdiction cannot be its own parent");
                }
            }

            // Validate jurisdiction code uniqueness within parent
            if (!string.IsNullOrWhiteSpace(cmd.JurisdictionCode))
            {
                var codeUpper = cmd.JurisdictionCode.Trim().ToUpperInvariant();
                var exists = await _db.Jurisdictions.AnyAsync(j =>
                    j.JurisdictionId != cmd.JurisdictionId &&
                    j.CountryId == entity.CountryId &&
                    j.ParentJurisdictionId == cmd.ParentJurisdictionId &&
                    j.JurisdictionCode != null && j.JurisdictionCode.ToUpper() == codeUpper &&
                    j.DeletedAt == null);
                if (exists)
                {
                    throw new InvalidOperationException($"Jurisdiction with code '{cmd.JurisdictionCode}' already exists in this parent");
                }
            }

            entity.ParentJurisdictionId = cmd.ParentJurisdictionId;
            entity.JurisdictionName = cmd.JurisdictionName.Trim();
            entity.JurisdictionCode = string.IsNullOrWhiteSpace(cmd.JurisdictionCode) ? null : cmd.JurisdictionCode.Trim().ToUpperInvariant();
            entity.JurisdictionType = string.IsNullOrWhiteSpace(cmd.JurisdictionType) ? null : cmd.JurisdictionType.Trim();
            entity.IsActive = cmd.IsActive;
            entity.UpdatedAt = System.DateTimeOffset.UtcNow;

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
                    Action = "JurisdictionUpdated",
                    JurisdictionId = entity.JurisdictionId,
                    JurisdictionName = entity.JurisdictionName,
                    JurisdictionCode = entity.JurisdictionCode,
                    JurisdictionType = entity.JurisdictionType,
                    ParentJurisdictionId = entity.ParentJurisdictionId,
                    IsActive = entity.IsActive
                }),
                ChangedByUserId = cmd.UpdatedByUserId,
                ChangedAt = DateTimeOffset.UtcNow
            };
            _db.TaxCalculationLogs.Add(taxLog);
            await _db.SaveChangesAsync();

            // Reload to get updated related data
            entity = await _db.Jurisdictions
                .Include(j => j.Country)
                .Include(j => j.ParentJurisdiction)
                .FirstOrDefaultAsync(j => j.JurisdictionId == cmd.JurisdictionId);

            var dto = _mapper.Map<JurisdictionDto>(entity);
            if (entity?.Country != null)
            {
                dto.CountryName = entity.Country.CountryName;
            }
            if (entity?.ParentJurisdiction != null)
            {
                dto.ParentJurisdictionName = entity.ParentJurisdiction.JurisdictionName;
            }

            return dto;
        }
    }
}

