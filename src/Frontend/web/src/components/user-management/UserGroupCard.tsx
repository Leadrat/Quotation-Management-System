"use client";
import React from "react";
import Link from "next/link";
import { UserGroup } from "@/types/userManagement";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface UserGroupCardProps {
  group: UserGroup;
  onDelete?: (groupId: string) => void;
}

export default function UserGroupCard({ group, onDelete }: UserGroupCardProps) {
  return (
    <div className="rounded-lg border border-stroke bg-white p-6 shadow-default dark:border-strokedark dark:bg-boxdark">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-black dark:text-white mb-2">
            {group.name}
          </h3>
          {group.description && (
            <p className="text-sm text-body-color dark:text-body-color-dark mb-3">
              {group.description}
            </p>
          )}
          <div className="flex flex-wrap gap-2 mb-3">
            <Badge color="primary" className="text-xs">
              {group.memberCount} {group.memberCount === 1 ? "member" : "members"}
            </Badge>
            <Badge color="secondary" className="text-xs">
              {group.permissions.length} {group.permissions.length === 1 ? "permission" : "permissions"}
            </Badge>
          </div>
          <div className="text-sm text-body-color dark:text-body-color-dark">
            <p><span className="font-medium">Created by:</span> {group.createdByUserName}</p>
            <p className="mt-1">
              <span className="font-medium">Permissions:</span>{" "}
              {group.permissions.length > 0 ? (
                <span className="text-xs">{group.permissions.slice(0, 3).join(", ")}
                  {group.permissions.length > 3 && ` +${group.permissions.length - 3} more`}
                </span>
              ) : (
                <span className="text-xs text-body-color dark:text-body-color-dark">None</span>
              )}
            </p>
          </div>
        </div>
        {onDelete && (
          <button
            onClick={() => onDelete(group.groupId)}
            className="text-danger hover:text-danger-dark"
            aria-label="Delete group"
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

