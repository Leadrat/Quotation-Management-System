"use client";
import React, { useEffect, useState } from "react";
import { UserManagementApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Button from "@/components/tailadmin/ui/button/Button";
import { CustomRole, Permission, CreateCustomRoleRequest, UpdateRolePermissionsRequest } from "@/types/userManagement";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Badge from "@/components/tailadmin/ui/badge/Badge";

export default function CustomRolesPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [roles, setRoles] = useState<CustomRole[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingRole, setEditingRole] = useState<CustomRole | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  const [newRoleName, setNewRoleName] = useState("");
  const [newRoleDescription, setNewRoleDescription] = useState("");
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);

  useEffect(() => {
    loadRoles();
    loadPermissions();
  }, [pageNumber]);

  const loadRoles = async () => {
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await UserManagementApi.customRoles.list({ pageNumber, pageSize });
      setRoles(result.data || []);
      setTotalCount(result.totalCount || 0);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setRoles([]);
        setTotalCount(0);
        return;
      }
      setError(e.message || "Failed to load custom roles");
    } finally {
      setLoading(false);
    }
  };

  const loadPermissions = async () => {
    try {
      const result = await UserManagementApi.customRoles.getAvailablePermissions();
      setPermissions(result.data || []);
    } catch (e: any) {
      console.error("Failed to load permissions", e);
    }
  };

  const handleCreateRole = async () => {
    if (!newRoleName.trim()) {
      setError("Role name is required");
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const payload: CreateCustomRoleRequest = {
        roleName: newRoleName,
        description: newRoleDescription || undefined,
        permissions: selectedPermissions,
      };
      await UserManagementApi.customRoles.create(payload);
      setShowCreateForm(false);
      setNewRoleName("");
      setNewRoleDescription("");
      setSelectedPermissions([]);
      await loadRoles();
    } catch (e: any) {
      setError(e.message || "Failed to create role");
    } finally {
      setLoading(false);
    }
  };

  const handleUpdatePermissions = async (roleId: string) => {
    setLoading(true);
    setError(null);
    try {
      const payload: UpdateRolePermissionsRequest = {
        permissions: selectedPermissions,
      };
      await UserManagementApi.customRoles.updatePermissions(roleId, payload);
      setEditingRole(null);
      setSelectedPermissions([]);
      await loadRoles();
    } catch (e: any) {
      setError(e.message || "Failed to update permissions");
    } finally {
      setLoading(false);
    }
  };

  const togglePermission = (permissionKey: string) => {
    if (selectedPermissions.includes(permissionKey)) {
      setSelectedPermissions(selectedPermissions.filter(p => p !== permissionKey));
    } else {
      setSelectedPermissions([...selectedPermissions, permissionKey]);
    }
  };

  const startEdit = (role: CustomRole) => {
    setEditingRole(role);
    setSelectedPermissions(role.permissions);
  };

  const groupedPermissions = permissions.reduce((acc, perm) => {
    if (!acc[perm.category]) {
      acc[perm.category] = [];
    }
    acc[perm.category].push(perm);
    return acc;
  }, {} as Record<string, Permission[]>);

  if (loading && roles.length === 0) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageTitle="Custom Roles" />
        <ComponentCard title="Loading">
          <div className="text-center py-8">Loading custom roles...</div>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb pageTitle="Custom Roles" />

      <ComponentCard title="Custom Roles">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold text-black dark:text-white">Custom Roles & Permissions</h2>
          <Button variant="primary" onClick={() => setShowCreateForm(true)}>
            Create Custom Role
          </Button>
        </div>

        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Error" message={error} />
          </div>
        )}

        {showCreateForm && (
          <div className="mb-6 p-4 border border-stroke rounded-lg dark:border-strokedark dark:bg-boxdark">
            <h3 className="text-lg font-semibold text-black dark:text-white mb-4">Create New Role</h3>
            <div className="space-y-4">
              <div>
                <label className="block mb-2 text-black dark:text-white">Role Name *</label>
                <input
                  type="text"
                  value={newRoleName}
                  onChange={(e) => setNewRoleName(e.target.value)}
                  className="w-full px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                />
              </div>
              <div>
                <label className="block mb-2 text-black dark:text-white">Description</label>
                <textarea
                  value={newRoleDescription}
                  onChange={(e) => setNewRoleDescription(e.target.value)}
                  rows={3}
                  className="w-full px-3 py-2 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
                />
              </div>
              <div>
                <label className="block mb-2 text-black dark:text-white">Permissions</label>
                <div className="max-h-64 overflow-y-auto border border-stroke rounded p-4 dark:border-strokedark dark:bg-boxdark">
                  {Object.entries(groupedPermissions).map(([category, perms]) => (
                    <div key={category} className="mb-4">
                      <h4 className="font-semibold text-black dark:text-white mb-2">{category}</h4>
                      <div className="space-y-2">
                        {perms.map((perm) => (
                          <label key={perm.key} className="flex items-center gap-2 cursor-pointer">
                            <input
                              type="checkbox"
                              checked={selectedPermissions.includes(perm.key)}
                              onChange={() => togglePermission(perm.key)}
                              className="rounded border-stroke"
                            />
                            <span className="text-sm text-black dark:text-white">
                              {perm.name}
                            </span>
                          </label>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
              <div className="flex gap-2">
                <Button variant="outline" onClick={() => { setShowCreateForm(false); setNewRoleName(""); setNewRoleDescription(""); setSelectedPermissions([]); }}>
                  Cancel
                </Button>
                <Button variant="primary" onClick={handleCreateRole} disabled={loading}>
                  Create Role
                </Button>
              </div>
            </div>
          </div>
        )}

        <div className="rounded-lg border border-stroke dark:border-strokedark dark:bg-boxdark">
          <Table>
            <TableHeader>
              <TableRow>
                <TableCell className="font-medium">Role Name</TableCell>
                <TableCell className="font-medium">Description</TableCell>
                <TableCell className="font-medium">Permissions</TableCell>
                <TableCell className="font-medium">Users</TableCell>
                <TableCell className="font-medium">Status</TableCell>
                <TableCell className="font-medium">Actions</TableCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {roles.map((role) => (
                <TableRow key={role.roleId}>
                  <TableCell className="font-medium text-black dark:text-white">{role.roleName}</TableCell>
                  <TableCell className="text-body-color dark:text-body-color-dark">{role.description || "-"}</TableCell>
                  <TableCell>
                    <Badge color="primary" size="sm">
                      {role.permissions.length} permissions
                    </Badge>
                  </TableCell>
                  <TableCell>{role.userCount}</TableCell>
                  <TableCell>
                    <Badge color={role.isActive ? "success" : "error"}>
                      {role.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    {!role.isBuiltIn && (
                      <button
                        onClick={() => startEdit(role)}
                        className="text-primary hover:text-primary-dark text-sm"
                      >
                        Edit Permissions
                      </button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        {roles.length === 0 && !loading && (
          <div className="text-center py-8 text-body-color dark:text-body-color-dark">
            No custom roles found. Create your first role to get started.
          </div>
        )}

        {editingRole && (
          <div className="mt-6 p-4 border border-stroke rounded-lg dark:border-strokedark dark:bg-boxdark">
            <h3 className="text-lg font-semibold text-black dark:text-white mb-4">
              Edit Permissions: {editingRole.roleName}
            </h3>
            <div className="max-h-64 overflow-y-auto border border-stroke rounded p-4 dark:border-strokedark dark:bg-boxdark mb-4">
              {Object.entries(groupedPermissions).map(([category, perms]) => (
                <div key={category} className="mb-4">
                  <h4 className="font-semibold text-black dark:text-white mb-2">{category}</h4>
                  <div className="space-y-2">
                    {perms.map((perm) => (
                      <label key={perm.key} className="flex items-center gap-2 cursor-pointer">
                        <input
                          type="checkbox"
                          checked={selectedPermissions.includes(perm.key)}
                          onChange={() => togglePermission(perm.key)}
                          className="rounded border-stroke"
                        />
                        <span className="text-sm text-black dark:text-white">
                          {perm.name}
                        </span>
                      </label>
                    ))}
                  </div>
                </div>
              ))}
            </div>
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => { setEditingRole(null); setSelectedPermissions([]); }}>
                Cancel
              </Button>
              <Button variant="primary" onClick={() => handleUpdatePermissions(editingRole.roleId)} disabled={loading}>
                Update Permissions
              </Button>
            </div>
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

