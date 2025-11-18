using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.Payments.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Payments.Queries.Handlers
{
    public class GetPaymentsByUserQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetPaymentsByUserQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsByUserQuery query)
        {
            // Get quotations created by user
            var quotationIds = await _db.Quotations
                .Where(q => q.CreatedByUserId == query.UserId)
                .Select(q => q.QuotationId)
                .ToListAsync();

            // Build query
            var paymentsQuery = _db.Payments
                .Where(p => quotationIds.Contains(p.QuotationId));

            // Apply filters
            if (!string.IsNullOrEmpty(query.Status))
            {
                if (Enum.TryParse<PaymentStatus>(query.Status, true, out var status))
                {
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentStatus == status);
                }
            }

            if (query.QuotationId.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.QuotationId == query.QuotationId.Value);
            }

            if (query.StartDate.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.CreatedAt >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.CreatedAt <= query.EndDate.Value);
            }

            // Get total count
            var totalCount = await paymentsQuery.CountAsync();

            // Apply pagination
            var payments = await paymentsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = payments.Select(p => new PaymentDto
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
            }).ToList();

            return new PagedResult<PaymentDto>
            {
                Data = items.ToArray(),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}

