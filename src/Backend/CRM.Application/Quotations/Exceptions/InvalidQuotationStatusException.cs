using System;

namespace CRM.Application.Quotations.Exceptions
{
    public class InvalidQuotationStatusException : Exception
    {
        public InvalidQuotationStatusException(string message)
            : base(message)
        {
        }
    }
}

