"use client";
import Link from "next/link";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { PlugInIcon, LockIcon, TableIcon, BoxIcon, FolderIcon, BellIcon } from "@/icons";

export default function AdminConsolePage() {
  const adminFeatures = [
    {
      title: "System Settings",
      description: "Manage company name, date formats, currencies, and notification preferences",
      href: "/admin/settings/system",
      icon: <PlugInIcon className="w-8 h-8 text-brand-500" />,
    },
    {
      title: "Integration Keys",
      description: "Securely manage API keys and credentials for third-party services",
      href: "/admin/integrations",
      icon: <LockIcon className="w-8 h-8 text-brand-500" />,
    },
    {
      title: "Audit Logs",
      description: "View, search, filter, and export audit logs of all system actions",
      href: "/admin/audit-logs",
      icon: <TableIcon className="w-8 h-8 text-brand-500" />,
    },
    {
      title: "Custom Branding",
      description: "Customize application branding (logo, colors, footer) with live preview",
      href: "/admin/branding",
      icon: <BoxIcon className="w-8 h-8 text-brand-500" />,
    },
    {
      title: "Data Retention",
      description: "Configure data retention policies for different entity types",
      href: "/admin/data-retention",
      icon: <FolderIcon className="w-8 h-8 text-brand-500" />,
    },
    {
      title: "Global Messages",
      description: "Set global banner messages that appear to all users",
      href: "/admin/notifications",
      icon: <BellIcon className="w-8 h-8 text-brand-500" />,
    },
  ];

  return (
    <>
      <PageBreadcrumb pageTitle="System Administration" />

      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">
          System Administration & Configuration
        </h2>
        <p className="text-gray-500 dark:text-gray-400 mt-1">
          Manage system settings, integrations, branding, and compliance policies
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {adminFeatures.map((feature) => (
          <Link
            key={feature.href}
            href={feature.href}
            className="block"
          >
            <ComponentCard title={feature.title} className="hover:border-brand-500 transition-colors cursor-pointer h-full">
              <div className="flex items-start gap-4">
                <div className="flex-shrink-0">
                  {feature.icon}
                </div>
                <div className="flex-1">
                  <h3 className="text-lg font-semibold text-gray-800 dark:text-white/90 mb-2">
                    {feature.title}
                  </h3>
                  <p className="text-sm text-gray-500 dark:text-gray-400">
                    {feature.description}
                  </p>
                </div>
              </div>
            </ComponentCard>
          </Link>
        ))}
      </div>
    </>
  );
}

