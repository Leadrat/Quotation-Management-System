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
    public class CreateCountryCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateCountryCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CountryDto> Handle(CreateCountryCommand cmd)
        {
            // Validate country code uniqueness
            var codeUpper = cmd.CountryCode.Trim().ToUpperInvariant();
            var existsByCode = await _db.Countries.AnyAsync(c => 
                c.CountryCode.ToUpper() == codeUpper && c.DeletedAt == null);
            if (existsByCode)
            {
                throw new InvalidOperationException($"Country with code '{cmd.CountryCode}' already exists");
            }

            // Validate country name uniqueness
            var nameLower = cmd.CountryName.Trim().ToLowerInvariant();
            var existsByName = await _db.Countries.AnyAsync(c => 
                c.CountryName.ToLower() == nameLower && c.DeletedAt == null);
            if (existsByName)
            {
                throw new InvalidOperationException($"Country with name '{cmd.CountryName}' already exists");
            }

            // If setting as default, unset other defaults
            if (cmd.IsDefault)
            {
                var existingDefaults = await _db.Countries
                    .Where(c => c.IsDefault && c.DeletedAt == null)
                    .ToListAsync();
                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new Country
            {
                CountryId = Guid.NewGuid(),
                CountryName = cmd.CountryName.Trim(),
                CountryCode = codeUpper,
                TaxFrameworkType = cmd.TaxFrameworkType,
                DefaultCurrency = cmd.DefaultCurrency.Trim().ToUpperInvariant(),
                IsActive = cmd.IsActive,
                IsDefault = cmd.IsDefault,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Countries.Add(entity);
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
                    Action = "CountryCreated",
                    CountryId = entity.CountryId,
                    CountryName = entity.CountryName,
                    CountryCode = entity.CountryCode,
                    TaxFrameworkType = entity.TaxFrameworkType,
                    IsDefault = entity.IsDefault,
                    IsActive = entity.IsActive
                }),
                ChangedByUserId = cmd.CreatedByUserId,
                ChangedAt = DateTimeOffset.UtcNow
            };
            _db.TaxCalculationLogs.Add(taxLog);
            await _db.SaveChangesAsync();

            return _mapper.Map<CountryDto>(entity);
        }
    }
}

