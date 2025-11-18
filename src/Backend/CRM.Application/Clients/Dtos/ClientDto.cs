using System;

namespace CRM.Application.Clients.Dtos
{
    public class ClientDto
    {
        public Guid ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? PhoneCode { get; set; }
        public string? Gstin { get; set; }
        public string? StateCode { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
