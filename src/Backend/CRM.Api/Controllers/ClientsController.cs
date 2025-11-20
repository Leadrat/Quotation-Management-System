using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Commands;
using CRM.Application.Clients.Commands.Handlers;
using CRM.Application.Clients.Validators;
using CRM.Application.Clients.Queries;
using CRM.Application.Clients.Queries.Handlers;
using CRM.Application.Common.Results;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IMapper _mapper;

        public ClientsController(AppDbContext db, IAuditLogger audit, IMapper mapper)
        {
            _db = db;
            _audit = audit;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateClientRequest body)
        {
            try
            {
                var validator = new CreateClientRequestValidator();
                var result = validator.Validate(body);
                if (!result.IsValid)
                {
                    return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
                }

                // Try multiple claim types to find user ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue("userId");

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
                {
                    return Unauthorized(new { success = false, error = "Invalid user token - user ID not found" });
                }

                await _audit.LogAsync("client_create_attempt", new { userId, body.Email });

                var cmd = new CreateClientCommand
                {
                    CompanyName = body.CompanyName,
                    ContactName = body.ContactName,
                    Email = body.Email,
                    Mobile = body.Mobile,
                    PhoneCode = body.PhoneCode,
                    Gstin = body.Gstin,
                    StateCode = body.StateCode,
                    Address = body.Address,
                    City = body.City,
                    State = body.State,
                    PinCode = body.PinCode,
                    CreatedByUserId = userId
                };

                var handler = new CreateClientCommandHandler(_db, _mapper);
                var created = await handler.Handle(cmd);

                await _audit.LogAsync("client_create_success", new { userId, created.ClientId });
                return StatusCode(201, new { success = true, message = "Client created successfully", data = created });
            }
            catch (CRM.Application.Clients.Exceptions.DuplicateEmailException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist") || ex.Message.Contains("constraint violation") || ex.Message.Contains("Database error"))
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Handle database constraint violations
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                if (innerException.Contains("foreign key") || innerException.Contains("FK_") || innerException.Contains("violates foreign key constraint"))
                {
                    return BadRequest(new { success = false, error = "Invalid user reference. Please ensure you are properly authenticated.", details = innerException });
                }
                return StatusCode(500, new { success = false, error = "Database error occurred while creating client", message = innerException });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while creating client", message = ex.Message, stackTrace = System.Diagnostics.Debugger.IsAttached ? ex.StackTrace : null });
            }
        }

        [HttpGet]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> List([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] Guid? userId = null)
        {
            try
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
            var role = User.FindFirstValue("role") ?? string.Empty;

            var handler = new GetAllClientsQueryHandler(_db, _mapper);
            var result = await handler.Handle(new GetAllClientsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                CreatedByUserId = userId,
                RequestorUserId = requestorId,
                RequestorRole = role
            });
            return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving clients", message = ex.Message });
            }
        }

        [HttpGet("{clientId}")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> GetById([FromRoute] Guid clientId)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
            var role = User.FindFirstValue("role") ?? string.Empty;

            var handler = new GetClientByIdQueryHandler(_db, _mapper);
            try
            {
                var dto = await handler.Handle(new GetClientByIdQuery
                {
                    ClientId = clientId,
                    RequestorUserId = requestorId,
                    RequestorRole = role
                });
                return Ok(new { success = true, data = dto });
            }
            catch (CRM.Application.Clients.Exceptions.ClientNotFoundException)
            {
                return NotFound(new { success = false, error = "Client not found" });
            }
            catch
            {
                return Forbid();
            }
        }

        [HttpPut("{clientId}")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> Update([FromRoute] Guid clientId, [FromBody] UpdateClientRequest body)
        {
            var validator = new UpdateClientRequestValidator();
            var result = validator.Validate(body);
            if (!result.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
            }

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
            var role = User.FindFirstValue("role") ?? string.Empty;

            await _audit.LogAsync("client_update_attempt", new { userId, clientId });

            var cmd = new UpdateClientCommand
            {
                ClientId = clientId,
                CompanyName = body.CompanyName,
                ContactName = body.ContactName,
                Email = body.Email,
                Mobile = body.Mobile,
                PhoneCode = body.PhoneCode,
                Gstin = body.Gstin,
                StateCode = body.StateCode,
                Address = body.Address,
                City = body.City,
                State = body.State,
                PinCode = body.PinCode,
                UpdatedByUserId = userId,
                RequestorRole = role
            };

            var handler = new UpdateClientCommandHandler(_db, _mapper);
            try
            {
                var dto = await handler.Handle(cmd);
                await _audit.LogAsync("client_update_success", new { userId, clientId });
                return Ok(new { success = true, data = dto });
            }
            catch (CRM.Application.Clients.Exceptions.DuplicateEmailException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch (CRM.Application.Clients.Exceptions.ClientNotFoundException)
            {
                return NotFound(new { success = false, error = "Client not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { success = false, error = "Cannot update other user's client" });
            }
        }

        [HttpDelete("{clientId}")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid clientId)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
            var role = User.FindFirstValue("role") ?? string.Empty;

            await _audit.LogAsync("client_delete_attempt", new { userId, clientId });

            var handler = new DeleteClientCommandHandler(_db);
            try
            {
                var result = await handler.Handle(new DeleteClientCommand
                {
                    ClientId = clientId,
                    DeletedByUserId = userId,
                    RequestorRole = role
                });
                await _audit.LogAsync("client_delete_success", new { userId, clientId });
                return Ok(result);
            }
            catch (CRM.Application.Clients.Exceptions.ClientNotFoundException)
            {
                return NotFound(new { success = false, error = "Client not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { success = false, error = "Cannot delete other user's client" });
            }
        }
    }
}
