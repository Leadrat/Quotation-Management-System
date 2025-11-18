"use client";

import { useState } from "react";
import { PaymentsApi } from "@/lib/api";
import type { InitiatePaymentRequest, PaymentDto } from "@/types/payments";

interface PaymentModalProps {
  quotationId: string;
  amount: number;
  currency?: string;
  onSuccess: (payment: PaymentDto) => void;
  onClose: () => void;
}

export function PaymentModal({ quotationId, amount, currency = "INR", onSuccess, onClose }: PaymentModalProps) {
  const [selectedGateway, setSelectedGateway] = useState<string>("Stripe");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const request: InitiatePaymentRequest = {
        quotationId,
        paymentGateway: selectedGateway,
        amount,
        currency,
      };

      const payment = await PaymentsApi.initiate(request);
      
      if (payment.paymentUrl) {
        // Redirect to payment gateway
        window.location.href = payment.paymentUrl;
      } else {
        onSuccess(payment);
        onClose();
      }
    } catch (err: any) {
      setError(err.message || "Failed to initiate payment");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <h2 className="text-xl font-bold mb-4">Initiate Payment</h2>

        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Payment Gateway
            </label>
            <select
              value={selectedGateway}
              onChange={(e) => setSelectedGateway(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary"
              required
            >
              <option value="Stripe">Stripe</option>
              <option value="Razorpay">Razorpay</option>
            </select>
          </div>

          <div className="mb-4">
            <div className="bg-gray-50 p-4 rounded-md">
              <div className="flex justify-between mb-2">
                <span className="text-gray-600">Amount:</span>
                <span className="font-bold">
                  {currency} {amount.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                </span>
              </div>
            </div>
          </div>

          {error && (
            <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          <div className="flex gap-3">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
              disabled={loading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="flex-1 px-4 py-2 bg-primary text-white rounded-md hover:bg-primary-dark disabled:opacity-50"
              disabled={loading}
            >
              {loading ? "Processing..." : "Proceed to Payment"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

