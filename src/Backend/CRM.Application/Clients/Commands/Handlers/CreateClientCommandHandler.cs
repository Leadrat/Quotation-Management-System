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
    public class CreateClientCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        public CreateClientCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ClientDto> Handle(CreateClientCommand cmd)
        {
            var emailLower = (cmd.Email ?? string.Empty).Trim().ToLowerInvariant();
            var exists = await _db.Clients.AnyAsync(c => c.DeletedAt == null && c.Email.ToLower() == emailLower);
            if (exists)
            {
                throw new DuplicateEmailException(cmd.Email);
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new Client
            {
                ClientId = Guid.NewGuid(),
                CompanyName = cmd.CompanyName,
                ContactName = cmd.ContactName,
                Email = emailLower,
                Mobile = cmd.Mobile,
                PhoneCode = cmd.PhoneCode,
                Gstin = cmd.Gstin,
                StateCode = cmd.StateCode,
                Address = cmd.Address,
                City = cmd.City,
                State = cmd.State,
                PinCode = cmd.PinCode,
                CreatedByUserId = cmd.CreatedByUserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Clients.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<ClientDto>(entity);
        }
    }
}
