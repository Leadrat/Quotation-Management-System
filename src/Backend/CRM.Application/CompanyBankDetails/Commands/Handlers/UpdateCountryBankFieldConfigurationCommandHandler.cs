using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyBankDetails.Commands;
using CRM.Application.CompanyBankDetails.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyBankDetails.Commands.Handlers
{
    public class UpdateCountryBankFieldConfigurationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateCountryBankFieldConfigurationCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CountryBankFieldConfigurationDto> Handle(UpdateCountryBankFieldConfigurationCommand command)
        {
            var entity = await _db.CountryBankFieldConfigurations
                .Include(c => c.Country)
                .Include(c => c.BankFieldType)
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

            return _mapper.Map<CountryBankFieldConfigurationDto>(entity);
        }
    }
}

