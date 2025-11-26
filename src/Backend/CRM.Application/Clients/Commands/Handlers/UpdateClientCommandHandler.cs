using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Exceptions;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Commands.Handlers
{
    public class UpdateClientCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        public UpdateClientCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ClientDto> Handle(UpdateClientCommand cmd)
        {
            var entity = await _db.Clients.Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.ClientId == cmd.ClientId && c.DeletedAt == null);
            if (entity == null)
            {
                throw new ClientNotFoundException(cmd.ClientId);
            }

            var isAdmin = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && entity.CreatedByUserId != cmd.UpdatedByUserId)
            {
                throw new UnauthorizedAccessException("Cannot update other user's client");
            }

            if (!string.IsNullOrWhiteSpace(cmd.Email))
            {
                var emailLower = cmd.Email.Trim().ToLowerInvariant();
                if (!string.Equals(emailLower, entity.Email, StringComparison.Ordinal))
                {
                    var exists = await _db.Clients.AnyAsync(c => c.DeletedAt == null && c.Email != null && c.Email.ToLower() == emailLower && c.ClientId != entity.ClientId);
                    if (exists) throw new DuplicateEmailException(cmd.Email);
                    entity.Email = emailLower;
                }
            }

            entity.CompanyName = cmd.CompanyName ?? entity.CompanyName;
            entity.ContactName = cmd.ContactName ?? entity.ContactName;
            entity.Mobile = cmd.Mobile ?? entity.Mobile;
            entity.PhoneCode = cmd.PhoneCode ?? entity.PhoneCode;
            entity.Gstin = cmd.Gstin ?? entity.Gstin;
            entity.StateCode = cmd.StateCode ?? entity.StateCode;
            entity.Address = cmd.Address ?? entity.Address;
            entity.City = cmd.City ?? entity.City;
            entity.State = cmd.State ?? entity.State;
            entity.PinCode = cmd.PinCode ?? entity.PinCode;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
            return _mapper.Map<ClientDto>(entity);
        }
    }
}
