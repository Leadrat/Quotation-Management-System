using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetQuotationResponseQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetQuotationResponseQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<QuotationResponseDto?> Handle(GetQuotationResponseQuery query)
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

            var response = await _db.QuotationResponses
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.QuotationId == query.QuotationId);

            return response == null ? null : _mapper.Map<QuotationResponseDto>(response);
        }

        private static void EnsureAuthorized(Guid userId, string role, Guid ownerId)
        {
            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);
            var canAccess = isAdmin || isManager || ownerId == userId;
            
            if (!canAccess)
            {
                throw new UnauthorizedAccessException("You do not have permission to access this quotation.");
            }
        }
    }
}


