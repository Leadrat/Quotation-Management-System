"use client";
import { useEffect, useState, useRef } from "react";
import { useParams } from "next/navigation";
import { ClientPortalApi, RefundsApi, PaymentsApi } from "@/lib/api";
import { formatCurrency, formatDate, getStatusLabel } from "@/utils/quotationFormatter";
import { ClientResponseModal } from "@/components/quotations";
import { RefundRequestForm } from "@/components/refunds";

type ResponseType = "ACCEPTED" | "REJECTED" | "NEEDS_MODIFICATION";

type AuthStep = "email" | "otp" | "verified";

export default function ClientPortalQuotationPage() {
  const params = useParams();
  const quotationId = params.quotationId as string;
  const accessToken = params.token as string;

  const [authStep, setAuthStep] = useState<AuthStep>("email");
  const [email, setEmail] = useState("");
  const [otpCode, setOtpCode] = useState("");
  const [otpSent, setOtpSent] = useState(false);
  const [otpError, setOtpError] = useState<string | null>(null);
  const [quotation, setQuotation] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [modalType, setModalType] = useState<ResponseType>("ACCEPTED");
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [payment, setPayment] = useState<any | null>(null);
  const [showRefundForm, setShowRefundForm] = useState(false);
  const [viewId, setViewId] = useState<string | null>(null);
  const viewStartTimeRef = useRef<number | null>(null);
  const [linkValidated, setLinkValidated] = useState(false);
  const [showAcceptedPopup, setShowAcceptedPopup] = useState(false);
  const [acceptedPopupShown, setAcceptedPopupShown] = useState(false);

  // Validate access link on page load
  useEffect(() => {
    if (quotationId && accessToken && !linkValidated) {
      validateAccessLink();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [quotationId, accessToken]);

  const validateAccessLink = async () => {
    try {
      setLoading(true);
      setError(null);
      // Validate the access link without loading the full quotation
      const res = await ClientPortalApi.validateLink(quotationId, accessToken);
      if (res.success) {
        setLinkValidated(true);
        // Pre-fill email from link if available
        if (res.clientEmail) {
          setEmail(res.clientEmail);
        }
      } else {
        setError(res.error || "Invalid or expired access link. Please contact your sales representative.");
        setLinkValidated(false);
      }
    } catch (err: any) {
      const errorMessage = err.message || err.error || "Invalid or expired access link. Please contact your sales representative.";
      setError(errorMessage);
      setLinkValidated(false);
    } finally {
      setLoading(false);
    }
  };

  // Track page view time
  useEffect(() => {
    if (authStep === "verified" && quotation && !viewId) {
      // Start tracking
      ClientPortalApi.startPageView(quotationId, accessToken, email)
        .then((res) => {
          setViewId(res.viewId);
          viewStartTimeRef.current = Date.now();
        })
        .catch((err) => console.error("Failed to start page view tracking:", err));

      // End tracking when page unloads
      const handleBeforeUnload = () => {
        if (viewId && viewStartTimeRef.current) {
          // Use sendBeacon for reliable tracking on page unload
          const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || "";
          navigator.sendBeacon(
            `${API_BASE}/api/v1/client-portal/quotations/${quotationId}/${accessToken}/end-view`,
            JSON.stringify({ viewId })
          );
        }
      };

      window.addEventListener("beforeunload", handleBeforeUnload);
      return () => {
        window.removeEventListener("beforeunload", handleBeforeUnload);
        if (viewId) {
          ClientPortalApi.endPageView(quotationId, accessToken, viewId).catch(console.error);
        }
      };
    }
  }, [authStep, quotation, viewId, quotationId, accessToken, email]);

  const handleRequestOtp = async () => {
    if (!email || !email.includes("@")) {
      setOtpError("Please enter a valid email address");
      return;
    }

    try {
      setLoading(true);
      setOtpError(null);
      await ClientPortalApi.requestOtp(quotationId, accessToken, email);
      setOtpSent(true);
      setAuthStep("otp");
    } catch (err: any) {
      setOtpError(err.message || "Failed to send OTP. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyOtp = async () => {
    if (!otpCode || otpCode.length !== 6) {
      setOtpError("Please enter a valid 6-digit OTP code");
      return;
    }

    try {
      setLoading(true);
      setOtpError(null);
      await ClientPortalApi.verifyOtp(quotationId, accessToken, email, otpCode);
      setAuthStep("verified");
      await loadQuotation();
    } catch (err: any) {
      setOtpError(err.message || "Invalid OTP code. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const loadQuotation = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await ClientPortalApi.getQuotation(quotationId, accessToken);
      setQuotation(res.data);
      
      // Try to load payment if quotation is accepted
      if (res.data?.status === "ACCEPTED") {
        // Show popup if not already shown
        if (!acceptedPopupShown) {
          setShowAcceptedPopup(true);
          setAcceptedPopupShown(true);
        }
        
        try {
          const paymentRes = await PaymentsApi.getByQuotation(quotationId);
          if (paymentRes?.data && paymentRes.data.length > 0) {
            setPayment(paymentRes.data[0]);
          }
        } catch (err) {
          // Payment not found or not accessible - that's okay
          console.warn("Could not load payment:", err);
        }
      }
    } catch (err: any) {
      setError(err.message || "Unable to load the quotation. The link may be invalid or expired.");
    } finally {
      setLoading(false);
    }
  };

  const openModal = (type: ResponseType) => {
    setModalType(type);
    setModalOpen(true);
    setSuccessMessage(null);
  };

  const handleResponse = async (payload: { responseType: ResponseType; clientName?: string; clientEmail?: string; responseMessage?: string }) => {
    try {
      await ClientPortalApi.submitResponse(quotationId, accessToken, payload);
      setSuccessMessage("Thank you! Your response has been submitted.");
      setModalOpen(false);
    } catch (err: any) {
      alert(err.message || "Failed to submit response.");
    }
  };

  const handleDownload = () => ClientPortalApi.downloadPdf(quotationId, accessToken);

  const handleRequestPaymentLink = async () => {
    try {
      setLoading(true);
      // Initiate payment request
      const paymentRequest = await PaymentsApi.initiate({
        quotationId: quotationId,
        amount: quotation?.totalAmount || 0,
        currency: "INR",
        description: `Payment for Quotation #${quotation?.quotationNumber}`,
      });
      
      setSuccessMessage("Payment link has been requested. You will receive it via email shortly.");
      setShowAcceptedPopup(false);
      
      // Reload quotation to get payment info
      await loadQuotation();
    } catch (err: any) {
      alert(err.message || "Failed to request payment link. Please contact your sales representative.");
    } finally {
      setLoading(false);
    }
  };

  const handleViewDetails = () => {
    setShowAcceptedPopup(false);
    // Scroll to payment section if it exists
    setTimeout(() => {
      const paymentSection = document.getElementById("payment-section");
      if (paymentSection) {
        paymentSection.scrollIntoView({ behavior: "smooth" });
      }
    }, 100);
  };

  // Loading or error state
  if (loading && !linkValidated) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-16">
        <div className="rounded-lg bg-white px-6 py-8 text-center shadow">Validating access link...</div>
      </div>
    );
  }

  if (error && !linkValidated) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-16">
        <div className="max-w-md rounded-lg bg-white px-6 py-8 text-center text-meta-1 shadow">
          {error}
          <p className="mt-3 text-sm text-gray-500">
            Please contact your sales representative for assistance.
          </p>
        </div>
      </div>
    );
  }

  // Email Input Step
  if (authStep === "email" && linkValidated) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-16">
        <div className="w-full max-w-md rounded-lg bg-white p-8 shadow-lg">
          <h1 className="mb-2 text-2xl font-semibold text-black">Access Your Quotation</h1>
          <p className="mb-6 text-sm text-gray-600">
            Please enter your email address to receive a verification code.
          </p>
          
          {otpError && (
            <div className="mb-4 rounded border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700">
              {otpError}
            </div>
          )}

          <div className="mb-4">
            <label htmlFor="email" className="mb-2 block text-sm font-medium text-gray-700">
              Email Address
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              onKeyPress={(e) => e.key === "Enter" && handleRequestOtp()}
              className="w-full rounded border border-gray-300 px-4 py-2 text-black focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
              placeholder="your.email@example.com"
              disabled={loading}
            />
          </div>

          <button
            onClick={handleRequestOtp}
            disabled={loading || !email}
            className="w-full rounded bg-primary px-4 py-2 font-semibold text-white hover:bg-primary/90 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            {loading ? "Sending..." : "Send Verification Code"}
          </button>
        </div>
      </div>
    );
  }

  // OTP Verification Step
  if (authStep === "otp") {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-16">
        <div className="w-full max-w-md rounded-lg bg-white p-8 shadow-lg">
          <h1 className="mb-2 text-2xl font-semibold text-black">Enter Verification Code</h1>
          <p className="mb-6 text-sm text-gray-600">
            We've sent a 6-digit code to <strong>{email}</strong>. Please enter it below.
          </p>
          
          {otpError && (
            <div className="mb-4 rounded border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700">
              {otpError}
            </div>
          )}

          <div className="mb-4">
            <label htmlFor="otp" className="mb-2 block text-sm font-medium text-gray-700">
              Verification Code
            </label>
            <input
              id="otp"
              type="text"
              value={otpCode}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, "").slice(0, 6);
                setOtpCode(value);
              }}
              onKeyPress={(e) => e.key === "Enter" && handleVerifyOtp()}
              className="w-full rounded border border-gray-300 px-4 py-2 text-center text-2xl font-mono tracking-widest text-black focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
              placeholder="000000"
              maxLength={6}
              disabled={loading}
              autoFocus
            />
          </div>

          <div className="mb-4 flex gap-2">
            <button
              onClick={handleVerifyOtp}
              disabled={loading || otpCode.length !== 6}
              className="flex-1 rounded bg-primary px-4 py-2 font-semibold text-white hover:bg-primary/90 disabled:bg-gray-400 disabled:cursor-not-allowed"
            >
              {loading ? "Verifying..." : "Verify Code"}
            </button>
            <button
              onClick={() => {
                setAuthStep("email");
                setOtpCode("");
                setOtpError(null);
              }}
              disabled={loading}
              className="rounded border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50 disabled:bg-gray-100"
            >
              Back
            </button>
          </div>

          <button
            onClick={handleRequestOtp}
            disabled={loading}
            className="w-full text-sm text-primary hover:underline disabled:text-gray-400"
          >
            Resend Code
          </button>
        </div>
      </div>
    );
  }

  // Loading State
  if (loading && !quotation) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-16">
        <div className="rounded-lg bg-white px-6 py-8 text-center shadow">Loading your quotation...</div>
      </div>
    );
  }

  // Error State
  if (error || !quotation) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-16">
        <div className="max-w-md rounded-lg bg-white px-6 py-8 text-center text-meta-1 shadow">
          {error || "Quotation not found"}
          <p className="mt-3 text-sm text-gray-500">
            Please contact your sales representative for assistance.
          </p>
        </div>
      </div>
    );
  }

  // Quotation Display (after verification)
  return (
    <div className="min-h-screen bg-gray-50 px-4 py-10">
      <div className="mx-auto max-w-5xl rounded-2xl bg-white p-6 shadow-lg">
        <header className="mb-8 border-b border-gray-200 pb-6 text-center">
          <h1 className="text-2xl font-semibold text-black">Quotation #{quotation.quotationNumber}</h1>
          <p className="text-sm text-gray-500">
            Issued on {formatDate(quotation.quotationDate)} • Valid until {formatDate(quotation.validUntil)}
          </p>
          <p className="mt-2 inline-flex rounded-full bg-gray-100 px-3 py-1 text-xs font-medium">
            Status: {getStatusLabel(quotation.status)}
          </p>
          {successMessage && (
            <div className="mt-4 rounded border border-success bg-success/10 px-4 py-3 text-success">
              {successMessage}
            </div>
          )}
        </header>

        <section className="mb-6 grid grid-cols-1 gap-6 md:grid-cols-2">
          <div className="rounded-lg border border-gray-200 p-4">
            <h2 className="mb-3 font-semibold text-black">Company</h2>
            <p className="text-sm text-gray-600">Your Company Name</p>
            <p className="text-sm text-gray-600">sales@example.com</p>
            <p className="text-sm text-gray-600">+1 (555) 123-4567</p>
          </div>
          <div className="rounded-lg border border-gray-200 p-4">
            <h2 className="mb-3 font-semibold text-black">Bill To</h2>
            <p className="text-sm text-gray-600">{quotation.clientName}</p>
          </div>
        </section>

        <section className="mb-6">
          <h2 className="mb-3 font-semibold text-black">Line Items</h2>
          <div className="overflow-x-auto rounded-lg border border-gray-200">
            <table className="w-full table-auto text-sm">
              <thead className="bg-gray-100 text-left text-xs uppercase text-gray-500">
                <tr>
                  <th className="px-4 py-3">Item</th>
                  <th className="px-4 py-3">Description</th>
                  <th className="px-4 py-3">Quantity</th>
                  <th className="px-4 py-3">Rate</th>
                  <th className="px-4 py-3">Amount</th>
                </tr>
              </thead>
              <tbody>
                {quotation.lineItems?.map((item: any) => (
                  <tr key={item.lineItemId} className="border-t border-gray-100">
                    <td className="px-4 py-3 font-medium text-black">{item.itemName}</td>
                    <td className="px-4 py-3 text-gray-600">{item.description || "—"}</td>
                    <td className="px-4 py-3">{item.quantity}</td>
                    <td className="px-4 py-3">{formatCurrency(item.unitRate)}</td>
                    <td className="px-4 py-3 font-semibold text-black">{formatCurrency(item.amount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-2">
          <div className="rounded-lg border border-gray-200 bg-gray-50 p-4">
            <h3 className="mb-2 font-semibold text-black">Summary</h3>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span>Subtotal</span>
                <span>{formatCurrency(quotation.subTotal)}</span>
              </div>
              {quotation.discountAmount > 0 && (
                <div className="flex justify-between">
                  <span>Discount ({quotation.discountPercentage}%)</span>
                  <span>-{formatCurrency(quotation.discountAmount)}</span>
                </div>
              )}
              {quotation.taxAmount > 0 && (
                <div className="flex justify-between">
                  <span>Tax</span>
                  <span>{formatCurrency(quotation.taxAmount)}</span>
                </div>
              )}
              <div className="flex justify-between border-t border-gray-200 pt-2 text-base font-semibold">
                <span>Total</span>
                <span className="text-primary">{formatCurrency(quotation.totalAmount)}</span>
              </div>
            </div>
          </div>
          {quotation.notes && (
            <div className="rounded-lg border border-gray-200 p-4">
              <h3 className="mb-2 font-semibold text-black">Notes</h3>
              <p className="text-sm text-gray-600 whitespace-pre-wrap">{quotation.notes}</p>
            </div>
          )}
        </section>

        {/* Payment & Refund Section */}
        {quotation.status && quotation.status === "ACCEPTED" && (
          <section id="payment-section" className="mb-6 rounded-lg border border-gray-200 p-4">
            <h3 className="mb-3 text-lg font-semibold text-black">Payment Information</h3>
            {payment ? (
              <>
                <div className="mb-4 space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Payment Status:</span>
                    <span className={`font-medium ${
                      payment.paymentStatus === "Success" ? "text-green-600" : 
                      payment.paymentStatus === "Pending" ? "text-yellow-600" : 
                      "text-red-600"
                    }`}>
                      {payment.paymentStatus}
                    </span>
                  </div>
                  {payment.paymentAmount && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">Amount Paid:</span>
                      <span className="font-medium text-black">{formatCurrency(payment.paymentAmount)}</span>
                    </div>
                  )}
                </div>
                
                {payment.paymentStatus === "Success" && (
                  <div>
                    <button
                      onClick={() => setShowRefundForm(true)}
                      className="rounded bg-orange-500 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-600"
                    >
                      Request Refund
                    </button>
                  </div>
                )}
              </>
            ) : (
              <div className="text-sm text-gray-600 dark:text-gray-400">
                <p>No payment information available yet. Click "ASK FOR PAYMENT LINK" to request a payment link.</p>
              </div>
            )}
          </section>
        )}

        <section className="rounded-lg border border-gray-200 p-4 text-center">
          <h3 className="mb-4 text-lg font-semibold text-black">Ready to proceed?</h3>
          <div className="flex flex-wrap justify-center gap-3">
            {quotation.status && quotation.status !== "ACCEPTED" && (
              <>
                <button
                  onClick={() => openModal("ACCEPTED")}
                  className="rounded bg-emerald-500 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-600"
                >
                  Accept Quotation
                </button>
                <button
                  onClick={() => openModal("REJECTED")}
                  className="rounded bg-rose-500 px-4 py-2 text-sm font-semibold text-white hover:bg-rose-600"
                >
                  Reject Quotation
                </button>
              </>
            )}
            <button
              onClick={handleDownload}
              className="rounded border border-stroke px-4 py-2 text-sm text-black hover:bg-gray-50"
            >
              Download PDF
            </button>
            <a
              href={`mailto:sales@example.com?subject=Quotation%20${quotation.quotationNumber}`}
              className="rounded border border-stroke px-4 py-2 text-sm text-black hover:bg-gray-50"
            >
              Contact Salesperson
            </a>
          </div>
        </section>
      </div>

      <ClientResponseModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        defaultType={modalType}
        onSubmit={handleResponse}
      />

      {/* Accepted Quotation Popup */}
      {showAcceptedPopup && quotation && quotation.status === "ACCEPTED" && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl dark:bg-boxdark">
            <div className="mb-4 text-center">
              <h3 className="mb-2 text-xl font-semibold text-black dark:text-white">
                Quotation Accepted!
              </h3>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Your quotation has been accepted. What would you like to do next?
              </p>
            </div>
            
            <div className="space-y-3">
              <button
                onClick={handleRequestPaymentLink}
                disabled={loading}
                className="w-full rounded-lg bg-primary px-4 py-3 text-sm font-semibold text-white hover:bg-primary/90 disabled:bg-gray-400 disabled:cursor-not-allowed"
              >
                {loading ? "Processing..." : "ASK FOR PAYMENT LINK"}
              </button>
              
              <button
                onClick={handleViewDetails}
                className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm font-semibold text-black hover:bg-gray-50 dark:border-gray-600 dark:text-white dark:hover:bg-gray-700"
              >
                DETAILS
              </button>
            </div>
            
            <button
              onClick={() => setShowAcceptedPopup(false)}
              className="mt-4 w-full text-sm text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
            >
              Close
            </button>
          </div>
        </div>
      )}

      {/* Refund Request Modal */}
      {showRefundForm && payment && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
            <h3 className="mb-4 text-lg font-semibold">Request Refund</h3>
            <RefundRequestForm
              paymentId={payment.paymentId}
              quotationId={quotationId}
              maxRefundAmount={payment.paymentAmount || quotation.totalAmount}
              onSuccess={() => {
                setShowRefundForm(false);
                setSuccessMessage("Your refund request has been submitted. We will review it shortly.");
                loadQuotation();
              }}
              onCancel={() => setShowRefundForm(false)}
            />
          </div>
        </div>
      )}
    </div>
  );
}
