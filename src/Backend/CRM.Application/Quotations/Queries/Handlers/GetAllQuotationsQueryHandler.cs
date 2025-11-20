using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.Quotations.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetAllQuotationsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllQuotationsQueryHandler> _logger;

        public GetAllQuotationsQueryHandler(IAppDbContext db, IMapper mapper, ILogger<GetAllQuotationsQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<QuotationDto>> Handle(GetAllQuotationsQuery query)
        {
            try
            {
                var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
                var isManager = string.Equals(query.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase);
                var canSeeAll = isAdmin || isManager;

                var baseQuery = _db.Quotations
                    .Include(q => q.Client)
                    .Include(q => q.CreatedByUser)
                    .Include(q => q.LineItems)
                    .AsNoTracking();

                // Authorization: SalesRep sees only own quotations, Manager and Admin see all
                if (!canSeeAll)
                {
                    baseQuery = baseQuery.Where(q => q.CreatedByUserId == query.RequestorUserId);
                }

                // Apply filters
                if (query.ClientId.HasValue)
                {
                    baseQuery = baseQuery.Where(q => q.ClientId == query.ClientId.Value);
                }

                if (query.CreatedByUserId.HasValue && canSeeAll)
                {
                    baseQuery = baseQuery.Where(q => q.CreatedByUserId == query.CreatedByUserId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.Status))
                {
                    baseQuery = baseQuery.Where(q => q.Status.ToString().ToUpper() == query.Status.ToUpper());
                }

                if (query.DateFrom.HasValue)
                {
                    baseQuery = baseQuery.Where(q => q.QuotationDate >= query.DateFrom.Value);
                }

                if (query.DateTo.HasValue)
                {
                    baseQuery = baseQuery.Where(q => q.QuotationDate <= query.DateTo.Value);
                }

                // Get total count
                var totalCount = await baseQuery.CountAsync();

                // Apply pagination and ordering
                var quotations = await baseQuery
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var items = quotations.Select(q => _mapper.Map<QuotationDto>(q)).ToArray();

                return new PagedResult<QuotationDto>
                {
                    Data = items,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex) when (ex.Message.Contains("42P01") || ex.Message.Contains("does not exist") || ex.Message.Contains("relation") && ex.Message.Contains("not exist"))
            {
                _logger.LogWarning("Quotations table does not exist, returning empty result");
                return new PagedResult<QuotationDto>
                {
                    Data = Array.Empty<QuotationDto>(),
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotations for user {UserId}", query.RequestorUserId);
                throw;
            }
        }
    }
}

