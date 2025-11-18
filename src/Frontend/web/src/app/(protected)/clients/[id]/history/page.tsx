"use client";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { ClientHistoryApi } from "@/lib/api";

export default function ClientHistoryPage() {
  const params = useParams();
  const router = useRouter();
  const clientId = String(params?.id || "");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [history, setHistory] = useState<any[]>([]);
  const [timeline, setTimeline] = useState<any | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [includeAccessLogs, setIncludeAccessLogs] = useState(false);
  const [showRestoreModal, setShowRestoreModal] = useState(false);
  const [restoreReason, setRestoreReason] = useState("");
  const [restoring, setRestoring] = useState(false);

  const totalPages = Math.ceil(total / pageSize);

  useEffect(() => {
    loadData();
  }, [clientId, pageNumber, pageSize, includeAccessLogs]);

  async function loadData() {
    setLoading(true);
    setError(null);
    try {
      const [historyRes, timelineRes] = await Promise.all([
        ClientHistoryApi.getHistory(clientId, pageNumber, pageSize, includeAccessLogs),
        ClientHistoryApi.getTimeline(clientId)
      ]);
      setHistory(historyRes.data || []);
      setTotal(historyRes.totalCount);
      setTimeline(timelineRes.data);
    } catch (e: any) {
      setError(e.message || "Failed to load history");
    } finally {
      setLoading(false);
    }
  }

  async function handleRestore() {
    if (!restoreReason.trim()) {
      alert("Please provide a reason for restoration");
      return;
    }
    setRestoring(true);
    try {
      await ClientHistoryApi.restore(clientId, restoreReason);
      alert("Client restored successfully");
      setShowRestoreModal(false);
      setRestoreReason("");
      await loadData();
      router.push(`/clients/${clientId}`);
    } catch (e: any) {
      alert(e.message || "Restore failed");
    } finally {
      setRestoring(false);
    }
  }

  function formatDate(date: string) {
    return new Date(date).toLocaleString();
  }

  function formatActionType(type: string) {
    return type.charAt(0) + type.slice(1).toLowerCase();
  }

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <Link href={`/clients/${clientId}`} className="text-blue-600 hover:underline text-sm mb-2 inline-block">
            ← Back to Client Details
          </Link>
          <h1 className="text-2xl font-semibold">Client History & Timeline</h1>
          {timeline && (
            <p className="text-gray-600 text-sm mt-1">
              {timeline.companyName} • {timeline.totalChangeCount} total changes
              {timeline.isDeleted && (
                <span className="ml-2 text-red-600">
                  (Deleted {timeline.deletedAt ? formatDate(timeline.deletedAt) : ""})
                </span>
              )}
            </p>
          )}
        </div>
        <div className="space-x-2">
          {timeline?.isDeleted && timeline?.restorationWindowExpiresAt && 
           new Date(timeline.restorationWindowExpiresAt) > new Date() && (
            <button
              onClick={() => setShowRestoreModal(true)}
              className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
            >
              Restore Client
            </button>
          )}
          <button
            onClick={() => ClientHistoryApi.exportHistory({ clientIds: clientId })}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Export CSV
          </button>
        </div>
      </div>

      {error && <div className="text-red-600 mb-3 text-sm bg-red-50 p-3 rounded">{error}</div>}

      {/* Timeline Summary */}
      {timeline && (
        <div className="bg-white rounded border p-4 mb-4">
          <h2 className="text-lg font-semibold mb-3">Timeline Summary</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div>
              <span className="text-gray-500">Created:</span>
              <div className="font-medium">{formatDate(timeline.createdAt)}</div>
            </div>
            {timeline.lastModifiedAt && (
              <div>
                <span className="text-gray-500">Last Modified:</span>
                <div className="font-medium">{formatDate(timeline.lastModifiedAt)}</div>
                {timeline.lastModifiedBy && (
                  <div className="text-xs text-gray-400">by {timeline.lastModifiedBy}</div>
                )}
              </div>
            )}
            <div>
              <span className="text-gray-500">Total Changes:</span>
              <div className="font-medium">{timeline.totalChangeCount}</div>
            </div>
            {timeline.restorationWindowExpiresAt && (
              <div>
                <span className="text-gray-500">Restore Window:</span>
                <div className="font-medium text-sm">
                  {new Date(timeline.restorationWindowExpiresAt) > new Date() 
                    ? `Expires ${formatDate(timeline.restorationWindowExpiresAt)}`
                    : "Expired"}
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="bg-white rounded border p-4 mb-4">
        <div className="flex items-center justify-between">
          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              checked={includeAccessLogs}
              onChange={(e) => {
                setIncludeAccessLogs(e.target.checked);
                setPageNumber(1);
              }}
              className="rounded"
            />
            <span className="text-sm">Include Access Logs</span>
          </label>
          <div className="text-sm text-gray-600">
            Page {pageNumber} of {totalPages} ({total} total entries)
          </div>
        </div>
      </div>

      {/* History Entries */}
      {loading ? (
        <div className="text-center py-8">Loading...</div>
      ) : history.length === 0 ? (
        <div className="text-center py-8 text-gray-500">No history entries found.</div>
      ) : (
        <div className="space-y-4">
          {history.map((entry: any) => (
            <div key={entry.historyId} className="bg-white rounded border p-4">
              <div className="flex items-start justify-between mb-2">
                <div>
                  <div className="flex items-center space-x-2">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                      entry.actionType === "CREATED" ? "bg-green-100 text-green-800" :
                      entry.actionType === "UPDATED" ? "bg-blue-100 text-blue-800" :
                      entry.actionType === "DELETED" ? "bg-red-100 text-red-800" :
                      entry.actionType === "RESTORED" ? "bg-purple-100 text-purple-800" :
                      "bg-gray-100 text-gray-800"
                    }`}>
                      {formatActionType(entry.actionType)}
                    </span>
                    {entry.suspicionScore > 0 && (
                      <span className="px-2 py-1 rounded text-xs font-medium bg-yellow-100 text-yellow-800">
                        Suspicious (Score: {entry.suspicionScore})
                      </span>
                    )}
                  </div>
                  <div className="text-sm text-gray-600 mt-1">
                    by {entry.actorDisplayName || "System"} • {formatDate(entry.createdAt)}
                  </div>
                </div>
              </div>
              {entry.reason && (
                <div className="text-sm text-gray-700 mt-2">
                  <span className="font-medium">Reason:</span> {entry.reason}
                </div>
              )}
              {entry.changedFields && entry.changedFields.length > 0 && (
                <div className="text-sm text-gray-700 mt-2">
                  <span className="font-medium">Changed Fields:</span> {entry.changedFields.join(", ")}
                </div>
              )}
              {entry.beforeSnapshot && entry.afterSnapshot && (
                <details className="mt-3">
                  <summary className="text-sm text-blue-600 cursor-pointer hover:underline">
                    View Before/After
                  </summary>
                  <div className="mt-2 grid grid-cols-2 gap-4 text-xs">
                    <div>
                      <div className="font-medium mb-1">Before:</div>
                      <pre className="bg-gray-50 p-2 rounded overflow-auto max-h-40">
                        {JSON.stringify(entry.beforeSnapshot, null, 2)}
                      </pre>
                    </div>
                    <div>
                      <div className="font-medium mb-1">After:</div>
                      <pre className="bg-gray-50 p-2 rounded overflow-auto max-h-40">
                        {JSON.stringify(entry.afterSnapshot, null, 2)}
                      </pre>
                    </div>
                  </div>
                </details>
              )}
              {entry.metadata && (
                <div className="text-xs text-gray-500 mt-2">
                  IP: {entry.metadata.ipAddress || "N/A"} • 
                  User Agent: {entry.metadata.userAgent || "N/A"}
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

      {/* Restore Modal */}
      {showRestoreModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h2 className="text-xl font-semibold mb-4">Restore Client</h2>
            <p className="text-sm text-gray-600 mb-4">
              Please provide a reason for restoring this client. This action will be logged in the history.
            </p>
            <textarea
              value={restoreReason}
              onChange={(e) => setRestoreReason(e.target.value)}
              placeholder="Enter restoration reason..."
              className="w-full p-2 border rounded mb-4"
              rows={4}
            />
            <div className="flex justify-end space-x-2">
              <button
                onClick={() => {
                  setShowRestoreModal(false);
                  setRestoreReason("");
                }}
                className="px-4 py-2 border rounded hover:bg-gray-50"
                disabled={restoring}
              >
                Cancel
              </button>
              <button
                onClick={handleRestore}
                disabled={restoring}
                className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50"
              >
                {restoring ? "Restoring..." : "Restore"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

