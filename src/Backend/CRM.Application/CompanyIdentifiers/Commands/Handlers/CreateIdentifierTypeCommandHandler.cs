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
    public class CreateIdentifierTypeCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateIdentifierTypeCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IdentifierTypeDto> Handle(CreateIdentifierTypeCommand command)
        {
            // Check if name already exists (case-insensitive)
            var nameUpper = command.Request.Name.ToUpperInvariant();
            var exists = await _db.IdentifierTypes
                .AnyAsync(i => i.DeletedAt == null && i.Name.ToUpper() == nameUpper);

            if (exists)
            {
                throw new InvalidOperationException($"Identifier type with name '{command.Request.Name}' already exists.");
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new IdentifierType
            {
                IdentifierTypeId = Guid.NewGuid(),
                Name = command.Request.Name.ToUpperInvariant(),
                DisplayName = command.Request.DisplayName,
                Description = command.Request.Description,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.IdentifierTypes.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<IdentifierTypeDto>(entity);
        }
    }
}

