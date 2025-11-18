using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CRM.Application.Clients.Queries;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Validation;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class GetFilterOptionsQueryHandler
    {
        private readonly IAppDbContext _db;
        public GetFilterOptionsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<FilterOptionsDto> Handle(GetFilterOptionsQuery _)
        {
            var baseQ = _db.Clients.AsNoTracking().Where(c => c.DeletedAt == null);

            var states = await baseQ
                .Where(c => c.State != null && c.State != "")
                .GroupBy(c => c.State!)
                .Select(g => new StateOption { State = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var cities = await baseQ
                .Where(c => c.City != null && c.City != "")
                .GroupBy(c => c.City!)
                .Select(g => new CityOption { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(20)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var ranges = new List<DateRangeOption>
            {
                new DateRangeOption { Label = "Last 7 days", From = today.AddDays(-7).ToString("yyyy-MM-dd"), To = today.ToString("yyyy-MM-dd") },
                new DateRangeOption { Label = "Last 30 days", From = today.AddDays(-30).ToString("yyyy-MM-dd"), To = today.ToString("yyyy-MM-dd") },
                new DateRangeOption { Label = "Last 90 days", From = today.AddDays(-90).ToString("yyyy-MM-dd"), To = today.ToString("yyyy-MM-dd") },
                new DateRangeOption { Label = "This year", From = new DateTime(today.Year,1,1).ToString("yyyy-MM-dd"), To = today.ToString("yyyy-MM-dd") }
            };

            var codes = StateCodeConstants.Codes
                .Select(code => new StateCodeOption { Code = code, Name = code }) // Name mapping TBD
                .ToList();

            return new FilterOptionsDto
            {
                States = states,
                Cities = cities,
                CreatedDateRanges = ranges,
                StateCodes = codes
            };
        }
    }
}
