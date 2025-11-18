using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Common.Results;
using CRM.Application.Payments.Commands;
using CRM.Application.Payments.Commands.Handlers;
using CRM.Application.Payments.Dtos;
using CRM.Application.Payments.Queries;
using CRM.Application.Payments.Queries.Handlers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly InitiatePaymentCommandHandler _initiateHandler;
        private readonly UpdatePaymentStatusCommandHandler _updateStatusHandler;
        private readonly RefundPaymentCommandHandler _refundHandler;
        private readonly CancelPaymentCommandHandler _cancelHandler;
        private readonly GetPaymentByIdQueryHandler _getByIdHandler;
        private readonly GetPaymentByQuotationQueryHandler _getByQuotationHandler;
        private readonly GetPaymentsByUserQueryHandler _getByUserHandler;
        private readonly GetPaymentsDashboardQueryHandler _dashboardHandler;
        private readonly IValidator<InitiatePaymentRequest> _initiateValidator;
        private readonly IValidator<UpdatePaymentStatusRequest> _updateStatusValidator;
        private readonly IValidator<RefundPaymentRequest> _refundValidator;
        private readonly IValidator<GetPaymentByQuotationQuery> _getByQuotationValidator;
        private readonly IValidator<GetPaymentByIdQuery> _getByIdValidator;

        public PaymentsController(
            InitiatePaymentCommandHandler initiateHandler,
            UpdatePaymentStatusCommandHandler updateStatusHandler,
            RefundPaymentCommandHandler refundHandler,
            CancelPaymentCommandHandler cancelHandler,
            GetPaymentByIdQueryHandler getByIdHandler,
            GetPaymentByQuotationQueryHandler getByQuotationHandler,
            GetPaymentsByUserQueryHandler getByUserHandler,
            GetPaymentsDashboardQueryHandler dashboardHandler,
            IValidator<InitiatePaymentRequest> initiateValidator,
            IValidator<UpdatePaymentStatusRequest> updateStatusValidator,
            IValidator<RefundPaymentRequest> refundValidator,
            IValidator<GetPaymentByQuotationQuery> getByQuotationValidator,
            IValidator<GetPaymentByIdQuery> getByIdValidator)
        {
            _initiateHandler = initiateHandler;
            _updateStatusHandler = updateStatusHandler;
            _refundHandler = refundHandler;
            _cancelHandler = cancelHandler;
            _getByIdHandler = getByIdHandler;
            _getByQuotationHandler = getByQuotationHandler;
            _getByUserHandler = getByUserHandler;
            _dashboardHandler = dashboardHandler;
            _initiateValidator = initiateValidator;
            _updateStatusValidator = updateStatusValidator;
            _refundValidator = refundValidator;
            _getByQuotationValidator = getByQuotationValidator;
            _getByIdValidator = getByIdValidator;
        }

        [HttpPost("initiate")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
        {
            var validationResult = await _initiateValidator.ValidateAsync(request);
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
                var command = new InitiatePaymentCommand
                {
                    Request = request,
                    InitiatedByUserId = userId
                };

                var result = await _initiateHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while initiating payment" });
            }
        }

        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPayment([FromRoute] Guid paymentId)
        {
            var query = new GetPaymentByIdQuery { PaymentId = paymentId };
            var validationResult = await _getByIdValidator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _getByIdHandler.Handle(query);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("quotations/{quotationId}")]
        [ProducesResponseType(typeof(List<PaymentDto>), 200)]
        public async Task<IActionResult> GetPaymentsByQuotation([FromRoute] Guid quotationId)
        {
            var query = new GetPaymentByQuotationQuery { QuotationId = quotationId };
            var validationResult = await _getByQuotationValidator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _getByQuotationHandler.Handle(query);
            return Ok(result);
        }

        [HttpPost("{paymentId}/refund")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RefundPayment([FromRoute] Guid paymentId, [FromBody] RefundPaymentRequest request)
        {
            var validationResult = await _refundValidator.ValidateAsync(request);
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
                var command = new RefundPaymentCommand
                {
                    PaymentId = paymentId,
                    Request = request,
                    RefundedByUserId = userId
                };

                var result = await _refundHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing refund" });
            }
        }

        [HttpPost("{paymentId}/cancel")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelPayment([FromRoute] Guid paymentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var command = new CancelPaymentCommand
                {
                    PaymentId = paymentId,
                    CancelledByUserId = userId
                };

                var result = await _cancelHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while cancelling payment" });
            }
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(PaymentDashboardDto), 200)]
        public async Task<IActionResult> GetDashboard([FromQuery] Guid? userId, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid? queryUserId = null;

            // If user is not admin, they can only see their own data
            if (User.IsInRole("Admin"))
            {
                queryUserId = userId;
            }
            else if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var currentUserId))
            {
                queryUserId = currentUserId;
            }

            var query = new GetPaymentsDashboardQuery
            {
                UserId = queryUserId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _dashboardHandler.Handle(query);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(PagedResult<PaymentDto>), 200)]
        public async Task<IActionResult> GetPaymentsByUser(
            [FromRoute] Guid userId,
            [FromQuery] string? status,
            [FromQuery] DateTimeOffset? startDate,
            [FromQuery] DateTimeOffset? endDate,
            [FromQuery] Guid? quotationId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            // Authorization: Users can only see their own payments unless they're admin
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!User.IsInRole("Admin"))
            {
                if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId) || currentUserId != userId)
                {
                    return Forbid();
                }
            }

            var query = new GetPaymentsByUserQuery
            {
                UserId = userId,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                QuotationId = quotationId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _getByUserHandler.Handle(query);
            return Ok(result);
        }
    }
}

