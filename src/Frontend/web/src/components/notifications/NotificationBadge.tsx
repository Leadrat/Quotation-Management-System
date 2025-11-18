"use client";
import Link from "next/link";
import { Bell } from "lucide-react";

interface NotificationBadgeProps {
  count: number;
  href?: string;
  className?: string;
}

export function NotificationBadge({ count, href = "/notifications", className = "" }: NotificationBadgeProps) {
  const content = (
    <div className={`relative ${className}`}>
      <Bell size={16} />
      {count > 0 && (
        <span className="absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
          {count > 99 ? "99+" : count}
        </span>
      )}
    </div>
  );

  if (href) {
    return <Link href={href}>{content}</Link>;
  }

  return content;
}

