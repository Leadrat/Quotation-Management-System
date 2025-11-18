using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CRM.Application.Payments.Commands;
using CRM.Application.Payments.Commands.Handlers;
using CRM.Application.Payments.Dtos;
using CRM.Application.Payments.Services;
using CRM.Application.Common.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/payment-webhook")]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly UpdatePaymentStatusCommandHandler _updateStatusHandler;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly IAppDbContext _db;
        private readonly ILogger<PaymentWebhookController> _logger;

        public PaymentWebhookController(
            UpdatePaymentStatusCommandHandler updateStatusHandler,
            IPaymentGatewayFactory gatewayFactory,
            IAppDbContext db,
            ILogger<PaymentWebhookController> logger)
        {
            _updateStatusHandler = updateStatusHandler;
            _gatewayFactory = gatewayFactory;
            _db = db;
            _logger = logger;
        }

        [HttpPost("{gateway}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> HandleWebhook([FromRoute] string gateway)
        {
            try
            {
                // Read request body
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var payload = await reader.ReadToEndAsync();

                // Get signature from headers (gateway-specific)
                var signature = Request.Headers["X-Signature"].ToString() 
                    ?? Request.Headers["Stripe-Signature"].ToString()
                    ?? Request.Headers["X-Razorpay-Signature"].ToString();

                // Get gateway service
                var gatewayService = await _gatewayFactory.GetGatewayServiceAsync(gateway);
                if (gatewayService == null)
                {
                    _logger.LogWarning("Unknown payment gateway: {Gateway}", gateway);
                    return BadRequest(new { error = "Unknown payment gateway" });
                }

                // Get gateway config for webhook secret
                var gatewayConfig = await _db.PaymentGatewayConfigs
                    .FirstOrDefaultAsync(c => c.GatewayName == gateway && c.Enabled);

                if (gatewayConfig == null)
                {
                    _logger.LogWarning("Gateway config not found for: {Gateway}", gateway);
                    return BadRequest(new { error = "Gateway not configured" });
                }

                // Verify webhook signature
                if (!string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(gatewayConfig.WebhookSecret))
                {
                    var isValid = await gatewayService.VerifyWebhookSignatureAsync(
                        payload,
                        signature,
                        gatewayConfig.WebhookSecret);

                    if (!isValid)
                    {
                        _logger.LogWarning("Invalid webhook signature for gateway: {Gateway}", gateway);
                        return Unauthorized(new { error = "Invalid signature" });
                    }
                }

                // Parse webhook payload (gateway-specific)
                var paymentReference = ExtractPaymentReference(gateway, payload);
                var status = ExtractStatus(gateway, payload);
                var amount = ExtractAmount(gateway, payload);
                var currency = ExtractCurrency(gateway, payload);
                var paymentDate = ExtractPaymentDate(gateway, payload);
                var failureReason = ExtractFailureReason(gateway, payload);

                if (string.IsNullOrEmpty(paymentReference))
                {
                    _logger.LogWarning("Could not extract payment reference from webhook payload");
                    return BadRequest(new { error = "Invalid webhook payload" });
                }

                // Update payment status
                var updateRequest = new UpdatePaymentStatusRequest
                {
                    PaymentReference = paymentReference,
                    Status = status ?? "pending",
                    Amount = amount,
                    Currency = currency,
                    PaymentDate = paymentDate,
                    FailureReason = failureReason
                };

                var command = new UpdatePaymentStatusCommand
                {
                    Request = updateRequest,
                    GatewayName = gateway
                };

                await _updateStatusHandler.Handle(command);

                _logger.LogInformation("Webhook processed successfully for gateway: {Gateway}, payment: {PaymentReference}",
                    gateway, paymentReference);

                return Ok(new { received = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook for gateway: {Gateway}", gateway);
                return StatusCode(500, new { error = "Error processing webhook" });
            }
        }

        private string? ExtractPaymentReference(string gateway, string payload)
        {
            try
            {
                if (gateway.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
                {
                    var stripeEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var eventType = stripeEvent?["type"]?.ToString();

                    // Handle payment_intent events
                    if (eventType?.StartsWith("payment_intent.") == true)
                    {
                        var paymentIntent = stripeEvent?["data"]?["object"] as JObject;
                        return paymentIntent?["id"]?.ToString();
                    }

                    // Handle charge events
                    if (eventType?.StartsWith("charge.") == true)
                    {
                        var charge = stripeEvent?["data"]?["object"] as JObject;
                        var paymentIntentId = charge?["payment_intent"]?.ToString();
                        return paymentIntentId ?? charge?["id"]?.ToString();
                    }
                }
                else if (gateway.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
                {
                    var razorpayEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var entity = razorpayEvent?["payload"]?["payment"]?["entity"] as JObject
                        ?? razorpayEvent?["payload"]?["order"]?["entity"] as JObject;

                    // Try to get order_id or payment_id
                    return entity?["order_id"]?.ToString() 
                        ?? entity?["id"]?.ToString()
                        ?? razorpayEvent?["payload"]?["payment"]?["entity"]?["id"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting payment reference for gateway: {Gateway}", gateway);
            }

            return null;
        }

        private string? ExtractStatus(string gateway, string payload)
        {
            try
            {
                if (gateway.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
                {
                    var stripeEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var eventType = stripeEvent?["type"]?.ToString();
                    var paymentIntent = stripeEvent?["data"]?["object"] as JObject;
                    var status = paymentIntent?["status"]?.ToString();

                    if (eventType == "payment_intent.succeeded" || status == "succeeded")
                        return "success";
                    if (eventType == "payment_intent.payment_failed" || status == "canceled")
                        return "failed";
                    if (status == "processing" || status == "requires_payment_method" || status == "requires_confirmation")
                        return "pending";
                }
                else if (gateway.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
                {
                    var razorpayEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var payment = razorpayEvent?["payload"]?["payment"]?["entity"] as JObject;
                    var status = payment?["status"]?.ToString();

                    return status switch
                    {
                        "authorized" or "captured" => "success",
                        "failed" => "failed",
                        "refunded" => "refunded",
                        _ => "pending"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting status for gateway: {Gateway}", gateway);
            }

            return null;
        }

        private decimal? ExtractAmount(string gateway, string payload)
        {
            try
            {
                if (gateway.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
                {
                    var stripeEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var paymentIntent = stripeEvent?["data"]?["object"] as JObject;
                    var amount = paymentIntent?["amount"]?.ToObject<long?>();
                    return amount.HasValue ? amount.Value / 100m : null;
                }
                else if (gateway.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
                {
                    var razorpayEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var payment = razorpayEvent?["payload"]?["payment"]?["entity"] as JObject;
                    var amount = payment?["amount"]?.ToObject<decimal?>();
                    return amount.HasValue ? amount.Value / 100m : null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting amount for gateway: {Gateway}", gateway);
            }

            return null;
        }

        private string? ExtractCurrency(string gateway, string payload)
        {
            try
            {
                if (gateway.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
                {
                    var stripeEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var paymentIntent = stripeEvent?["data"]?["object"] as JObject;
                    return paymentIntent?["currency"]?.ToString()?.ToUpperInvariant();
                }
                else if (gateway.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
                {
                    var razorpayEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var payment = razorpayEvent?["payload"]?["payment"]?["entity"] as JObject;
                    return payment?["currency"]?.ToString()?.ToUpperInvariant();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting currency for gateway: {Gateway}", gateway);
            }

            return null;
        }

        private DateTimeOffset? ExtractPaymentDate(string gateway, string payload)
        {
            try
            {
                if (gateway.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
                {
                    var stripeEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var paymentIntent = stripeEvent?["data"]?["object"] as JObject;
                    var created = paymentIntent?["created"]?.ToObject<long?>();
                    return created.HasValue ? DateTimeOffset.FromUnixTimeSeconds(created.Value) : null;
                }
                else if (gateway.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
                {
                    var razorpayEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var payment = razorpayEvent?["payload"]?["payment"]?["entity"] as JObject;
                    var createdAt = payment?["created_at"]?.ToObject<long?>();
                    return createdAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(createdAt.Value) : null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting payment date for gateway: {Gateway}", gateway);
            }

            return null;
        }

        private string? ExtractFailureReason(string gateway, string payload)
        {
            try
            {
                if (gateway.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
                {
                    var stripeEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var paymentIntent = stripeEvent?["data"]?["object"] as JObject;
                    var lastPaymentError = paymentIntent?["last_payment_error"] as JObject;
                    return lastPaymentError?["message"]?.ToString();
                }
                else if (gateway.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
                {
                    var razorpayEvent = JsonConvert.DeserializeObject<JObject>(payload);
                    var payment = razorpayEvent?["payload"]?["payment"]?["entity"] as JObject;
                    return payment?["error_description"]?.ToString() 
                        ?? payment?["error_code"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting failure reason for gateway: {Gateway}", gateway);
            }

            return null;
        }
    }
}
