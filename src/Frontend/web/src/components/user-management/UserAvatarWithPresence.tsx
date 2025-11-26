"use client";
import React from "react";
import { PresenceStatus } from "@/types/userManagement";
import PresenceIndicator from "./PresenceIndicator";

interface UserAvatarWithPresenceProps {
  name: string;
  avatarUrl?: string;
  presenceStatus: PresenceStatus;
  size?: "sm" | "md" | "lg";
}

export default function UserAvatarWithPresence({
  name,
  avatarUrl,
  presenceStatus,
  size = "md",
}: UserAvatarWithPresenceProps) {
  const getInitials = () => {
    return name
      .split(" ")
      .map(n => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);
  };

  const getSizeClass = () => {
    switch (size) {
      case "sm":
        return "w-8 h-8 text-xs";
      case "lg":
        return "w-16 h-16 text-lg";
      default:
        return "w-12 h-12 text-sm";
    }
  };

  return (
    <div className="relative inline-block">
      {avatarUrl ? (
        <img
          src={avatarUrl}
          alt={name}
          className={`${getSizeClass()} rounded-full object-cover`}
        />
      ) : (
        <div
          className={`${getSizeClass()} rounded-full bg-primary flex items-center justify-center text-white font-semibold`}
        >
          {getInitials()}
        </div>
      )}
      <div className="absolute bottom-0 right-0">
        <PresenceIndicator status={presenceStatus} size="sm" />
      </div>
    </div>
  );
}

