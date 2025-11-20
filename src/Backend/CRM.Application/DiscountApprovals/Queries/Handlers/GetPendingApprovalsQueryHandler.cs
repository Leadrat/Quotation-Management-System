using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Queries;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.Queries.Handlers
{
    public class GetPendingApprovalsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPendingApprovalsQueryHandler> _logger;

        public GetPendingApprovalsQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetPendingApprovalsQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<DiscountApprovalDto>> Handle(GetPendingApprovalsQuery query)
        {
            try
            {
                _logger.LogInformation("Getting pending approvals for user {UserId} with role {Role}", 
                    query.RequestorUserId, query.RequestorRole);

                // Validate requestor user ID
                if (query.RequestorUserId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid RequestorUserId (empty GUID)");
                    return new PagedResult<DiscountApprovalDto>
                    {
                        Success = true,
                        Data = Array.Empty<DiscountApprovalDto>(),
                        PageNumber = query.PageNumber,
                        PageSize = query.PageSize,
                        TotalCount = 0
                    };
                }

                // Base query - include navigation properties
                IQueryable<Domain.Entities.DiscountApproval> baseQuery = _db.DiscountApprovals
                    .AsNoTracking()
                    .Include(a => a.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(a => a.RequestedByUser)
                    .Include(a => a.ApproverUser);

            // Filter by approver based on role
            if (query.RequestorRole.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            {
                // Manager sees only manager-level approvals assigned to them
                baseQuery = baseQuery.Where(a => 
                    a.ApprovalLevel == ApprovalLevel.Manager && 
                    (a.ApproverUserId == query.RequestorUserId || a.ApproverUserId == null));
            }
            else if (query.RequestorRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Admin sees all approvals
                // No additional filter
            }
            else
            {
                // SalesRep sees only their own requests
                baseQuery = baseQuery.Where(a => a.RequestedByUserId == query.RequestorUserId);
            }

            // Apply filters
            if (query.ApproverUserId.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.ApproverUserId == query.ApproverUserId.Value);
            }

            if (!string.IsNullOrEmpty(query.Status))
            {
                if (Enum.TryParse<ApprovalStatus>(query.Status, true, out var status))
                {
                    baseQuery = baseQuery.Where(a => a.Status == status);
                }
            }

            if (query.DiscountPercentageMin.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.CurrentDiscountPercentage >= query.DiscountPercentageMin.Value);
            }

            if (query.DiscountPercentageMax.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.CurrentDiscountPercentage <= query.DiscountPercentageMax.Value);
            }

            if (query.DateFrom.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.RequestDate >= query.DateFrom.Value);
            }

            if (query.DateTo.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.RequestDate <= query.DateTo.Value);
            }

            if (query.RequestedByUserId.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.RequestedByUserId == query.RequestedByUserId.Value);
            }

            // Get total count
            var totalCount = await baseQuery.CountAsync();

            // Apply pagination
            var approvals = await baseQuery
                .OrderByDescending(a => a.RequestDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

                var dtos = approvals.Select(a => _mapper.Map<DiscountApprovalDto>(a)).ToArray();

                return new PagedResult<DiscountApprovalDto>
                {
                    Success = true,
                    Data = dtos,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals for user {UserId} with role {Role}", 
                    query.RequestorUserId, query.RequestorRole);
                throw;
            }
        }
    }
}

