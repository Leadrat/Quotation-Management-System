"use client";
import React, { useState } from "react";
import { Team } from "@/types/userManagement";
import Link from "next/link";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface TeamHierarchyTreeProps {
  teams: Team[];
  rootTeamId?: string;
  onTeamSelect?: (teamId: string) => void;
}

export default function TeamHierarchyTree({ teams, rootTeamId, onTeamSelect }: TeamHierarchyTreeProps) {
  const [expanded, setExpanded] = useState<Set<string>>(new Set());

  const toggleExpand = (teamId: string) => {
    const newExpanded = new Set(expanded);
    if (newExpanded.has(teamId)) {
      newExpanded.delete(teamId);
    } else {
      newExpanded.add(teamId);
    }
    setExpanded(newExpanded);
  };

  const getRootTeams = () => {
    if (rootTeamId) {
      return teams.filter(t => t.teamId === rootTeamId);
    }
    return teams.filter(t => !t.parentTeamId);
  };

  const getChildTeams = (parentId: string) => {
    return teams.filter(t => t.parentTeamId === parentId);
  };

  const renderTeam = (team: Team, level: number = 0) => {
    const children = getChildTeams(team.teamId);
    const hasChildren = children.length > 0;
    const isExpanded = expanded.has(team.teamId);

    return (
      <div key={team.teamId} className="mb-2">
        <div
          className={`flex items-center gap-2 p-2 rounded hover:bg-gray-50 dark:hover:bg-boxdark-2 ${
            level > 0 ? "ml-6" : ""
          }`}
          style={{ paddingLeft: `${level * 24 + 8}px` }}
        >
          {hasChildren && (
            <button
              onClick={() => toggleExpand(team.teamId)}
              className="text-body-color dark:text-body-color-dark hover:text-primary"
              aria-label={isExpanded ? "Collapse" : "Expand"}
            >
              <svg
                className={`w-4 h-4 transition-transform ${isExpanded ? "rotate-90" : ""}`}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </button>
          )}
          {!hasChildren && <div className="w-4" />}
          <Link
            href={`/admin/teams/${team.teamId}`}
            className="flex-1 text-black dark:text-white hover:text-primary font-medium"
            onClick={() => onTeamSelect?.(team.teamId)}
          >
            {team.name}
          </Link>
          <Badge color={team.isActive ? "success" : "danger"} className="text-xs">
            {team.isActive ? "Active" : "Inactive"}
          </Badge>
          <span className="text-sm text-body-color dark:text-body-color-dark">
            {team.memberCount} {team.memberCount === 1 ? "member" : "members"}
          </span>
        </div>
        {hasChildren && isExpanded && (
          <div className="ml-4">
            {children.map(child => renderTeam(child, level + 1))}
          </div>
        )}
      </div>
    );
  };

  const rootTeams = getRootTeams();

  if (rootTeams.length === 0) {
    return (
      <div className="text-center py-8 text-body-color dark:text-body-color-dark">
        No teams found
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-stroke bg-white p-4 dark:border-strokedark dark:bg-boxdark">
      {rootTeams.map(team => renderTeam(team))}
    </div>
  );
}

