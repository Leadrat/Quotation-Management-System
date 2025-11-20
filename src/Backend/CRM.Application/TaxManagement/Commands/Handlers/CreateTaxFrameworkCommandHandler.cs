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
    public class CreateTaxFrameworkCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateTaxFrameworkCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<TaxFrameworkDto> Handle(CreateTaxFrameworkCommand cmd)
        {
            // Validate country exists
            var country = await _db.Countries
                .FirstOrDefaultAsync(c => c.CountryId == cmd.CountryId && c.DeletedAt == null);
            if (country == null)
            {
                throw new InvalidOperationException($"Country with ID '{cmd.CountryId}' not found");
            }

            // Validate no existing active framework for this country
            var existingFramework = await _db.TaxFrameworks
                .FirstOrDefaultAsync(f => f.CountryId == cmd.CountryId && f.DeletedAt == null);
            if (existingFramework != null)
            {
                throw new InvalidOperationException($"Country already has an active tax framework");
            }

            // Validate tax components
            if (cmd.TaxComponents == null || cmd.TaxComponents.Count == 0)
            {
                throw new InvalidOperationException("Tax framework must have at least one tax component");
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new TaxFramework
            {
                TaxFrameworkId = Guid.NewGuid(),
                CountryId = cmd.CountryId,
                FrameworkName = cmd.FrameworkName.Trim(),
                FrameworkType = cmd.FrameworkType,
                Description = cmd.Description?.Trim(),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            entity.SetTaxComponents(cmd.TaxComponents);

            _db.TaxFrameworks.Add(entity);
            await _db.SaveChangesAsync();

            // Log configuration change
            var taxLog = new TaxCalculationLog
            {
                LogId = Guid.NewGuid(),
                QuotationId = null,
                ActionType = TaxCalculationActionType.ConfigurationChange,
                CountryId = entity.CountryId,
                JurisdictionId = null,
                CalculationDetails = JsonSerializer.Serialize(new
                {
                    Action = "TaxFrameworkCreated",
                    TaxFrameworkId = entity.TaxFrameworkId,
                    FrameworkName = entity.FrameworkName,
                    FrameworkType = entity.FrameworkType,
                    TaxComponents = entity.GetTaxComponents()
                }),
                ChangedByUserId = cmd.CreatedByUserId,
                ChangedAt = DateTimeOffset.UtcNow
            };
            _db.TaxCalculationLogs.Add(taxLog);
            await _db.SaveChangesAsync();

            return _mapper.Map<TaxFrameworkDto>(entity);
        }
    }
}

