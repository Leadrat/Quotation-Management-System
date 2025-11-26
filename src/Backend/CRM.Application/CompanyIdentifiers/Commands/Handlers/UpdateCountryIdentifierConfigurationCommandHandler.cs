using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyIdentifiers.Commands;
using CRM.Application.CompanyIdentifiers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyIdentifiers.Commands.Handlers
{
    public class UpdateCountryIdentifierConfigurationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateCountryIdentifierConfigurationCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CountryIdentifierConfigurationDto> Handle(UpdateCountryIdentifierConfigurationCommand command)
        {
            var entity = await _db.CountryIdentifierConfigurations
                .Include(c => c.Country)
                .Include(c => c.IdentifierType)
                .FirstOrDefaultAsync(c => c.ConfigurationId == command.ConfigurationId && c.DeletedAt == null);

            if (entity == null)
            {
                throw new InvalidOperationException($"Configuration with ID '{command.ConfigurationId}' not found.");
            }

            entity.IsRequired = command.Request.IsRequired;
            entity.ValidationRegex = command.Request.ValidationRegex;
            entity.MinLength = command.Request.MinLength;
            entity.MaxLength = command.Request.MaxLength;
            entity.DisplayName = command.Request.DisplayName;
            entity.HelpText = command.Request.HelpText;
            entity.DisplayOrder = command.Request.DisplayOrder;
            entity.IsActive = command.Request.IsActive;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return _mapper.Map<CountryIdentifierConfigurationDto>(entity);
        }
    }
}

