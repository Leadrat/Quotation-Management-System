"use client";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { TemplatesApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import { TemplateStatusBadge, TemplatePreview } from "@/components/templates";
import { formatCurrency } from "@/utils/quotationFormatter";
import type { QuotationTemplate } from "@/types/templates";

export default function TemplateDetailPage() {
  const params = useParams();
  const router = useRouter();
  const templateId = String(params?.id || "");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [template, setTemplate] = useState<QuotationTemplate | null>(null);
  const [showPreview, setShowPreview] = useState(false);
  const [userRole, setUserRole] = useState<string | null>(null);

  useEffect(() => {
    const token = getAccessToken();
    const role = getRoleFromToken(token);
    setUserRole(role);
    
    // Allow Admin and SalesRep to access templates, redirect others
    if (role !== "Admin" && role !== "SalesRep") {
      router.replace("/dashboard");
      return;
    }
    
    if (templateId) {
      loadTemplate();
    }
  }, [templateId, router]);

  const loadTemplate = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await TemplatesApi.get(templateId);
      setTemplate(result.data);
    } catch (err: any) {
      setError(err.message || "Failed to load template");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!confirm("Are you sure you want to delete this template? It will be soft-deleted and can be restored later.")) return;
    try {
      await TemplatesApi.delete(templateId);
      router.push("/templates");
    } catch (err: any) {
      alert(err.message || "Failed to delete template");
    }
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleString();
  };

  if (loading) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center">Loading template...</div>
      </div>
    );
  }

  if (error || !template) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error || "Template not found"}</p>
        </div>
      </div>
    );
  }

  if (showPreview) {
    return <TemplatePreview template={template} onClose={() => setShowPreview(false)} />;
  }

  const subtotal = template.lineItems.reduce((sum, item) => sum + item.amount, 0);
  const discountAmount = template.discountDefault ? (subtotal * template.discountDefault) / 100 : 0;
  const afterDiscount = subtotal - discountAmount;
  const taxAmount = afterDiscount * 0.18;
  const total = afterDiscount + taxAmount;

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h4 className="text-title-md2 font-bold text-black dark:text-white">{template.name}</h4>
          {template.description && (
            <p className="mt-2 text-gray-600 dark:text-gray-400">{template.description}</p>
          )}
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setShowPreview(true)}
            className="rounded bg-blue-500 px-4 py-2 text-sm text-white hover:bg-opacity-90"
          >
            Preview
          </button>
          {(userRole === "Admin" || userRole === "SalesRep") && (
            <Link
              href={`/templates/${templateId}/edit`}
              className="rounded bg-yellow-500 px-4 py-2 text-sm text-white hover:bg-opacity-90"
            >
              Edit
            </Link>
          )}
          {(userRole === "Admin" || userRole === "SalesRep") && (
            <Link
              href={`/templates/${templateId}/placeholders`}
              className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-opacity-90"
            >
              Configure Placeholders
            </Link>
          )}
          {userRole === "Admin" && (
            <button
              onClick={handleDelete}
              className="rounded bg-red-500 px-4 py-2 text-sm text-white hover:bg-opacity-90"
            >
              Delete
            </button>
          )}
        </div>
      </div>

      {/* Template Info */}
      <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-3">
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <p className="text-sm text-gray-600 dark:text-gray-400">Owner</p>
          <p className="mt-1 font-semibold text-black dark:text-white">{template.ownerUserName}</p>
        </div>
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <p className="text-sm text-gray-600 dark:text-gray-400">Status</p>
          <div className="mt-1">
            <TemplateStatusBadge visibility={template.visibility} isApproved={template.isApproved} />
          </div>
        </div>
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <p className="text-sm text-gray-600 dark:text-gray-400">Version</p>
          <p className="mt-1 font-semibold text-black dark:text-white">v{template.version}</p>
        </div>
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <p className="text-sm text-gray-600 dark:text-gray-400">Usage Count</p>
          <p className="mt-1 font-semibold text-black dark:text-white">{template.usageCount}</p>
        </div>
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <p className="text-sm text-gray-600 dark:text-gray-400">Last Used</p>
          <p className="mt-1 font-semibold text-black dark:text-white">
            {template.lastUsedAt ? formatDate(template.lastUsedAt) : "Never"}
          </p>
        </div>
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <p className="text-sm text-gray-600 dark:text-gray-400">Created</p>
          <p className="mt-1 font-semibold text-black dark:text-white">{formatDate(template.createdAt)}</p>
        </div>
      </div>

      {/* Line Items */}
      <div className="mb-6">
        <h5 className="mb-4 text-lg font-semibold text-black dark:text-white">Line Items</h5>
        <div className="max-w-full overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="bg-gray-2 text-left dark:bg-meta-4">
                <th className="px-4 py-3 font-medium text-black dark:text-white">#</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Item Name</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Description</th>
                <th className="px-4 py-3 text-right font-medium text-black dark:text-white">Quantity</th>
                <th className="px-4 py-3 text-right font-medium text-black dark:text-white">Unit Rate</th>
                <th className="px-4 py-3 text-right font-medium text-black dark:text-white">Amount</th>
              </tr>
            </thead>
            <tbody>
              {template.lineItems.map((item, index) => (
                <tr key={item.lineItemId || index} className="border-b border-[#eee] dark:border-strokedark">
                  <td className="px-4 py-3 text-black dark:text-white">{item.sequenceNumber || index + 1}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{item.itemName}</td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{item.description || "-"}</td>
                  <td className="px-4 py-3 text-right text-black dark:text-white">{item.quantity}</td>
                  <td className="px-4 py-3 text-right text-black dark:text-white">{formatCurrency(item.unitRate)}</td>
                  <td className="px-4 py-3 text-right font-medium text-black dark:text-white">
                    {formatCurrency(item.amount)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Totals Summary */}
      <div className="mb-6 ml-auto w-full max-w-md space-y-2 border-t border-stroke pt-4 dark:border-strokedark">
        <div className="flex justify-between text-black dark:text-white">
          <span>Subtotal:</span>
          <span>{formatCurrency(subtotal)}</span>
        </div>
        {template.discountDefault && template.discountDefault > 0 && (
          <div className="flex justify-between text-black dark:text-white">
            <span>Discount ({template.discountDefault}%):</span>
            <span>-{formatCurrency(discountAmount)}</span>
          </div>
        )}
        <div className="flex justify-between text-black dark:text-white">
          <span>Tax (18% GST):</span>
          <span>{formatCurrency(taxAmount)}</span>
        </div>
        <div className="flex justify-between border-t border-stroke pt-2 text-lg font-bold text-black dark:border-strokedark dark:text-white">
          <span>Total:</span>
          <span>{formatCurrency(total)}</span>
        </div>
      </div>

      {/* Notes */}
      {template.notes && (
        <div className="mb-6 rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
          <h5 className="mb-2 font-semibold text-black dark:text-white">Notes & Terms</h5>
          <p className="whitespace-pre-wrap text-sm text-gray-700 dark:text-gray-300">{template.notes}</p>
        </div>
      )}

      {/* Actions */}
      {(userRole === "Admin" || userRole === "SalesRep") && (
        <div className="flex items-center gap-2">
          <Link
            href={`/templates/${templateId}/versions`}
            className="rounded bg-blue-500 px-4 py-2 text-sm text-white hover:bg-opacity-90"
          >
            View Version History
          </Link>
        </div>
      )}
    </div>
  );
}

