using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Exceptions;
using CRM.Application.Common.Interfaces;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Commands.Handlers
{
    public class CreateClientCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenantContext;

        public CreateClientCommandHandler(IAppDbContext db, IMapper mapper, ITenantContext tenantContext)
        {
            _db = db;
            _mapper = mapper;
            _tenantContext = tenantContext;
        }

        public async Task<ClientDto> Handle(CreateClientCommand cmd)
        {
            // Validate that the user exists
            var userExists = await _db.Users.AnyAsync(u => u.UserId == cmd.CreatedByUserId && u.DeletedAt == null);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {cmd.CreatedByUserId} does not exist in the database.");
            }

            var emailLower = (cmd.Email ?? string.Empty).Trim().ToLowerInvariant();
            var currentTenantId = _tenantContext.CurrentTenantId;
            var exists = await _db.Clients.AnyAsync(c => c.DeletedAt == null && (c.TenantId == currentTenantId || c.TenantId == null) && c.Email != null && c.Email.ToLower() == emailLower);
            if (exists)
            {
                throw new DuplicateEmailException(cmd.Email);
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new Client
            {
                ClientId = Guid.NewGuid(),
                TenantId = currentTenantId,
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
            
            try
            {
            await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                // Check for foreign key constraint violation
                if (innerException.Contains("foreign key") || innerException.Contains("FK_") || 
                    innerException.Contains("violates foreign key constraint"))
                {
                    throw new InvalidOperationException($"Database constraint violation: The user with ID {cmd.CreatedByUserId} may not exist or the foreign key constraint is misconfigured. Details: {innerException}", dbEx);
                }
                // Check for unique constraint violation (email)
                if (innerException.Contains("unique") || innerException.Contains("duplicate"))
                {
                    throw new DuplicateEmailException(cmd.Email);
                }
                // Re-throw with more context
                throw new InvalidOperationException($"Database error while saving client: {innerException}", dbEx);
            }

            // Reload entity with CreatedByUser navigation property for mapping
            var entityWithUser = await _db.Clients
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.ClientId == entity.ClientId);

            if (entityWithUser == null)
            {
                throw new InvalidOperationException("Failed to retrieve created client");
            }

            return _mapper.Map<ClientDto>(entityWithUser);
        }
    }
}
