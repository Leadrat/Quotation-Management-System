"use client";
import React, { useEffect, useRef, useState,useCallback } from "react";
import Link from "next/link";
import Image from "next/image";
import { usePathname } from "next/navigation";
import { useSidebar } from "../context/SidebarContext";
import {
  BoxCubeIcon,
  CalenderIcon,
  ChevronDownIcon,
  GridIcon,
  HorizontaLDots,
  ListIcon,
  PageIcon,
  PieChartIcon,
  PlugInIcon,
  TableIcon,
  UserCircleIcon,
  FileIcon,
  DollarLineIcon,
  CheckCircleIcon,
  BellIcon,
  ArrowDownIcon,
  FolderIcon,
} from "../icons/index";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import { useNotifications } from "@/contexts/NotificationContext";

type NavItem = {
  name: string;
  icon: React.ReactNode;
  path?: string;
  subItems?: { name: string; path: string; pro?: boolean; new?: boolean }[];
  badge?: number;
};

const baseNavItems: NavItem[] = [
  {
    icon: <GridIcon />,
    name: "Dashboard",
    path: "/dashboard",
  },
  {
    icon: <FolderIcon />,
    name: "Clients",
    path: "/clients",
  },
  {
    icon: <FileIcon />,
    name: "Quotations",
    path: "/quotations",
  },
  {
    icon: <PageIcon />,
    name: "Templates",
    path: "/templates",
  },
  {
    icon: <DollarLineIcon />,
    name: "Payments",
    path: "/payments",
  },
  {
    icon: <CheckCircleIcon />,
    name: "Approvals",
    path: "/approvals",
  },
  {
    icon: <ArrowDownIcon />,
    name: "Refunds",
    path: "/refunds",
  },
  {
    icon: <PieChartIcon />,
    name: "Reports",
    path: "/reports",
  },
  {
    icon: <BellIcon />,
    name: "Notifications",
    path: "/notifications",
  },
];

const adminNavItems: NavItem[] = [
  {
    icon: <PlugInIcon />,
    name: "Admin",
    path: "/admin",
    subItems: [
      { name: "System Settings", path: "/admin/settings/system" },
      { name: "Company Details", path: "/admin/company-details" },
      { name: "Audit Logs", path: "/admin/audit-logs" },
      { name: "Branding", path: "/admin/branding" },
      { name: "Localization", path: "/admin/localization" },
      { name: "Currencies", path: "/admin/currencies" },
      { name: "Integrations", path: "/admin/integrations" },
      { name: "Data Retention", path: "/admin/data-retention" },
      { name: "Notification Settings", path: "/admin/notifications" },
      { name: "Suspicious Activity", path: "/admin/suspicious-activity" },
      { name: "Template Stats", path: "/admin/templates/stats" },
      { name: "Pending Templates", path: "/admin/templates/pending" },
    ],
  },
  {
    icon: <UserCircleIcon />,
    name: "Users",
    path: "/users",
  },
];

const othersItems: NavItem[] = [
  {
    icon: <UserCircleIcon />,
    name: "Profile",
    path: "/profile",
  },
];

const AppSidebar: React.FC = () => {
  const { isExpanded, isMobileOpen, isHovered, setIsHovered } = useSidebar();
  const pathname = usePathname();
  const [isAdmin, setIsAdmin] = useState(false);
  const [role, setRole] = useState<string | null>(null);
  const { unreadCount } = useNotifications();

  useEffect(() => {
    const token = getAccessToken();
    const userRole = getRoleFromToken(token);
    setRole(userRole);
    setIsAdmin(userRole === "Admin");
  }, []);

  const navItems: NavItem[] = React.useMemo(() => {
    const items = [...baseNavItems];
    
    // Remove Templates for Manager role
    if (role === "Manager") {
      const templatesIndex = items.findIndex(item => item.path === "/templates");
      if (templatesIndex !== -1) {
        items.splice(templatesIndex, 1);
      }
    }
    
    // Remove Approvals for Admin role
    if (role === "Admin") {
      const approvalsIndex = items.findIndex(item => item.path === "/approvals");
      if (approvalsIndex !== -1) {
        items.splice(approvalsIndex, 1);
      }
    }
    
    // Add notification badge
    const notificationsIndex = items.findIndex(item => item.path === "/notifications");
    if (notificationsIndex !== -1) {
      items[notificationsIndex] = { ...items[notificationsIndex], badge: unreadCount };
    }
    // Add admin items if admin
    if (isAdmin) {
      items.push(...adminNavItems);
    }
    return items;
  }, [isAdmin, role, unreadCount]);

  const renderMenuItems = (
    navItems: NavItem[],
    menuType: "main" | "others"
  ) => (
    <ul className="flex flex-col gap-4">
      {navItems.map((nav, index) => (
        <li key={nav.name}>
          {nav.subItems ? (
            <button
              onClick={() => handleSubmenuToggle(index, menuType)}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors group w-full ${
                openSubmenu?.type === menuType && openSubmenu?.index === index
                  ? "bg-brand-50 text-brand-500 dark:bg-brand-500/15 dark:text-brand-400"
                  : "text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-white/5"
              } ${
                !isExpanded && !isHovered && !isMobileOpen
                  ? "lg:justify-center"
                  : "justify-start"
              }`}
            >
              <span
                className={`flex items-center justify-center flex-shrink-0 ${
                  openSubmenu?.type === menuType && openSubmenu?.index === index
                    ? "text-brand-500 dark:text-brand-400"
                    : "text-gray-500 dark:text-gray-400"
                }`}
              >
                {nav.icon}
              </span>
              {(isExpanded || isHovered || isMobileOpen) && (
                <span className="flex-1 text-sm font-medium whitespace-nowrap text-left">{nav.name}</span>
              )}
              {(isExpanded || isHovered || isMobileOpen) && (
                <ChevronDownIcon
                  className={`ml-auto w-5 h-5 transition-transform duration-200 flex-shrink-0 ${
                    openSubmenu?.type === menuType &&
                    openSubmenu?.index === index
                      ? "rotate-180 text-brand-500"
                      : "text-gray-400"
                  }`}
                />
              )}
            </button>
          ) : (
            nav.path && (
              <Link
                href={nav.path}
                className={`flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors group ${
                  isActive(nav.path)
                    ? "bg-brand-50 text-brand-500 dark:bg-brand-500/15 dark:text-brand-400"
                    : "text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-white/5"
                } ${
                  !isExpanded && !isHovered && !isMobileOpen
                    ? "lg:justify-center"
                    : "justify-start"
                }`}
              >
                <span
                  className={`flex items-center justify-center flex-shrink-0 ${
                    isActive(nav.path)
                      ? "text-brand-500 dark:text-brand-400"
                      : "text-gray-500 dark:text-gray-400"
                  }`}
                >
                  {nav.icon}
                </span>
                {(isExpanded || isHovered || isMobileOpen) && (
                  <span className="flex-1 text-sm font-medium whitespace-nowrap">{nav.name}</span>
                )}
                {nav.badge !== undefined && nav.badge > 0 && (isExpanded || isHovered || isMobileOpen) && (
                  <span className="ml-auto flex h-5 w-5 items-center justify-center rounded-full bg-error-500 text-[10px] font-bold text-white flex-shrink-0">
                    {nav.badge > 99 ? "99+" : nav.badge}
                  </span>
                )}
              </Link>
            )
          )}
          {nav.subItems && (isExpanded || isHovered || isMobileOpen) && (
            <div
              ref={(el) => {
                subMenuRefs.current[`${menuType}-${index}`] = el;
              }}
              className="overflow-hidden transition-all duration-300"
              style={{
                height:
                  openSubmenu?.type === menuType && openSubmenu?.index === index
                    ? `${subMenuHeight[`${menuType}-${index}`]}px`
                    : "0px",
              }}
            >
              <ul className="mt-2 space-y-1 ml-9">
                {nav.subItems.map((subItem) => (
                  <li key={subItem.name}>
                    <Link
                      href={subItem.path}
                      className={`flex items-center justify-between px-3 py-2 rounded-lg text-sm transition-colors ${
                        isActive(subItem.path)
                          ? "bg-brand-50 text-brand-500 dark:bg-brand-500/15 dark:text-brand-400 font-medium"
                          : "text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-white/5"
                      }`}
                    >
                      <span>{subItem.name}</span>
                      <span className="flex items-center gap-1 ml-2">
                        {subItem.new && (
                          <span
                            className={`px-1.5 py-0.5 text-[10px] font-medium rounded ${
                              isActive(subItem.path)
                                ? "bg-brand-100 text-brand-600 dark:bg-brand-500/30 dark:text-brand-400"
                                : "bg-gray-100 text-gray-600 dark:bg-white/10 dark:text-gray-400"
                            }`}
                          >
                            new
                          </span>
                        )}
                        {subItem.pro && (
                          <span
                            className={`px-1.5 py-0.5 text-[10px] font-medium rounded ${
                              isActive(subItem.path)
                                ? "bg-brand-100 text-brand-600 dark:bg-brand-500/30 dark:text-brand-400"
                                : "bg-gray-100 text-gray-600 dark:bg-white/10 dark:text-gray-400"
                            }`}
                          >
                            pro
                          </span>
                        )}
                      </span>
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </li>
      ))}
    </ul>
  );

  const [openSubmenu, setOpenSubmenu] = useState<{
    type: "main" | "others";
    index: number;
  } | null>(null);
  const [subMenuHeight, setSubMenuHeight] = useState<Record<string, number>>(
    {}
  );
  const subMenuRefs = useRef<Record<string, HTMLDivElement | null>>({});

  // Active when pathname matches or is a nested route
  const isActive = useCallback((path: string) => pathname === path || pathname.startsWith(path + "/"), [pathname]);

  useEffect(() => {
    // Check if the current path matches any submenu item
    let submenuMatched = false;
    ["main", "others"].forEach((menuType) => {
      const items = menuType === "main" ? navItems : othersItems;
      items.forEach((nav, index) => {
        if (nav.subItems) {
          nav.subItems.forEach((subItem) => {
            if (isActive(subItem.path)) {
              setOpenSubmenu({
                type: menuType as "main" | "others",
                index,
              });
              submenuMatched = true;
            }
          });
        }
      });
    });

    // If no submenu item matches, close the open submenu
    if (!submenuMatched) {
      setOpenSubmenu(null);
    }
  }, [pathname,isActive]);

  useEffect(() => {
    // Set the height of the submenu items when the submenu is opened
    if (openSubmenu !== null) {
      const key = `${openSubmenu.type}-${openSubmenu.index}`;
      if (subMenuRefs.current[key]) {
        setSubMenuHeight((prevHeights) => ({
          ...prevHeights,
          [key]: subMenuRefs.current[key]?.scrollHeight || 0,
        }));
      }
    }
  }, [openSubmenu]);

  const handleSubmenuToggle = (index: number, menuType: "main" | "others") => {
    setOpenSubmenu((prevOpenSubmenu) => {
      if (
        prevOpenSubmenu &&
        prevOpenSubmenu.type === menuType &&
        prevOpenSubmenu.index === index
      ) {
        return null;
      }
      return { type: menuType, index };
    });
  };

  return (
    <aside
      className={`fixed mt-16 flex flex-col lg:mt-0 top-0 px-5 left-0 bg-white dark:bg-gray-900 dark:border-gray-800 text-gray-900 h-screen transition-all duration-300 ease-in-out z-50 border-r border-gray-200 
        ${
          isExpanded || isMobileOpen
            ? "w-[290px]"
            : isHovered
            ? "w-[290px]"
            : "w-[90px]"
        }
        ${isMobileOpen ? "translate-x-0" : "-translate-x-full"}
        lg:translate-x-0`}
      onMouseEnter={() => !isExpanded && setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <div className={`py-8 flex ${!isExpanded && !isHovered ? "lg:justify-center" : "justify-start"}`}>
        <Link href="/dashboard" className="select-none">
          <span className={`font-semibold text-gray-900 dark:text-white/90 ${isExpanded || isHovered || isMobileOpen ? "text-xl" : "text-base"}`}>
            Q-Manage
          </span>
        </Link>
      </div>
      <div className="flex flex-col overflow-y-auto duration-300 ease-linear no-scrollbar">
        <nav className="mb-6">
          <div className="flex flex-col gap-4">
            <div>
              <h2
                className={`mb-4 text-xs uppercase flex leading-[20px] text-gray-400 ${
                  !isExpanded && !isHovered
                    ? "lg:justify-center"
                    : "justify-start"
                }`}
              >
                {isExpanded || isHovered || isMobileOpen ? (
                  "Menu"
                ) : (
                  <HorizontaLDots />
                )}
              </h2>
              {renderMenuItems(navItems, "main")}
            </div>

            {othersItems.length > 0 && (
              <div className="mt-6">
                <h2
                  className={`mb-4 text-xs uppercase flex leading-[20px] text-gray-400 ${
                    !isExpanded && !isHovered
                      ? "lg:justify-center"
                      : "justify-start"
                  }`}
                >
                  {isExpanded || isHovered || isMobileOpen ? (
                    "Others"
                  ) : (
                    <HorizontaLDots />
                  )}
                </h2>
                {renderMenuItems(othersItems, "others")}
              </div>
            )}
          </div>
        </nav>
        {/* no promo widget */}
      </div>
    </aside>
  );
};

export default AppSidebar;
