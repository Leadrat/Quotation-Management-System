using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyBankDetails.Commands;
using CRM.Application.CompanyBankDetails.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyBankDetails.Commands.Handlers
{
    public class UpdateBankFieldTypeCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateBankFieldTypeCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<BankFieldTypeDto> Handle(UpdateBankFieldTypeCommand command)
        {
            var entity = await _db.BankFieldTypes
                .FirstOrDefaultAsync(b => b.BankFieldTypeId == command.BankFieldTypeId && b.DeletedAt == null);

            if (entity == null)
            {
                throw new InvalidOperationException($"Bank field type with ID '{command.BankFieldTypeId}' not found.");
            }

            entity.DisplayName = command.Request.DisplayName;
            entity.Description = command.Request.Description;
            entity.IsActive = command.Request.IsActive;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return _mapper.Map<BankFieldTypeDto>(entity);
        }
    }
}

