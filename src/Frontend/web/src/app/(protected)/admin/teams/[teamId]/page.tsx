"use client";
import React, { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { UserManagementApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Button from "@/components/tailadmin/ui/button/Button";
import TeamMemberList from "@/components/user-management/TeamMemberList";
import { Team, TeamMember } from "@/types/userManagement";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import Link from "next/link";
import Badge from "@/components/tailadmin/ui/badge/Badge";

export default function TeamDetailPage() {
  const params = useParams();
  const router = useRouter();
  const teamId = params.teamId as string;
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [team, setTeam] = useState<Team | null>(null);
  const [members, setMembers] = useState<TeamMember[]>([]);
  const [showAddMember, setShowAddMember] = useState(false);

  useEffect(() => {
    if (!teamId) return;
    loadTeam();
    loadMembers();
  }, [teamId]);

  const loadTeam = async () => {
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await UserManagementApi.teams.getById(teamId);
      setTeam(result.data);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        router.push("/login");
        return;
      }
      setError(e.message || "Failed to load team");
    } finally {
      setLoading(false);
    }
  };

  const loadMembers = async () => {
    try {
      const result = await UserManagementApi.teams.getMembers(teamId);
      setMembers(result.data || []);
    } catch (e: any) {
      console.error("Failed to load members", e);
    }
  };

  const handleRemoveMember = async (userId: string) => {
    if (!confirm("Are you sure you want to remove this member?")) return;
    try {
      await UserManagementApi.teams.removeMember(teamId, userId);
      await loadMembers();
    } catch (e: any) {
      alert(e.message || "Failed to remove member");
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageName="Team Details" />
        <ComponentCard>
          <div className="text-center py-8">Loading team...</div>
        </ComponentCard>
      </div>
    );
  }

  if (!team) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageName="Team Details" />
        <ComponentCard>
          <Alert color="danger">Team not found</Alert>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb pageName={`Team: ${team.name}`} />
      
      <ComponentCard className="mb-6">
        <div className="flex items-start justify-between mb-4">
          <div>
            <h1 className="text-2xl font-bold text-black dark:text-white mb-2">{team.name}</h1>
            {team.description && (
              <p className="text-body-color dark:text-body-color-dark mb-3">{team.description}</p>
            )}
            <div className="flex gap-2 mb-3">
              <Badge color={team.isActive ? "success" : "danger"}>
                {team.isActive ? "Active" : "Inactive"}
              </Badge>
              <Badge color="primary">{team.memberCount} members</Badge>
            </div>
            <div className="text-sm text-body-color dark:text-body-color-dark">
              <p><span className="font-medium">Team Lead:</span> {team.teamLeadName}</p>
              {team.parentTeamName && (
                <p><span className="font-medium">Parent Team:</span> {team.parentTeamName}</p>
              )}
            </div>
          </div>
          <div className="flex gap-2">
            <Link href={`/admin/teams/${teamId}/edit`}>
              <Button color="primary">Edit Team</Button>
            </Link>
          </div>
        </div>
      </ComponentCard>

      <ComponentCard>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold text-black dark:text-white">Team Members</h2>
          <Button color="primary" onClick={() => setShowAddMember(true)}>
            Add Member
          </Button>
        </div>
        <TeamMemberList
          members={members}
          onRemove={handleRemoveMember}
          canRemove={true}
        />
      </ComponentCard>
    </div>
  );
}

