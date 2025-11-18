"use client";
import Link from "next/link";

export default function AdminConsolePage() {
  const adminFeatures = [
    {
      title: "System Settings",
      description: "Manage company name, date formats, currencies, and notification preferences",
      href: "/admin/settings/system",
      icon: "⚙️",
    },
    {
      title: "Integration Keys",
      description: "Securely manage API keys and credentials for third-party services",
      href: "/admin/integrations",
      icon: "🔑",
    },
    {
      title: "Audit Logs",
      description: "View, search, filter, and export audit logs of all system actions",
      href: "/admin/audit-logs",
      icon: "📋",
    },
    {
      title: "Custom Branding",
      description: "Customize application branding (logo, colors, footer) with live preview",
      href: "/admin/branding",
      icon: "🎨",
    },
    {
      title: "Data Retention",
      description: "Configure data retention policies for different entity types",
      href: "/admin/data-retention",
      icon: "🗄️",
    },
    {
      title: "Global Messages",
      description: "Set global banner messages that appear to all users",
      href: "/admin/notifications",
      icon: "📢",
    },
  ];

  return (
    <div className="p-6">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
          System Administration & Configuration
        </h1>
        <p className="text-gray-600 dark:text-gray-400">
          Manage system settings, integrations, branding, and compliance policies
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {adminFeatures.map((feature) => (
          <Link
            key={feature.href}
            href={feature.href}
            className="block p-6 bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 hover:border-brand-500 dark:hover:border-brand-500 transition-colors"
          >
            <div className="text-4xl mb-4">{feature.icon}</div>
            <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
              {feature.title}
            </h3>
            <p className="text-sm text-gray-600 dark:text-gray-400">
              {feature.description}
            </p>
          </Link>
        ))}
      </div>
    </div>
  );
}

