using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetQuotationByAccessTokenQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetQuotationByAccessTokenQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PublicQuotationDto> Handle(GetQuotationByAccessTokenQuery query)
        {
            var link = await _db.QuotationAccessLinks
                .Include(l => l.Quotation)
                    .ThenInclude(q => q.Client)
                .Include(l => l.Quotation)
                    .ThenInclude(q => q.LineItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.QuotationId == query.QuotationId &&
                    l.AccessToken == query.AccessToken);

            if (link == null)
            {
                throw new QuotationAccessLinkNotFoundException();
            }

            if (!link.IsActive || link.IsExpired())
            {
                throw new InvalidOperationException("Access link is inactive or expired.");
            }

            if (link.Quotation == null)
            {
                throw new QuotationNotFoundException(query.QuotationId);
            }

            return _mapper.Map<PublicQuotationDto>(link.Quotation);
        }
    }
}


