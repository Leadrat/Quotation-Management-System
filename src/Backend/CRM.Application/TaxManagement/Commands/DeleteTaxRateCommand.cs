using System;

namespace CRM.Application.TaxManagement.Commands
{
    public class DeleteTaxRateCommand
    {
        public Guid TaxRateId { get; set; }
        public Guid DeletedByUserId { get; set; }
    }
}

