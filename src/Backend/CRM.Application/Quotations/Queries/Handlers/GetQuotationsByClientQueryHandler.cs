using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetQuotationsByClientQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetQuotationsByClientQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<QuotationDto>> Handle(GetQuotationsByClientQuery query)
        {
            // Verify client exists and user has access
            var client = await _db.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == query.ClientId);

            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {query.ClientId} not found.");
            }

            var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && client.CreatedByUserId != query.RequestorUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view quotations for this client.");
            }

            var quotations = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Include(q => q.LineItems)
                .AsNoTracking()
                .Where(q => q.ClientId == query.ClientId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return quotations.Select(q => _mapper.Map<QuotationDto>(q)).ToList();
        }
    }
}

