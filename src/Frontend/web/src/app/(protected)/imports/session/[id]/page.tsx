"use client";

import React, { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { getImport } from "@/lib/api/imports";
import ParsedPreview from "@/components/imports/ParsedPreview";
import ChatPanel from "@/components/imports/ChatPanel";
import MappingEditor from "@/components/imports/MappingEditor";
import TemplatePreview from "@/components/imports/TemplatePreview";

export default function ImportSessionPage() {
  const params = useParams<{ id: string }>();
  const id = params?.id as string;
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>("");
  const [session, setSession] = useState<any>(null);

  useEffect(() => {
    let mounted = true;
    (async () => {
      try {
        const s = await getImport(id);
        if (mounted) setSession(s);
      } catch (e: any) {
        if (mounted) setError(e?.message ?? "Failed to load session");
      } finally {
        if (mounted) setLoading(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [id]);

  return (
    <div className="p-6 min-h-[60vh] bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-800">
      <h1 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">Import Session</h1>
      {loading && <p className="text-gray-600 dark:text-gray-300">Loading...</p>}
      {error && <p className="text-red-500">{error}</p>}
      {session && (
        <div className="space-y-3">
          <p className="text-sm text-gray-500 dark:text-gray-400">Status: {session.status}</p>
          <ParsedPreview suggestedMappingsJson={session.suggestedMappingsJson ?? session.suggestedMappings} />
          <ChatPanel sessionId={id} />
          <MappingEditor sessionId={id} suggestedMappingsJson={session.suggestedMappingsJson ?? session.suggestedMappings} />
          <TemplatePreview sessionId={id} />
        </div>
      )}
    </div>
  );
}
