using System;

namespace CRM.Application.Common.Results
{
    public class PagedResult<T>
    {
        public bool Success { get; set; } = true;
        public T[] Data { get; set; } = Array.Empty<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}

