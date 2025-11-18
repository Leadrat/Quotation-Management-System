"use client";
import Link from "next/link";

export default function Topbar({ onLogout }: { onLogout?: () => void }) {
  return (
    <header className="sticky top-0 z-40 w-full border-b bg-white/80 backdrop-blur supports-[backdrop-filter]:bg-white/60">
      <div className="mx-auto flex h-14 max-w-7xl items-center justify-between px-4">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-zinc-900 text-white">C</div>
          <span className="text-sm font-semibold">CRM Admin</span>
        </div>
        <nav className="flex items-center gap-4 text-sm">
          <Link className="hover:underline" href="/dashboard">Dashboard</Link>
          <Link className="hover:underline" href="/clients">Clients</Link>
          <Link className="hover:underline" href="/profile">Profile</Link>
          <button onClick={onLogout} className="text-red-600 hover:underline">Logout</button>
        </nav>
      </div>
    </header>
  );
}
