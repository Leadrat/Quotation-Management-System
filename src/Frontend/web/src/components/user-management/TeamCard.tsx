"use client";
import React from "react";
import Link from "next/link";
import { Team } from "@/types/userManagement";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface TeamCardProps {
  team: Team;
  onDelete?: (teamId: string) => void;
}

export default function TeamCard({ team, onDelete }: TeamCardProps) {
  return (
    <div className="rounded-lg border border-stroke bg-white p-6 shadow-default dark:border-strokedark dark:bg-boxdark">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-black dark:text-white mb-2">
            <Link href={`/admin/teams/${team.teamId}`} className="hover:text-primary">
              {team.name}
            </Link>
          </h3>
          {team.description && (
            <p className="text-sm text-body-color dark:text-body-color-dark mb-3">
              {team.description}
            </p>
          )}
          <div className="flex flex-wrap gap-2 mb-3">
            <Badge color="primary" className="text-xs">
              {team.memberCount} {team.memberCount === 1 ? "member" : "members"}
            </Badge>
            {team.isActive ? (
              <Badge color="success" className="text-xs">Active</Badge>
            ) : (
              <Badge color="danger" className="text-xs">Inactive</Badge>
            )}
          </div>
          <div className="text-sm text-body-color dark:text-body-color-dark">
            <p><span className="font-medium">Team Lead:</span> {team.teamLeadName}</p>
            {team.parentTeamName && (
              <p><span className="font-medium">Parent Team:</span> {team.parentTeamName}</p>
            )}
          </div>
        </div>
        {onDelete && (
          <button
            onClick={() => onDelete(team.teamId)}
            className="text-danger hover:text-danger-dark"
            aria-label="Delete team"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        )}
      </div>
    </div>
  );
}

