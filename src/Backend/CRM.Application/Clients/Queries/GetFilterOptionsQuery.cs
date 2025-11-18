using System.Collections.Generic;

namespace CRM.Application.Clients.Queries
{
    public class GetFilterOptionsQuery { }

    public class FilterOptionsDto
    {
        public List<StateOption> States { get; set; } = new();
        public List<CityOption> Cities { get; set; } = new();
        public List<DateRangeOption> CreatedDateRanges { get; set; } = new();
        public List<StateCodeOption> StateCodes { get; set; } = new();
    }

    public class StateOption { public string State { get; set; } = string.Empty; public int Count { get; set; } }
    public class CityOption { public string City { get; set; } = string.Empty; public int Count { get; set; } }
    public class DateRangeOption { public string Label { get; set; } = string.Empty; public string From { get; set; } = string.Empty; public string To { get; set; } = string.Empty; }
    public class StateCodeOption { public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; }
}
