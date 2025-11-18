using System;
using System.Collections.Generic;

namespace CRM.Application.Refunds.Queries
{
    public class GetRefundsByPaymentQuery
    {
        public Guid PaymentId { get; set; }
    }
}

