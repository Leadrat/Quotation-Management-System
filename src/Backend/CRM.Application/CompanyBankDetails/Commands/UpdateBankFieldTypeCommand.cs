using System;
using CRM.Application.CompanyBankDetails.DTOs;

namespace CRM.Application.CompanyBankDetails.Commands
{
    public class UpdateBankFieldTypeCommand
    {
        public Guid BankFieldTypeId { get; set; }
        public UpdateBankFieldTypeRequest Request { get; set; } = null!;
    }
}

