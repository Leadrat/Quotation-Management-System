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
    public class CreateBankFieldTypeCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateBankFieldTypeCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<BankFieldTypeDto> Handle(CreateBankFieldTypeCommand command)
        {
            // Check if name already exists (case-insensitive)
            var nameUpper = command.Request.Name.ToUpperInvariant();
            var exists = await _db.BankFieldTypes
                .AnyAsync(b => b.DeletedAt == null && b.Name.ToUpper() == nameUpper);

            if (exists)
            {
                throw new InvalidOperationException($"Bank field type with name '{command.Request.Name}' already exists.");
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new BankFieldType
            {
                BankFieldTypeId = Guid.NewGuid(),
                Name = command.Request.Name.ToUpperInvariant(),
                DisplayName = command.Request.DisplayName,
                Description = command.Request.Description,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.BankFieldTypes.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<BankFieldTypeDto>(entity);
        }
    }
}

