"use client";
import React from "react";
import { Mention } from "@/types/userManagement";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface MentionBadgeProps {
  mention: Mention;
  onClick?: () => void;
  onMarkRead?: () => void;
}

export default function MentionBadge({ mention, onClick, onMarkRead }: MentionBadgeProps) {
  return (
    <div
      className={`p-3 rounded-lg border ${
        mention.isRead
          ? "border-stroke bg-white dark:border-strokedark dark:bg-boxdark"
          : "border-primary bg-primary/5"
      } ${onClick ? "cursor-pointer hover:bg-gray-50 dark:hover:bg-boxdark-2" : ""}`}
      onClick={onClick}
    >
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <span className="font-medium text-black dark:text-white">
              {mention.mentionedByUserName}
            </span>
            <span className="text-sm text-body-color dark:text-body-color-dark">mentioned you in</span>
            <Badge color="primary" className="text-xs">{mention.entityType}</Badge>
          </div>
          {!mention.isRead && (
            <Badge color="danger" className="text-xs">Unread</Badge>
          )}
          <p className="text-xs text-body-color dark:text-body-color-dark mt-1">
            {new Date(mention.createdAt).toLocaleString()}
          </p>
        </div>
        {!mention.isRead && onMarkRead && (
          <button
            onClick={(e) => {
              e.stopPropagation();
              onMarkRead();
            }}
            className="text-xs text-primary hover:text-primary-dark"
          >
            Mark as read
          </button>
        )}
      </div>
    </div>
  );
}

