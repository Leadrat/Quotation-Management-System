"use client";
import React from "react";

export default function KpiCard({ title, value, subtitle }: { title: string; value: React.ReactNode; subtitle?: string }) {
  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
      <p className="text-sm text-gray-500 dark:text-gray-400">{title}</p>
      <div className="mt-2 text-2xl font-semibold text-gray-800 dark:text-white/90">{value}</div>
      {subtitle && <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">{subtitle}</p>}
    </div>
  );
}
