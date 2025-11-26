"use client";
import React, { useEffect, useState } from "react";
import { UserManagementApi } from "@/lib/api";
import { getAccessToken, parseJwt } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import TaskCard from "@/components/user-management/TaskCard";
import { TaskAssignment } from "@/types/userManagement";
import Alert from "@/components/tailadmin/ui/alert/Alert";

export default function TasksPage() {
  const [userId, setUserId] = useState<string>("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tasks, setTasks] = useState<TaskAssignment[]>([]);
  const [filter, setFilter] = useState<{ status?: string; entityType?: string }>({});
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    const token = getAccessToken();
    const jwt = parseJwt(token);
    setUserId(jwt?.sub || jwt?.userId || "");
  }, []);

  const loadTasks = async () => {
    if (!getAccessToken() || !userId) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await UserManagementApi.tasks.getUserTasks(userId, {
        pageNumber,
        pageSize,
        ...filter,
      });
      setTasks(result.data || []);
      setTotalCount(result.totalCount || 0);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setTasks([]);
        setTotalCount(0);
        return;
      }
      setError(e.message || "Failed to load tasks");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (userId) {
      loadTasks();
    }
  }, [userId, pageNumber, filter]);

  const handleStatusUpdate = async (assignmentId: string, status: string) => {
    try {
      await UserManagementApi.tasks.updateStatus(assignmentId, { status });
      await loadTasks();
    } catch (e: any) {
      alert(e.message || "Failed to update task status");
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageTitle="My Tasks" />
        <ComponentCard title="Task Assignments">
          <div className="text-center py-8">Loading tasks...</div>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb pageTitle="My Tasks" />

      <ComponentCard title="Task Assignments">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold text-black dark:text-white">Task Assignments</h2>
          <div className="flex gap-2">
            <select
              value={filter.status || ""}
              onChange={(e) => setFilter({ ...filter, status: e.target.value || undefined })}
              className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
            >
              <option value="">All Statuses</option>
              <option value="Pending">Pending</option>
              <option value="InProgress">In Progress</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
            </select>
            <select
              value={filter.entityType || ""}
              onChange={(e) => setFilter({ ...filter, entityType: e.target.value || undefined })}
              className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
            >
              <option value="">All Types</option>
              <option value="Quotation">Quotation</option>
              <option value="Approval">Approval</option>
              <option value="Client">Client</option>
            </select>
          </div>
        </div>

        {error && (
          <Alert color="danger" className="mb-4">
            {error}
          </Alert>
        )}

        <div className="space-y-4">
          {tasks.map((task) => (
            <TaskCard
              key={task.assignmentId}
              task={task}
              onStatusUpdate={handleStatusUpdate}
              canUpdate={true}
            />
          ))}
        </div>

        {tasks.length === 0 && !loading && (
          <div className="text-center py-8 text-body-color dark:text-body-color-dark">
            No tasks assigned to you.
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
