using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyBankDetails.Commands;
using CRM.Application.CompanyBankDetails.DTOs;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyBankDetails.Commands.Handlers
{
    public class ConfigureCountryBankFieldCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public ConfigureCountryBankFieldCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CountryBankFieldConfigurationDto> Handle(ConfigureCountryBankFieldCommand command)
        {
            // Verify country exists
            var countryExists = await _db.Countries
                .AnyAsync(c => c.CountryId == command.Request.CountryId && c.DeletedAt == null);

            if (!countryExists)
            {
                throw new InvalidOperationException($"Country with ID '{command.Request.CountryId}' not found.");
            }

            // Verify bank field type exists
            var bankFieldTypeExists = await _db.BankFieldTypes
                .AnyAsync(b => b.BankFieldTypeId == command.Request.BankFieldTypeId && b.DeletedAt == null);

            if (!bankFieldTypeExists)
            {
                throw new InvalidOperationException($"Bank field type with ID '{command.Request.BankFieldTypeId}' not found.");
            }

            // Check if configuration already exists
            var existing = await _db.CountryBankFieldConfigurations
                .FirstOrDefaultAsync(c => c.CountryId == command.Request.CountryId 
                    && c.BankFieldTypeId == command.Request.BankFieldTypeId 
                    && c.DeletedAt == null);

            if (existing != null)
            {
                throw new InvalidOperationException($"Configuration for country '{command.Request.CountryId}' and bank field type '{command.Request.BankFieldTypeId}' already exists.");
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new CountryBankFieldConfiguration
            {
                ConfigurationId = Guid.NewGuid(),
                CountryId = command.Request.CountryId,
                BankFieldTypeId = command.Request.BankFieldTypeId,
                IsRequired = command.Request.IsRequired,
                ValidationRegex = command.Request.ValidationRegex,
                MinLength = command.Request.MinLength,
                MaxLength = command.Request.MaxLength,
                DisplayName = command.Request.DisplayName,
                HelpText = command.Request.HelpText,
                DisplayOrder = command.Request.DisplayOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.CountryBankFieldConfigurations.Add(entity);
            await _db.SaveChangesAsync();

            // Load with navigation properties for DTO mapping
            var entityWithNav = await _db.CountryBankFieldConfigurations
                .Include(c => c.Country)
                .Include(c => c.BankFieldType)
                .FirstOrDefaultAsync(c => c.ConfigurationId == entity.ConfigurationId);

            return _mapper.Map<CountryBankFieldConfigurationDto>(entityWithNav);
        }
    }
}

