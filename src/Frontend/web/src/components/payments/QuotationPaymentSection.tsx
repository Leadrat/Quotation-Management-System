"use client";

import { useState, useEffect } from "react";
import { PaymentsApi } from "@/lib/api";
import type { PaymentDto } from "@/types/payments";
import { PaymentStatusBadge } from "./PaymentStatusBadge";
import { PaymentModal } from "./PaymentModal";

interface QuotationPaymentSectionProps {
  quotationId: string;
  totalAmount: number;
  currency?: string;
}

export function QuotationPaymentSection({
  quotationId,
  totalAmount,
  currency = "INR",
}: QuotationPaymentSectionProps) {
  const [payments, setPayments] = useState<PaymentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);

  useEffect(() => {
    loadPayments();
  }, [quotationId]);

  const loadPayments = async () => {
    try {
      setLoading(true);
      const data = await PaymentsApi.getByQuotation(quotationId);
      setPayments(data);
    } catch (err) {
      console.error("Failed to load payments:", err);
    } finally {
      setLoading(false);
    }
  };

  const hasSuccessfulPayment = payments.some((p) => p.paymentStatus === 2); // Success

  if (loading) {
    return <div className="text-sm text-gray-500">Loading payment information...</div>;
  }

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold">Payment Information</h3>
        {!hasSuccessfulPayment && (
          <button
            onClick={() => setShowModal(true)}
            className="px-4 py-2 bg-primary text-white rounded-md hover:bg-primary-dark"
          >
            Initiate Payment
          </button>
        )}
      </div>

      {payments.length === 0 ? (
        <p className="text-sm text-gray-500">No payments found for this quotation.</p>
      ) : (
        <div className="space-y-3">
          {payments.map((payment) => (
            <div key={payment.paymentId} className="border rounded-lg p-4">
              <div className="flex items-center justify-between mb-2">
                <div>
                  <p className="text-sm font-medium text-gray-900">
                    {payment.paymentGateway} Payment
                  </p>
                  <p className="text-xs text-gray-500 font-mono">
                    {payment.paymentReference}
                  </p>
                </div>
                <PaymentStatusBadge status={payment.paymentStatus} />
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="text-gray-600">Amount:</span>
                <span className="font-semibold">
                  {payment.currency} {payment.amountPaid.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                </span>
              </div>
              {payment.paymentDate && (
                <div className="flex items-center justify-between text-sm mt-1">
                  <span className="text-gray-600">Date:</span>
                  <span className="text-gray-900">
                    {new Date(payment.paymentDate).toLocaleString()}
                  </span>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <PaymentModal
          quotationId={quotationId}
          amount={totalAmount}
          currency={currency}
          onSuccess={(payment) => {
            setPayments([...payments, payment]);
            setShowModal(false);
          }}
          onClose={() => setShowModal(false)}
        />
      )}
    </div>
  );
}

