using System;

namespace CRM.Application.Products.Commands
{
    public class DeleteProductCommand
    {
        public Guid ProductId { get; set; }
        public Guid DeletedByUserId { get; set; }
    }
}

