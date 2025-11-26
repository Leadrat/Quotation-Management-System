using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.Common.Interfaces;
using CRM.Application.Reports.Commands;
using CRM.Application.Reports.Commands.Handlers;
using CRM.Application.Reports.Dtos;
using CRM.Application.Reports.Queries;
using CRM.Application.Reports.Queries.Handlers;
using CRM.Domain.Enums;
using CRM.Shared.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        private readonly ITenantContext _tenantContext;
        private readonly IValidator<GetSalesDashboardMetricsQuery> _salesDashboardValidator;

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
            IAppDbContext db,
            ITenantContext tenantContext,
            IValidator<GetSalesDashboardMetricsQuery> salesDashboardValidator)
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
            _tenantContext = tenantContext;
            _salesDashboardValidator = salesDashboardValidator;
        }

        [HttpGet("dashboard/sales")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetSalesDashboard(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid user token", details = "UserId claim is missing or invalid" });
                }

                var role = User.FindFirstValue("role") ?? string.Empty;

                var query = new GetSalesDashboardMetricsQuery
                {
                    UserId = userId,
                    RequestorRole = role,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                var validationResult = await _salesDashboardValidator.ValidateAsync(query);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new 
                    { 
                        error = "Validation failed", 
                        errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                        details = $"UserId: {query.UserId}, FromDate: {query.FromDate}, ToDate: {query.ToDate}"
                    });
                }

                var result = await _salesDashboardHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                var errorDetails = new
                {
                    error = "An error occurred while processing the request",
                    message = ex.Message,
                    stackTrace = System.Diagnostics.Debugger.IsAttached ? ex.StackTrace : null,
                    innerException = ex.InnerException?.Message
                };
                return StatusCode(500, errorDetails);
            }
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

        [HttpGet("dashboard/stats")]
        [Authorize(Roles = "SalesRep,Manager,Admin,Client")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine("GetDashboardStats: Unauthorized - invalid user token");
                    return Unauthorized(new { error = "Invalid user token", details = "UserId claim is missing or invalid" });
                }

                var role = User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
                var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
                var isManager = string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"GetDashboardStats: UserId={userId}, Role={role}, IsAdmin={isAdmin}, IsManager={isManager}");
                System.Diagnostics.Debug.WriteLine("GetDashboardStats: Starting dashboard stats calculation...");

                // Get ALL SalesRep user IDs for manager (not just assigned to them)
                List<Guid> salesRepUserIds = new List<Guid>();
                if (isManager)
                {
                    try
                    {
                        salesRepUserIds = await _db.Users
                            .AsNoTracking()
                            // Temporarily disable tenant filter for debugging
                            // .Where(u => u.DeletedAt == null 
                            //     && u.IsActive 
                            //     && u.RoleId == RoleIds.SalesRep) // All SalesReps
                            .Where(u => u.DeletedAt == null 
                                && u.IsActive 
                                && (u.RoleId == RoleIds.SalesRep || u.RoleId == RoleIds.Manager || u.RoleId == RoleIds.Admin)) // All active users
                            .Select(u => u.UserId)
                            .ToListAsync();
                        System.Diagnostics.Debug.WriteLine($"GetDashboardStats: Manager found {salesRepUserIds.Count} SalesReps");
                    }
                    catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P01")
                    {
                        // Users table does not exist yet (relation does not exist error)
                        salesRepUserIds = new List<Guid>();
                    }
                    catch (Exception)
                    {
                        // Any other error - set to empty list
                        salesRepUserIds = new List<Guid>();
                    }
                }

                // Get total clients count
                int totalClients = 0;
                try
                {
                    // Re-enable tenant filtering with correct tenant ID
                    var currentTenantId = _tenantContext.CurrentTenantId;
                    totalClients = await _db.Clients.AsNoTracking().Where(c => c.DeletedAt == null && c.TenantId == currentTenantId).CountAsync();
                    System.Diagnostics.Debug.WriteLine($"GetDashboardStats: Clients count = {totalClients} for tenant {currentTenantId}");
                }
                catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P01")
                {
                    // Clients table does not exist yet (relation does not exist error)
                    totalClients = 0;
                    System.Diagnostics.Debug.WriteLine("GetDashboardStats: Clients table does not exist (42P01)");
                }
                catch (Exception ex)
                {
                    // Any other error - set to 0
                    totalClients = 0;
                    System.Diagnostics.Debug.WriteLine($"GetDashboardStats: Error counting clients: {ex.Message}");
                }

                // Get total quotations count
                int totalQuotations = 0;
                try
                {
                    // Re-enable tenant filtering with correct tenant ID
                    var currentTenantId = _tenantContext.CurrentTenantId;
                    totalQuotations = await _db.Quotations.AsNoTracking().Where(q => q.TenantId == currentTenantId).CountAsync();
                    System.Diagnostics.Debug.WriteLine($"GetDashboardStats: Quotations count = {totalQuotations} for tenant {currentTenantId}");
                }
                catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P01")
                {
                    // Quotations table does not exist yet (relation does not exist error)
                    totalQuotations = 0;
                    System.Diagnostics.Debug.WriteLine("GetDashboardStats: Quotations table does not exist (42P01)");
                }
                catch (Exception ex)
                {
                    // Any other error - set to 0
                    totalQuotations = 0;
                    System.Diagnostics.Debug.WriteLine($"GetDashboardStats: Error counting quotations: {ex.Message}");
                }

                // Get pending approvals count
                int pendingApprovals = 0;
                try
                {
                    var approvalsQuery = _db.DiscountApprovals.AsNoTracking()
                        .Where(a => a.Status == Domain.Enums.ApprovalStatus.Pending);
                    if (isAdmin)
                    {
                        // Admin sees all pending approvals
                    }
                    else if (isManager)
                    {
                        // Manager sees ALL Manager-level pending approvals (not just assigned to them)
                        approvalsQuery = approvalsQuery.Where(a => 
                            a.ApprovalLevel == Domain.Enums.ApprovalLevel.Manager && 
                            !a.EscalatedToAdmin);
                    }
                    else
                    {
                        // SalesRep sees approvals they requested
                        approvalsQuery = approvalsQuery.Where(a => a.RequestedByUserId == userId);
                    }
                    pendingApprovals = await approvalsQuery.CountAsync();
                }
                catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P01")
                {
                    // DiscountApprovals table does not exist yet (relation does not exist error)
                    pendingApprovals = 0;
                }
                catch (Exception)
                {
                    // Any other error - set to 0
                    pendingApprovals = 0;
                }

                // Get total payments count (join with Quotations to filter by user)
                int totalPayments = 0;
                try
                {
                    System.Diagnostics.Debug.WriteLine("GetDashboardStats: Starting payments count query...");
                    // Re-enable tenant filtering with correct tenant ID
                    var currentTenantId = _tenantContext.CurrentTenantId;
                    totalPayments = await _db.Payments.AsNoTracking().Where(p => p.TenantId == currentTenantId).CountAsync();
                    System.Diagnostics.Debug.WriteLine($"GetDashboardStats: Payments table exists, count = {totalPayments} for tenant {currentTenantId}");
                }
                catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P01")
                {
                    // Payments table does not exist yet (relation does not exist error)
                    totalPayments = 0;
                    System.Diagnostics.Debug.WriteLine("GetDashboardStats: Payments table does not exist (42P01)");
                }
                catch (Exception ex)
                {
                    // Any other error - set to 0
                    totalPayments = 0;
                    System.Diagnostics.Debug.WriteLine($"GetDashboardStats: Error counting payments: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"GetDashboardStats: Error details: {ex}");
                }

                var response = new
                {
                    success = true,
                    data = new
                    {
                        totalClients,
                        totalQuotations,
                        totalPayments,
                        pendingApprovals
                    }
                };
                
                System.Diagnostics.Debug.WriteLine($"GetDashboardStats Final Response: TotalClients={totalClients}, TotalQuotations={totalQuotations}, TotalPayments={totalPayments}, PendingApprovals={pendingApprovals}");
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDashboardStats Error: {ex}");
                return StatusCode(500, new
                {
                    error = "An error occurred while processing the request",
                    message = ex.Message,
                    stackTrace = System.Diagnostics.Debugger.IsAttached ? ex.StackTrace : null
                });
            }
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
            var totalClients = await _db.Clients.CountAsync(c => c.DeletedAt == null);
            var totalQuotations = await _db.Quotations.CountAsync();
            
            // Get total revenue - handle case where Payments table might not exist
            decimal totalRevenue = 0;
            try
            {
                totalRevenue = await _db.Payments
                    .Where(p => p.PaymentStatus == PaymentStatus.Success)
                    .SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
            }
            catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P01")
            {
                // Payments table does not exist yet (relation does not exist error)
                totalRevenue = 0;
            }
            catch (Exception)
            {
                // Any other error - set to 0
                totalRevenue = 0;
            }

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
                    TotalClientsLifetime = totalClients,
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

