using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Application.Payments.Services
{
    public interface IPaymentGatewayFactory
    {
        /// <summary>
        /// Gets a payment gateway service instance for the specified gateway name
        /// </summary>
        Task<IPaymentGatewayService?> GetGatewayServiceAsync(string gatewayName, Guid? companyId = null);

        /// <summary>
        /// Gets all enabled gateway services for a company
        /// </summary>
        Task<List<IPaymentGatewayService>> GetEnabledGatewayServicesAsync(Guid? companyId = null);
    }
}

