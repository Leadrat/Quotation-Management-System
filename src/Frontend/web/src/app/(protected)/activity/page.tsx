"use client";
import React, { useEffect, useState } from "react";
import { UserManagementApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import ActivityFeedItem from "@/components/user-management/ActivityFeedItem";
import { UserActivity } from "@/types/userManagement";
import Alert from "@/components/tailadmin/ui/alert/Alert";

export default function ActivityFeedPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activities, setActivities] = useState<UserActivity[]>([]);
  const [filters, setFilters] = useState<{
    userId?: string;
    actionType?: string;
    entityType?: string;
    fromDate?: string;
    toDate?: string;
  }>({});
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  const loadActivities = async () => {
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await UserManagementApi.activity.getFeed({
        pageNumber,
        pageSize,
        ...filters,
      });
      setActivities(result.data || []);
      setTotalCount(result.totalCount || 0);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setActivities([]);
        setTotalCount(0);
        return;
      }
      setError(e.message || "Failed to load activity feed");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadActivities();
  }, [pageNumber, filters]);

  if (loading) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageTitle="Activity Feed" />
        <ComponentCard title="Activity Feed">
          <div className="text-center py-8">Loading activity feed...</div>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb pageTitle="Activity Feed" />

      <ComponentCard title="Activity Feed">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold text-black dark:text-white">Activity Feed</h2>
          <div className="flex gap-2">
            <input
              type="text"
              placeholder="Action Type"
              value={filters.actionType || ""}
              onChange={(e) => setFilters({ ...filters, actionType: e.target.value || undefined })}
              className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
            />
            <input
              type="text"
              placeholder="Entity Type"
              value={filters.entityType || ""}
              onChange={(e) => setFilters({ ...filters, entityType: e.target.value || undefined })}
              className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
            />
            <input
              type="date"
              value={filters.fromDate || ""}
              onChange={(e) => setFilters({ ...filters, fromDate: e.target.value || undefined })}
              className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
            />
            <input
              type="date"
              value={filters.toDate || ""}
              onChange={(e) => setFilters({ ...filters, toDate: e.target.value || undefined })}
              className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
            />
          </div>
        </div>

        {error && (
          <Alert variant="error" title="Error" message={error} />
        )}

        <div className="rounded-lg border border-stroke bg-white dark:border-strokedark dark:bg-boxdark">
          {activities.map((activity) => (
            <ActivityFeedItem key={activity.activityId} activity={activity} />
          ))}
        </div>

        {activities.length === 0 && !loading && (
          <div className="text-center py-8 text-body-color dark:text-body-color-dark">
            No activities found.
          </div>
        )}

        {totalCount > pageSize && (
          <div className="flex items-center justify-between mt-6">
            <button
              onClick={() => setPageNumber(p => Math.max(1, p - 1))}
              disabled={pageNumber === 1}
              className="px-4 py-2 border border-stroke rounded hover:bg-gray-50 dark:hover:bg-boxdark-2 disabled:opacity-50"
            >
              Previous
            </button>
            <span className="text-sm text-body-color dark:text-body-color-dark">
              Page {pageNumber} of {Math.ceil(totalCount / pageSize)}
            </span>
            <button
              onClick={() => setPageNumber(p => p + 1)}
              disabled={pageNumber >= Math.ceil(totalCount / pageSize)}
              className="px-4 py-2 border border-stroke rounded hover:bg-gray-50 dark:hover:bg-boxdark-2 disabled:opacity-50"
            >
              Next
            </button>
          </div>
        )}
      </ComponentCard>
    </div>
  );
}
