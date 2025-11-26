"use client";
import React, { useEffect, useState } from "react";
import { UserManagementApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Button from "@/components/tailadmin/ui/button/Button";
import UserGroupCard from "@/components/user-management/UserGroupCard";
import { UserGroup } from "@/types/userManagement";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import Link from "next/link";

export default function UserGroupsPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [groups, setGroups] = useState<UserGroup[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  const loadGroups = async () => {
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await UserManagementApi.userGroups.list({ pageNumber, pageSize });
      setGroups(result.data || []);
      setTotalCount(result.totalCount || 0);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setGroups([]);
        setTotalCount(0);
        return;
      }
      setError(e.message || "Failed to load user groups");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadGroups();
  }, [pageNumber]);

  const handleDelete = async (groupId: string) => {
    if (!confirm("Are you sure you want to delete this user group?")) return;
    // Note: Delete endpoint not implemented in backend yet
    alert("Delete functionality not yet implemented");
  };

  if (loading) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageTitle="User Groups" />
        <ComponentCard>
          <div className="text-center py-8">Loading user groups...</div>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb pageTitle="User Groups" />
      
      <ComponentCard>
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold text-black dark:text-white">User Groups</h2>
          <Link href="/admin/user-groups/new">
            <Button color="primary">Create User Group</Button>
          </Link>
        </div>

        {error && (
          <Alert color="danger" className="mb-4">
            {error}
          </Alert>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {groups.map((group) => (
            <UserGroupCard key={group.groupId} group={group} onDelete={handleDelete} />
          ))}
        </div>

        {groups.length === 0 && !loading && (
          <div className="text-center py-8 text-body-color dark:text-body-color-dark">
            No user groups found. Create your first group to get started.
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

