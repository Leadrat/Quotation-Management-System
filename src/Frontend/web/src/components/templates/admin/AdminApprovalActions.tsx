"use client";
import Link from "next/link";
import type { QuotationTemplate } from "@/types/templates";

interface AdminApprovalActionsProps {
  template: QuotationTemplate;
  onApprove: () => void;
}

export default function AdminApprovalActions({ template, onApprove }: AdminApprovalActionsProps) {
  const handleApprove = () => {
    if (confirm(`Are you sure you want to approve "${template.name}"? This will make it available to all users.`)) {
      onApprove();
    }
  };

  return (
    <div className="flex items-center gap-2 flex-wrap">
      <Link
        href={`/templates/${template.templateId}`}
        className="rounded bg-blue-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
      >
        View
      </Link>
      <button
        onClick={handleApprove}
        className="rounded bg-green-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
      >
        Approve
      </button>
    </div>
  );
}

