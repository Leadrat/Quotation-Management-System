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
    public class GetClientEngagementMetricsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetClientEngagementMetricsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<ClientEngagementDto> Handle(GetClientEngagementMetricsQuery query)
        {
            var fromDate = query.FromDate ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.UtcNow;

            var baseQuery = _db.Quotations
                .Include(q => q.Client)
                .Where(q => q.CreatedAt >= fromDate && q.CreatedAt <= toDate);

            if (query.ClientId.HasValue)
            {
                baseQuery = baseQuery.Where(q => q.ClientId == query.ClientId.Value);
            }

            var quotations = await baseQuery.ToListAsync();

            // Calculate overall metrics
            var quotationsSent = quotations.Count(q => q.Status != QuotationStatus.Draft);
            var quotationsViewed = quotations.Count(q => q.Status == QuotationStatus.Viewed || 
                                                         q.Status == QuotationStatus.Accepted || 
                                                         q.Status == QuotationStatus.Rejected);
            
            // Get responses separately
            // Get responses separately
            var quotationIds = quotations.Select(q => q.QuotationId).ToList();
            var responses = await _db.QuotationResponses
                .Where(r => quotationIds.Contains(r.QuotationId))
                .ToListAsync();
            
            var quotationsResponded = responses.Select(r => r.QuotationId).Distinct().Count();
            var quotationsAccepted = quotations.Count(q => q.Status == QuotationStatus.Accepted);
            
            var quotationDict = quotations.ToDictionary(q => q.QuotationId);

            var viewRate = quotationsSent > 0 ? (decimal)quotationsViewed / quotationsSent * 100 : 0;
            var responseRate = quotationsSent > 0 ? (decimal)quotationsResponded / quotationsSent * 100 : 0;
            var conversionRate = quotationsSent > 0 ? (decimal)quotationsAccepted / quotationsSent * 100 : 0;

            // Average response time (responses already loaded above)
            var averageResponseTimeHours = 0m;
            if (responses.Any())
            {
                averageResponseTimeHours = (decimal)responses
                    .Where(r => quotationDict.ContainsKey(r.QuotationId))
                    .Select(r => (r.ResponseDate - quotationDict[r.QuotationId].CreatedAt).TotalHours)
                    .Average();
            }

            // Client engagement data
            var clientEngagement = await baseQuery
                .GroupBy(q => new { q.ClientId, q.Client.CompanyName })
                .Select(g => new ClientEngagementData
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.CompanyName,
                    QuotationsSent = g.Count(q => q.Status != QuotationStatus.Draft),
                    QuotationsViewed = g.Count(q => q.Status == QuotationStatus.Viewed || 
                                                     q.Status == QuotationStatus.Accepted || 
                                                     q.Status == QuotationStatus.Rejected),
                    QuotationsResponded = g.Count(q => responses.Any(r => r.QuotationId == q.QuotationId)),
                    QuotationsAccepted = g.Count(q => q.Status == QuotationStatus.Accepted)
                })
                .ToListAsync();

            foreach (var item in clientEngagement)
            {
                item.ViewRate = item.QuotationsSent > 0 ? (decimal)item.QuotationsViewed / item.QuotationsSent * 100 : 0;
                item.ResponseRate = item.QuotationsSent > 0 ? (decimal)item.QuotationsResponded / item.QuotationsSent * 100 : 0;
                item.ConversionRate = item.QuotationsSent > 0 ? (decimal)item.QuotationsAccepted / item.QuotationsSent * 100 : 0;
            }

            // Response time by period (weekly)
            var responseTimeByPeriod = responses
                .Where(r => quotationDict.ContainsKey(r.QuotationId))
                .GroupBy(r => GetWeekKey(r.ResponseDate))
                .Select(g => new ResponseTimeData
                {
                    Period = g.Key,
                    AverageHours = (decimal)g.Average(r => (r.ResponseDate - quotationDict[r.QuotationId].CreatedAt).TotalHours)
                })
                .OrderBy(x => x.Period)
                .ToList();

            return new ClientEngagementDto
            {
                ViewRate = viewRate,
                ResponseRate = responseRate,
                ConversionRate = conversionRate,
                AverageResponseTimeHours = averageResponseTimeHours,
                ClientEngagement = clientEngagement,
                ResponseTimeByPeriod = responseTimeByPeriod
            };
        }

        private string GetWeekKey(DateTimeOffset date)
        {
            var startOfYear = new DateTime(date.Year, 1, 1);
            var daysSinceStart = (date.Date - startOfYear.Date).Days;
            var weekNumber = (daysSinceStart / 7) + 1;
            return $"{date.Year}-W{weekNumber:D2}";
        }
    }
}

