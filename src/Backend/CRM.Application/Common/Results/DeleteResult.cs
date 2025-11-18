using System;

namespace CRM.Application.Common.Results
{
    public class DeleteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset DeletedAt { get; set; }
    }
}
