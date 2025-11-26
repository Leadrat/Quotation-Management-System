using System;

namespace CRM.Application.TaxManagement.Commands
{
    public class DeleteCountryCommand
    {
        public Guid CountryId { get; set; }
        public Guid DeletedByUserId { get; set; }
    }
}

