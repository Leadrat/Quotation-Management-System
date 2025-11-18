using System;

namespace CRM.Application.Clients.Commands
{
    public class CreateClientCommand
    {
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
        public Guid CreatedByUserId { get; set; }
    }
}
