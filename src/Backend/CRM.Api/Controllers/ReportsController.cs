using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.Reports.Commands;
using CRM.Application.Reports.Commands.Handlers;
using CRM.Application.Reports.Dtos;
using CRM.Application.Reports.Queries;
using CRM.Application.Reports.Queries.Handlers;
using CRM.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly GetSalesDashboardMetricsQueryHandler _salesDashboardHandler;
        private readonly GetTeamPerformanceMetricsQueryHandler _teamPerformanceHandler;
        private readonly GetApprovalWorkflowMetricsQueryHandler _approvalMetricsHandler;
        private readonly GetDiscountAnalyticsQueryHandler _discountAnalyticsHandler;
        private readonly GetPaymentAnalyticsQueryHandler _paymentAnalyticsHandler;
        private readonly GetClientEngagementMetricsQueryHandler _clientEngagementHandler;
        private readonly GenerateCustomReportQueryHandler _customReportHandler;
        private readonly GetForecastingDataQueryHandler _forecastingHandler;
        private readonly GetAuditComplianceReportQueryHandler _auditHandler;
        private readonly ExportReportCommandHandler _exportHandler;
        private readonly IAppDbContext _db;

        public ReportsController(
            GetSalesDashboardMetricsQueryHandler salesDashboardHandler,
            GetTeamPerformanceMetricsQueryHandler teamPerformanceHandler,
            GetApprovalWorkflowMetricsQueryHandler approvalMetricsHandler,
            GetDiscountAnalyticsQueryHandler discountAnalyticsHandler,
            GetPaymentAnalyticsQueryHandler paymentAnalyticsHandler,
            GetClientEngagementMetricsQueryHandler clientEngagementHandler,
            GenerateCustomReportQueryHandler customReportHandler,
            GetForecastingDataQueryHandler forecastingHandler,
            GetAuditComplianceReportQueryHandler auditHandler,
            ExportReportCommandHandler exportHandler,
            IAppDbContext db)
        {
            _salesDashboardHandler = salesDashboardHandler;
            _teamPerformanceHandler = teamPerformanceHandler;
            _approvalMetricsHandler = approvalMetricsHandler;
            _discountAnalyticsHandler = discountAnalyticsHandler;
            _paymentAnalyticsHandler = paymentAnalyticsHandler;
            _clientEngagementHandler = clientEngagementHandler;
            _customReportHandler = customReportHandler;
            _forecastingHandler = forecastingHandler;
            _auditHandler = auditHandler;
            _exportHandler = exportHandler;
            _db = db;
        }

        [HttpGet("dashboard/sales")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetSalesDashboard(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var query = new GetSalesDashboardMetricsQuery
            {
                UserId = userId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _salesDashboardHandler.Handle(query);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("dashboard/manager")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetManagerDashboard(
            [FromQuery] Guid? teamId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var managerId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            // Get team performance metrics
            var teamQuery = new GetTeamPerformanceMetricsQuery
            {
                TeamId = teamId ?? managerId,
                FromDate = fromDate,
                ToDate = toDate
            };
            var teamMetrics = await _teamPerformanceHandler.Handle(teamQuery);

            // Get approval metrics
            var approvalQuery = new GetApprovalWorkflowMetricsQuery
            {
                ManagerId = managerId,
                FromDate = fromDate,
                ToDate = toDate
            };
            var approvalMetrics = await _approvalMetricsHandler.Handle(approvalQuery);

            // Get discount analytics
            var discountQuery = new GetDiscountAnalyticsQuery
            {
                TeamId = teamId ?? managerId,
                FromDate = fromDate,
                ToDate = toDate
            };
            var discountAnalytics = await _discountAnalyticsHandler.Handle(discountQuery);

            return Ok(new
            {
                success = true,
                data = new
                {
                    TeamPerformance = teamMetrics,
                    ApprovalMetrics = approvalMetrics,
                    DiscountAnalytics = discountAnalytics
                }
            });
        }

        [HttpGet("dashboard/finance")]
        [Authorize(Roles = "Finance,Admin")]
        public async Task<IActionResult> GetFinanceDashboard(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = new GetPaymentAnalyticsQuery
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _paymentAnalyticsHandler.Handle(query);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("dashboard/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            // Get all metrics for admin overview
            var teamMetrics = await _teamPerformanceHandler.Handle(new GetTeamPerformanceMetricsQuery());
            var paymentAnalytics = await _paymentAnalyticsHandler.Handle(new GetPaymentAnalyticsQuery());
            var approvalMetrics = await _approvalMetricsHandler.Handle(new GetApprovalWorkflowMetricsQuery());

            var activeUsers = await _db.Users.CountAsync(u => u.IsActive);
            var totalQuotations = await _db.Quotations.CountAsync();
            var totalRevenue = await _db.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Success)
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0;

            // Get role counts (simplified - would need proper role lookup)
            // Note: Role entity structure may vary - using RoleId for now
            var activeSalesReps = await _db.Users
                .CountAsync(u => u.IsActive); // Simplified - would need proper role filtering

            var activeManagers = await _db.Users
                .CountAsync(u => u.IsActive); // Simplified - would need proper role filtering

            return Ok(new
            {
                success = true,
                data = new AdminDashboardMetricsDto
                {
                    ActiveUsers = activeUsers,
                    ActiveSalesReps = activeSalesReps,
                    ActiveManagers = activeManagers,
                    TotalQuotationsLifetime = totalQuotations,
                    TotalRevenue = totalRevenue,
                    SystemHealth = new SystemHealthData
                    {
                        ErrorCount = 0, // Would come from error logging system
                        ApiUptime = 99.9m,
                        DatabaseSizeMB = 0, // Would be calculated
                        AverageResponseTimeMs = 0 // Would be calculated
                    },
                    GrowthChart = new List<GrowthData>(),
                    UsageChart = new List<UsageData>()
                }
            });
        }

        [HttpGet("custom")]
        [Authorize]
        public async Task<IActionResult> GetCustomReport(
            [FromQuery] string reportType,
            [FromQuery] string? filters = null,
            [FromQuery] string? groupBy = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] int? limit = null)
        {
            var query = new GenerateCustomReportQuery
            {
                ReportType = reportType,
                Filters = filters != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(filters) : null,
                GroupBy = groupBy,
                SortBy = sortBy,
                Limit = limit
            };

            var result = await _customReportHandler.Handle(query);
            return Ok(new { success = true, data = result });
        }

        [HttpPost("export")]
        [Authorize]
        public async Task<IActionResult> ExportReport([FromBody] ExportReportRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new ExportReportCommand
            {
                Request = request,
                UserId = userId
            };

            var result = await _exportHandler.Handle(command);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("export-history")]
        [Authorize]
        public async Task<IActionResult> GetExportHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var role = User.FindFirstValue("role") ?? string.Empty;
            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            var query = _db.ExportedReports.AsQueryable();
            if (!isAdmin)
            {
                query = query.Where(e => e.CreatedByUserId == userId);
            }

            var totalCount = await query.CountAsync();
            var exports = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ExportedReportDto
                {
                    ExportId = e.ExportId,
                    ReportType = e.ReportType,
                    ExportFormat = e.ExportFormat,
                    FilePath = e.FilePath,
                    FileSize = e.FileSize,
                    CreatedAt = e.CreatedAt,
                    DownloadUrl = $"/api/v1/reports/exports/{e.ExportId}/download"
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = exports,
                pageNumber,
                pageSize,
                totalCount
            });
        }

        [HttpGet("forecasting")]
        [Authorize]
        public async Task<IActionResult> GetForecasting(
            [FromQuery] int days = 30,
            [FromQuery] decimal confidenceLevel = 0.95m)
        {
            var query = new GetForecastingDataQuery
            {
                Days = days,
                ConfidenceLevel = confidenceLevel
            };

            var result = await _forecastingHandler.Handle(query);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("audit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuditReport(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] string? entityType = null,
            [FromQuery] Guid? userId = null)
        {
            var query = new GetAuditComplianceReportQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                EntityType = entityType,
                UserId = userId
            };

            var result = await _auditHandler.Handle(query);
            return Ok(new { success = true, data = result });
        }
    }
}

