using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyIdentifiers.Commands;
using CRM.Application.CompanyIdentifiers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyIdentifiers.Commands.Handlers
{
    public class UpdateIdentifierTypeCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateIdentifierTypeCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IdentifierTypeDto> Handle(UpdateIdentifierTypeCommand command)
        {
            var entity = await _db.IdentifierTypes
                .FirstOrDefaultAsync(i => i.IdentifierTypeId == command.IdentifierTypeId && i.DeletedAt == null);

            if (entity == null)
            {
                throw new InvalidOperationException($"Identifier type with ID '{command.IdentifierTypeId}' not found.");
            }

            entity.DisplayName = command.Request.DisplayName;
            entity.Description = command.Request.Description;
            entity.IsActive = command.Request.IsActive;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return _mapper.Map<IdentifierTypeDto>(entity);
        }
    }
}

