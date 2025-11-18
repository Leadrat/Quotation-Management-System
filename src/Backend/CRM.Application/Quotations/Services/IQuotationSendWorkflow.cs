using System;
using System.Threading.Tasks;
using CRM.Application.Quotations.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Quotations.Services
{
    public interface IQuotationSendWorkflow
    {
        Task<QuotationAccessLinkDto> ExecuteAsync(
            Quotation quotation,
            SendQuotationRequest request,
            Guid requestedByUserId,
            bool isResend);
    }
}


