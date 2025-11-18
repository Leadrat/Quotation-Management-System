using System;
using System.Threading.Tasks;
using Razorpay.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CRM.Application.Payments.Services
{
    /// <summary>
    /// Razorpay payment gateway service implementation using Razorpay .NET SDK
    /// </summary>
    public class RazorpayPaymentGatewayService : IPaymentGatewayService
    {
        private readonly ILogger<RazorpayPaymentGatewayService> _logger;

        public RazorpayPaymentGatewayService(ILogger<RazorpayPaymentGatewayService> logger)
        {
            _logger = logger;
        }

        public string GatewayName => "Razorpay";

        public async Task<PaymentGatewayResponse> InitiatePaymentAsync(PaymentGatewayRequest request)
        {
            try
            {
                var razorpayClient = new RazorpayClient(request.ApiKey, request.ApiSecret);

                // Create Order
                var orderOptions = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "amount", (int)(request.Amount * 100) }, // Convert to paise
                    { "currency", request.Currency.ToUpperInvariant() },
                    { "receipt", $"quotation_{request.QuotationId}" },
                    { "notes", new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "PaymentId", request.PaymentId.ToString() },
                            { "QuotationId", request.QuotationId.ToString() },
                            { "CustomerEmail", request.CustomerEmail ?? "" }
                        }
                    }
                };

                var order = razorpayClient.Order.Create(orderOptions);

                // Extract order ID and payment URL
                var orderId = order["id"].ToString();
                var paymentUrl = $"https://checkout.razorpay.com/v1/checkout.js?key={request.ApiKey}&order_id={orderId}";

                return new PaymentGatewayResponse
                {
                    Success = true,
                    PaymentReference = orderId,
                    PaymentUrl = paymentUrl,
                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "OrderId", orderId },
                        { "Amount", order["amount"].ToString() },
                        { "Currency", order["currency"].ToString() }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating Razorpay payment");
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
                var razorpayClient = new RazorpayClient(apiKey, apiSecret);

                // Get order details
                var order = razorpayClient.Order.Fetch(paymentReference);

                // Check order status - Razorpay Order object uses indexer for properties
                string status = "pending";
                DateTimeOffset? paymentDate = null;
                string? failureReason = null;

                // Access order properties using indexer
                var orderStatus = order["status"]?.ToString() ?? "created";
                
                // Check if order has amount_paid (payment was successful)
                if (order["amount_paid"] != null && Convert.ToDecimal(order["amount_paid"]) > 0)
                {
                    status = "success";
                    if (order["created_at"] != null)
                    {
                        var timestamp = Convert.ToInt64(order["created_at"]);
                        paymentDate = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                    }
                }
                else if (orderStatus == "paid")
                {
                    status = "success";
                }
                else if (orderStatus == "attempted")
                {
                    status = "pending";
                }
                else if (orderStatus == "failed")
                {
                    status = "failed";
                    failureReason = "Payment failed";
                }

                var amount = Convert.ToDecimal(order["amount"]) / 100m; // Convert from paise
                var currency = order["currency"]?.ToString()?.ToUpperInvariant() ?? "INR";

                return new PaymentVerificationResponse
                {
                    IsValid = true,
                    PaymentReference = paymentReference,
                    Amount = amount,
                    Currency = currency,
                    Status = status,
                    PaymentDate = paymentDate,
                    FailureReason = failureReason
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Razorpay payment");
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
                var razorpayClient = new RazorpayClient(apiKey, apiSecret);

                // Get order to find payment ID
                var order = razorpayClient.Order.Fetch(paymentReference);
                
                // For Razorpay, we need the payment ID to refund
                // In a real scenario, this would come from webhook or be stored when payment is captured
                // For now, we'll try to get it from order metadata or require it to be passed
                string? paymentId = null;
                
                // Check if order has payment_id in notes/metadata
                var notesValue = order["notes"];
                if (notesValue != null)
                {
                    var notesDict = notesValue as System.Collections.Generic.Dictionary<string, object>;
                    if (notesDict != null && notesDict.ContainsKey("payment_id"))
                    {
                        paymentId = notesDict["payment_id"]?.ToString();
                    }
                }

                // If no payment ID found, we cannot proceed with refund
                // In production, payment ID should be stored when payment is captured via webhook
                if (string.IsNullOrEmpty(paymentId))
                {
                    throw new InvalidOperationException("Payment ID not found. Cannot process refund without payment ID. Payment ID should be stored when payment is captured via webhook.");
                }

                // Create refund
                // Razorpay SDK Refund method may have different signature
                // Try with just payment ID first, then add options if needed
                var refundOptions = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "amount", (int)(refundAmount * 100) } // Convert to paise
                };

                // Add notes if reason provided
                if (!string.IsNullOrEmpty(reason))
                {
                    refundOptions["notes"] = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "reason", reason }
                    };
                }

                // Razorpay SDK - use Refund service to create refund
                // The Refund.Create method takes options dictionary with payment_id
                refundOptions["payment_id"] = paymentId;
                var refund = razorpayClient.Refund.Create(refundOptions);

                var refundAmountPaise = Convert.ToDecimal(refund["amount"]);
                var refundTimestamp = Convert.ToInt64(refund["created_at"]);

                return new RefundGatewayResponse
                {
                    Success = true,
                    RefundReference = refund["id"].ToString(),
                    RefundAmount = refundAmountPaise / 100m, // Convert from paise
                    RefundedAt = DateTimeOffset.FromUnixTimeSeconds(refundTimestamp)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Razorpay refund");
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
                var razorpayClient = new RazorpayClient(apiKey, apiSecret);

                // Razorpay doesn't have a direct cancel API for orders
                // Orders expire automatically after a certain time
                // We can mark it as cancelled locally
                _logger.LogInformation("Razorpay order {OrderId} cannot be cancelled via API. Orders expire automatically.", paymentReference);
                return true; // Return true as cancellation is handled by expiration
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling Razorpay payment");
                return false;
            }
        }

        public Task<bool> VerifyWebhookSignatureAsync(string payload, string signature, string webhookSecret)
        {
            try
            {
                // Razorpay webhook signature verification
                // The signature is HMAC SHA256 of payload with webhook secret
                using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(webhookSecret));
                var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                var isValid = computedSignature == signature.ToLowerInvariant();
                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Razorpay webhook signature");
                return Task.FromResult(false);
            }
        }

        public async Task<RefundStatusResponse> GetRefundStatusAsync(string refundReference, string apiKey, string apiSecret, bool isTestMode)
        {
            try
            {
                var razorpayClient = new RazorpayClient(apiKey, apiSecret);

                var refund = razorpayClient.Refund.Fetch(refundReference);
                
                var status = refund["status"].ToString() switch
                {
                    "processed" => "succeeded",
                    "pending" => "pending",
                    "failed" => "failed",
                    _ => "pending"
                };

                var refundAmount = Convert.ToDecimal(refund["amount"]) / 100m; // Convert from paise
                var refundTimestamp = Convert.ToInt64(refund["created_at"]);
                var refundedAt = DateTimeOffset.FromUnixTimeSeconds(refundTimestamp);

                return new RefundStatusResponse
                {
                    Success = true,
                    RefundReference = refund["id"].ToString(),
                    Status = status,
                    RefundAmount = refundAmount,
                    RefundedAt = status == "succeeded" ? refundedAt : null,
                    FailureReason = status == "failed" ? refund["notes"]?["error"]?.ToString() : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Razorpay refund status");
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
                var razorpayClient = new RazorpayClient(apiKey, apiSecret);

                // Razorpay doesn't support direct refund reversal
                // Similar to Stripe, a new payment would need to be created
                _logger.LogWarning("Razorpay does not support direct refund reversal. A new payment would need to be created.");
                
                return new RefundGatewayResponse
                {
                    Success = false,
                    ErrorMessage = "Razorpay does not support direct refund reversal. Please create a new payment instead.",
                    ErrorCode = "REVERSAL_NOT_SUPPORTED",
                    RefundedAt = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing Razorpay refund");
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
