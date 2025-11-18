using System;

namespace CRM.Application.Quotations.Exceptions
{
    public class QuotationAccessLinkNotFoundException : Exception
    {
        public QuotationAccessLinkNotFoundException()
            : base("Quotation access link not found.")
        {
        }
    }
}


