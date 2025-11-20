"use client";
import React, { useEffect, useState } from "react";
import { UserManagementApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Button from "@/components/tailadmin/ui/button/Button";
import TeamCard from "@/components/user-management/TeamCard";
import TeamHierarchyTree from "@/components/user-management/TeamHierarchyTree";
import { Team } from "@/types/userManagement";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import Link from "next/link";

export default function TeamsPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [teams, setTeams] = useState<Team[]>([]);
  const [viewMode, setViewMode] = useState<"grid" | "tree">("grid");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  const loadTeams = async () => {
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await UserManagementApi.teams.list({ pageNumber, pageSize });
      setTeams(result.data || []);
      setTotalCount(result.totalCount || 0);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setTeams([]);
        setTotalCount(0);
        return;
      }
      setError(e.message || "Failed to load teams");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTeams();
  }, [pageNumber]);

  const handleDelete = async (teamId: string) => {
    if (!confirm("Are you sure you want to delete this team?")) return;
    try {
      await UserManagementApi.teams.delete(teamId);
      await loadTeams();
    } catch (e: any) {
      alert(e.message || "Failed to delete team");
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageName="Teams" />
        <ComponentCard>
          <div className="text-center py-8">Loading teams...</div>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb pageName="Teams" />
      
      <ComponentCard>
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold text-black dark:text-white">Team Management</h2>
          <div className="flex gap-3">
            <div className="flex gap-2 border border-stroke rounded-lg p-1 dark:border-strokedark">
              <button
                onClick={() => setViewMode("grid")}
                className={`px-3 py-1 rounded text-sm ${
                  viewMode === "grid"
                    ? "bg-primary text-white"
                    : "text-body-color dark:text-body-color-dark"
                }`}
              >
                Grid
              </button>
              <button
                onClick={() => setViewMode("tree")}
                className={`px-3 py-1 rounded text-sm ${
                  viewMode === "tree"
                    ? "bg-primary text-white"
                    : "text-body-color dark:text-body-color-dark"
                }`}
              >
                Tree
              </button>
            </div>
            <Link href="/admin/teams/new">
              <Button color="primary">Create Team</Button>
            </Link>
          </div>
        </div>

        {error && (
          <Alert color="danger" className="mb-4">
            {error}
          </Alert>
        )}

        {viewMode === "grid" ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {teams.map((team) => (
              <TeamCard key={team.teamId} team={team} onDelete={handleDelete} />
            ))}
          </div>
        ) : (
          <TeamHierarchyTree teams={teams} />
        )}

        {teams.length === 0 && !loading && (
          <div className="text-center py-8 text-body-color dark:text-body-color-dark">
            No teams found. Create your first team to get started.
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

