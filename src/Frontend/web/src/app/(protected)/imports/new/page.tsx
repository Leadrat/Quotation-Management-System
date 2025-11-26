"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import { createImport } from "@/lib/api/imports";

export default function ImportNewPage() {
  const [fileName, setFileName] = useState<string>("");
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>("");
  const router = useRouter();

  return (
    <div className="p-6 min-h-[60vh] bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-800">
      <h1 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">New Import</h1>
      <div
        className="flex flex-col items-center justify-center gap-3 rounded-xl border-2 border-dashed border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 p-10 text-center"
      >
        <p className="text-gray-600 dark:text-gray-300">Drag & drop a file here (pdf, docx, xlsx, xslt, dotx) or click to select</p>
        <input
          type="file"
          className="block w-full max-w-md text-sm text-gray-600 dark:text-gray-300"
          aria-label="Upload document to import"
          accept=".pdf,.docx,.xlsx,.xslt,.dotx,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,text/xml,application/xml,application/vnd.openxmlformats-officedocument.wordprocessingml.template"
          onChange={(e) => {
            const f = e.target.files?.[0];
            setError("");
            if (f) {
              const maxBytes = 10 * 1024 * 1024; // 10MB
              const allowed = ["pdf","docx","xlsx","xslt","dotx"];
              const ext = f.name.split(".").pop()?.toLowerCase();
              if (!ext || !allowed.includes(ext)) {
                setError("Unsupported file type. Allowed: pdf, docx, xlsx, xslt, dotx");
                e.currentTarget.value = "";
                setFile(null);
                setFileName("");
                return;
              }
              if (f.size > maxBytes) {
                setError("File is larger than 10MB limit");
                e.currentTarget.value = "";
                setFile(null);
                setFileName("");
                return;
              }
              setFileName(f.name);
              setFile(f);
            } else {
              setFileName("");
              setFile(null);
            }
          }}
        />
        {fileName && (
          <p className="text-sm text-gray-500 dark:text-gray-400">Selected: {fileName}</p>
        )}
        <button
          disabled={!file || loading}
          onClick={async () => {
            if (!file) return;
            setError("");
            setLoading(true);
            try {
              const fd = new FormData();
              fd.append("file", file);
              const session = await createImport(fd);
              router.push(`/imports/session/${session.importSessionId}`);
            } catch (e: any) {
              setError(e?.message ?? "Failed to import file");
            } finally {
              setLoading(false);
            }
          }}
          className="mt-2 inline-flex items-center rounded-md bg-primary px-4 py-2 text-white disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-primary/60"
          aria-label="Create import session"
        >
          {loading ? "Uploading..." : "Create Import Session"}
        </button>
        {error && <p className="text-sm text-red-500 mt-2">{error}</p>}
      </div>
    </div>
  );
}
