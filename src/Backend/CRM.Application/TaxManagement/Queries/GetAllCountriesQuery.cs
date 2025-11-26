using System;

namespace CRM.Application.TaxManagement.Queries
{
    public class GetAllCountriesQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsActive { get; set; }
    }
}

