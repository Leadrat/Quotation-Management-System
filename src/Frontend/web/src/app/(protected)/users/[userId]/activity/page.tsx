"use client";
import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { ClientHistoryApi } from "@/lib/api";

export default function UserActivityPage() {
  const params = useParams();
  const userId = String(params?.userId || "");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activities, setActivities] = useState<any[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [actionType, setActionType] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  const totalPages = Math.ceil(total / pageSize);

  useEffect(() => {
    loadData();
  }, [userId, pageNumber, pageSize, actionType, dateFrom, dateTo]);

  async function loadData() {
    setLoading(true);
    setError(null);
    try {
      const res = await ClientHistoryApi.getUserActivity(userId, {
        pageNumber,
        pageSize,
        actionType: actionType || undefined,
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
      });
      setActivities(res.data || []);
      setTotal(res.totalCount);
    } catch (e: any) {
      setError(e.message || "Failed to load activity");
    } finally {
      setLoading(false);
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
      <div className="mb-4">
        <h1 className="text-2xl font-semibold">User Activity</h1>
        <p className="text-gray-600 text-sm mt-1">Activity history for user ID: {userId}</p>
      </div>

      {error && <div className="text-red-600 mb-3 text-sm bg-red-50 p-3 rounded">{error}</div>}

      {/* Filters */}
      <div className="bg-white rounded border p-4 mb-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Action Type</label>
            <select
              value={actionType}
              onChange={(e) => {
                setActionType(e.target.value);
                setPageNumber(1);
              }}
              className="w-full p-2 border rounded"
            >
              <option value="">All Actions</option>
              <option value="CREATED">Created</option>
              <option value="UPDATED">Updated</option>
              <option value="DELETED">Deleted</option>
              <option value="RESTORED">Restored</option>
              <option value="ACCESSED">Accessed</option>
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
                setActionType("");
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

      {/* Activity List */}
      {loading ? (
        <div className="text-center py-8">Loading...</div>
      ) : activities.length === 0 ? (
        <div className="text-center py-8 text-gray-500">No activity found.</div>
      ) : (
        <div className="space-y-4">
          {activities.map((activity: any) => (
            <div key={activity.historyId} className="bg-white rounded border p-4">
              <div className="flex items-start justify-between mb-2">
                <div>
                  <div className="flex items-center space-x-2">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                      activity.actionType === "CREATED" ? "bg-green-100 text-green-800" :
                      activity.actionType === "UPDATED" ? "bg-blue-100 text-blue-800" :
                      activity.actionType === "DELETED" ? "bg-red-100 text-red-800" :
                      activity.actionType === "RESTORED" ? "bg-purple-100 text-purple-800" :
                      "bg-gray-100 text-gray-800"
                    }`}>
                      {formatActionType(activity.actionType)}
                    </span>
                    <Link
                      href={`/clients/${activity.clientId}`}
                      className="text-blue-600 hover:underline text-sm"
                    >
                      Client: {activity.clientId.substring(0, 8)}...
                    </Link>
                  </div>
                  <div className="text-sm text-gray-600 mt-1">
                    {formatDate(activity.createdAt)}
                  </div>
                </div>
              </div>
              {activity.reason && (
                <div className="text-sm text-gray-700 mt-2">
                  <span className="font-medium">Reason:</span> {activity.reason}
                </div>
              )}
              {activity.changedFields && activity.changedFields.length > 0 && (
                <div className="text-sm text-gray-700 mt-2">
                  <span className="font-medium">Changed Fields:</span> {activity.changedFields.join(", ")}
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

