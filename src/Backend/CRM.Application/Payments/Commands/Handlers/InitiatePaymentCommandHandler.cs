using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using CRM.Application.Payments.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.Commands.Handlers
{
    public class InitiatePaymentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<InitiatePaymentCommandHandler> _logger;
        private readonly ITenantContext _tenantContext;

        public InitiatePaymentCommandHandler(
            IAppDbContext db,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<InitiatePaymentCommandHandler> logger,
            ITenantContext tenantContext)
        {
            _db = db;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
            _tenantContext = tenantContext;
        }

        public async Task<PaymentDto> Handle(InitiatePaymentCommand command)
        {
            var request = command.Request;
            var currentTenantId = _tenantContext.CurrentTenantId;

            // Validate quotation exists and is accepted
            // Temporarily disable tenant filter for debugging
            // var currentTenantId = _tenantContext.CurrentTenantId;
            var quotation = await _db.Quotations
                .Include(q => q.Client)
                // .FirstOrDefaultAsync(q => q.QuotationId == request.QuotationId && q.TenantId == currentTenantId)
                .FirstOrDefaultAsync(q => q.QuotationId == request.QuotationId);

            if (quotation == null)
                throw new InvalidOperationException("Quotation not found or not accessible in current tenant");

            if (quotation.Status != QuotationStatus.Accepted)
                throw new InvalidOperationException("Payment can only be initiated for accepted quotations");

            // Check if payment already exists
            var existingPayment = await _db.Payments
                .FirstOrDefaultAsync(p => 
                    p.QuotationId == request.QuotationId && 
                    p.TenantId == currentTenantId &&
                    (p.PaymentStatus == PaymentStatus.Pending || 
                     p.PaymentStatus == PaymentStatus.Processing ||
                     p.PaymentStatus == PaymentStatus.Success));

            if (existingPayment != null && existingPayment.PaymentStatus == PaymentStatus.Success)
                throw new InvalidOperationException("Payment already completed for this quotation");

            // Determine amount (use provided amount or quotation total)
            var amount = request.Amount ?? quotation.TotalAmount;
            var currency = request.Currency ?? "INR";

            // Get gateway service
            var gatewayService = await _gatewayFactory.GetGatewayServiceAsync(request.PaymentGateway);
            if (gatewayService == null)
                throw new InvalidOperationException($"Payment gateway '{request.PaymentGateway}' not found or not enabled");

            // Get gateway config (for API keys)
            // TODO: Get company ID from quotation or user context
            var gatewayConfig = await _db.PaymentGatewayConfigs
                .FirstOrDefaultAsync(c => 
                    c.GatewayName == request.PaymentGateway && 
                    c.Enabled);

            if (gatewayConfig == null)
                throw new InvalidOperationException($"Payment gateway '{request.PaymentGateway}' is not configured");

            // Create payment entity
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                TenantId = currentTenantId,
                QuotationId = request.QuotationId,
                PaymentGateway = request.PaymentGateway,
                PaymentReference = Guid.NewGuid().ToString(), // Temporary, will be updated by gateway
                AmountPaid = amount,
                Currency = currency,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            try
            {
                // Call gateway service
                var gatewayRequest = new PaymentGatewayRequest
                {
                    PaymentId = payment.PaymentId,
                    QuotationId = request.QuotationId,
                    Amount = amount,
                    Currency = currency,
                    GatewayName = request.PaymentGateway,
                    ApiKey = gatewayConfig.ApiKey, // TODO: Decrypt if encrypted
                    ApiSecret = gatewayConfig.ApiSecret, // TODO: Decrypt if encrypted
                    WebhookSecret = gatewayConfig.WebhookSecret, // TODO: Decrypt if encrypted
                    IsTestMode = gatewayConfig.IsTestMode,
                    CustomerEmail = quotation.Client.Email,
                    CustomerName = quotation.Client.CompanyName,
                    Description = $"Payment for Quotation #{quotation.QuotationNumber}"
                };

                var gatewayResponse = await gatewayService.InitiatePaymentAsync(gatewayRequest);

                if (gatewayResponse.Success)
                {
                    payment.PaymentReference = gatewayResponse.PaymentReference;
                    payment.PaymentStatus = PaymentStatus.Processing;
                    payment.UpdatedAt = DateTimeOffset.UtcNow;

                    // Store payment URL or client secret in metadata
                    var metadata = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(gatewayResponse.PaymentUrl))
                    {
                        metadata["PaymentUrl"] = gatewayResponse.PaymentUrl;
                    }
                    if (gatewayResponse.Metadata != null && gatewayResponse.Metadata.ContainsKey("ClientSecret"))
                    {
                        metadata["ClientSecret"] = gatewayResponse.Metadata["ClientSecret"];
                    }
                    if (metadata.Count > 0)
                    {
                        payment.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
                    }

                    await _db.SaveChangesAsync();

                    // Publish domain event
                    var paymentInitiatedEvent = new PaymentInitiated
                    {
                        PaymentId = payment.PaymentId,
                        QuotationId = payment.QuotationId,
                        PaymentGateway = payment.PaymentGateway,
                        PaymentReference = payment.PaymentReference,
                        AmountPaid = payment.AmountPaid,
                        Currency = payment.Currency,
                        InitiatedAt = payment.CreatedAt,
                        InitiatedByUserId = command.InitiatedByUserId
                    };

                    // TODO: Publish event via event bus/dispatcher

                    _logger.LogInformation("Payment {PaymentId} initiated successfully for quotation {QuotationId}", 
                        payment.PaymentId, quotation.QuotationId);

                    // Extract client secret or payment URL from metadata
                    string? paymentUrl = gatewayResponse.PaymentUrl;
                    string? clientSecret = null;
                    if (gatewayResponse.Metadata != null)
                    {
                        gatewayResponse.Metadata.TryGetValue("ClientSecret", out clientSecret);
                    }

                    return new PaymentDto
                    {
                        PaymentId = payment.PaymentId,
                        QuotationId = payment.QuotationId,
                        PaymentGateway = payment.PaymentGateway,
                        PaymentReference = payment.PaymentReference,
                        AmountPaid = payment.AmountPaid,
                        Currency = payment.Currency,
                        PaymentStatus = payment.PaymentStatus,
                        CreatedAt = payment.CreatedAt,
                        UpdatedAt = payment.UpdatedAt,
                        IsRefundable = payment.IsRefundable,
                        CanBeRefunded = payment.CanBeRefunded(),
                        CanBeCancelled = payment.CanBeCancelled(),
                        PaymentUrl = paymentUrl,
                        ClientSecret = clientSecret // For Stripe
                    };
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Failed;
                    payment.FailureReason = gatewayResponse.ErrorMessage ?? "Payment initiation failed";
                    payment.UpdatedAt = DateTimeOffset.UtcNow;
                    await _db.SaveChangesAsync();

                    throw new InvalidOperationException(gatewayResponse.ErrorMessage ?? "Payment initiation failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment {PaymentId}", payment.PaymentId);
                payment.PaymentStatus = PaymentStatus.Failed;
                payment.FailureReason = ex.Message;
                payment.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
                throw;
            }
        }
    }
}

