"use client";
import React, { useState } from "react";
import { UserManagementApi } from "@/lib/api";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Button from "@/components/tailadmin/ui/button/Button";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import { BulkInviteUsersRequest, BulkUpdateUsersRequest, BulkOperationResult } from "@/types/userManagement";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Badge from "@/components/tailadmin/ui/badge/Badge";

export default function BulkUserOperationsPage() {
  const [activeTab, setActiveTab] = useState<"invite" | "update" | "deactivate" | "export">("invite");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<BulkOperationResult | null>(null);

  const [inviteUsers, setInviteUsers] = useState<Array<{ email: string; firstName: string; lastName: string; mobile?: string }>>([
    { email: "", firstName: "", lastName: "", mobile: "" }
  ]);
  const [updateUserIds, setUpdateUserIds] = useState<string>("");
  const [deactivateUserIds, setDeactivateUserIds] = useState<string>("");
  const [exportFilters, setExportFilters] = useState<{ format?: string; roleId?: string; teamId?: string; isActive?: boolean }>({});

  const handleBulkInvite = async () => {
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      const payload: BulkInviteUsersRequest = {
        users: inviteUsers.filter(u => u.email && u.firstName && u.lastName),
      };
      const response = await UserManagementApi.bulk.inviteUsers(payload);
      setResult(response.data);
    } catch (e: any) {
      setError(e.message || "Failed to invite users");
    } finally {
      setLoading(false);
    }
  };

  const handleBulkUpdate = async () => {
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      const userIds = updateUserIds.split("\n").filter(id => id.trim());
      const payload: BulkUpdateUsersRequest = {
        userIds,
        ...exportFilters,
      };
      const response = await UserManagementApi.bulk.updateUsers(payload);
      setResult(response.data);
    } catch (e: any) {
      setError(e.message || "Failed to update users");
    } finally {
      setLoading(false);
    }
  };

  const handleBulkDeactivate = async () => {
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      const userIds = deactivateUserIds.split("\n").filter(id => id.trim());
      const response = await UserManagementApi.bulk.deactivateUsers(userIds);
      setResult(response.data);
    } catch (e: any) {
      setError(e.message || "Failed to deactivate users");
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async () => {
    setLoading(true);
    setError(null);
    try {
      await UserManagementApi.bulk.exportUsers(exportFilters);
    } catch (e: any) {
      setError(e.message || "Failed to export users");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6">
      <PageBreadcrumb pageName="Bulk User Operations" />
      
      <ComponentCard>
        <div className="flex gap-2 border-b border-stroke mb-6 dark:border-strokedark">
          {(["invite", "update", "deactivate", "export"] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-4 py-2 border-b-2 ${
                activeTab === tab
                  ? "border-primary text-primary"
                  : "border-transparent text-body-color dark:text-body-color-dark"
              }`}
            >
              {tab.charAt(0).toUpperCase() + tab.slice(1)}
            </button>
          ))}
        </div>

        {error && (
          <Alert color="danger" className="mb-4">
            {error}
          </Alert>
        )}

        {activeTab === "invite" && (
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-black dark:text-white">Bulk Invite Users</h3>
            {inviteUsers.map((user, index) => (
              <div key={index} className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <input
                  type="email"
                  placeholder="Email"
                  value={user.email}
                  onChange={(e) => {
                    const newUsers = [...inviteUsers];
                    newUsers[index].email = e.target.value;
                    setInviteUsers(newUsers);
                  }}
                  className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                />
                <input
                  type="text"
                  placeholder="First Name"
                  value={user.firstName}
                  onChange={(e) => {
                    const newUsers = [...inviteUsers];
                    newUsers[index].firstName = e.target.value;
                    setInviteUsers(newUsers);
                  }}
                  className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                />
                <input
                  type="text"
                  placeholder="Last Name"
                  value={user.lastName}
                  onChange={(e) => {
                    const newUsers = [...inviteUsers];
                    newUsers[index].lastName = e.target.value;
                    setInviteUsers(newUsers);
                  }}
                  className="px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                />
                <div className="flex gap-2">
                  <input
                    type="text"
                    placeholder="Mobile (optional)"
                    value={user.mobile || ""}
                    onChange={(e) => {
                      const newUsers = [...inviteUsers];
                      newUsers[index].mobile = e.target.value;
                      setInviteUsers(newUsers);
                    }}
                    className="flex-1 px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                  />
                  {inviteUsers.length > 1 && (
                    <button
                      type="button"
                      onClick={() => setInviteUsers(inviteUsers.filter((_, i) => i !== index))}
                      className="text-danger"
                    >
                      ×
                    </button>
                  )}
                </div>
              </div>
            ))}
            <Button
              type="button"
              onClick={() => setInviteUsers([...inviteUsers, { email: "", firstName: "", lastName: "", mobile: "" }])}
            >
              Add Another User
            </Button>
            <Button color="primary" onClick={handleBulkInvite} disabled={loading}>
              {loading ? "Inviting..." : "Invite Users"}
            </Button>
          </div>
        )}

        {activeTab === "update" && (
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-black dark:text-white">Bulk Update Users</h3>
            <div>
              <label className="block mb-2 text-black dark:text-white">User IDs (one per line)</label>
              <textarea
                value={updateUserIds}
                onChange={(e) => setUpdateUserIds(e.target.value)}
                rows={5}
                className="w-full rounded border border-stroke bg-transparent px-5 py-3 text-black outline-none focus:border-primary dark:border-strokedark dark:bg-boxdark dark:text-white"
              />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block mb-2 text-black dark:text-white">Role ID (optional)</label>
                <input
                  type="text"
                  value={exportFilters.roleId || ""}
                  onChange={(e) => setExportFilters({ ...exportFilters, roleId: e.target.value || undefined })}
                  className="w-full px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                />
              </div>
              <div>
                <label className="block mb-2 text-black dark:text-white">Is Active</label>
                <select
                  value={exportFilters.isActive === undefined ? "" : exportFilters.isActive.toString()}
                  onChange={(e) => setExportFilters({ ...exportFilters, isActive: e.target.value ? e.target.value === "true" : undefined })}
                  className="w-full px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                >
                  <option value="">No change</option>
                  <option value="true">Active</option>
                  <option value="false">Inactive</option>
                </select>
              </div>
            </div>
            <Button color="primary" onClick={handleBulkUpdate} disabled={loading}>
              {loading ? "Updating..." : "Update Users"}
            </Button>
          </div>
        )}

        {activeTab === "deactivate" && (
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-black dark:text-white">Bulk Deactivate Users</h3>
            <div>
              <label className="block mb-2 text-black dark:text-white">User IDs (one per line)</label>
              <textarea
                value={deactivateUserIds}
                onChange={(e) => setDeactivateUserIds(e.target.value)}
                rows={5}
                className="w-full rounded border border-stroke bg-transparent px-5 py-3 text-black outline-none focus:border-primary dark:border-strokedark dark:bg-boxdark dark:text-white"
              />
            </div>
            <Button color="danger" onClick={handleBulkDeactivate} disabled={loading}>
              {loading ? "Deactivating..." : "Deactivate Users"}
            </Button>
          </div>
        )}

        {activeTab === "export" && (
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-black dark:text-white">Export Users</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block mb-2 text-black dark:text-white">Format</label>
                <select
                  value={exportFilters.format || "CSV"}
                  onChange={(e) => setExportFilters({ ...exportFilters, format: e.target.value })}
                  className="w-full px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                >
                  <option value="CSV">CSV</option>
                  <option value="Excel">Excel</option>
                  <option value="JSON">JSON</option>
                </select>
              </div>
              <div>
                <label className="block mb-2 text-black dark:text-white">Is Active</label>
                <select
                  value={exportFilters.isActive === undefined ? "" : exportFilters.isActive.toString()}
                  onChange={(e) => setExportFilters({ ...exportFilters, isActive: e.target.value ? e.target.value === "true" : undefined })}
                  className="w-full px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                >
                  <option value="">All</option>
                  <option value="true">Active</option>
                  <option value="false">Inactive</option>
                </select>
              </div>
            </div>
            <Button color="primary" onClick={handleExport} disabled={loading}>
              {loading ? "Exporting..." : "Export Users"}
            </Button>
          </div>
        )}

        {result && (
          <div className="mt-6">
            <h3 className="text-lg font-semibold text-black dark:text-white mb-4">Operation Results</h3>
            <div className="flex gap-4 mb-4">
              <Badge color="success">Success: {result.successCount}</Badge>
              <Badge color="danger">Failed: {result.failureCount}</Badge>
              <Badge color="primary">Total: {result.totalCount}</Badge>
            </div>
            {result.results.length > 0 && (
              <div className="rounded-lg border border-stroke dark:border-strokedark dark:bg-boxdark">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableCell className="font-medium">User Email</TableCell>
                      <TableCell className="font-medium">Status</TableCell>
                      <TableCell className="font-medium">Error</TableCell>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {result.results.map((item, index) => (
                      <TableRow key={index}>
                        <TableCell>{item.userEmail}</TableCell>
                        <TableCell>
                          <Badge color={item.success ? "success" : "danger"}>
                            {item.success ? "Success" : "Failed"}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-danger">{item.errorMessage || "-"}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
          </div>
        )}
      </ComponentCard>
    </div>
  );
}

