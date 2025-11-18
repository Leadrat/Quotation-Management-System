using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Payments.Queries.Handlers
{
    public class GetPaymentsDashboardQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetPaymentsDashboardQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentDashboardDto> Handle(GetPaymentsDashboardQuery query)
        {
            // Build base query
            var paymentsQuery = _db.Payments.AsQueryable();

            // Filter by user if provided (SalesRep sees own, Admin sees all)
            if (query.UserId.HasValue)
            {
                var quotationIds = await _db.Quotations
                    .Where(q => q.CreatedByUserId == query.UserId.Value)
                    .Select(q => q.QuotationId)
                    .ToListAsync();

                paymentsQuery = paymentsQuery.Where(p => quotationIds.Contains(p.QuotationId));
            }

            // Apply date range filter
            if (query.StartDate.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.CreatedAt >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.CreatedAt <= query.EndDate.Value);
            }

            var payments = await paymentsQuery.ToListAsync();

            // Calculate summary
            var summary = new PaymentSummaryDto
            {
                TotalPending = payments
                    .Where(p => p.PaymentStatus == PaymentStatus.Pending || p.PaymentStatus == PaymentStatus.Processing)
                    .Sum(p => p.AmountPaid),
                TotalPaid = payments
                    .Where(p => p.PaymentStatus == PaymentStatus.Success)
                    .Sum(p => p.AmountPaid),
                TotalRefunded = payments
                    .Where(p => p.PaymentStatus == PaymentStatus.Refunded || p.PaymentStatus == PaymentStatus.PartiallyRefunded)
                    .Sum(p => p.RefundAmount ?? 0),
                TotalFailed = payments
                    .Where(p => p.PaymentStatus == PaymentStatus.Failed)
                    .Sum(p => p.AmountPaid),
                PendingCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Pending || p.PaymentStatus == PaymentStatus.Processing),
                PaidCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Success),
                RefundedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Refunded || p.PaymentStatus == PaymentStatus.PartiallyRefunded),
                FailedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Failed)
            };

            // Status counts
            var statusCounts = payments
                .GroupBy(p => p.PaymentStatus)
                .Select(g => new PaymentStatusCountDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.AmountPaid)
                })
                .ToList();

            // Recent payments (last 10)
            var recentPayments = payments
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
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
                    CanBeCancelled = p.CanBeCancelled()
                })
                .ToList();

            return new PaymentDashboardDto
            {
                Summary = summary,
                RecentPayments = recentPayments,
                StatusCounts = statusCounts
            };
        }
    }
}

