using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyIdentifiers.Commands;
using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyIdentifiers.Commands.Handlers
{
    public class ConfigureCountryIdentifierCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public ConfigureCountryIdentifierCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CountryIdentifierConfigurationDto> Handle(ConfigureCountryIdentifierCommand command)
        {
            // Verify country exists
            var countryExists = await _db.Countries
                .AnyAsync(c => c.CountryId == command.Request.CountryId && c.DeletedAt == null);

            if (!countryExists)
            {
                throw new InvalidOperationException($"Country with ID '{command.Request.CountryId}' not found.");
            }

            // Verify identifier type exists
            var identifierTypeExists = await _db.IdentifierTypes
                .AnyAsync(i => i.IdentifierTypeId == command.Request.IdentifierTypeId && i.DeletedAt == null);

            if (!identifierTypeExists)
            {
                throw new InvalidOperationException($"Identifier type with ID '{command.Request.IdentifierTypeId}' not found.");
            }

            // Check if configuration already exists
            var existing = await _db.CountryIdentifierConfigurations
                .FirstOrDefaultAsync(c => c.CountryId == command.Request.CountryId 
                    && c.IdentifierTypeId == command.Request.IdentifierTypeId 
                    && c.DeletedAt == null);

            if (existing != null)
            {
                throw new InvalidOperationException($"Configuration for country '{command.Request.CountryId}' and identifier type '{command.Request.IdentifierTypeId}' already exists.");
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new CountryIdentifierConfiguration
            {
                ConfigurationId = Guid.NewGuid(),
                CountryId = command.Request.CountryId,
                IdentifierTypeId = command.Request.IdentifierTypeId,
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

            _db.CountryIdentifierConfigurations.Add(entity);
            await _db.SaveChangesAsync();

            // Load with navigation properties for DTO mapping
            var entityWithNav = await _db.CountryIdentifierConfigurations
                .Include(c => c.Country)
                .Include(c => c.IdentifierType)
                .FirstOrDefaultAsync(c => c.ConfigurationId == entity.ConfigurationId);

            return _mapper.Map<CountryIdentifierConfigurationDto>(entityWithNav);
        }
    }
}

