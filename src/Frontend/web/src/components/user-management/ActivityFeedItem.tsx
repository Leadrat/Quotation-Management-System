"use client";
import React from "react";
import { UserActivity } from "@/types/userManagement";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface ActivityFeedItemProps {
  activity: UserActivity;
  onClick?: () => void;
}

export default function ActivityFeedItem({ activity, onClick }: ActivityFeedItemProps) {
  const getActionColor = (actionType: string) => {
    if (actionType.includes("CREATE") || actionType.includes("CREATE")) return "success";
    if (actionType.includes("UPDATE") || actionType.includes("UPDATE")) return "primary";
    if (actionType.includes("DELETE") || actionType.includes("DELETE")) return "danger";
    return "secondary";
  };

  return (
    <div
      className={`p-4 border-b border-stroke dark:border-strokedark ${
        onClick ? "cursor-pointer hover:bg-gray-50 dark:hover:bg-boxdark-2" : ""
      }`}
      onClick={onClick}
    >
      <div className="flex items-start gap-3">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <span className="font-medium text-black dark:text-white">{activity.userName}</span>
            <Badge color={getActionColor(activity.actionType)} className="text-xs">
              {activity.actionType}
            </Badge>
          </div>
          {activity.entityType && activity.entityId && (
            <p className="text-sm text-body-color dark:text-body-color-dark">
              {activity.entityType} {activity.entityId.substring(0, 8)}...
            </p>
          )}
          <p className="text-xs text-body-color dark:text-body-color-dark mt-1">
            {new Date(activity.timestamp).toLocaleString()}
          </p>
        </div>
      </div>
    </div>
  );
}

