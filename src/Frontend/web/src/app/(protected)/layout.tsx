"use client";
import { ReactNode, useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuth } from "@/store/auth";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import AppShell from "@/components/ui/AppShell";
import { NotificationProvider } from "@/contexts/NotificationContext";
import { LocaleProvider } from "@/context/LocaleContext";

export default function ProtectedLayout({ children }: { children: ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const auth = useAuth();
  const [role, setRole] = useState<string | null>(null);

  useEffect(() => {
    auth.hydrate();
  }, []);

  useEffect(() => {
    if (!auth.accessToken) {
      if (!pathname?.startsWith("/login")) router.replace("/login");
    }
  }, [auth.accessToken, pathname, router]);

  useEffect(() => {
    const token = getAccessToken();
    setRole(getRoleFromToken(token));
  }, [auth.accessToken]);

  return (
    <LocaleProvider>
      <NotificationProvider>
        <AppShell
          showUsers={role === "Admin"}
          onLogout={() => auth.logout().then(() => router.replace("/login"))}
        >
          {children}
        </AppShell>
      </NotificationProvider>
    </LocaleProvider>
  );
}
