using System;
using CRM.Application.CompanyBankDetails.DTOs;

namespace CRM.Application.CompanyBankDetails.Commands
{
    public class SaveCompanyBankDetailsCommand
    {
        public SaveCompanyBankDetailsRequest Request { get; set; } = null!;
        public Guid UpdatedBy { get; set; }
    }
}

