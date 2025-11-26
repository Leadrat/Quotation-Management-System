"use client";
import React, { useState, useEffect } from "react";
import { CreateUserGroupRequest, UpdateUserGroupRequest, UserGroup, Permission } from "@/types/userManagement";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import { UserManagementApi } from "@/lib/api";

interface UserGroupFormProps {
  group?: UserGroup;
  onSubmit: (data: CreateUserGroupRequest | UpdateUserGroupRequest) => Promise<void>;
  onCancel: () => void;
}

export default function UserGroupForm({ group, onSubmit, onCancel }: UserGroupFormProps) {
  const [name, setName] = useState(group?.name || "");
  const [description, setDescription] = useState(group?.description || "");
  const [permissions, setPermissions] = useState<string[]>(group?.permissions || []);
  const [availablePermissions, setAvailablePermissions] = useState<Permission[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadPermissions();
  }, []);

  const loadPermissions = async () => {
    try {
      const result = await UserManagementApi.customRoles.getAvailablePermissions();
      setAvailablePermissions(result.data || []);
    } catch (e: any) {
      console.error("Failed to load permissions", e);
    }
  };

  const togglePermission = (permissionKey: string) => {
    if (permissions.includes(permissionKey)) {
      setPermissions(permissions.filter(p => p !== permissionKey));
    } else {
      setPermissions([...permissions, permissionKey]);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      if (group) {
        await onSubmit({
          name: name || undefined,
          description: description || undefined,
          permissions: permissions.length > 0 ? permissions : undefined,
        } as UpdateUserGroupRequest);
      } else {
        await onSubmit({
          name,
          description: description || undefined,
          permissions,
        } as CreateUserGroupRequest);
      }
    } catch (e: any) {
      setError(e.message || "Failed to save user group");
    } finally {
      setLoading(false);
    }
  };

  const groupedPermissions = availablePermissions.reduce((acc, perm) => {
    if (!acc[perm.category]) {
      acc[perm.category] = [];
    }
    acc[perm.category].push(perm);
    return acc;
  }, {} as Record<string, Permission[]>);

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && (
        <Alert color="danger">{error}</Alert>
      )}
      
      <div>
        <Label htmlFor="name">Group Name *</Label>
        <Input
          id="name"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          disabled={loading}
        />
      </div>

      <div>
        <Label htmlFor="description">Description</Label>
        <textarea
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={3}
          className="w-full rounded border border-stroke bg-transparent px-5 py-3 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-strokedark dark:bg-boxdark dark:text-white dark:focus:border-primary"
          disabled={loading}
        />
      </div>

      <div>
        <Label>Permissions</Label>
        <div className="max-h-64 overflow-y-auto border border-stroke rounded p-4 dark:border-strokedark dark:bg-boxdark">
          {Object.entries(groupedPermissions).map(([category, perms]) => (
            <div key={category} className="mb-4">
              <h4 className="font-semibold text-black dark:text-white mb-2">{category}</h4>
              <div className="space-y-2">
                {perms.map((perm) => (
                  <label key={perm.key} className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={permissions.includes(perm.key)}
                      onChange={() => togglePermission(perm.key)}
                      disabled={loading}
                      className="rounded border-stroke"
                    />
                    <span className="text-sm text-black dark:text-white">
                      {perm.name}
                      {perm.description && (
                        <span className="text-body-color dark:text-body-color-dark ml-2">
                          - {perm.description}
                        </span>
                      )}
                    </span>
                  </label>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="flex gap-2 justify-end">
        <Button type="button" onClick={onCancel} disabled={loading}>
          Cancel
        </Button>
        <Button type="submit" color="primary" disabled={loading}>
          {loading ? "Saving..." : group ? "Update Group" : "Create Group"}
        </Button>
      </div>
    </form>
  );
}

