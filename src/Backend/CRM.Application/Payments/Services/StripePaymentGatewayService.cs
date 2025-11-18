using System;
using System.Threading.Tasks;
using Stripe;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.Services
{
    /// <summary>
    /// Stripe payment gateway service implementation using Stripe.NET SDK
    /// </summary>
    public class StripePaymentGatewayService : IPaymentGatewayService
    {
        private readonly ILogger<StripePaymentGatewayService> _logger;

        public StripePaymentGatewayService(ILogger<StripePaymentGatewayService> logger)
        {
            _logger = logger;
        }

        public string GatewayName => "Stripe";

        public async Task<PaymentGatewayResponse> InitiatePaymentAsync(PaymentGatewayRequest request)
        {
            try
            {
                // Set Stripe API key
                StripeConfiguration.ApiKey = request.ApiKey;

                // Create Payment Intent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Convert to cents/paisa
                    Currency = request.Currency.ToLowerInvariant(),
                    Description = request.Description ?? $"Payment for Quotation {request.QuotationId}",
                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "PaymentId", request.PaymentId.ToString() },
                        { "QuotationId", request.QuotationId.ToString() },
                        { "CustomerEmail", request.CustomerEmail ?? "" }
                    },
                    ReceiptEmail = request.CustomerEmail
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                // Return response with client secret for frontend
                return new PaymentGatewayResponse
                {
                    Success = true,
                    PaymentReference = paymentIntent.Id,
                    PaymentUrl = null, // Stripe uses client secret, not URL
                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "ClientSecret", paymentIntent.ClientSecret },
                        { "PaymentIntentId", paymentIntent.Id }
                    }
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe API error while initiating payment");
                return new PaymentGatewayResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = ex.StripeError?.Code
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating Stripe payment");
                return new PaymentGatewayResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PaymentVerificationResponse> VerifyPaymentAsync(string paymentReference, string apiKey, string apiSecret, bool isTestMode)
        {
            try
            {
                StripeConfiguration.ApiKey = apiKey;

                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentReference);

                var status = paymentIntent.Status switch
                {
                    "succeeded" => "success",
                    "processing" => "pending",
                    "requires_payment_method" or "requires_confirmation" or "requires_action" => "pending",
                    "canceled" => "failed",
                    _ => "failed"
                };

                return new PaymentVerificationResponse
                {
                    IsValid = true,
                    PaymentReference = paymentIntent.Id,
                    Amount = paymentIntent.Amount / 100m, // Convert from cents
                    Currency = paymentIntent.Currency.ToUpperInvariant(),
                    Status = status,
                    PaymentDate = paymentIntent.Status == "succeeded" ? new DateTimeOffset(paymentIntent.Created) : null,
                    FailureReason = paymentIntent.Status == "canceled" ? "Payment was canceled" : null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe API error while verifying payment");
                return new PaymentVerificationResponse
                {
                    IsValid = false,
                    FailureReason = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Stripe payment");
                return new PaymentVerificationResponse
                {
                    IsValid = false,
                    FailureReason = ex.Message
                };
            }
        }

        public async Task<RefundGatewayResponse> RefundPaymentAsync(string paymentReference, decimal refundAmount, string reason, string apiKey, string apiSecret, bool isTestMode)
        {
            try
            {
                StripeConfiguration.ApiKey = apiKey;

                // First, get the payment intent to find the charge
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.GetAsync(paymentReference, new PaymentIntentGetOptions
                {
                    Expand = new System.Collections.Generic.List<string> { "charges" }
                });

                // Get charges from payment intent
                string? chargeId = null;
                if (paymentIntent.LatestCharge != null)
                {
                    chargeId = paymentIntent.LatestCharge.Id;
                }
                else
                {
                    // Try to get charge from charges list if available
                    var chargeService = new ChargeService();
                    var charges = await chargeService.ListAsync(new ChargeListOptions
                    {
                        PaymentIntent = paymentReference,
                        Limit = 1
                    });
                    
                    if (charges?.Data != null && charges.Data.Count > 0)
                    {
                        chargeId = charges.Data[0].Id;
                    }
                }

                if (string.IsNullOrEmpty(chargeId))
                {
                    throw new InvalidOperationException("No charge found for this payment intent");
                }

                // Create refund
                var refundOptions = new RefundCreateOptions
                {
                    Charge = chargeId,
                    Amount = (long)(refundAmount * 100), // Convert to cents
                    Reason = reason
                };

                var refundService = new RefundService();
                var refund = await refundService.CreateAsync(refundOptions);

                return new RefundGatewayResponse
                {
                    Success = true,
                    RefundReference = refund.Id,
                    RefundAmount = refund.Amount / 100m, // Convert from cents
                    RefundedAt = new DateTimeOffset(refund.Created)
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe API error while processing refund");
                return new RefundGatewayResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = ex.StripeError?.Code,
                    RefundedAt = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe refund");
                return new RefundGatewayResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    RefundedAt = DateTimeOffset.UtcNow
                };
            }
        }

        public async Task<bool> CancelPaymentAsync(string paymentReference, string apiKey, string apiSecret, bool isTestMode)
        {
            try
            {
                StripeConfiguration.ApiKey = apiKey;

                var service = new PaymentIntentService();
                var cancelOptions = new PaymentIntentCancelOptions();

                var paymentIntent = await service.CancelAsync(paymentReference, cancelOptions);
                return paymentIntent.Status == "canceled";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe API error while canceling payment");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling Stripe payment");
                return false;
            }
        }

        public Task<bool> VerifyWebhookSignatureAsync(string payload, string signature, string webhookSecret)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signature,
                    webhookSecret,
                    throwOnApiVersionMismatch: false);

                return Task.FromResult(stripeEvent != null);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Stripe webhook signature");
                return Task.FromResult(false);
            }
        }

        public async Task<RefundStatusResponse> GetRefundStatusAsync(string refundReference, string apiKey, string apiSecret, bool isTestMode)
        {
            try
            {
                StripeConfiguration.ApiKey = apiKey;

                var refundService = new RefundService();
                var refund = await refundService.GetAsync(refundReference);

                var status = refund.Status switch
                {
                    "pending" => "pending",
                    "succeeded" => "succeeded",
                    "failed" => "failed",
                    "canceled" => "canceled",
                    _ => "pending"
                };

                return new RefundStatusResponse
                {
                    Success = true,
                    RefundReference = refund.Id,
                    Status = status,
                    RefundAmount = refund.Amount / 100m, // Convert from cents
                    RefundedAt = refund.Status == "succeeded" ? new DateTimeOffset(refund.Created) : null,
                    FailureReason = refund.Status == "failed" ? refund.FailureReason : null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe API error while getting refund status");
                return new RefundStatusResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Stripe refund status");
                return new RefundStatusResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefundGatewayResponse> ReverseRefundAsync(string refundReference, string reason, string apiKey, string apiSecret, bool isTestMode)
        {
            try
            {
                StripeConfiguration.ApiKey = apiKey;

                // Stripe doesn't support reversing refunds directly
                // Instead, we would need to create a new charge for the amount
                // This is a complex operation and may not be supported in all cases
                // For now, we'll return an error indicating this is not supported
                _logger.LogWarning("Stripe does not support direct refund reversal. A new charge would need to be created.");
                
                return new RefundGatewayResponse
                {
                    Success = false,
                    ErrorMessage = "Stripe does not support direct refund reversal. Please create a new charge instead.",
                    ErrorCode = "REVERSAL_NOT_SUPPORTED",
                    RefundedAt = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing Stripe refund");
                return new RefundGatewayResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    RefundedAt = DateTimeOffset.UtcNow
                };
            }
        }
    }
}
