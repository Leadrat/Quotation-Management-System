using System.Collections.Generic;
using CRM.Application.Refunds.Dtos;

namespace CRM.Application.Refunds.Commands
{
    public class BulkProcessRefundsCommand
    {
        public BulkProcessRefundsRequest Request { get; set; } = null!;
    }
}

