using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Queries;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class GetClientByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        public GetClientByIdQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ClientDto> Handle(GetClientByIdQuery q)
        {
            var entity = await _db.Clients
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.ClientId == q.ClientId && c.DeletedAt == null);
            if (entity == null)
            {
                throw new CRM.Application.Clients.Exceptions.ClientNotFoundException(q.ClientId);
            }

            var isAdmin = string.Equals(q.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && entity.CreatedByUserId != q.RequestorUserId)
            {
                throw new CRM.Shared.Exceptions.UserNotActiveException("Cannot access other user's client");
            }

            return _mapper.Map<ClientDto>(entity);
        }
    }
}
