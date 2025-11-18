using System;

namespace CRM.Application.Quotations.Exceptions
{
    public class QuotationNotFoundException : Exception
    {
        public QuotationNotFoundException(Guid quotationId)
            : base($"Quotation with ID {quotationId} was not found.")
        {
            QuotationId = quotationId;
        }

        public Guid QuotationId { get; }
    }
}

