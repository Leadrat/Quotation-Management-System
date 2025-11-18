"use client";
import { useState, useEffect } from "react";
import { useNotificationSettings } from "@/hooks";
import Link from "next/link";

export default function NotificationSettingsPage() {
  const { settings, loading, error, saving, updateSettings } = useNotificationSettings();
  const [form, setForm] = useState({
    bannerMessage: "",
    bannerType: "info" as "info" | "warning" | "error" | "success",
    isVisible: false,
  });
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    if (settings) {
      setForm({
        bannerMessage: settings.bannerMessage || "",
        bannerType: settings.bannerType || "info",
        isVisible: settings.isVisible ?? false,
      });
    }
  }, [settings]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    const result = await updateSettings(form);
    if (result.success) {
      setMessage("Notification settings updated successfully!");
      setTimeout(() => setMessage(null), 3000);
    } else {
      setMessage(result.message);
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <div className="text-center py-12">Loading notification settings...</div>
      </div>
    );
  }

  const bannerColors = {
    info: "bg-blue-50 border-blue-200 text-blue-800 dark:bg-blue-900/20 dark:border-blue-800 dark:text-blue-400",
    warning: "bg-yellow-50 border-yellow-200 text-yellow-800 dark:bg-yellow-900/20 dark:border-yellow-800 dark:text-yellow-400",
    error: "bg-red-50 border-red-200 text-red-800 dark:bg-red-900/20 dark:border-red-800 dark:text-red-400",
    success: "bg-green-50 border-green-200 text-green-800 dark:bg-green-900/20 dark:border-green-800 dark:text-green-400",
  };

  return (
    <div className="p-6">
      <div className="mb-6">
        <Link
          href="/admin"
          className="text-sm text-brand-600 hover:text-brand-700 mb-4 inline-block"
        >
          ← Back to Admin Console
        </Link>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
          Global System Messages
        </h1>
        <p className="text-gray-600 dark:text-gray-400">
          Set global banner messages that appear to all users
        </p>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg text-red-700 dark:text-red-400">
          {error}
        </div>
      )}

      {message && (
        <div
          className={`mb-4 p-4 rounded-lg ${
            message.includes("success")
              ? "bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 text-green-700 dark:text-green-400"
              : "bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-400"
          }`}
        >
          {message}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <form
          onSubmit={handleSubmit}
          className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6"
        >
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Banner Settings
          </h2>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Banner Message
              </label>
              <textarea
                value={form.bannerMessage}
                onChange={(e) => setForm({ ...form, bannerMessage: e.target.value })}
                rows={4}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
                placeholder="Enter banner message (HTML allowed)"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Banner Type
              </label>
              <select
                value={form.bannerType}
                onChange={(e) =>
                  setForm({
                    ...form,
                    bannerType: e.target.value as "info" | "warning" | "error" | "success",
                  })
                }
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
              >
                <option value="info">Info</option>
                <option value="warning">Warning</option>
                <option value="error">Error</option>
                <option value="success">Success</option>
              </select>
            </div>

            <div className="flex items-center">
              <input
                type="checkbox"
                id="isVisible"
                checked={form.isVisible}
                onChange={(e) => setForm({ ...form, isVisible: e.target.checked })}
                className="w-4 h-4 text-brand-600 border-gray-300 rounded focus:ring-brand-500"
              />
              <label
                htmlFor="isVisible"
                className="ml-2 text-sm text-gray-700 dark:text-gray-300"
              >
                Make banner visible to all users
              </label>
            </div>

            <button
              type="submit"
              disabled={saving}
              className="w-full px-4 py-2 text-sm font-medium text-white bg-brand-600 rounded-lg hover:bg-brand-700 disabled:opacity-50"
            >
              {saving ? "Saving..." : "Save Settings"}
            </button>
          </div>
        </form>

        {/* Preview */}
        <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Preview
          </h2>
          {form.isVisible && form.bannerMessage ? (
            <div
              className={`p-4 rounded-lg border ${bannerColors[form.bannerType]}`}
              dangerouslySetInnerHTML={{ __html: form.bannerMessage }}
            />
          ) : (
            <div className="p-4 text-center text-gray-500 dark:text-gray-400 border border-gray-200 dark:border-gray-700 rounded-lg">
              {form.isVisible
                ? "Enter a message to see preview"
                : "Enable visibility to see preview"}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

