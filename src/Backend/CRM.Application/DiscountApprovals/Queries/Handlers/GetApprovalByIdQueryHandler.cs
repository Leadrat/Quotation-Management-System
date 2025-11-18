using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Exceptions;
using CRM.Application.DiscountApprovals.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.Queries.Handlers
{
    public class GetApprovalByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetApprovalByIdQueryHandler> _logger;

        public GetApprovalByIdQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetApprovalByIdQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DiscountApprovalDto> Handle(GetApprovalByIdQuery query)
        {
            _logger.LogInformation("Getting approval {ApprovalId} for user {UserId}", 
                query.ApprovalId, query.RequestorUserId);

            var approval = await _db.DiscountApprovals
                .AsNoTracking()
                .Include(a => a.Quotation)
                    .ThenInclude(q => q.Client)
                .Include(a => a.RequestedByUser)
                .Include(a => a.ApproverUser)
                .FirstOrDefaultAsync(a => a.ApprovalId == query.ApprovalId);

            if (approval == null)
            {
                throw new DiscountApprovalNotFoundException(query.ApprovalId);
            }

            // Verify user has access (requester, approver, or admin)
            var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isRequester = approval.RequestedByUserId == query.RequestorUserId;
            var isApprover = approval.ApproverUserId == query.RequestorUserId;

            if (!isAdmin && !isRequester && !isApprover)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this approval.");
            }

            return _mapper.Map<DiscountApprovalDto>(approval);
        }
    }
}

