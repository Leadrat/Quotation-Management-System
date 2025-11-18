using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetQuotationByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetQuotationByIdQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<QuotationDto> Handle(GetQuotationByIdQuery query)
        {
            var quotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Include(q => q.LineItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuotationId == query.QuotationId);

            if (quotation == null)
            {
                throw new QuotationNotFoundException(query.QuotationId);
            }

            // Authorization: User owns quotation or is admin
            var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && quotation.CreatedByUserId != query.RequestorUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this quotation.");
            }

            return _mapper.Map<QuotationDto>(quotation);
        }
    }
}

