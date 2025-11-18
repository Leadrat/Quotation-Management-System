using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.Commands.Handlers
{
    public class UpdatePaymentGatewayConfigCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<UpdatePaymentGatewayConfigCommandHandler> _logger;

        public UpdatePaymentGatewayConfigCommandHandler(
            IAppDbContext db,
            ILogger<UpdatePaymentGatewayConfigCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<PaymentGatewayConfigDto> Handle(UpdatePaymentGatewayConfigCommand command)
        {
            var config = await _db.PaymentGatewayConfigs
                .FirstOrDefaultAsync(c => c.ConfigId == command.ConfigId);

            if (config == null)
                throw new InvalidOperationException("Payment gateway config not found");

            var request = command.Request;

            // Update credentials if provided
            // Note: Encryption should be handled at the infrastructure layer
            if (!string.IsNullOrEmpty(request.ApiKey))
            {
                config.ApiKey = request.ApiKey;
            }

            if (!string.IsNullOrEmpty(request.ApiSecret))
            {
                config.ApiSecret = request.ApiSecret;
            }

            if (request.WebhookSecret != null)
            {
                config.WebhookSecret = request.WebhookSecret;
            }

            // Update flags if provided
            if (request.Enabled.HasValue)
            {
                if (request.Enabled.Value)
                    config.Enable();
                else
                    config.Disable();
            }

            if (request.IsTestMode.HasValue)
            {
                if (request.IsTestMode.Value)
                    config.SwitchToTestMode();
                else
                    config.SwitchToProduction();
            }

            config.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Publish event
            var configUpdatedEvent = new PaymentGatewayConfigUpdated
            {
                ConfigId = config.ConfigId,
                CompanyId = config.CompanyId,
                GatewayName = config.GatewayName,
                Enabled = config.Enabled,
                IsTestMode = config.IsTestMode,
                UpdatedAt = config.UpdatedAt,
                UpdatedByUserId = command.UpdatedByUserId
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Payment gateway config updated: {ConfigId}", config.ConfigId);

            return new PaymentGatewayConfigDto
            {
                ConfigId = config.ConfigId,
                CompanyId = config.CompanyId,
                GatewayName = config.GatewayName,
                Enabled = config.Enabled,
                IsTestMode = config.IsTestMode,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt
            };
        }
    }
}

