using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetQuotationStatusHistoryQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetQuotationStatusHistoryQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<QuotationStatusHistoryDto>> Handle(GetQuotationStatusHistoryQuery query)
        {
            var quotation = await _db.Quotations
                .AsNoTracking()
                .Select(q => new { q.QuotationId, q.CreatedByUserId })
                .FirstOrDefaultAsync(q => q.QuotationId == query.QuotationId);

            if (quotation == null)
            {
                throw new QuotationNotFoundException(query.QuotationId);
            }

            EnsureAuthorized(query.RequestorUserId, query.RequestorRole, quotation.CreatedByUserId);

            var history = await _db.QuotationStatusHistory
                .Include(h => h.ChangedByUser)
                .Where(h => h.QuotationId == query.QuotationId)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<QuotationStatusHistoryDto>>(history);
        }

        private static void EnsureAuthorized(Guid userId, string role, Guid ownerId)
        {
            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && ownerId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to access this quotation.");
            }
        }
    }
}


