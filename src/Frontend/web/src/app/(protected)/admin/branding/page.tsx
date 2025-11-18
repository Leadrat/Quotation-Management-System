"use client";
import { useState, useEffect } from "react";
import { useBranding } from "@/hooks";
import Link from "next/link";

export default function BrandingPage() {
  const { branding, loading, error, saving, uploading, updateBranding, uploadLogo } =
    useBranding();
  const [form, setForm] = useState({
    primaryColor: "#3B82F6",
    secondaryColor: "#10B981",
    accentColor: "#F59E0B",
    footerHtml: "",
  });
  const [message, setMessage] = useState<string | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);

  useEffect(() => {
    if (branding) {
      setForm({
        primaryColor: branding.primaryColor || "#3B82F6",
        secondaryColor: branding.secondaryColor || "#10B981",
        accentColor: branding.accentColor || "#F59E0B",
        footerHtml: branding.footerHtml || "",
      });
    }
  }, [branding]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    const result = await updateBranding(form);
    if (result.success) {
      setMessage("Branding updated successfully!");
      setTimeout(() => setMessage(null), 3000);
    } else {
      setMessage(result.message);
    }
  };

  const handleLogoUpload = async () => {
    if (!logoFile) return;
    setMessage(null);
    const result = await uploadLogo(logoFile);
    if (result.success) {
      setMessage("Logo uploaded successfully!");
      setLogoFile(null);
      setTimeout(() => setMessage(null), 3000);
    } else {
      setMessage(result.message);
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <div className="text-center py-12">Loading branding settings...</div>
      </div>
    );
  }

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
          Custom Branding
        </h1>
        <p className="text-gray-600 dark:text-gray-400">
          Customize application branding with logo, colors, and footer
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
        <div className="space-y-6">
          {/* Logo Upload */}
          <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Logo Upload
            </h2>
            {branding?.logoUrl && (
              <div className="mb-4">
                <img
                  src={branding.logoUrl}
                  alt="Current logo"
                  className="max-h-32 mb-4"
                />
              </div>
            )}
            <div className="space-y-4">
              <input
                type="file"
                accept="image/png,image/jpeg,image/jpg,image/svg+xml"
                onChange={(e) => setLogoFile(e.target.files?.[0] || null)}
                className="w-full text-sm text-gray-600 dark:text-gray-400"
              />
              <button
                onClick={handleLogoUpload}
                disabled={!logoFile || uploading}
                className="px-4 py-2 text-sm font-medium text-white bg-brand-600 rounded-lg hover:bg-brand-700 disabled:opacity-50"
              >
                {uploading ? "Uploading..." : "Upload Logo"}
              </button>
            </div>
          </div>

          {/* Colors */}
          <form onSubmit={handleSubmit} className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Color Scheme
            </h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Primary Color
                </label>
                <div className="flex gap-2">
                  <input
                    type="color"
                    value={form.primaryColor}
                    onChange={(e) => setForm({ ...form, primaryColor: e.target.value })}
                    className="h-10 w-20 border border-gray-300 dark:border-gray-600 rounded"
                  />
                  <input
                    type="text"
                    value={form.primaryColor}
                    onChange={(e) => setForm({ ...form, primaryColor: e.target.value })}
                    className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Secondary Color
                </label>
                <div className="flex gap-2">
                  <input
                    type="color"
                    value={form.secondaryColor}
                    onChange={(e) => setForm({ ...form, secondaryColor: e.target.value })}
                    className="h-10 w-20 border border-gray-300 dark:border-gray-600 rounded"
                  />
                  <input
                    type="text"
                    value={form.secondaryColor}
                    onChange={(e) => setForm({ ...form, secondaryColor: e.target.value })}
                    className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Accent Color
                </label>
                <div className="flex gap-2">
                  <input
                    type="color"
                    value={form.accentColor}
                    onChange={(e) => setForm({ ...form, accentColor: e.target.value })}
                    className="h-10 w-20 border border-gray-300 dark:border-gray-600 rounded"
                  />
                  <input
                    type="text"
                    value={form.accentColor}
                    onChange={(e) => setForm({ ...form, accentColor: e.target.value })}
                    className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Footer HTML
                </label>
                <textarea
                  value={form.footerHtml}
                  onChange={(e) => setForm({ ...form, footerHtml: e.target.value })}
                  rows={4}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg"
                  placeholder="Enter HTML for footer"
                />
              </div>
              <button
                type="submit"
                disabled={saving}
                className="w-full px-4 py-2 text-sm font-medium text-white bg-brand-600 rounded-lg hover:bg-brand-700 disabled:opacity-50"
              >
                {saving ? "Saving..." : "Save Branding"}
              </button>
            </div>
          </form>
        </div>

        {/* Preview */}
        <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Preview
          </h2>
          <div className="space-y-4">
            <div
              className="p-4 rounded-lg text-white"
              style={{ backgroundColor: form.primaryColor }}
            >
              Primary Color
            </div>
            <div
              className="p-4 rounded-lg text-white"
              style={{ backgroundColor: form.secondaryColor }}
            >
              Secondary Color
            </div>
            <div
              className="p-4 rounded-lg text-white"
              style={{ backgroundColor: form.accentColor }}
            >
              Accent Color
            </div>
            {form.footerHtml && (
              <div
                className="p-4 border-t border-gray-200 dark:border-gray-700"
                dangerouslySetInnerHTML={{ __html: form.footerHtml }}
              />
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

