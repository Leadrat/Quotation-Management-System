using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Results;
using CRM.Application.Refunds.Commands;
using CRM.Application.Refunds.Commands.Handlers;
using CRM.Application.Refunds.Dtos;
using CRM.Application.Refunds.Queries;
using CRM.Application.Refunds.Queries.Handlers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/refunds")]
    [Authorize]
    public class RefundsController : ControllerBase
    {
        private readonly InitiateRefundCommandHandler _initiateHandler;
        private readonly ApproveRefundCommandHandler _approveHandler;
        private readonly RejectRefundCommandHandler _rejectHandler;
        private readonly ProcessRefundCommandHandler _processHandler;
        private readonly ReverseRefundCommandHandler _reverseHandler;
        private readonly BulkProcessRefundsCommandHandler _bulkProcessHandler;
        private readonly GetRefundByIdQueryHandler _getByIdHandler;
        private readonly GetRefundsByPaymentQueryHandler _getByPaymentHandler;
        private readonly GetRefundsByQuotationQueryHandler _getByQuotationHandler;
        private readonly GetPendingRefundsQueryHandler _getPendingHandler;
        private readonly GetRefundTimelineQueryHandler _getTimelineHandler;
        private readonly GetRefundMetricsQueryHandler _getMetricsHandler;
        private readonly IValidator<CreateRefundRequest> _createValidator;

        public RefundsController(
            InitiateRefundCommandHandler initiateHandler,
            ApproveRefundCommandHandler approveHandler,
            RejectRefundCommandHandler rejectHandler,
            ProcessRefundCommandHandler processHandler,
            ReverseRefundCommandHandler reverseHandler,
            BulkProcessRefundsCommandHandler bulkProcessHandler,
            GetRefundByIdQueryHandler getByIdHandler,
            GetRefundsByPaymentQueryHandler getByPaymentHandler,
            GetRefundsByQuotationQueryHandler getByQuotationHandler,
            GetPendingRefundsQueryHandler getPendingHandler,
            GetRefundTimelineQueryHandler getTimelineHandler,
            GetRefundMetricsQueryHandler getMetricsHandler,
            IValidator<CreateRefundRequest> createValidator)
        {
            _initiateHandler = initiateHandler;
            _approveHandler = approveHandler;
            _rejectHandler = rejectHandler;
            _processHandler = processHandler;
            _reverseHandler = reverseHandler;
            _bulkProcessHandler = bulkProcessHandler;
            _getByIdHandler = getByIdHandler;
            _getByPaymentHandler = getByPaymentHandler;
            _getByQuotationHandler = getByQuotationHandler;
            _getPendingHandler = getPendingHandler;
            _getTimelineHandler = getTimelineHandler;
            _getMetricsHandler = getMetricsHandler;
            _createValidator = createValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RefundDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateRefund([FromBody] CreateRefundRequest request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var command = new InitiateRefundCommand
                {
                    Request = request,
                    RequestedByUserId = userId
                };

                var result = await _initiateHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{refundId}")]
        [ProducesResponseType(typeof(RefundDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRefundById(Guid refundId)
        {
            var query = new GetRefundByIdQuery { RefundId = refundId };
            var result = await _getByIdHandler.Handle(query);
            
            if (result == null)
                return NotFound();
            
            return Ok(result);
        }

        [HttpGet("payment/{paymentId}")]
        [ProducesResponseType(typeof(List<RefundDto>), 200)]
        public async Task<IActionResult> GetRefundsByPayment(Guid paymentId)
        {
            var query = new GetRefundsByPaymentQuery { PaymentId = paymentId };
            var result = await _getByPaymentHandler.Handle(query);
            return Ok(result);
        }

        [HttpGet("quotation/{quotationId}")]
        [ProducesResponseType(typeof(List<RefundDto>), 200)]
        public async Task<IActionResult> GetRefundsByQuotation(Guid quotationId)
        {
            var query = new GetRefundsByQuotationQuery { QuotationId = quotationId };
            var result = await _getByQuotationHandler.Handle(query);
            return Ok(result);
        }

        [HttpPost("{refundId}/approve")]
        [ProducesResponseType(typeof(RefundDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApproveRefund(Guid refundId, [FromBody] ApproveRefundRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var command = new ApproveRefundCommand
                {
                    RefundId = refundId,
                    Request = request,
                    ApprovedByUserId = userId
                };

                var result = await _approveHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{refundId}/reject")]
        [ProducesResponseType(typeof(RefundDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RejectRefund(Guid refundId, [FromBody] RejectRefundRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var command = new RejectRefundCommand
                {
                    RefundId = refundId,
                    Request = request,
                    RejectedByUserId = userId
                };

                var result = await _rejectHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{refundId}/process")]
        [ProducesResponseType(typeof(RefundDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ProcessRefund(Guid refundId)
        {
            try
            {
                var command = new ProcessRefundCommand { RefundId = refundId };
                var result = await _processHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{refundId}/reverse")]
        [ProducesResponseType(typeof(RefundDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ReverseRefund(Guid refundId, [FromBody] ReverseRefundRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var command = new ReverseRefundCommand
                {
                    RefundId = refundId,
                    Request = request,
                    ReversedByUserId = userId
                };

                var result = await _reverseHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{refundId}/timeline")]
        [ProducesResponseType(typeof(List<RefundTimelineDto>), 200)]
        public async Task<IActionResult> GetRefundTimeline(Guid refundId)
        {
            var query = new GetRefundTimelineQuery { RefundId = refundId };
            var result = await _getTimelineHandler.Handle(query);
            return Ok(result);
        }

        [HttpGet("pending")]
        [ProducesResponseType(typeof(List<RefundDto>), 200)]
        public async Task<IActionResult> GetPendingRefunds([FromQuery] string? approvalLevel)
        {
            var query = new GetPendingRefundsQuery { ApprovalLevel = approvalLevel };
            var result = await _getPendingHandler.Handle(query);
            return Ok(result);
        }

        [HttpGet("metrics")]
        [ProducesResponseType(typeof(RefundMetricsDto), 200)]
        public async Task<IActionResult> GetRefundMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = new GetRefundMetricsQuery
            {
                StartDate = startDate,
                EndDate = endDate
            };
            var result = await _getMetricsHandler.Handle(query);
            return Ok(result);
        }

        [HttpPost("bulk-process")]
        [ProducesResponseType(typeof(BulkProcessRefundsResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> BulkProcessRefunds([FromBody] BulkProcessRefundsRequest request)
        {
            try
            {
                var command = new BulkProcessRefundsCommand { Request = request };
                var result = await _bulkProcessHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

