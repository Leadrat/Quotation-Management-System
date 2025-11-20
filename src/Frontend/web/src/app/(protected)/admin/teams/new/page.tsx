"use client";
import React from "react";
import { useRouter } from "next/navigation";
import { UserManagementApi } from "@/lib/api";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import TeamForm from "@/components/user-management/TeamForm";
import { CreateTeamRequest } from "@/types/userManagement";

export default function NewTeamPage() {
  const router = useRouter();

  const handleSubmit = async (data: CreateTeamRequest) => {
    try {
      const result = await UserManagementApi.teams.create(data);
      router.push(`/admin/teams/${result.data.teamId}`);
    } catch (e: any) {
      throw e;
    }
  };

  return (
    <div className="p-6">
      <PageBreadcrumb pageName="Create Team" />
      <ComponentCard>
        <h2 className="text-2xl font-bold text-black dark:text-white mb-6">Create New Team</h2>
        <TeamForm
          onSubmit={handleSubmit}
          onCancel={() => router.push("/admin/teams")}
        />
      </ComponentCard>
    </div>
  );
}

