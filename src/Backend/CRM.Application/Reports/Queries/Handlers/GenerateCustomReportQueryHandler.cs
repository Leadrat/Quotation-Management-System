using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Reports.Queries.Handlers
{
    public class GenerateCustomReportQueryHandler
    {
        private readonly IAppDbContext _db;

        public GenerateCustomReportQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<ReportData> Handle(GenerateCustomReportQuery query)
        {
            var reportData = new ReportData
            {
                ReportType = query.ReportType,
                Title = $"Custom {query.ReportType} Report",
                Summary = "Custom report generated based on selected criteria",
                Metrics = new List<KPIMetric>(),
                Charts = new List<ChartData>(),
                Details = new List<Dictionary<string, object>>()
            };

            // Based on report type, generate appropriate data
            switch (query.ReportType.ToLowerInvariant())
            {
                case "quotations":
                    return await GenerateQuotationsReport(query, reportData);
                case "payments":
                    return await GeneratePaymentsReport(query, reportData);
                case "approvals":
                    return await GenerateApprovalsReport(query, reportData);
                default:
                    return reportData;
            }
        }

        private async Task<ReportData> GenerateQuotationsReport(GenerateCustomReportQuery query, ReportData reportData)
        {
            var baseQuery = _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .AsQueryable();

            // Apply filters
            if (query.Filters != null)
            {
                if (query.Filters.ContainsKey("fromDate") && query.Filters["fromDate"] is DateTime fromDate)
                {
                    baseQuery = baseQuery.Where(q => q.CreatedAt >= fromDate);
                }

                if (query.Filters.ContainsKey("toDate") && query.Filters["toDate"] is DateTime toDate)
                {
                    baseQuery = baseQuery.Where(q => q.CreatedAt <= toDate);
                }

                if (query.Filters.ContainsKey("status") && query.Filters["status"] is string status)
                {
                    if (Enum.TryParse<QuotationStatus>(status, out var statusEnum))
                    {
                        baseQuery = baseQuery.Where(q => q.Status == statusEnum);
                    }
                }

                if (query.Filters.ContainsKey("userId") && query.Filters["userId"] is Guid userId)
                {
                    baseQuery = baseQuery.Where(q => q.CreatedByUserId == userId);
                }
            }

            // Group by
            if (!string.IsNullOrEmpty(query.GroupBy))
            {
                switch (query.GroupBy.ToLowerInvariant())
                {
                    case "date":
                        var groupedByDate = await baseQuery
                            .GroupBy(q => q.CreatedAt.Date)
                            .Select(g => new Dictionary<string, object>
                            {
                                { "Date", g.Key },
                                { "Count", g.Count() },
                                { "TotalValue", g.Sum(q => q.TotalAmount) }
                            })
                            .ToListAsync();

                        reportData.Details = groupedByDate;
                        break;

                    case "user":
                        var groupedByUser = await baseQuery
                            .GroupBy(q => new { q.CreatedByUserId, q.CreatedByUser.FirstName, q.CreatedByUser.LastName })
                            .Select(g => new Dictionary<string, object>
                            {
                                { "UserId", g.Key.CreatedByUserId },
                                { "UserName", g.Key.FirstName + " " + g.Key.LastName },
                                { "Count", g.Count() },
                                { "TotalValue", g.Sum(q => q.TotalAmount) }
                            })
                            .ToListAsync();

                        reportData.Details = groupedByUser;
                        break;

                    case "status":
                        var groupedByStatus = await baseQuery
                            .GroupBy(q => q.Status)
                            .Select(g => new Dictionary<string, object>
                            {
                                { "Status", g.Key.ToString() },
                                { "Count", g.Count() },
                                { "TotalValue", g.Sum(q => q.TotalAmount) }
                            })
                            .ToListAsync();

                        reportData.Details = groupedByStatus;
                        break;
                }
            }
            else
            {
                // No grouping - return detailed list
                var details = await baseQuery
                    .Take(query.Limit ?? 1000)
                    .Select(q => new Dictionary<string, object>
                    {
                        { "QuotationId", q.QuotationId },
                        { "QuotationNumber", q.QuotationNumber },
                        { "ClientName", q.Client.CompanyName },
                        { "Status", q.Status.ToString() },
                        { "TotalAmount", q.TotalAmount },
                        { "CreatedAt", q.CreatedAt }
                    })
                    .ToListAsync();

                reportData.Details = details;
            }

            // Add KPI metrics
            var totalCount = await baseQuery.CountAsync();
            var totalValue = await baseQuery.SumAsync(q => (decimal?)q.TotalAmount) ?? 0;

            reportData.Metrics.Add(new KPIMetric
            {
                Name = "Total Quotations",
                Value = totalCount.ToString(),
                NumericValue = totalCount
            });

            reportData.Metrics.Add(new KPIMetric
            {
                Name = "Total Value",
                Value = totalValue.ToString("C"),
                NumericValue = totalValue
            });

            return reportData;
        }

        private async Task<ReportData> GeneratePaymentsReport(GenerateCustomReportQuery query, ReportData reportData)
        {
            var baseQuery = _db.Payments
                .Include(p => p.Quotation)
                .ThenInclude(q => q.Client)
                .AsQueryable();

            // Apply filters
            if (query.Filters != null)
            {
                if (query.Filters.ContainsKey("fromDate") && query.Filters["fromDate"] is DateTime fromDate)
                {
                    baseQuery = baseQuery.Where(p => p.CreatedAt >= fromDate);
                }

                if (query.Filters.ContainsKey("toDate") && query.Filters["toDate"] is DateTime toDate)
                {
                    baseQuery = baseQuery.Where(p => p.CreatedAt <= toDate);
                }

                if (query.Filters.ContainsKey("status") && query.Filters["status"] is string status)
                {
                    if (Enum.TryParse<PaymentStatus>(status, out var statusEnum))
                    {
                        baseQuery = baseQuery.Where(p => p.PaymentStatus == statusEnum);
                    }
                }
            }

            var details = await baseQuery
                .Take(query.Limit ?? 1000)
                .Select(p => new Dictionary<string, object>
                {
                    { "PaymentId", p.PaymentId },
                    { "QuotationNumber", p.Quotation.QuotationNumber },
                    { "ClientName", p.Quotation.Client.CompanyName },
                    { "Amount", p.AmountPaid },
                    { "Status", p.PaymentStatus.ToString() },
                    { "PaymentDate", p.PaymentDate ?? p.CreatedAt }
                })
                .ToListAsync();

            reportData.Details = details;

            var totalAmount = await baseQuery.SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
            reportData.Metrics.Add(new KPIMetric
            {
                Name = "Total Payments",
                Value = details.Count.ToString(),
                NumericValue = details.Count
            });

            reportData.Metrics.Add(new KPIMetric
            {
                Name = "Total Amount",
                Value = totalAmount.ToString("C"),
                NumericValue = totalAmount
            });

            return reportData;
        }

        private async Task<ReportData> GenerateApprovalsReport(GenerateCustomReportQuery query, ReportData reportData)
        {
            var baseQuery = _db.DiscountApprovals
                .Include(a => a.Quotation)
                .Include(a => a.RequestedByUser)
                .AsQueryable();

            // Apply filters
            if (query.Filters != null)
            {
                if (query.Filters.ContainsKey("fromDate") && query.Filters["fromDate"] is DateTime fromDate)
                {
                    baseQuery = baseQuery.Where(a => a.RequestDate >= fromDate);
                }

                if (query.Filters.ContainsKey("toDate") && query.Filters["toDate"] is DateTime toDate)
                {
                    baseQuery = baseQuery.Where(a => a.RequestDate <= toDate);
                }

                if (query.Filters.ContainsKey("status") && query.Filters["status"] is string status)
                {
                    if (Enum.TryParse<ApprovalStatus>(status, out var statusEnum))
                    {
                        baseQuery = baseQuery.Where(a => a.Status == statusEnum);
                    }
                }
            }

            var details = await baseQuery
                .Take(query.Limit ?? 1000)
                .Select(a => new Dictionary<string, object>
                {
                    { "ApprovalId", a.ApprovalId },
                    { "QuotationNumber", a.Quotation.QuotationNumber },
                    { "RequestedBy", a.RequestedByUser.FirstName + " " + a.RequestedByUser.LastName },
                    { "Status", a.Status.ToString() },
                    { "DiscountPercent", a.CurrentDiscountPercentage },
                    { "RequestDate", a.RequestDate }
                })
                .ToListAsync();

            reportData.Details = details;

            return reportData;
        }
    }
}

