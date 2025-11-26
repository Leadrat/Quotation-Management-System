"use client";
import React from "react";
import { useRouter } from "next/navigation";
import { UserManagementApi } from "@/lib/api";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import UserGroupForm from "@/components/user-management/UserGroupForm";
import { CreateUserGroupRequest } from "@/types/userManagement";

export default function NewUserGroupPage() {
  const router = useRouter();

  const handleSubmit = async (data: CreateUserGroupRequest) => {
    try {
      await UserManagementApi.userGroups.create(data);
      router.push("/admin/user-groups");
    } catch (e: any) {
      throw e;
    }
  };

  return (
    <div className="p-6">
      <PageBreadcrumb pageTitle="Create User Group" />
      <ComponentCard>
        <h2 className="text-2xl font-bold text-black dark:text-white mb-6">Create New User Group</h2>
        <UserGroupForm
          onSubmit={handleSubmit}
          onCancel={() => router.push("/admin/user-groups")}
        />
      </ComponentCard>
    </div>
  );
}

