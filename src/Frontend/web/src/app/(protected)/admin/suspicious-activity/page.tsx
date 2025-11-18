"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { ClientHistoryApi } from "@/lib/api";

export default function SuspiciousActivityPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [flags, setFlags] = useState<any[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [minScore, setMinScore] = useState(7);
  const [status, setStatus] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  const totalPages = Math.ceil(total / pageSize);

  useEffect(() => {
    loadData();
  }, [pageNumber, pageSize, minScore, status, dateFrom, dateTo]);

  async function loadData() {
    setLoading(true);
    setError(null);
    try {
      const res = await ClientHistoryApi.getSuspiciousActivity({
        pageNumber,
        pageSize,
        minScore,
        status: status || undefined,
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
      });
      setFlags(res.data || []);
      setTotal(res.totalCount);
    } catch (e: any) {
      setError(e.message || "Failed to load suspicious activity");
    } finally {
      setLoading(false);
    }
  }

  function formatDate(date: string) {
    return new Date(date).toLocaleString();
  }

  function getScoreColor(score: number) {
    if (score >= 9) return "bg-red-100 text-red-800";
    if (score >= 7) return "bg-orange-100 text-orange-800";
    return "bg-yellow-100 text-yellow-800";
  }

  function getStatusColor(status: string) {
    if (status === "OPEN") return "bg-red-100 text-red-800";
    if (status === "REVIEWED") return "bg-blue-100 text-blue-800";
    if (status === "DISMISSED") return "bg-gray-100 text-gray-800";
    return "bg-gray-100 text-gray-800";
  }

  return (
    <div className="p-6">
      <div className="mb-4">
        <h1 className="text-2xl font-semibold">Suspicious Activity Dashboard</h1>
        <p className="text-gray-600 text-sm mt-1">Monitor and review flagged activities for potential security issues</p>
      </div>

      {error && <div className="text-red-600 mb-3 text-sm bg-red-50 p-3 rounded">{error}</div>}

      {/* Filters */}
      <div className="bg-white rounded border p-4 mb-4">
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Min Score</label>
            <input
              type="number"
              min="0"
              max="10"
              value={minScore}
              onChange={(e) => {
                setMinScore(Number(e.target.value));
                setPageNumber(1);
              }}
              className="w-full p-2 border rounded"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              value={status}
              onChange={(e) => {
                setStatus(e.target.value);
                setPageNumber(1);
              }}
              className="w-full p-2 border rounded"
            >
              <option value="">All Statuses</option>
              <option value="OPEN">Open</option>
              <option value="REVIEWED">Reviewed</option>
              <option value="DISMISSED">Dismissed</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Date From</label>
            <input
              type="date"
              value={dateFrom}
              onChange={(e) => {
                setDateFrom(e.target.value);
                setPageNumber(1);
              }}
              className="w-full p-2 border rounded"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Date To</label>
            <input
              type="date"
              value={dateTo}
              onChange={(e) => {
                setDateTo(e.target.value);
                setPageNumber(1);
              }}
              className="w-full p-2 border rounded"
            />
          </div>
          <div className="flex items-end">
            <button
              onClick={() => {
                setMinScore(7);
                setStatus("");
                setDateFrom("");
                setDateTo("");
                setPageNumber(1);
              }}
              className="w-full px-4 py-2 border rounded hover:bg-gray-50"
            >
              Clear Filters
            </button>
          </div>
        </div>
      </div>

      {/* Flags List */}
      {loading ? (
        <div className="text-center py-8">Loading...</div>
      ) : flags.length === 0 ? (
        <div className="text-center py-8 text-gray-500">No suspicious activity found.</div>
      ) : (
        <div className="space-y-4">
          {flags.map((flag: any) => (
            <div key={flag.flagId} className="bg-white rounded border p-4">
              <div className="flex items-start justify-between mb-2">
                <div className="flex-1">
                  <div className="flex items-center space-x-2 mb-2">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${getScoreColor(flag.score)}`}>
                      Score: {flag.score}/10
                    </span>
                    <span className={`px-2 py-1 rounded text-xs font-medium ${getStatusColor(flag.status)}`}>
                      {flag.status}
                    </span>
                    {flag.historyEntry && (
                      <Link
                        href={`/clients/${flag.clientId}/history`}
                        className="text-blue-600 hover:underline text-sm"
                      >
                        View History
                      </Link>
                    )}
                  </div>
                  <div className="text-sm text-gray-600">
                    Detected: {formatDate(flag.detectedAt)}
                  </div>
                  {flag.reasons && flag.reasons.length > 0 && (
                    <div className="mt-2">
                      <span className="text-sm font-medium text-gray-700">Reasons:</span>
                      <div className="flex flex-wrap gap-2 mt-1">
                        {flag.reasons.map((reason: string, idx: number) => (
                          <span
                            key={idx}
                            className="px-2 py-1 bg-yellow-50 text-yellow-800 rounded text-xs"
                          >
                            {reason}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>
              {flag.historyEntry && (
                <div className="mt-3 pt-3 border-t">
                  <div className="text-sm">
                    <span className="font-medium">Action:</span> {flag.historyEntry.actionType}
                  </div>
                  <div className="text-sm text-gray-600 mt-1">
                    Actor: {flag.historyEntry.actorDisplayName || "System"} • 
                    Client: {flag.clientId.substring(0, 8)}...
                  </div>
                  {flag.historyEntry.reason && (
                    <div className="text-sm text-gray-700 mt-1">
                      <span className="font-medium">Reason:</span> {flag.historyEntry.reason}
                    </div>
                  )}
                </div>
              )}
              {flag.reviewedBy && flag.reviewedAt && (
                <div className="mt-2 text-xs text-gray-500">
                  Reviewed by {flag.reviewedBy} on {formatDate(flag.reviewedAt)}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between mt-4 text-sm">
          <div>Page {pageNumber} / {totalPages} ({total} total)</div>
          <div className="space-x-2">
            <button
              onClick={() => {
                const newPage = Math.max(1, pageNumber - 1);
                setPageNumber(newPage);
              }}
              disabled={pageNumber <= 1}
              className="px-3 py-1 rounded border disabled:opacity-50 hover:bg-gray-50"
            >
              Prev
            </button>
            <button
              onClick={() => {
                const newPage = Math.min(totalPages, pageNumber + 1);
                setPageNumber(newPage);
              }}
              disabled={pageNumber >= totalPages}
              className="px-3 py-1 rounded border disabled:opacity-50 hover:bg-gray-50"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

