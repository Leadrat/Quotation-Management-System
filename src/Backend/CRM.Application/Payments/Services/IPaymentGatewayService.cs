using System.Threading.Tasks;

namespace CRM.Application.Payments.Services
{
    public interface IPaymentGatewayService
    {
        /// <summary>
        /// Initiates a payment with the gateway and returns a payment URL or reference
        /// </summary>
        Task<PaymentGatewayResponse> InitiatePaymentAsync(PaymentGatewayRequest request);

        /// <summary>
        /// Verifies a payment status with the gateway
        /// </summary>
        Task<PaymentVerificationResponse> VerifyPaymentAsync(string paymentReference, string apiKey, string apiSecret, bool isTestMode);

        /// <summary>
        /// Processes a refund (full or partial)
        /// </summary>
        Task<RefundGatewayResponse> RefundPaymentAsync(
            string paymentReference,
            decimal refundAmount,
            string reason,
            string apiKey,
            string apiSecret,
            bool isTestMode);

        /// <summary>
        /// Cancels a pending payment
        /// </summary>
        Task<bool> CancelPaymentAsync(string paymentReference, string apiKey, string apiSecret, bool isTestMode);

        /// <summary>
        /// Verifies webhook signature from the gateway
        /// </summary>
        Task<bool> VerifyWebhookSignatureAsync(string payload, string signature, string webhookSecret);

        /// <summary>
        /// Gets the status of a refund from the gateway
        /// </summary>
        Task<RefundStatusResponse> GetRefundStatusAsync(
            string refundReference,
            string apiKey,
            string apiSecret,
            bool isTestMode);

        /// <summary>
        /// Reverses a completed refund (if supported by gateway)
        /// </summary>
        Task<RefundGatewayResponse> ReverseRefundAsync(
            string refundReference,
            string reason,
            string apiKey,
            string apiSecret,
            bool isTestMode);

        /// <summary>
        /// Gets the gateway name this service handles
        /// </summary>
        string GatewayName { get; }
    }
}

