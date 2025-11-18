using System;

namespace CRM.Application.Clients.Commands
{
    public class UpdateClientCommand
    {
        public Guid ClientId { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? PhoneCode { get; set; }
        public string? Gstin { get; set; }
        public string? StateCode { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty; // Admin or SalesRep
    }
}
