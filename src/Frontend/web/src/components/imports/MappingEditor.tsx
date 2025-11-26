"use client";

import React, { useEffect, useMemo, useState } from "react";
import { saveMappings } from "@/lib/api/imports";

type Props = {
  sessionId: string;
  suggestedMappingsJson?: string | null;
};

type Mappings = Record<string, any>;

function parseJsonOr<T>(input: string | null | undefined, fallback: T): T {
  if (!input) return fallback;
  try {
    return JSON.parse(input);
  } catch {
    return fallback;
  }
}

export default function MappingEditor({ sessionId, suggestedMappingsJson }: Props) {
  const [mappingsText, setMappingsText] = useState<string>("");
  const [error, setError] = useState<string>("");
  const [success, setSuccess] = useState<string>("");
  const [saving, setSaving] = useState(false);

  const initialObj = useMemo(() => parseJsonOr<Mappings>(suggestedMappingsJson, {
    company: {},
    customer: {},
    items: [],
    totals: {},
  }), [suggestedMappingsJson]);

  useEffect(() => {
    setMappingsText(JSON.stringify(initialObj, null, 2));
  }, [initialObj]);

  const validate = (obj: Mappings): string | null => {
    if (!obj.company || !obj.company.name) return "Missing required mapping: company.name";
    if (!obj.customer || !obj.customer.name) return "Missing required mapping: customer.name";
    return null;
  };

  const onSave = async () => {
    setError("");
    setSuccess("");
    let obj: Mappings;
    try {
      obj = JSON.parse(mappingsText);
    } catch {
      setError("Mappings JSON is invalid");
      return;
    }
    const v = validate(obj);
    if (v) {
      setError(v);
      return;
    }
    setSaving(true);
    try {
      await saveMappings(sessionId, obj);
      setSuccess("Mappings saved");
    } catch (e: any) {
      setError(e?.message ?? "Failed to save mappings");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-800 p-4 bg-white dark:bg-gray-900">
      <h2 className="font-semibold mb-2 text-gray-900 dark:text-white">Confirm Mappings</h2>
      <p className="text-sm text-gray-500 dark:text-gray-400 mb-2">Edit JSON mappings. Required: company.name, customer.name.</p>
      <textarea
        value={mappingsText}
        onChange={(e) => setMappingsText(e.target.value)}
        className="w-full h-56 rounded border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-xs p-2 text-gray-900 dark:text-gray-100"
      />
      <div className="mt-2 flex items-center gap-2">
        <button onClick={onSave} disabled={saving} className="rounded bg-primary text-white px-4 py-2 disabled:opacity-50">
          {saving ? "Saving..." : "Save Mappings"}
        </button>
        {error && <span className="text-sm text-red-500">{error}</span>}
        {success && <span className="text-sm text-green-600">{success}</span>}
      </div>
    </div>
  );
}
