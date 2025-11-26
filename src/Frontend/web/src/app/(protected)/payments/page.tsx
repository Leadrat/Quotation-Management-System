"use client";

import { useEffect, useMemo, useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { PaymentsApi, QuotationsApi } from "@/lib/api";
import ManualPaymentModal from "@/components/payments/ManualPaymentModal";
import { PaymentStatusBadge } from "@/components/payments/PaymentStatusBadge";
import { formatCurrency, getStatusLabel } from "@/utils/quotationFormatter";

type PaymentDto = import("@/types/payments").PaymentDto;
type QuotationSummary = { quotationId: string; quotationNumber: string; clientName: string; totalAmount: number; currency: string; status: string };

export default function PaymentsPage() {
  const [quotations, setQuotations] = useState<QuotationSummary[]>([]);
  const [selectedQuotationId, setSelectedQuotationId] = useState<string | null>(null);
  const [payments, setPayments] = useState<PaymentDto[]>([]);
  const [outstanding, setOutstanding] = useState<{ totalAmount: number; paidNet: number; outstanding: number } | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [manualModalOpen, setManualModalOpen] = useState(false);
  const [editingPayment, setEditingPayment] = useState<PaymentDto | null>(null);

  // Load a lightweight list of recent quotations for selection
  useEffect(() => {
    (async () => {
      try {
        const res = await QuotationsApi.list({ pageNumber: 1, pageSize: 50 });
        if (res.success && res.data) {
          const mapped: QuotationSummary[] = res.data.map((q: any) => ({
            quotationId: q.quotationId,
            quotationNumber: q.quotationNumber,
            clientName: q.clientName,
            totalAmount: q.totalAmount,
            currency: q.currency || "INR",
            status: q.status,
          }));
          setQuotations(mapped);
          if (mapped.length > 0) setSelectedQuotationId(mapped[0].quotationId);
        }
      } catch (e) {
        // ignore list failure; page will show error if nothing selected
      }
    })();
  }, []);

  const selectedQuotation = useMemo(
    () => quotations.find((q) => q.quotationId === selectedQuotationId) || null,
    [quotations, selectedQuotationId]
  );

  const loadForQuotation = async (quotationId: string | null) => {
    if (!quotationId) return;
    setLoading(true);
    setError(null);
    try {
      const [paymentsRes, outstandingRes] = await Promise.all([
        PaymentsApi.getByQuotation(quotationId).catch(() => [] as any),
        PaymentsApi.getOutstanding(quotationId).catch(() => null as any),
      ]);

      setPayments(Array.isArray(paymentsRes) ? paymentsRes : paymentsRes.data || []);
      if (outstandingRes && outstandingRes.success) {
        setOutstanding(outstandingRes.data);
      } else {
        setOutstanding(null);
      }
    } catch (e: any) {
      setError(e?.message ?? "Failed to load payments");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadForQuotation(selectedQuotationId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedQuotationId]);

  const statusLabel = useMemo(() => {
    if (!selectedQuotation) return "";
    return getStatusLabel(selectedQuotation.status) || selectedQuotation.status;
  }, [selectedQuotation]);

  const isFullyPaid = useMemo(() => {
    if (!outstanding) return false;
    return outstanding.outstanding <= 0.009; // handle rounding
  }, [outstanding]);

  const openCreateModal = () => {
    setEditingPayment(null);
    setManualModalOpen(true);
  };

  const openEditModal = (p: PaymentDto) => {
    setEditingPayment(p);
    setManualModalOpen(true);
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Payments" />

      <ComponentCard title="Payments" desc="Select a quotation and record or update manual payments.">
        <div className="flex flex-col gap-6">
          {/* Quotation selector */}
          <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
            <div className="md:col-span-2">
              <label className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">Select Quotation</label>
              <select
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary/60"
                value={selectedQuotationId ?? ""}
                onChange={(e) => setSelectedQuotationId(e.target.value || null)}
              >
                {quotations.length === 0 && <option value="">No quotations found</option>}
                {quotations.map((q) => (
                  <option key={q.quotationId} value={q.quotationId}>
                    {q.quotationNumber} • {q.clientName}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex items-end justify-start md:justify-end">
              <button
                type="button"
                disabled={!selectedQuotationId}
                onClick={openCreateModal}
                className="rounded-md border-2 border-blue-500 px-4 py-2 text-sm font-medium text-black bg-white disabled:opacity-50 dark:bg-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-400"
              >
                + Record Manual Payment
              </button>
            </div>
          </div>

          {/* Summary */}
          {selectedQuotation && (
            <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
              <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03]">
                <div className="text-xs text-gray-500 dark:text-gray-400">Quotation</div>
                <div className="mt-1 text-sm font-semibold text-gray-800 dark:text-white/90">{selectedQuotation.quotationNumber}</div>
                <div className="text-xs text-gray-500 dark:text-gray-400">{selectedQuotation.clientName}</div>
              </div>
              <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03]">
                <div className="text-xs text-gray-500 dark:text-gray-400">Total Amount</div>
                <div className="mt-1 text-lg font-bold text-gray-800 dark:text-white/90">{formatCurrency(selectedQuotation.totalAmount)}</div>
              </div>
              <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03]">
                <div className="text-xs text-gray-500 dark:text-gray-400">Received</div>
                <div className="mt-1 text-lg font-bold text-green-600 dark:text-green-400">
                  {outstanding ? formatCurrency(outstanding.paidNet) : "—"}
                </div>
              </div>
              <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03]">
                <div className="flex items-center justify-between">
                  <div className="text-xs text-gray-500 dark:text-gray-400">Status</div>
                  {isFullyPaid && (
                    <span className="rounded-full bg-green-100 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-green-700 dark:bg-green-900/40 dark:text-green-300">
                      Done
                    </span>
                  )}
                </div>
                <div className="mt-1 text-lg font-bold text-gray-800 dark:text-white/90">
                  {outstanding ? formatCurrency(outstanding.outstanding) : "—"}
                </div>
                <div className="text-xs text-gray-500 dark:text-gray-400">Pending amount</div>
              </div>
            </div>
          )}

          {/* Payments list for selected quotation */}
          <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03]">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-gray-800 dark:text-white/90">Payments</h3>
              {selectedQuotation && (
                <span className="text-xs text-gray-500 dark:text-gray-400">{statusLabel}</span>
              )}
            </div>

            {error && (
              <div className="mb-3 rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700" role="alert">
                {error}
              </div>
            )}

            {loading ? (
              <div className="py-10 text-center text-gray-500">Loading...</div>
            ) : !selectedQuotationId ? (
              <div className="py-10 text-center text-gray-500">Select a quotation to view payments.</div>
            ) : payments.length === 0 ? (
              <div className="py-10 text-center text-gray-500">No payments recorded for this quotation.</div>
            ) : (
              <div className="max-w-full overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50 dark:bg-gray-800/60">
                    <tr>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Gateway</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Reference</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Amount</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200 bg-white dark:divide-gray-800 dark:bg-gray-900/40">
                    {payments.map((p) => (
                      <tr key={p.paymentId}>
                        <td className="px-4 py-2 whitespace-nowrap text-sm text-gray-700 dark:text-gray-200">
                          {p.paymentDate
                            ? new Date(p.paymentDate).toLocaleDateString()
                            : new Date(p.createdAt).toLocaleDateString()}
                        </td>
                        <td className="px-4 py-2 whitespace-nowrap text-sm text-gray-700 dark:text-gray-200">{p.paymentGateway}</td>
                        <td className="px-4 py-2 whitespace-nowrap text-xs font-mono text-gray-500">
                          {p.paymentReference.substring(0, 10)}...
                        </td>
                        <td className="px-4 py-2 whitespace-nowrap text-sm font-semibold text-gray-900 dark:text-white">
                          {formatCurrency(p.amountPaid)}
                        </td>
                        <td className="px-4 py-2 whitespace-nowrap text-sm">
                          <PaymentStatusBadge status={p.paymentStatus} />
                        </td>
                        <td className="px-4 py-2 whitespace-nowrap text-sm">
                          {p.paymentGateway === "Manual" && (
                            <button
                              type="button"
                              onClick={() => openEditModal(p)}
                              className="text-xs font-medium text-primary hover:underline"
                            >
                              Edit
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      </ComponentCard>

      {manualModalOpen && selectedQuotationId && (
        <ManualPaymentModal
          quotationId={selectedQuotationId}
          defaultCurrency={selectedQuotation?.currency || "INR"}
          payment={editingPayment ?? undefined}
          onSuccess={async () => {
            await loadForQuotation(selectedQuotationId);
            setManualModalOpen(false);
            setEditingPayment(null);
          }}
          onClose={() => {
            setManualModalOpen(false);
            setEditingPayment(null);
          }}
        />
      )}
    </>
  );
}

