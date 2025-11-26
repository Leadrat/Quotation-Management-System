using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.Quotations.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetAllQuotationsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllQuotationsQueryHandler> _logger;
        private readonly ITenantContext _tenantContext;

        public GetAllQuotationsQueryHandler(IAppDbContext db, IMapper mapper, ILogger<GetAllQuotationsQueryHandler> logger, ITenantContext tenantContext)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _tenantContext = tenantContext;
        }

        public async Task<PagedResult<QuotationDto>> Handle(GetAllQuotationsQuery query)
        {
            try
            {
                var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
                var isManager = string.Equals(query.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase);
                var canSeeAll = isAdmin || isManager;

                _logger.LogInformation("GetAllQuotations: RequestorRole={Role}, RequestorUserId={UserId}, CanSeeAll={CanSeeAll}", 
                    query.RequestorRole, query.RequestorUserId, canSeeAll);

                // Temporarily disable tenant filter completely for debugging
                // var currentTenantId = _tenantContext.CurrentTenantId;
                var baseQuery = _db.Quotations
                    .Include(q => q.Client)
                    .Include(q => q.CreatedByUser)
                    .Include(q => q.LineItems)
                    // .Where(q => q.TenantId == currentTenantId || q.TenantId == null)
                    .AsNoTracking();

                // Authorization: SalesRep sees only own quotations, Manager and Admin see all
                if (!canSeeAll)
                {
                    _logger.LogInformation("Filtering quotations for SalesRep - showing only quotations created by UserId={UserId}", query.RequestorUserId);
                    baseQuery = baseQuery.Where(q => q.CreatedByUserId == query.RequestorUserId);
                }
                else
                {
                    _logger.LogInformation("Manager/Admin access - showing all quotations");
                }

                // Log total count before filters
                var totalBeforeFilters = await baseQuery.CountAsync();
                _logger.LogInformation("Total quotations before filters: {Count}", totalBeforeFilters);

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
                    // Parse the status string to enum for proper comparison
                    if (Enum.TryParse<QuotationStatus>(query.Status, true, out var statusEnum))
                    {
                        baseQuery = baseQuery.Where(q => q.Status == statusEnum);
                    }
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
                _logger.LogInformation("Total quotations after all filters: {Count}", totalCount);

                // Apply pagination and ordering
                var quotations = await baseQuery
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} quotations from database", quotations.Count);

                var items = quotations.Select(q => _mapper.Map<QuotationDto>(q)).ToArray();

                _logger.LogInformation("Mapped {Count} quotations to DTOs", items.Length);

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

