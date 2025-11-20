using System;

namespace CRM.Application.TaxManagement.Commands
{
    public class DeleteJurisdictionCommand
    {
        public Guid JurisdictionId { get; set; }
        public Guid DeletedByUserId { get; set; }
    }
}

