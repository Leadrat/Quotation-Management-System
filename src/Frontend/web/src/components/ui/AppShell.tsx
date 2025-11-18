"use client";
import { ReactNode } from "react";
import Topbar from "./Topbar";
import Sidebar from "./Sidebar";

export default function AppShell({ children, showUsers, onLogout }: { children: ReactNode; showUsers?: boolean; onLogout?: () => void }) {
  return (
    <div className="min-h-screen bg-zinc-50">
      <Topbar onLogout={onLogout} />
      <div className="mx-auto flex max-w-7xl gap-6 px-4 py-6">
        <Sidebar showUsers={showUsers} />
        <main className="flex-1">{children}</main>
      </div>
    </div>
  );
}
