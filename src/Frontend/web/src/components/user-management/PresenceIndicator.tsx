"use client";
import React from "react";
import { PresenceStatus } from "@/types/userManagement";

interface PresenceIndicatorProps {
  status: PresenceStatus;
  size?: "sm" | "md" | "lg";
  showLabel?: boolean;
}

export default function PresenceIndicator({ status, size = "md", showLabel = false }: PresenceIndicatorProps) {
  const getStatusColor = () => {
    switch (status) {
      case "Online":
        return "bg-success";
      case "Busy":
        return "bg-danger";
      case "Away":
        return "bg-warning";
      default:
        return "bg-gray-400";
    }
  };

  const getSizeClass = () => {
    switch (size) {
      case "sm":
        return "w-2 h-2";
      case "lg":
        return "w-4 h-4";
      default:
        return "w-3 h-3";
    }
  };

  return (
    <div className="flex items-center gap-2">
      <span className={`${getSizeClass()} ${getStatusColor()} rounded-full inline-block`} />
      {showLabel && (
        <span className="text-sm text-body-color dark:text-body-color-dark capitalize">
          {status}
        </span>
      )}
    </div>
  );
}

