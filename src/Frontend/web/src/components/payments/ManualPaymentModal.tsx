"use client";

import { useState, useEffect, useMemo } from "react";
import { PaymentsApi } from "@/lib/api";

type Props = {
  quotationId: string;
  defaultCurrency?: string;
  payment?: import("@/types/payments").PaymentDto;
  onSuccess: () => void;
  onClose: () => void;
};

export default function ManualPaymentModal({ quotationId, defaultCurrency = "INR", payment, onSuccess, onClose }: Props) {
  const [amountReceived, setAmountReceived] = useState<string>(payment ? String(payment.amountPaid) : "");
  const [currency, setCurrency] = useState<string>(payment ? payment.currency : defaultCurrency);
  const [method, setMethod] = useState<string>("Cash");
  const [paymentDate, setPaymentDate] = useState<string>(payment?.paymentDate ? new Date(payment.paymentDate).toISOString().slice(0,10) : new Date().toISOString().slice(0, 10));
  const [remarks, setRemarks] = useState<string>("");
  const [error, setError] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(false);
  const [summary, setSummary] = useState<{ totalAmount: number; paidNet: number; outstanding: number } | null>(null);
  const [summaryError, setSummaryError] = useState<string | null>(null);

  useEffect(() => {
    if (payment) return; // Only fetch for create mode; edit mode already has amount
    (async () => {
      try {
        setSummaryError(null);
        const res = await PaymentsApi.getOutstanding(quotationId);
        if (res.success) {
          setSummary(res.data);
        } else {
          setSummaryError("Failed to load outstanding amount");
        }
      } catch (e: any) {
        setSummaryError(e?.message ?? "Failed to load outstanding amount");
      }
    })();
  }, [quotationId, payment]);

  const outstandingAfter = useMemo(() => {
    if (!summary) return null;
    const amt = parseFloat(amountReceived || "0");
    if (isNaN(amt) || amt <= 0) return summary.outstanding;
    const remaining = summary.outstanding - amt;
    return remaining < 0 ? 0 : remaining;
  }, [summary, amountReceived]);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    const amt = parseFloat(amountReceived);
    if (isNaN(amt) || amt <= 0) {
      setError("Enter a valid amount greater than 0");
      return;
    }

    try {
      setLoading(true);
      if (payment) {
        await PaymentsApi.updateManual(payment.paymentId, {
          amountReceived: amt,
          currency,
          method,
          paymentDate,
          remarks,
        });
      } else {
        await PaymentsApi.createManual(quotationId, {
          amountReceived: amt,
          currency,
          method,
          paymentDate,
          remarks,
        });
      }
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err?.message ?? "Failed to add manual payment");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
      <div className="w-full max-w-lg rounded-2xl bg-white shadow-2xl dark:bg-gray-900">
        <div className="border-b border-gray-200 px-6 py-4 dark:border-gray-800">
          <div className="flex items-center justify-between">
            <h3 className="text-xl font-bold text-gray-900 dark:text-white">{payment ? "Edit Manual Payment" : "Record Manual Payment"}</h3>
            <button onClick={onClose} className="rounded-lg p-2 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-800 dark:hover:text-gray-300" aria-label="Close">
              <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
            </button>
          </div>
        </div>
        <form onSubmit={submit} className="px-6 py-5 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Amount Received</label>
            <input
              type="number"
              inputMode="decimal"
              min="0"
              step="0.01"
              value={amountReceived}
              onChange={(e) => setAmountReceived(e.target.value)}
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary/60"
              placeholder="0.00"
              required
            />
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Currency</label>
              <input
                type="text"
                value={currency}
                onChange={(e) => setCurrency(e.target.value.toUpperCase())}
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm uppercase dark:border-gray-700 dark:bg-gray-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary/60"
                maxLength={3}
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Method</label>
              <select
                value={method}
                onChange={(e) => setMethod(e.target.value)}
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary/60"
              >
                <option>Cash</option>
                <option>BankTransfer</option>
                <option>Cheque</option>
                <option>Other</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Payment Date</label>
            <input
              type="date"
              value={paymentDate}
              onChange={(e) => setPaymentDate(e.target.value)}
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary/60"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Remarks</label>
            <textarea
              value={remarks}
              onChange={(e) => setRemarks(e.target.value)}
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary/60"
              rows={3}
              placeholder="Optional notes"
            />
          </div>

          {error && (
            <p className="text-sm text-red-600" role="alert">
              {error}
            </p>
          )}

          {!payment && (
            <div className="rounded-md bg-gray-50 p-3 text-xs text-gray-700 dark:bg-gray-800 dark:text-gray-200">
              {summaryError && <p className="mb-1 text-red-600">{summaryError}</p>}
              {summary && (
                <div className="space-y-1">
                  <div className="flex justify-between">
                    <span>Total amount</span>
                    <span className="font-medium">₹{summary.totalAmount.toLocaleString("en-IN", { minimumFractionDigits: 2 })}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Already paid (net)</span>
                    <span className="font-medium">₹{summary.paidNet.toLocaleString("en-IN", { minimumFractionDigits: 2 })}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Outstanding before this payment</span>
                    <span className="font-semibold">₹{summary.outstanding.toLocaleString("en-IN", { minimumFractionDigits: 2 })}</span>
                  </div>
                  {outstandingAfter !== null && (
                    <div className="flex justify-between border-t border-gray-200 pt-1 mt-1 dark:border-gray-700">
                      <span>Outstanding after this payment</span>
                      <span className="font-semibold text-primary">₹{outstandingAfter.toLocaleString("en-IN", { minimumFractionDigits: 2 })}</span>
                    </div>
                  )}
                </div>
              )}
            </div>
          )}
          <div className="flex items-center justify-end gap-3 pt-2">
            <button type="button" onClick={onClose} className="rounded-md border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:text-gray-200 dark:hover:bg-gray-800" disabled={loading}>Cancel</button>
            <button type="submit" className="rounded-md border-2 border-blue-500 bg-white px-4 py-2 text-sm text-black disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-blue-500/60" disabled={loading}>{loading ? "Saving..." : "Save Payment"}</button>
          </div>
        </form>
      </div>
    </div>
  );
}
