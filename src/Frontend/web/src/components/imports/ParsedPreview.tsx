"use client";

import React from "react";

type Props = { suggestedMappingsJson?: string | null };

export default function ParsedPreview({ suggestedMappingsJson }: Props) {
  let pretty = "";
  try {
    pretty = suggestedMappingsJson ? JSON.stringify(JSON.parse(suggestedMappingsJson), null, 2) : "{}";
  } catch {
    pretty = suggestedMappingsJson ?? "{}";
  }

  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-900 p-4 overflow-auto">
      <pre className="text-xs text-gray-800 dark:text-gray-200 whitespace-pre-wrap">{pretty}</pre>
    </div>
  );
}
