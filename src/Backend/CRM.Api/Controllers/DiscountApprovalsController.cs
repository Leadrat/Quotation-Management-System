using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Results;
using CRM.Application.DiscountApprovals.Commands;
using CRM.Application.DiscountApprovals.Commands.Handlers;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Exceptions;
using CRM.Application.DiscountApprovals.Queries;
using CRM.Application.DiscountApprovals.Queries.Handlers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/discount-approvals")]
    [Authorize]
    public class DiscountApprovalsController : ControllerBase
    {
        private readonly RequestDiscountApprovalCommandHandler _requestHandler;
        private readonly ApproveDiscountApprovalCommandHandler _approveHandler;
        private readonly RejectDiscountApprovalCommandHandler _rejectHandler;
        private readonly EscalateDiscountApprovalCommandHandler _escalateHandler;
        private readonly ResubmitDiscountApprovalCommandHandler _resubmitHandler;
        private readonly BulkApproveDiscountApprovalsCommandHandler _bulkApproveHandler;
        private readonly GetPendingApprovalsQueryHandler _getPendingHandler;
        private readonly GetApprovalByIdQueryHandler _getByIdHandler;
        private readonly GetApprovalTimelineQueryHandler _getTimelineHandler;
        private readonly GetQuotationApprovalsQueryHandler _getQuotationApprovalsHandler;
        private readonly GetApprovalMetricsQueryHandler _getMetricsHandler;
        private readonly IValidator<RequestDiscountApprovalCommand> _requestValidator;
        private readonly IValidator<ApproveDiscountApprovalCommand> _approveValidator;
        private readonly IValidator<RejectDiscountApprovalCommand> _rejectValidator;
        private readonly IValidator<EscalateDiscountApprovalCommand> _escalateValidator;
        private readonly IValidator<ResubmitDiscountApprovalCommand> _resubmitValidator;
        private readonly IValidator<BulkApproveDiscountApprovalsCommand> _bulkApproveValidator;

        public DiscountApprovalsController(
            RequestDiscountApprovalCommandHandler requestHandler,
            ApproveDiscountApprovalCommandHandler approveHandler,
            RejectDiscountApprovalCommandHandler rejectHandler,
            EscalateDiscountApprovalCommandHandler escalateHandler,
            ResubmitDiscountApprovalCommandHandler resubmitHandler,
            BulkApproveDiscountApprovalsCommandHandler bulkApproveHandler,
            GetPendingApprovalsQueryHandler getPendingHandler,
            GetApprovalByIdQueryHandler getByIdHandler,
            GetApprovalTimelineQueryHandler getTimelineHandler,
            GetQuotationApprovalsQueryHandler getQuotationApprovalsHandler,
            GetApprovalMetricsQueryHandler getMetricsHandler,
            IValidator<RequestDiscountApprovalCommand> requestValidator,
            IValidator<ApproveDiscountApprovalCommand> approveValidator,
            IValidator<RejectDiscountApprovalCommand> rejectValidator,
            IValidator<EscalateDiscountApprovalCommand> escalateValidator,
            IValidator<ResubmitDiscountApprovalCommand> resubmitValidator,
            IValidator<BulkApproveDiscountApprovalsCommand> bulkApproveValidator)
        {
            _requestHandler = requestHandler;
            _approveHandler = approveHandler;
            _rejectHandler = rejectHandler;
            _escalateHandler = escalateHandler;
            _resubmitHandler = resubmitHandler;
            _bulkApproveHandler = bulkApproveHandler;
            _getPendingHandler = getPendingHandler;
            _getByIdHandler = getByIdHandler;
            _getTimelineHandler = getTimelineHandler;
            _getQuotationApprovalsHandler = getQuotationApprovalsHandler;
            _getMetricsHandler = getMetricsHandler;
            _requestValidator = requestValidator;
            _approveValidator = approveValidator;
            _rejectValidator = rejectValidator;
            _escalateValidator = escalateValidator;
            _resubmitValidator = resubmitValidator;
            _bulkApproveValidator = bulkApproveValidator;
        }

        private bool TryGetUserContext(out Guid userId, out string role)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            role = User.FindFirstValue("role") ?? string.Empty;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
            {
                userId = Guid.Empty;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Request discount approval for a quotation
        /// </summary>
        [HttpPost("request")]
        [Authorize(Roles = "SalesRep,Admin")]
        [ProducesResponseType(typeof(DiscountApprovalDto), 201)]
        public async Task<IActionResult> RequestApproval([FromBody] CreateDiscountApprovalRequest request)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new RequestDiscountApprovalCommand
                {
                    Request = request,
                    RequestedByUserId = userId
                };

                var validation = await _requestValidator.ValidateAsync(command);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var result = await _requestHandler.Handle(command);
                return Created($"/api/v1/discount-approvals/{result.ApprovalId}", new { success = true, data = result });
            }
            catch (QuotationLockedException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get pending discount approvals
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        [ProducesResponseType(typeof(PagedResult<DiscountApprovalDto>), 200)]
        public async Task<IActionResult> GetPending(
            [FromQuery] Guid? approverUserId,
            [FromQuery] string? status,
            [FromQuery] decimal? discountPercentageMin,
            [FromQuery] decimal? discountPercentageMax,
            [FromQuery] DateTimeOffset? dateFrom,
            [FromQuery] DateTimeOffset? dateTo,
            [FromQuery] Guid? requestedByUserId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var query = new GetPendingApprovalsQuery
                {
                    ApproverUserId = approverUserId,
                    Status = status,
                    DiscountPercentageMin = discountPercentageMin,
                    DiscountPercentageMax = discountPercentageMax,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    RequestedByUserId = requestedByUserId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    RequestorUserId = userId,
                    RequestorRole = role
                };

                var result = await _getPendingHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Approve a discount approval request
        /// </summary>
        [HttpPost("{approvalId}/approve")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(DiscountApprovalDto), 200)]
        public async Task<IActionResult> Approve(Guid approvalId, [FromBody] ApproveDiscountApprovalRequest request)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new ApproveDiscountApprovalCommand
                {
                    ApprovalId = approvalId,
                    Request = request,
                    ApprovedByUserId = userId
                };

                var validation = await _approveValidator.ValidateAsync(command);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var result = await _approveHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (DiscountApprovalNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidApprovalStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedApprovalActionException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reject a discount approval request
        /// </summary>
        [HttpPost("{approvalId}/reject")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(DiscountApprovalDto), 200)]
        public async Task<IActionResult> Reject(Guid approvalId, [FromBody] RejectDiscountApprovalRequest request)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new RejectDiscountApprovalCommand
                {
                    ApprovalId = approvalId,
                    Request = request,
                    RejectedByUserId = userId
                };

                var validation = await _rejectValidator.ValidateAsync(command);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var result = await _rejectHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (DiscountApprovalNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidApprovalStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedApprovalActionException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get discount approval by ID
        /// </summary>
        [HttpGet("{approvalId}")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        [ProducesResponseType(typeof(DiscountApprovalDto), 200)]
        public async Task<IActionResult> GetById(Guid approvalId)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var query = new GetApprovalByIdQuery
                {
                    ApprovalId = approvalId,
                    RequestorUserId = userId,
                    RequestorRole = role
                };

                var result = await _getByIdHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (DiscountApprovalNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Escalate a discount approval to admin
        /// </summary>
        [HttpPost("{approvalId}/escalate")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(DiscountApprovalDto), 200)]
        public async Task<IActionResult> Escalate(Guid approvalId, [FromBody] EscalateRequest? request = null)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new EscalateDiscountApprovalCommand
                {
                    ApprovalId = approvalId,
                    EscalatedByUserId = userId,
                    Reason = request?.Reason
                };

                var validation = await _escalateValidator.ValidateAsync(command);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var result = await _escalateHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (DiscountApprovalNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidApprovalStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedApprovalActionException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all approvals for a quotation
        /// </summary>
        [HttpGet("by-quotation/{quotationId}")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        [ProducesResponseType(typeof(List<DiscountApprovalDto>), 200)]
        public async Task<IActionResult> GetQuotationApprovals(Guid quotationId)
        {
            try
            {
                var query = new GetQuotationApprovalsQuery
                {
                    QuotationId = quotationId
                };

                var result = await _getQuotationApprovalsHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get approval metrics and analytics (admin only)
        /// </summary>
        [HttpGet("reports")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApprovalMetricsDto), 200)]
        public async Task<IActionResult> GetReports(
            [FromQuery] DateTimeOffset? dateFrom,
            [FromQuery] DateTimeOffset? dateTo,
            [FromQuery] Guid? approverUserId)
        {
            try
            {
                var query = new GetApprovalMetricsQuery
                {
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    ApproverUserId = approverUserId
                };

                var result = await _getMetricsHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get approval timeline
        /// </summary>
        [HttpGet("timeline")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        [ProducesResponseType(typeof(List<ApprovalTimelineDto>), 200)]
        public async Task<IActionResult> GetTimeline(
            [FromQuery] Guid? approvalId,
            [FromQuery] Guid? quotationId)
        {
            try
            {
                var query = new GetApprovalTimelineQuery
                {
                    ApprovalId = approvalId,
                    QuotationId = quotationId
                };

                var result = await _getTimelineHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Resubmit a rejected discount approval
        /// </summary>
        [HttpPost("{approvalId}/resubmit")]
        [Authorize(Roles = "SalesRep,Admin")]
        [ProducesResponseType(typeof(DiscountApprovalDto), 201)]
        public async Task<IActionResult> Resubmit(Guid approvalId, [FromBody] ResubmitDiscountApprovalRequest request)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new ResubmitDiscountApprovalCommand
                {
                    ApprovalId = approvalId,
                    Request = request,
                    ResubmittedByUserId = userId
                };

                var validation = await _resubmitValidator.ValidateAsync(command);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var result = await _resubmitHandler.Handle(command);
                return Created($"/api/v1/discount-approvals/{result.ApprovalId}", new { success = true, data = result });
            }
            catch (DiscountApprovalNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidApprovalStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (QuotationLockedException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (UnauthorizedApprovalActionException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk approve multiple discount approvals
        /// </summary>
        [HttpPost("bulk-approve")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(List<DiscountApprovalDto>), 200)]
        public async Task<IActionResult> BulkApprove([FromBody] BulkApproveRequest request)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new BulkApproveDiscountApprovalsCommand
                {
                    Request = request,
                    ApprovedByUserId = userId
                };

                var validation = await _bulkApproveValidator.ValidateAsync(command);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var result = await _bulkApproveHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (DiscountApprovalNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidApprovalStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedApprovalActionException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class EscalateRequest
        {
            public string? Reason { get; set; }
        }
    }
}

