using System;

namespace CRM.Application.Payments.Queries
{
    public class GetPaymentsDashboardQuery
    {
        public Guid? UserId { get; set; } // If null, admin sees all
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}

