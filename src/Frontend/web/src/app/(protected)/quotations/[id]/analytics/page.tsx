"use client";
import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { QuotationsApi } from "@/lib/api";
import { formatCurrency, formatDateTime, getStatusLabel } from "@/utils/quotationFormatter";

type AccessLink = {
  clientEmail: string;
  sentAt?: string;
  firstViewedAt?: string;
  lastViewedAt?: string;
  viewCount?: number;
  viewUrl: string;
  isActive: boolean;
};

export default function QuotationAnalyticsPage() {
  const params = useParams();
  const quotationId = params.id as string;
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [quotation, setQuotation] = useState<any>(null);
  const [accessLink, setAccessLink] = useState<AccessLink | null>(null);
  const [statusHistory, setStatusHistory] = useState<any[]>([]);

  useEffect(() => {
    if (quotationId) {
      loadData();
    }
  }, [quotationId]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const [quotationRes, historyRes, linkRes] = await Promise.all([
        QuotationsApi.get(quotationId),
        QuotationsApi.statusHistory(quotationId),
        QuotationsApi.accessLink(quotationId).catch(() => undefined),
      ]);
      setQuotation(quotationRes.data);
      setStatusHistory(historyRes?.data || []);
      setAccessLink(linkRes && "data" in (linkRes || {}) ? linkRes.data : null);
    } catch (err: any) {
      setError(err.message || "Failed to load analytics");
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = () => QuotationsApi.downloadPdf(quotationId);

  const handleReminder = async () => {
    if (!accessLink?.clientEmail) return;
    try {
      await QuotationsApi.resend(quotationId, {
        recipientEmail: accessLink.clientEmail,
        customMessage: "Just checking in—please review the quotation when you have a moment.",
      });
      alert("Reminder sent successfully.");
    } catch (err: any) {
      alert(err.message || "Failed to send reminder.");
    }
  };

  if (loading) {
    return (
      <div className="rounded-sm border border-stroke bg-white p-6 shadow-default dark:border-strokedark dark:bg-boxdark">
        <p>Loading analytics...</p>
      </div>
    );
  }

  if (error || !quotation) {
    return (
      <div className="rounded-sm border border-stroke bg-white p-6 text-center text-meta-1 shadow-default dark:border-strokedark dark:bg-boxdark">
        {error || "Quotation not found"}
      </div>
    );
  }

  const metrics = [
    { label: "Sent On", value: accessLink?.sentAt ? formatDateTime(accessLink.sentAt) : "Not sent" },
    { label: "First Viewed", value: accessLink?.firstViewedAt ? formatDateTime(accessLink.firstViewedAt) : "—" },
    { label: "View Count", value: accessLink?.viewCount ?? 0 },
    { label: "Last Viewed", value: accessLink?.lastViewedAt ? formatDateTime(accessLink.lastViewedAt) : "—" },
    { label: "Status", value: getStatusLabel(quotation.status) },
    { label: "Total Amount", value: formatCurrency(quotation.totalAmount) },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-xl font-semibold text-black dark:text-white">Quotation Analytics</h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">Quotation {quotation.quotationNumber}</p>
        </div>
        <div className="flex gap-2">

          <button
            onClick={handleReminder}
            className="rounded bg-primary px-4 py-2 text-sm text-white hover:bg-opacity-90 disabled:opacity-50"
            disabled={!accessLink?.clientEmail}
          >
            Send Reminder
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {metrics.map((metric) => (
          <div key={metric.label} className="rounded border border-stroke bg-white p-4 shadow dark:border-strokedark dark:bg-boxdark">
            <p className="text-xs uppercase text-gray-500">{metric.label}</p>
            <p className="mt-1 text-lg font-semibold text-black dark:text-white">{metric.value as any}</p>
          </div>
        ))}
      </div>

      <div className="rounded border border-stroke bg-white p-4 shadow dark:border-strokedark dark:bg-boxdark">
        <h3 className="mb-4 text-base font-semibold text-black dark:text-white">Status History</h3>
        <div className="max-h-80 overflow-y-auto">
          <table className="w-full table-auto text-sm">
            <thead>
              <tr className="bg-gray-2 text-left text-xs uppercase text-gray-500 dark:bg-meta-4">
                <th className="px-3 py-2">Status</th>
                <th className="px-3 py-2">Changed At</th>
                <th className="px-3 py-2">Changed By</th>
                <th className="px-3 py-2">Reason</th>
              </tr>
            </thead>
            <tbody>
              {statusHistory.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-3 py-4 text-center text-gray-500">
                    No history available.
                  </td>
                </tr>
              )}
              {statusHistory.map((entry) => (
                <tr key={entry.historyId} className="border-b border-stroke text-sm dark:border-strokedark">
                  <td className="px-3 py-2 font-medium">{entry.newStatus}</td>
                  <td className="px-3 py-2">{formatDateTime(entry.changedAt)}</td>
                  <td className="px-3 py-2">{entry.changedByUserName || "System"}</td>
                  <td className="px-3 py-2 text-gray-500">{entry.reason || "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <div className="rounded border border-stroke bg-white p-4 text-sm shadow dark:border-strokedark dark:bg-boxdark">
        <h3 className="mb-3 text-base font-semibold text-black dark:text-white">Access Link</h3>
        {accessLink ? (
          <div className="space-y-2">
            <p>
              <span className="font-medium text-black dark:text-white">Recipient:</span> {accessLink.clientEmail}
            </p>
            <p>
              <span className="font-medium text-black dark:text-white">Link:</span>{" "}
              <button
                className="text-primary underline-offset-2 hover:underline"
                onClick={() => navigator.clipboard?.writeText(accessLink.viewUrl)}
              >
                Copy link
              </button>
            </p>
            <p>
              <span className="font-medium text-black dark:text-white">Status:</span>{" "}
              {accessLink.isActive ? "Active" : "Inactive"}
            </p>
          </div>
        ) : (
          <p className="text-gray-500">No access link available.</p>
        )}
      </div>
    </div>
  );
}

