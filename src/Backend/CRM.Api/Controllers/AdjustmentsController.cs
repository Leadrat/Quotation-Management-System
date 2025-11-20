using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
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
    [Route("api/v1/adjustments")]
    [Authorize]
    public class AdjustmentsController : ControllerBase
    {
        private readonly InitiateAdjustmentCommandHandler _initiateHandler;
        private readonly ApproveAdjustmentCommandHandler _approveHandler;
        private readonly RejectAdjustmentCommandHandler _rejectHandler;
        private readonly ApplyAdjustmentCommandHandler _applyHandler;
        private readonly GetAdjustmentsByQuotationQueryHandler _getByQuotationHandler;
        private readonly GetPendingAdjustmentsQueryHandler _getPendingHandler;
        private readonly IValidator<CreateAdjustmentRequest> _createValidator;

        public AdjustmentsController(
            InitiateAdjustmentCommandHandler initiateHandler,
            ApproveAdjustmentCommandHandler approveHandler,
            RejectAdjustmentCommandHandler rejectHandler,
            ApplyAdjustmentCommandHandler applyHandler,
            GetAdjustmentsByQuotationQueryHandler getByQuotationHandler,
            GetPendingAdjustmentsQueryHandler getPendingHandler,
            IValidator<CreateAdjustmentRequest> createValidator)
        {
            _initiateHandler = initiateHandler;
            _approveHandler = approveHandler;
            _rejectHandler = rejectHandler;
            _applyHandler = applyHandler;
            _getByQuotationHandler = getByQuotationHandler;
            _getPendingHandler = getPendingHandler;
            _createValidator = createValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AdjustmentDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateAdjustment([FromBody] CreateAdjustmentRequest request)
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
                var command = new InitiateAdjustmentCommand
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

        [HttpGet("{adjustmentId}")]
        [ProducesResponseType(typeof(AdjustmentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAdjustmentById(Guid adjustmentId)
        {
            // TODO: Implement GetAdjustmentByIdQueryHandler if needed
            return NotFound();
        }

        [HttpPost("{adjustmentId}/approve")]
        [ProducesResponseType(typeof(AdjustmentDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApproveAdjustment(Guid adjustmentId, [FromBody] ApproveAdjustmentRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var command = new ApproveAdjustmentCommand
                {
                    AdjustmentId = adjustmentId,
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

        [HttpPost("{adjustmentId}/apply")]
        [ProducesResponseType(typeof(AdjustmentDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApplyAdjustment(Guid adjustmentId)
        {
            try
            {
                var command = new ApplyAdjustmentCommand { AdjustmentId = adjustmentId };
                var result = await _applyHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("quotation/{quotationId}")]
        [ProducesResponseType(typeof(List<AdjustmentDto>), 200)]
        public async Task<IActionResult> GetAdjustmentsByQuotation(Guid quotationId)
        {
            try
            {
                var query = new GetAdjustmentsByQuotationQuery { QuotationId = quotationId };
                var result = await _getByQuotationHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("pending")]
        [ProducesResponseType(typeof(List<AdjustmentDto>), 200)]
        public async Task<IActionResult> GetPendingAdjustments([FromQuery] string? approvalLevel)
        {
            var query = new GetPendingAdjustmentsQuery { ApprovalLevel = approvalLevel };
            var result = await _getPendingHandler.Handle(query);
            return Ok(result);
        }
    }
}

