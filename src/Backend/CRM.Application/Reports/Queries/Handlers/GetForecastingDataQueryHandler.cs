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
    public class GetForecastingDataQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetForecastingDataQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<ForecastingDataDto> Handle(GetForecastingDataQuery query)
        {
            // Get historical data (last 90 days)
            var historicalStartDate = DateTime.UtcNow.AddDays(-90);
            var historicalDataRaw = await _db.Quotations
                .Where(q => q.CreatedAt >= historicalStartDate && 
                           q.Status == QuotationStatus.Accepted)
                .GroupBy(q => q.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(q => q.TotalAmount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var historicalData = historicalDataRaw
                .Select(x => (x.Date, x.Revenue, x.Count))
                .ToList();

            // Simple linear regression for prediction
            var predictedRevenue = CalculatePredictedRevenue(historicalData, query.Days);
            var predictedSuccessRate = CalculatePredictedSuccessRate();

            // Generate forecast data points
            var forecastData = new List<RevenueForecastData>();
            var startDate = DateTime.UtcNow.Date;
            
            for (int i = 1; i <= query.Days; i++)
            {
                var forecastDate = startDate.AddDays(i);
                var dailyPrediction = predictedRevenue / query.Days;
                
                forecastData.Add(new RevenueForecastData
                {
                    Date = forecastDate,
                    PredictedRevenue = dailyPrediction,
                    LowerBound = dailyPrediction * (1 - (1 - query.ConfidenceLevel)),
                    UpperBound = dailyPrediction * (1 + (1 - query.ConfidenceLevel))
                });
            }

            // Trend data (actual vs predicted)
            var trendData = historicalData
                .Select(h => new TrendData
                {
                    Date = h.Date,
                    ActualValue = h.Revenue
                })
                .ToList();

            // Add predicted values to trend
            var lastActualDate = historicalData.Any() ? historicalData.Last().Date : DateTime.UtcNow.Date;
            for (int i = 1; i <= Math.Min(query.Days, 30); i++)
            {
                var trendDate = lastActualDate.AddDays(i);
                trendData.Add(new TrendData
                {
                    Date = trendDate,
                    PredictedValue = predictedRevenue / query.Days
                });
            }

            return new ForecastingDataDto
            {
                PredictedRevenue = predictedRevenue,
                ConfidenceLevel = query.ConfidenceLevel,
                PredictedSuccessRate = predictedSuccessRate,
                RevenueForecast = forecastData,
                Trend = trendData
            };
        }

        private decimal CalculatePredictedRevenue(List<(DateTime Date, decimal Revenue, int Count)> historicalData, int days)
        {
            if (!historicalData.Any())
                return 0;

            // Simple average daily revenue * days
            var averageDailyRevenue = historicalData.Average(h => (decimal)h.Revenue);
            return averageDailyRevenue * days;
        }

        private decimal CalculatePredictedSuccessRate()
        {
            // Get recent quotations to calculate success rate
            var recentQuotations = _db.Quotations
                .Where(q => q.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .ToList();

            var sent = recentQuotations.Count(q => q.Status == QuotationStatus.Sent || 
                                                   q.Status == QuotationStatus.Viewed || 
                                                   q.Status == QuotationStatus.Accepted || 
                                                   q.Status == QuotationStatus.Rejected);
            var accepted = recentQuotations.Count(q => q.Status == QuotationStatus.Accepted);

            return sent > 0 ? (decimal)accepted / sent * 100 : 0;
        }
    }
}

