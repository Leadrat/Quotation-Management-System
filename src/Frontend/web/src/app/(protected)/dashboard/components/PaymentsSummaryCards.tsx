"use client";

import { useEffect, useState } from "react";
import { PaymentsApi } from "@/lib/api";

export default function PaymentsSummaryCards() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [summary, setSummary] = useState<{ totalPaid: number; totalPending: number; totalRefunded: number; totalFailed: number; paidCount: number; pendingCount: number; refundedCount: number; failedCount: number } | null>(null);
  const [acceptedPending, setAcceptedPending] = useState<{ amount: number; quotationCount: number } | null>(null);

  useEffect(() => {
    (async () => {
      try {
        setLoading(true);
        setError(null);
        const res = await PaymentsApi.getStats();
        if (res.success) {
          setSummary(res.summary);
          setAcceptedPending(res.acceptedPending ?? null);
        } else {
          setError("Failed to load payment stats");
        }
      } catch (e: any) {
        setError(e?.message ?? "Failed to load payment stats");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const Card = ({ title, amount, count, color }: { title: string; amount: number; count: number; color: string }) => (
    <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] md:p-6">
      <div className={`flex items-center justify-center w-12 h-12 rounded-xl ${color}`}></div>
      <div className="mt-5">
        <span className="text-sm text-gray-500 dark:text-gray-400">{title}</span>
        <div className="mt-2 flex items-end justify-between">
          <h4 className="font-bold text-gray-800 text-title-sm dark:text-white/90">₹{(amount || 0).toLocaleString("en-IN", { minimumFractionDigits: 2 })}</h4>
          <span className="text-xs text-gray-500 dark:text-gray-400">{count || 0} payments</span>
        </div>
      </div>
    </div>
  );

  if (loading) return <div className="rounded-2xl border border-gray-200 p-5 text-gray-500 dark:border-gray-800 dark:text-gray-400">Loading payment stats...</div>;
  if (error) return <div className="rounded-2xl border border-red-200 bg-red-50 p-5 text-red-700 dark:border-red-800 dark:bg-red-900/20">{error}</div>;
  if (!summary) return null;

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-2 mb-6">
      <Card title="Paid" amount={summary.totalPaid} count={summary.paidCount} color="bg-green-100 dark:bg-green-900/20" />
      <Card
        title="Pending"
        amount={acceptedPending?.amount ?? 0}
        count={acceptedPending?.quotationCount ?? 0}
        color="bg-yellow-100 dark:bg-yellow-900/20"
      />
    </div>
  );
}
