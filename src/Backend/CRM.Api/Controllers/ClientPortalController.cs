using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Commands;
using CRM.Application.Quotations.Commands.Handlers;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Queries;
using CRM.Application.Quotations.Queries.Handlers;
using CRM.Application.Quotations.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/client-portal/quotations")]
    [AllowAnonymous]
    public class ClientPortalController : ControllerBase
    {
        private readonly GetQuotationByAccessTokenQueryHandler _getByTokenHandler;
        private readonly SubmitQuotationResponseCommandHandler _responseHandler;
        private readonly MarkQuotationAsViewedCommandHandler _markViewedHandler;
        private readonly IValidator<SubmitQuotationResponseRequest> _responseValidator;
        private readonly IQuotationPdfGenerationService _pdfService;
        private readonly IClientPortalOtpService _otpService;
        private readonly Application.QuotationTemplates.Services.ITemplateProcessingService _templateProcessingService;
        private readonly IAppDbContext _db;
        private readonly Microsoft.Extensions.Logging.ILogger<ClientPortalController> _logger;

        public ClientPortalController(
            GetQuotationByAccessTokenQueryHandler getByTokenHandler,
            SubmitQuotationResponseCommandHandler responseHandler,
            MarkQuotationAsViewedCommandHandler markViewedHandler,
            IValidator<SubmitQuotationResponseRequest> responseValidator,
            IQuotationPdfGenerationService pdfService,
            IClientPortalOtpService otpService,
            Application.QuotationTemplates.Services.ITemplateProcessingService templateProcessingService,
            IAppDbContext db,
            Microsoft.Extensions.Logging.ILogger<ClientPortalController> logger)
        {
            _getByTokenHandler = getByTokenHandler;
            _responseHandler = responseHandler;
            _markViewedHandler = markViewedHandler;
            _responseValidator = responseValidator;
            _pdfService = pdfService;
            _otpService = otpService;
            _templateProcessingService = templateProcessingService;
            _db = db;
            _logger = logger;
        }

        [HttpGet("{quotationId:guid}/{accessToken}/validate")]
        public async Task<IActionResult> ValidateAccessLink(Guid quotationId, string accessToken)
        {
            try
            {
                var link = await _db.QuotationAccessLinks
                    .FirstOrDefaultAsync(l => l.QuotationId == quotationId && l.AccessToken == accessToken);

                if (link == null)
                {
                    return NotFound(new { error = "Access link not found." });
                }

                if (!link.IsActive)
                {
                    return BadRequest(new { error = "Access link is inactive." });
                }

                if (link.IsExpired())
                {
                    return BadRequest(new { error = "Access link has expired." });
                }

                return Ok(new { success = true, clientEmail = link.ClientEmail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{quotationId:guid}/{accessToken}/request-otp")]
        public async Task<IActionResult> RequestOtp(Guid quotationId, string accessToken, [FromBody] RequestOtpRequest request)
        {
            try
            {
                var link = await _db.QuotationAccessLinks
                    .FirstOrDefaultAsync(l => l.QuotationId == quotationId && l.AccessToken == accessToken);

                if (link == null || !link.IsActive || link.IsExpired())
                {
                    return BadRequest(new { error = "Invalid or expired access link." });
                }

                // Verify email matches the link's client email
                if (!string.Equals(link.ClientEmail, request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { error = "Email does not match the quotation recipient." });
                }

                var otp = await _otpService.GenerateOtpAsync(
                    link.AccessLinkId,
                    request.Email,
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new { success = true, message = "OTP sent to your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{quotationId:guid}/{accessToken}/verify-otp")]
        public async Task<IActionResult> VerifyOtp(Guid quotationId, string accessToken, [FromBody] VerifyOtpRequest request)
        {
            try
            {
                var link = await _db.QuotationAccessLinks
                    .FirstOrDefaultAsync(l => l.QuotationId == quotationId && l.AccessToken == accessToken);

                if (link == null || !link.IsActive || link.IsExpired())
                {
                    return BadRequest(new { error = "Invalid or expired access link." });
                }

                var isValid = await _otpService.VerifyOtpAsync(
                    link.AccessLinkId,
                    request.Email,
                    request.OtpCode,
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                if (!isValid)
                {
                    return BadRequest(new { error = "Invalid or expired OTP code." });
                }

                return Ok(new { success = true, message = "OTP verified successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{quotationId:guid}/{accessToken}/start-view")]
        public async Task<IActionResult> StartPageView(Guid quotationId, string accessToken, [FromBody] StartPageViewRequest request)
        {
            try
            {
                var link = await _db.QuotationAccessLinks
                    .FirstOrDefaultAsync(l => l.QuotationId == quotationId && l.AccessToken == accessToken);

                if (link == null || !link.IsActive || link.IsExpired())
                {
                    return BadRequest(new { error = "Invalid or expired access link." });
                }

                var pageView = new CRM.Domain.Entities.QuotationPageView
                {
                    ViewId = Guid.NewGuid(),
                    AccessLinkId = link.AccessLinkId,
                    QuotationId = quotationId,
                    ClientEmail = request.Email ?? link.ClientEmail,
                    ViewStartTime = DateTimeOffset.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _db.QuotationPageViews.Add(pageView);
                await _db.SaveChangesAsync();

                return Ok(new { success = true, viewId = pageView.ViewId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{quotationId:guid}/{accessToken}/end-view")]
        public async Task<IActionResult> EndPageView(Guid quotationId, string accessToken, [FromBody] EndPageViewRequest request)
        {
            try
            {
                var pageView = await _db.QuotationPageViews
                    .FirstOrDefaultAsync(v => v.ViewId == request.ViewId && v.QuotationId == quotationId);

                if (pageView == null)
                {
                    return NotFound(new { error = "Page view not found." });
                }

                pageView.EndView();
                await _db.SaveChangesAsync();

                return Ok(new { success = true, durationSeconds = pageView.DurationSeconds });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{quotationId:guid}/{accessToken}")]
        public async Task<IActionResult> GetQuotation(Guid quotationId, string accessToken)
        {
            try
            {
                var query = new GetQuotationByAccessTokenQuery
                {
                    QuotationId = quotationId,
                    AccessToken = accessToken
                };

                var quotation = await _getByTokenHandler.Handle(query);

            _ = _markViewedHandler.Handle(new MarkQuotationAsViewedCommand
                {
                    AccessToken = accessToken,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return Ok(new { success = true, data = quotation });
            }
            catch (QuotationAccessLinkNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{quotationId:guid}/{accessToken}/download")]
        public async Task<IActionResult> DownloadQuotation(Guid quotationId, string accessToken)
        {
            try
            {
                var quotation = await GetQuotationEntityAsync(quotationId, accessToken);
                var pdfBytes = await _pdfService.GenerateQuotationPdfAsync(quotation);
                var fileName = $"Quotation-{quotation.QuotationNumber}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (QuotationAccessLinkNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{quotationId:guid}/{accessToken}/template-preview")]
        public async Task<IActionResult> GetQuotationTemplatePreview(Guid quotationId, string accessToken)
        {
            try
            {
                var link = await _db.QuotationAccessLinks
                    .Include(l => l.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(l => l.Quotation)
                        .ThenInclude(q => q.CreatedByUser)
                    .Include(l => l.Quotation)
                        .ThenInclude(q => q.LineItems)
                    .FirstOrDefaultAsync(l => l.QuotationId == quotationId && l.AccessToken == accessToken);

                if (link == null || !link.IsActive || link.IsExpired())
                {
                    return NotFound(new { error = "Invalid or expired access link." });
                }

                var quotation = link.Quotation;
                if (quotation == null)
                {
                    return NotFound(new { error = "Quotation not found." });
                }

                if (!quotation.TemplateId.HasValue)
                {
                    return Ok(new { hasTemplate = false, message = "No template applied to this quotation." });
                }

                var template = await _db.QuotationTemplates
                    .FirstOrDefaultAsync(t => t.TemplateId == quotation.TemplateId.Value);

                if (template == null || !template.IsFileBased)
                {
                    return Ok(new { hasTemplate = false, message = "Applied template not found or is not file-based." });
                }

                var htmlContent = await _templateProcessingService.ProcessTemplateToHtmlAsync(template, quotation);

                return Ok(new { hasTemplate = true, templateName = template.Name, content = htmlContent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client portal template preview for quotation {QuotationId}", quotationId);
                return StatusCode(500, new { error = "Failed to get template preview", details = ex.Message });
            }
        }

        [HttpPost("{quotationId:guid}/{accessToken}/respond")]
        public async Task<IActionResult> SubmitResponse(Guid quotationId, string accessToken, [FromBody] SubmitQuotationResponseRequest request)
        {
            var validation = await _responseValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return BadRequest(new { errors = validation.Errors });
            }

            try
            {
                var command = new SubmitQuotationResponseCommand
                {
                    AccessToken = accessToken,
                    Request = request,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                var result = await _responseHandler.Handle(command);

                return Ok(new { success = true, message = "Response recorded successfully", data = result });
            }
            catch (QuotationAccessLinkNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Check if it's a missing table error
                if (ex.Message.Contains("42P01") || ex.Message.Contains("does not exist") || ex.Message.Contains("table") && ex.Message.Contains("missing"))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator to run migrations." });
                }
                return BadRequest(new { error = ex.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                // Check for missing table
                if (innerMessage.Contains("42P01") || innerMessage.Contains("does not exist") || innerMessage.Contains("QuotationResponses"))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator to run migrations." });
                }
                // Check for foreign key violations
                if (innerMessage.Contains("foreign key") || innerMessage.Contains("violates foreign key"))
                {
                    return BadRequest(new { error = "Invalid quotation or related data not found." });
                }
                return StatusCode(500, new { error = $"Database error: {innerMessage}" });
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = $"An unexpected error occurred: {innerMessage}" });
            }
        }
        private async Task<CRM.Domain.Entities.Quotation> GetQuotationEntityAsync(Guid quotationId, string accessToken)
        {
            var link = await _db.QuotationAccessLinks
                .Include(l => l.Quotation)
                    .ThenInclude(q => q.Client)
                .Include(l => l.Quotation)
                    .ThenInclude(q => q.LineItems)
                .FirstOrDefaultAsync(l => l.QuotationId == quotationId && l.AccessToken == accessToken);

            if (link == null)
            {
                throw new QuotationAccessLinkNotFoundException();
            }

            if (!link.IsActive || link.IsExpired())
            {
                throw new InvalidOperationException("Access link is inactive or expired.");
            }

            return link.Quotation ?? throw new QuotationNotFoundException(quotationId);
        }
    }

    public class RequestOtpRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }

    public class StartPageViewRequest
    {
        public string? Email { get; set; }
    }

    public class EndPageViewRequest
    {
        public Guid ViewId { get; set; }
    }
}


