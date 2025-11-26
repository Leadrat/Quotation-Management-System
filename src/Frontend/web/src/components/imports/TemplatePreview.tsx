"use client";

import React, { useState } from "react";
import { generateTemplate, getPreviewUrl, saveTemplateApi } from "@/lib/api/imports";

type Props = { sessionId: string };

export default function TemplatePreview({ sessionId }: Props) {
  const [previewReady, setPreviewReady] = useState(false);
  const [saving, setSaving] = useState(false);
  const [name, setName] = useState("");
  const [type, setType] = useState("generic");
  const [error, setError] = useState("");
  const [ok, setOk] = useState<string>("");

  const onGenerate = async () => {
    setError("");
    setOk("");
    try {
      await generateTemplate(sessionId);
      setPreviewReady(true);
    } catch (e: any) {
      setError(e?.message ?? "Failed to generate");
    }
  };

  const onSave = async () => {
    setError("");
    setOk("");
    setSaving(true);
    try {
      const res = await saveTemplateApi(sessionId, name, type);
      setOk(`Saved template: ${res?.templateId ?? "ok"}`);
    } catch (e: any) {
      setError(e?.message ?? "Failed to save template");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-800 p-4 bg-white dark:bg-gray-900">
      <h2 className="font-semibold mb-2 text-gray-900 dark:text-white">Template Preview</h2>
      <div className="flex items-center gap-2 mb-2">
        <button onClick={onGenerate} className="rounded bg-primary text-white px-4 py-2">Generate Preview</button>
        {previewReady && (
          <a href={getPreviewUrl(sessionId)} target="_blank" className="text-primary underline text-sm">Open Preview</a>
        )}
      </div>
      <div className="flex items-center gap-2">
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Template name" className="rounded border px-3 py-2 text-sm bg-white dark:bg-gray-800 border-gray-300 dark:border-gray-700" />
        <input value={type} onChange={(e) => setType(e.target.value)} placeholder="Type (invoice/quotation/generic)" className="rounded border px-3 py-2 text-sm bg-white dark:bg-gray-800 border-gray-300 dark:border-gray-700" />
        <button onClick={onSave} disabled={saving} className="rounded bg-primary text-white px-4 py-2 disabled:opacity-50">{saving ? "Saving..." : "Save as Template"}</button>
      </div>
      {error && <p className="text-sm text-red-500 mt-2">{error}</p>}
      {ok && <p className="text-sm text-green-600 mt-2">{ok}</p>}
    </div>
  );
}
