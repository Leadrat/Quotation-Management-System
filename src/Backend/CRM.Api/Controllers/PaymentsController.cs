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
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Interfaces;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Application.Payments.Services;
using Microsoft.EntityFrameworkCore;

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
        private readonly IAppDbContext _db;
        private readonly PaymentDomainService _paymentDomain;
        private readonly ITenantContext _tenantContext;

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
            IValidator<GetPaymentByIdQuery> getByIdValidator,
            IAppDbContext db,
            PaymentDomainService paymentDomain,
            ITenantContext tenantContext)
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
            _db = db;
            _paymentDomain = paymentDomain;
            _tenantContext = tenantContext;
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

        // Spec 028 US2: Payments list with filters
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> List(
            [FromQuery] string? status,
            [FromQuery] DateTimeOffset? startDate,
            [FromQuery] DateTimeOffset? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var actualPageSize = pageNumber < 1 ? 10 : pageNumber;
            if (actualPageSize < 1 || actualPageSize > 200) actualPageSize = 20;

            var currentTenantId = _tenantContext.CurrentTenantId;
            var query = _db.Payments.Where(p => p.TenantId == currentTenantId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var st))
            {
                query = query.Where(p => p.PaymentStatus == st);
            }
            if (startDate.HasValue)
            {
                query = query.Where(p => (p.PaymentDate ?? p.CreatedAt) >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => (p.PaymentDate ?? p.CreatedAt) <= endDate.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    QuotationId = p.QuotationId,
                    PaymentGateway = p.PaymentGateway,
                    PaymentReference = p.PaymentReference,
                    AmountPaid = p.AmountPaid,
                    Currency = p.Currency,
                    PaymentStatus = p.PaymentStatus,
                    PaymentDate = p.PaymentDate,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    FailureReason = p.FailureReason,
                    IsRefundable = p.IsRefundable,
                    RefundAmount = p.RefundAmount,
                    RefundReason = p.RefundReason,
                    RefundDate = p.RefundDate,
                    CanBeRefunded = p.CanBeRefunded(),
                    CanBeCancelled = p.CanBeCancelled(),
                    PaymentUrl = null,
                    ClientSecret = null
                })
                .ToListAsync();

            return Ok(new { success = true, data = items, pageNumber, pageSize, totalCount });
        }

        // Spec 028 US3: Payments stats summary
        [HttpGet("stats")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Stats([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
        {
            var currentTenantId = _tenantContext.CurrentTenantId;
            var query = _db.Payments.Where(p => p.TenantId == currentTenantId).AsQueryable();
            if (startDate.HasValue)
            {
                query = query.Where(p => (p.PaymentDate ?? p.CreatedAt) >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => (p.PaymentDate ?? p.CreatedAt) <= endDate.Value);
            }

            var grouped = await query
                .GroupBy(p => p.PaymentStatus)
                .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(x => x.AmountPaid) })
                .ToListAsync();

            // Pending amount based on payment status (legacy metric)
            var summary = new
            {
                pendingCount = grouped.Where(x => x.Status == PaymentStatus.Pending).Sum(x => x.Count),
                paidCount = grouped.Where(x => x.Status == PaymentStatus.Success).Sum(x => x.Count),
                refundedCount = grouped.Where(x => x.Status == PaymentStatus.Refunded || x.Status == PaymentStatus.PartiallyRefunded).Sum(x => x.Count),
                failedCount = grouped.Where(x => x.Status == PaymentStatus.Failed).Sum(x => x.Count),
                totalPending = grouped.Where(x => x.Status == PaymentStatus.Pending).Sum(x => x.Total),
                totalPaid = grouped.Where(x => x.Status == PaymentStatus.Success).Sum(x => x.Total),
                totalRefunded = grouped.Where(x => x.Status == PaymentStatus.Refunded || x.Status == PaymentStatus.PartiallyRefunded).Sum(x => x.Total),
                totalFailed = grouped.Where(x => x.Status == PaymentStatus.Failed).Sum(x => x.Total),
            };

            // Pending amount across accepted quotations only
            var acceptedQuotations = await _db.Quotations
                .Where(q => q.Status == QuotationStatus.Accepted && q.TenantId == currentTenantId)
                .Select(q => new { q.QuotationId, q.TotalAmount })
                .ToListAsync();

            var netByQuotation = await _db.Payments
                .Where(p => acceptedQuotations.Select(aq => aq.QuotationId).Contains(p.QuotationId)
                            && (p.PaymentStatus == PaymentStatus.Success
                                || p.PaymentStatus == PaymentStatus.PartiallyRefunded
                                || p.PaymentStatus == PaymentStatus.Refunded))
                .GroupBy(p => p.QuotationId)
                .Select(g => new
                {
                    QuotationId = g.Key,
                    NetPaid = g.Sum(x => x.AmountPaid - (x.RefundAmount ?? 0m))
                })
                .ToListAsync();

            var netLookup = netByQuotation.ToDictionary(x => x.QuotationId, x => x.NetPaid);
            decimal acceptedPendingAmount = 0m;
            int acceptedPendingCount = 0;
            foreach (var q in acceptedQuotations)
            {
                netLookup.TryGetValue(q.QuotationId, out var netPaid);
                var outstanding = q.TotalAmount - netPaid;
                if (outstanding > 0)
                {
                    acceptedPendingAmount += outstanding;
                    acceptedPendingCount++;
                }
            };

            var statusCounts = grouped.Select(x => new { status = x.Status.ToString(), count = x.Count, totalAmount = x.Total });
            return Ok(new
            {
                success = true,
                summary,
                statusCounts,
                acceptedPending = new
                {
                    amount = acceptedPendingAmount,
                    quotationCount = acceptedPendingCount
                }
            });
        }

        // Spec 028 Phase 6: Quotation payment history
        [HttpGet("/api/v1/quotations/{quotationId}/payments/history")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> QuotationPaymentHistory([FromRoute] Guid quotationId)
        {
            // Re-enable tenant filtering with correct tenant ID
            var currentTenantId = _tenantContext.CurrentTenantId;
            var exists = await _db.Quotations.AnyAsync(q => q.QuotationId == quotationId && q.TenantId == currentTenantId);
            if (!exists) return NotFound(new { error = "Quotation not found" });

            var items = await _db.Payments
                .Where(p => p.QuotationId == quotationId && p.TenantId == currentTenantId)
                .OrderByDescending(p => p.PaymentDate ?? p.CreatedAt)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    QuotationId = p.QuotationId,
                    PaymentGateway = p.PaymentGateway,
                    PaymentReference = p.PaymentReference,
                    AmountPaid = p.AmountPaid,
                    Currency = p.Currency,
                    PaymentStatus = p.PaymentStatus,
                    PaymentDate = p.PaymentDate,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    FailureReason = p.FailureReason,
                    IsRefundable = p.IsRefundable,
                    RefundAmount = p.RefundAmount,
                    RefundReason = p.RefundReason,
                    RefundDate = p.RefundDate,
                    CanBeRefunded = p.CanBeRefunded(),
                    CanBeCancelled = p.CanBeCancelled(),
                    PaymentUrl = null,
                    ClientSecret = null
                })
                .ToListAsync();

            // Debug logging for payment retrieval
            System.Diagnostics.Debug.WriteLine($"QuotationPaymentHistory: QuotationId={quotationId}, Found {items.Count} payments");
            foreach (var payment in items.Take(3)) // Log first 3 payments
            {
                System.Diagnostics.Debug.WriteLine($"Payment: Id={payment.PaymentId}, Amount={payment.AmountPaid}, Status={payment.PaymentStatus}");
            }

            return Ok(new { success = true, data = items });
        }

        // GET endpoint for fetching payments by quotation ID (alias for history)
        [HttpGet("/api/v1/quotations/{quotationId}/payments")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetQuotationPayments([FromRoute] Guid quotationId)
        {
            // This is an alias for the history endpoint - same logic
            return await QuotationPaymentHistory(quotationId);
        }

        // Spec 028 helper: Outstanding payment summary for a quotation
        [HttpGet("/api/v1/quotations/{quotationId}/payments/outstanding")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetOutstandingSummary([FromRoute] Guid quotationId)
        {
            // Re-enable tenant filtering with correct tenant ID
            var currentTenantId = _tenantContext.CurrentTenantId;
            var quotation = await _db.Quotations.AsNoTracking().FirstOrDefaultAsync(q => q.QuotationId == quotationId && q.TenantId == currentTenantId);
            if (quotation == null) return NotFound(new { error = "Quotation not found" });

            var outstanding = await _paymentDomain.GetOutstandingAsync(quotationId);

            // Recompute paidNet as Total - Outstanding for consistency with domain
            var paidNet = quotation.TotalAmount - outstanding;
            if (paidNet < 0) paidNet = 0;

            return Ok(new
            {
                success = true,
                data = new
                {
                    quotationId = quotation.QuotationId,
                    totalAmount = quotation.TotalAmount,
                    paidNet,
                    outstanding
                }
            });
        }

        // Spec 028 US1: Record Manual Payment
        [HttpPost("/api/v1/quotations/{quotationId}/payments")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        [ProducesResponseType(typeof(PaymentDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateManualPayment([FromRoute] Guid quotationId, [FromBody] CRM.Application.Payments.Dtos.CreateManualPaymentRequest request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required" });
            if (request.AmountReceived <= 0) return BadRequest(new { error = "AmountReceived must be greater than zero" });

            // Ensure path quotationId matches body if provided
            request.QuotationId = quotationId;

            // Re-enable tenant filtering with correct tenant ID
            var currentTenantId = _tenantContext.CurrentTenantId;
            var quotation = await _db.Quotations.FirstOrDefaultAsync(q => q.QuotationId == quotationId && q.TenantId == currentTenantId);
            if (quotation == null) return NotFound(new { error = "Quotation not found" });

            // Overpayment guard
            var (ok, error) = await _paymentDomain.ValidateManualPaymentAsync(quotationId, request.AmountReceived);
            if (!ok)
            {
                return BadRequest(new { error });
            }

            // Create manual payment row
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                TenantId = currentTenantId, // Use correct tenant ID
                QuotationId = quotationId,
                PaymentGateway = "Manual",
                PaymentReference = Guid.NewGuid().ToString(),
                AmountPaid = request.AmountReceived,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency,
                PaymentStatus = PaymentStatus.Success,
                PaymentDate = request.PaymentDate == default ? DateTimeOffset.UtcNow : request.PaymentDate,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Metadata = string.IsNullOrWhiteSpace(request.Remarks) ? null : System.Text.Json.JsonSerializer.Serialize(new { Remarks = request.Remarks, Method = request.Method })
            };

            await _db.Payments.AddAsync(payment);
            await _db.SaveChangesAsync();

            var dto = new PaymentDto
            {
                PaymentId = payment.PaymentId,
                QuotationId = payment.QuotationId,
                PaymentGateway = payment.PaymentGateway,
                PaymentReference = payment.PaymentReference,
                AmountPaid = payment.AmountPaid,
                Currency = payment.Currency,
                PaymentStatus = payment.PaymentStatus,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                IsRefundable = payment.IsRefundable,
                CanBeRefunded = payment.CanBeRefunded(),
                CanBeCancelled = payment.CanBeCancelled(),
                PaymentUrl = null,
                ClientSecret = null
            };

            return Created($"/api/v1/payments/{payment.PaymentId}", dto);
        }

        // Spec 028 US1: Update Manual Payment
        [HttpPut("/api/v1/payments/{paymentId}")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateManualPayment([FromRoute] Guid paymentId, [FromBody] CRM.Application.Payments.Dtos.CreateManualPaymentRequest request)
        {
            var currentTenantId = _tenantContext.CurrentTenantId;
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.TenantId == currentTenantId);
            if (payment == null) return NotFound();

            if (request.AmountReceived <= 0) return BadRequest(new { error = "AmountReceived must be greater than zero" });

            // Overpayment guard for update
            var (ok, error) = await _paymentDomain.ValidateManualPaymentUpdateAsync(paymentId, request.AmountReceived);
            if (!ok)
            {
                return BadRequest(new { error });
            }

            // Debug logging before update
            System.Diagnostics.Debug.WriteLine($"UpdateManualPayment: Before update - PaymentId={paymentId}, OldAmount={payment.AmountPaid}, NewAmount={request.AmountReceived}");

            payment.AmountPaid = request.AmountReceived;
            payment.Currency = string.IsNullOrWhiteSpace(request.Currency) ? payment.Currency : request.Currency;
            payment.PaymentDate = request.PaymentDate == default ? payment.PaymentDate : request.PaymentDate;
            payment.PaymentGateway = "Manual";
            payment.PaymentStatus = PaymentStatus.Success;
            payment.UpdatedAt = DateTimeOffset.UtcNow;
            payment.Metadata = string.IsNullOrWhiteSpace(request.Remarks) ? payment.Metadata : System.Text.Json.JsonSerializer.Serialize(new { Remarks = request.Remarks, Method = request.Method });

            await _db.SaveChangesAsync();

            // Debug logging after update
            System.Diagnostics.Debug.WriteLine($"UpdateManualPayment: After update - PaymentId={paymentId}, NewAmount={payment.AmountPaid}, Status={payment.PaymentStatus}");

            var dto = new PaymentDto
            {
                PaymentId = payment.PaymentId,
                QuotationId = payment.QuotationId,
                PaymentGateway = payment.PaymentGateway,
                PaymentReference = payment.PaymentReference,
                AmountPaid = payment.AmountPaid,
                Currency = payment.Currency,
                PaymentStatus = payment.PaymentStatus,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                IsRefundable = payment.IsRefundable,
                CanBeRefunded = payment.CanBeRefunded(),
                CanBeCancelled = payment.CanBeCancelled(),
                PaymentUrl = null,
                ClientSecret = null
            };
            return Ok(dto);
        }

        // Spec 028: Delete manual payment
        [HttpDelete("/api/v1/payments/{paymentId}")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteManualPayment([FromRoute] Guid paymentId)
        {
            var currentTenantId = _tenantContext.CurrentTenantId;
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.TenantId == currentTenantId);
            if (payment == null) return NotFound();
            if (!string.Equals(payment.PaymentGateway, "Manual", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Only manual payments can be deleted" });
            }

            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("quotations/{quotationId}")]
        [ProducesResponseType(typeof(List<PaymentDto>), 200)]
        public async Task<IActionResult> GetPaymentsByQuotation([FromRoute] Guid quotationId)
        {
            var currentTenantId = _tenantContext.CurrentTenantId;
            var quotation = await _db.Quotations.FirstOrDefaultAsync(q => q.QuotationId == quotationId && q.TenantId == currentTenantId);
            if (quotation == null) return NotFound(new { error = "Quotation not found or not accessible in current tenant" });

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

