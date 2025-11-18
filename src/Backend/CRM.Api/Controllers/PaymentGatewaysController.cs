using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [Route("api/v1/payment-gateways")]
    [Authorize(Roles = "Admin")]
    public class PaymentGatewaysController : ControllerBase
    {
        private readonly CreatePaymentGatewayConfigCommandHandler _createHandler;
        private readonly UpdatePaymentGatewayConfigCommandHandler _updateHandler;
        private readonly DeletePaymentGatewayConfigCommandHandler _deleteHandler;
        private readonly GetPaymentGatewayConfigQueryHandler _getHandler;
        private readonly IValidator<CreatePaymentGatewayConfigRequest> _createValidator;
        private readonly IValidator<UpdatePaymentGatewayConfigRequest> _updateValidator;

        public PaymentGatewaysController(
            CreatePaymentGatewayConfigCommandHandler createHandler,
            UpdatePaymentGatewayConfigCommandHandler updateHandler,
            DeletePaymentGatewayConfigCommandHandler deleteHandler,
            GetPaymentGatewayConfigQueryHandler getHandler,
            IValidator<CreatePaymentGatewayConfigRequest> createValidator,
            IValidator<UpdatePaymentGatewayConfigRequest> updateValidator)
        {
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
            _getHandler = getHandler;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpPost("config")]
        [ProducesResponseType(typeof(PaymentGatewayConfigDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateGatewayConfig([FromBody] CreatePaymentGatewayConfigRequest request)
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
                var command = new CreatePaymentGatewayConfigCommand
                {
                    Request = request,
                    CreatedByUserId = userId
                };

                var result = await _createHandler.Handle(command);
                return Created($"/api/v1/payment-gateways/config/{result.ConfigId}", result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating gateway config" });
            }
        }

        [HttpPut("config/{configId}")]
        [ProducesResponseType(typeof(PaymentGatewayConfigDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateGatewayConfig([FromRoute] Guid configId, [FromBody] UpdatePaymentGatewayConfigRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
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
                var command = new UpdatePaymentGatewayConfigCommand
                {
                    ConfigId = configId,
                    Request = request,
                    UpdatedByUserId = userId
                };

                var result = await _updateHandler.Handle(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating gateway config" });
            }
        }

        [HttpDelete("config/{configId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteGatewayConfig([FromRoute] Guid configId)
        {
            try
            {
                var command = new DeletePaymentGatewayConfigCommand
                {
                    ConfigId = configId
                };

                await _deleteHandler.Handle(command);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting gateway config" });
            }
        }

        [HttpGet("config")]
        [ProducesResponseType(typeof(List<PaymentGatewayConfigDto>), 200)]
        public async Task<IActionResult> GetGatewayConfigs([FromQuery] Guid? companyId)
        {
            var query = new GetPaymentGatewayConfigQuery
            {
                CompanyId = companyId
            };

            var result = await _getHandler.Handle(query);
            return Ok(result);
        }
    }
}

