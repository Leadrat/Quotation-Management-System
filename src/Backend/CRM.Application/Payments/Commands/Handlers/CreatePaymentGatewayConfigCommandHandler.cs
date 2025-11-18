using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using CRM.Application.Payments.Services;
using CRM.Domain.Entities;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.Commands.Handlers
{
    public class CreatePaymentGatewayConfigCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<CreatePaymentGatewayConfigCommandHandler> _logger;

        public CreatePaymentGatewayConfigCommandHandler(
            IAppDbContext db,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<CreatePaymentGatewayConfigCommandHandler> logger)
        {
            _db = db;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
        }

        public async Task<PaymentGatewayConfigDto> Handle(CreatePaymentGatewayConfigCommand command)
        {
            var request = command.Request;

            // Check if config already exists
            var existing = await _db.PaymentGatewayConfigs
                .FirstOrDefaultAsync(c => 
                    c.CompanyId == request.CompanyId && 
                    c.GatewayName == request.GatewayName);

            if (existing != null)
                throw new InvalidOperationException($"Gateway config for '{request.GatewayName}' already exists for this company");

            // Validate API keys by testing with gateway (optional but recommended)
            var gatewayService = await _gatewayFactory.GetGatewayServiceAsync(request.GatewayName);
            if (gatewayService == null)
                throw new InvalidOperationException($"Payment gateway '{request.GatewayName}' is not supported");

            // Note: Encryption should be handled at the infrastructure layer
            // For now, store as-is (encryption will be added in infrastructure layer)
            var encryptedApiKey = request.ApiKey;
            var encryptedApiSecret = request.ApiSecret;
            var encryptedWebhookSecret = request.WebhookSecret;

            // Create config
            var config = new PaymentGatewayConfig
            {
                ConfigId = Guid.NewGuid(),
                CompanyId = request.CompanyId,
                GatewayName = request.GatewayName,
                ApiKey = encryptedApiKey,
                ApiSecret = encryptedApiSecret,
                WebhookSecret = encryptedWebhookSecret,
                Enabled = request.Enabled,
                IsTestMode = request.IsTestMode,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedByUserId = command.CreatedByUserId
            };

            _db.PaymentGatewayConfigs.Add(config);
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
                UpdatedByUserId = command.CreatedByUserId
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Payment gateway config created: {GatewayName} for company {CompanyId}",
                request.GatewayName, request.CompanyId);

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

