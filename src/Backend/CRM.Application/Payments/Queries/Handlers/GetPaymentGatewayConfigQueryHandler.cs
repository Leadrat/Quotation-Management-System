using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Payments.Queries.Handlers
{
    public class GetPaymentGatewayConfigQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetPaymentGatewayConfigQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<PaymentGatewayConfigDto>> Handle(GetPaymentGatewayConfigQuery query)
        {
            var configsQuery = _db.PaymentGatewayConfigs.AsQueryable();

            if (query.CompanyId.HasValue)
            {
                configsQuery = configsQuery.Where(c => c.CompanyId == query.CompanyId.Value);
            }

            var configs = await configsQuery
                .OrderBy(c => c.GatewayName)
                .ToListAsync();

            return configs.Select(c => new PaymentGatewayConfigDto
            {
                ConfigId = c.ConfigId,
                CompanyId = c.CompanyId,
                GatewayName = c.GatewayName,
                Enabled = c.Enabled,
                IsTestMode = c.IsTestMode,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();
        }
    }
}

