"use client";
import { ReactNode, useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuth } from "@/store/auth";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import { NotificationProvider } from "@/contexts/NotificationContext";
import { LocaleProvider } from "@/context/LocaleContext";
import { SidebarProvider, useSidebar } from "@/context/SidebarContext";
import { ThemeProvider } from "@/context/ThemeContext";
import AppHeader from "@/layout/AppHeader";
import AppSidebar from "@/layout/AppSidebar";
import Backdrop from "@/layout/Backdrop";

function LayoutContent({ children }: { children: ReactNode }) {
  const { isExpanded, isHovered, isMobileOpen } = useSidebar();
  
  return (
    <div className="dark:bg-gray-900 dark:text-gray-200">
      <Backdrop />
      <div className="flex h-screen overflow-hidden">
        <AppSidebar />
        <div 
          className={`relative flex flex-1 flex-col overflow-y-auto overflow-x-hidden transition-all duration-300 bg-white dark:bg-gray-900 ${
            isExpanded || isHovered 
              ? 'lg:ml-[290px]' 
              : 'lg:ml-[90px]'
          }`}
        >
          <AppHeader />
          <main className="flex-1 bg-white dark:bg-gray-900">
            <div className="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
              {children}
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}

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
    <ThemeProvider>
      <LocaleProvider>
        <NotificationProvider>
          <SidebarProvider>
            <LayoutContent>{children}</LayoutContent>
          </SidebarProvider>
        </NotificationProvider>
      </LocaleProvider>
    </ThemeProvider>
  );
}
