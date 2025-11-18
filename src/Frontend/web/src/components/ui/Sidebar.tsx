"use client";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { LayoutDashboard, Users, User, FolderOpen, Bell } from "lucide-react";
import { useNotifications } from "@/contexts/NotificationContext";

function NavItem({ href, label, icon: Icon, badge }: { href: string; label: string; icon: any; badge?: number }) {
  const pathname = usePathname();
  const active = pathname === href || pathname?.startsWith(href + "/");
  return (
    <Link
      href={href}
      className={`flex items-center justify-between gap-2 rounded-md px-3 py-2 text-sm transition-colors hover:bg-zinc-100 ${
        active ? "bg-zinc-100 text-black" : "text-black"
      }`}
    >
      <div className="flex items-center gap-2">
        <Icon size={16} />
        <span>{label}</span>
      </div>
      {badge !== undefined && badge > 0 && (
        <span className="flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
          {badge > 99 ? "99+" : badge}
        </span>
      )}
    </Link>
  );
}

export default function Sidebar({ showUsers }: { showUsers?: boolean }) {
  const { unreadCount } = useNotifications();

  return (
    <aside className="hidden w-60 shrink-0 border-r border-black bg-white md:block">
      <div className="px-3 py-3">
        <div className="mb-2 px-2 text-[11px] font-semibold uppercase tracking-wider text-black">Menu</div>
        <nav className="flex flex-col gap-1">
          <NavItem href="/dashboard" label="Dashboard" icon={LayoutDashboard} />
          <NavItem href="/clients" label="Clients" icon={FolderOpen} />
          <NavItem href="/quotations" label="Quotations" icon={FolderOpen} />
          <NavItem href="/notifications" label="Notifications" icon={Bell} badge={unreadCount} />
          {showUsers && <NavItem href="/users/new" label="Users" icon={Users} />}
          <NavItem href="/profile" label="Profile" icon={User} />
        </nav>
      </div>
    </aside>
  );
}
