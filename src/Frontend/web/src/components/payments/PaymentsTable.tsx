"use client";

import { useState } from "react";
import type { PaymentDto } from "@/types/payments";
import { PaymentsApi } from "@/lib/api";
import { PaymentStatusBadge } from "./PaymentStatusBadge";

interface PaymentsTableProps {
  payments: PaymentDto[];
  onRefresh?: () => void;
}

export function PaymentsTable({ payments, onRefresh }: PaymentsTableProps) {
  const [processing, setProcessing] = useState<string | null>(null);

  const handleRefund = async (payment: PaymentDto) => {
    if (!confirm(`Refund ₹${payment.amountPaid} for this payment?`)) return;

    try {
      setProcessing(payment.paymentId);
      await PaymentsApi.refund(payment.paymentId, {
        reason: "Requested by user",
      });
      onRefresh?.();
    } catch (err: any) {
      alert(err.message || "Failed to process refund");
    } finally {
      setProcessing(null);
    }
  };

  const handleCancel = async (payment: PaymentDto) => {
    if (!confirm("Cancel this payment?")) return;

    try {
      setProcessing(payment.paymentId);
      await PaymentsApi.cancel(payment.paymentId);
      onRefresh?.();
    } catch (err: any) {
      alert(err.message || "Failed to cancel payment");
    } finally {
      setProcessing(null);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Payment Reference
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Gateway
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Amount
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Date
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {payments.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-4 text-center text-gray-500">
                  No payments found
                </td>
              </tr>
            ) : (
              payments.map((payment) => (
                <tr key={payment.paymentId}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900">
                    {payment.paymentReference.substring(0, 12)}...
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {payment.paymentGateway}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    ₹{payment.amountPaid.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <PaymentStatusBadge status={payment.paymentStatus} />
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {payment.paymentDate
                      ? new Date(payment.paymentDate).toLocaleDateString()
                      : new Date(payment.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <div className="flex gap-2">
                      {payment.canBeRefunded && (
                        <button
                          onClick={() => handleRefund(payment)}
                          disabled={processing === payment.paymentId}
                          className="text-blue-600 hover:text-blue-900 disabled:opacity-50"
                        >
                          Refund
                        </button>
                      )}
                      {payment.canBeCancelled && (
                        <button
                          onClick={() => handleCancel(payment)}
                          disabled={processing === payment.paymentId}
                          className="text-red-600 hover:text-red-900 disabled:opacity-50"
                        >
                          Cancel
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

