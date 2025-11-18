using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Commands;
using CRM.Application.Reports.Commands.Handlers;
using CRM.Application.Reports.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/reports/scheduled")]
    [Authorize]
    public class ScheduledReportsController : ControllerBase
    {
        private readonly ScheduleReportCommandHandler _scheduleHandler;
        private readonly CancelScheduledReportCommandHandler _cancelHandler;
        private readonly IAppDbContext _db;
        private readonly IValidator<ScheduleReportRequest> _scheduleValidator;

        public ScheduledReportsController(
            ScheduleReportCommandHandler scheduleHandler,
            CancelScheduledReportCommandHandler cancelHandler,
            IAppDbContext db,
            IValidator<ScheduleReportRequest> scheduleValidator)
        {
            _scheduleHandler = scheduleHandler;
            _cancelHandler = cancelHandler;
            _db = db;
            _scheduleValidator = scheduleValidator;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateScheduledReport([FromBody] ScheduleReportRequest request)
        {
            var validation = await _scheduleValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return BadRequest(new { success = false, errors = validation.ToDictionary() });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new ScheduleReportCommand
            {
                Request = request,
                UserId = userId
            };

            var result = await _scheduleHandler.Handle(command);
            return Ok(new { success = true, data = result });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetScheduledReports()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var role = User.FindFirstValue("role") ?? string.Empty;
            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            var query = _db.ScheduledReports.AsQueryable();
            if (!isAdmin)
            {
                query = query.Where(r => r.CreatedByUserId == userId);
            }

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReportId,
                    r.ReportName,
                    r.ReportType,
                    r.RecurrencePattern,
                    r.EmailRecipients,
                    r.IsActive,
                    r.LastSentAt,
                    r.NextScheduledAt,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = reports });
        }

        [HttpDelete("{reportId}")]
        [Authorize]
        public async Task<IActionResult> DeleteScheduledReport([FromRoute] Guid reportId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new CancelScheduledReportCommand
            {
                ReportId = reportId,
                UserId = userId
            };

            await _cancelHandler.Handle(command);
            return Ok(new { success = true, message = "Scheduled report cancelled" });
        }

        [HttpPost("send-test")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendTestEmail(
            [FromBody] Dictionary<string, object> request)
        {
            // TODO: Implement test email sending
            return Ok(new { success = true, message = "Test email sent" });
        }
    }
}

