using System;

namespace CRM.Application.Clients.Commands
{
    public class UpdateClientRequest
    {
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
    }
}
