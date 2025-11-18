using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Services;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IAppDbContext _context;
        private readonly Dictionary<string, IPaymentGatewayService> _gatewayServices;
        private readonly IPaymentGatewayEncryptionService? _encryptionService;

        public PaymentGatewayFactory(
            IAppDbContext context,
            IEnumerable<IPaymentGatewayService> gatewayServices,
            IPaymentGatewayEncryptionService? encryptionService = null)
        {
            _context = context;
            _encryptionService = encryptionService;
            _gatewayServices = gatewayServices.ToDictionary(gs => gs.GatewayName, gs => gs);
        }

        public async Task<IPaymentGatewayService?> GetGatewayServiceAsync(string gatewayName, Guid? companyId = null)
        {
            if (!_gatewayServices.TryGetValue(gatewayName, out var service))
                return null;

            // If companyId is provided, verify the gateway is configured and enabled for that company
            if (companyId.HasValue)
            {
                var config = await _context.PaymentGatewayConfigs
                    .FirstOrDefaultAsync(c => 
                        c.CompanyId == companyId && 
                        c.GatewayName == gatewayName && 
                        c.Enabled);

                if (config == null)
                    return null;
            }

            return service;
        }

        public async Task<List<IPaymentGatewayService>> GetEnabledGatewayServicesAsync(Guid? companyId = null)
        {
            var enabledGateways = new List<IPaymentGatewayService>();

            if (companyId.HasValue)
            {
                var configs = await _context.PaymentGatewayConfigs
                    .Where(c => c.CompanyId == companyId && c.Enabled)
                    .Select(c => c.GatewayName)
                    .ToListAsync();

                foreach (var gatewayName in configs)
                {
                    if (_gatewayServices.TryGetValue(gatewayName, out var service))
                    {
                        enabledGateways.Add(service);
                    }
                }
            }
            else
            {
                // Return all available gateways if no company filter
                enabledGateways.AddRange(_gatewayServices.Values);
            }

            return enabledGateways;
        }
    }
}

