"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { TemplatesApi } from "@/lib/api";
import { LineItemsEditor, TemplatePreview, TemplateErrorBoundary, TemplateFormSkeleton } from "@/components/templates";
import { useToast, ToastContainer } from "@/components/quotations/Toast";
import type { CreateQuotationTemplateRequest, CreateTemplateLineItemRequest, QuotationTemplate } from "@/types/templates";

export default function CreateTemplatePage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [visibility, setVisibility] = useState<"Public" | "Team" | "Private">("Private");
  const [discountDefault, setDiscountDefault] = useState<number | undefined>(undefined);
  const [notes, setNotes] = useState("");
  const [lineItems, setLineItems] = useState<CreateTemplateLineItemRequest[]>([
    { itemName: "", description: "", quantity: 1, unitRate: 0 },
  ]);
  const [showPreview, setShowPreview] = useState(false);
  const [previewTemplate, setPreviewTemplate] = useState<QuotationTemplate | null>(null);
  const toast = useToast();

  const updateLineItem = (index: number, field: keyof CreateTemplateLineItemRequest, value: any) => {
    const updated = [...lineItems];
    updated[index] = { ...updated[index], [field]: value };
    setLineItems(updated);
  };

  const addLineItem = () => {
    setLineItems([...lineItems, { itemName: "", description: "", quantity: 1, unitRate: 0 }]);
  };

  const removeLineItem = (index: number) => {
    if (lineItems.length > 1) {
      setLineItems(lineItems.filter((_, i) => i !== index));
    }
  };

  const handlePreview = () => {
    // Create a preview template object
    const preview: QuotationTemplate = {
      templateId: "",
      name: name || "Preview Template",
      description: description || undefined,
      ownerUserId: "",
      ownerUserName: "",
      ownerRole: "",
      visibility,
      isApproved: false,
      version: 1,
      usageCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      discountDefault,
      notes: notes || undefined,
      lineItems: lineItems.map((item, idx) => ({
        lineItemId: `preview-${idx}`,
        templateId: "",
        sequenceNumber: idx + 1,
        itemName: item.itemName,
        description: item.description,
        quantity: item.quantity,
        unitRate: item.unitRate,
        amount: item.quantity * item.unitRate,
        createdAt: new Date().toISOString(),
      })),
      isActive: true,
      isEditable: true,
    };
    setPreviewTemplate(preview);
    setShowPreview(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setError("Template name is required");
      return;
    }
    if (lineItems.some((item) => !item.itemName || item.quantity <= 0 || item.unitRate <= 0)) {
      setError("Please fill in all line items with valid values");
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const payload: CreateQuotationTemplateRequest = {
        name: name.trim(),
        description: description.trim() || undefined,
        visibility,
        discountDefault,
        notes: notes.trim() || undefined,
        lineItems: lineItems.map((item) => ({
          itemName: item.itemName.trim(),
          description: item.description?.trim() || undefined,
          quantity: item.quantity,
          unitRate: item.unitRate,
        })),
      };

      toast.info("Creating template...");
      const result = await TemplatesApi.create(payload);
      toast.success("Template created successfully!");
      router.push(`/templates/${result.data.templateId}`);
    } catch (err: any) {
      const errorMsg = err.message || "Failed to create template";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  if (showPreview && previewTemplate) {
    return <TemplatePreview template={previewTemplate} onClose={() => setShowPreview(false)} />;
  }

  return (
    <TemplateErrorBoundary>
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <div className="mb-6">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Create Template</h4>
      </div>

      {error && (
        <div className="mb-4 rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic Info */}
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <div className="md:col-span-2">
            <label className="mb-2.5 block text-black dark:text-white">Template Name *</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              maxLength={100}
              className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            />
          </div>
          <div className="md:col-span-2">
            <label className="mb-2.5 block text-black dark:text-white">Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              maxLength={255}
              rows={3}
              className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            />
          </div>
          <div>
            <label className="mb-2.5 block text-black dark:text-white">Visibility *</label>
            <select
              value={visibility}
              onChange={(e) => setVisibility(e.target.value as "Public" | "Team" | "Private")}
              required
              className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            >
              <option value="Private">Private (Only Me)</option>
              <option value="Team">Team (Same Role)</option>
              <option value="Public">Public (All Users - Requires Approval)</option>
            </select>
          </div>
          <div>
            <label className="mb-2.5 block text-black dark:text-white">Default Discount (%)</label>
            <input
              type="number"
              min="0"
              max="100"
              step="0.01"
              value={discountDefault || ""}
              onChange={(e) => setDiscountDefault(e.target.value ? parseFloat(e.target.value) : undefined)}
              className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            />
          </div>
        </div>

        {/* Line Items */}
        <LineItemsEditor
          lineItems={lineItems}
          onUpdate={updateLineItem}
          onAdd={addLineItem}
          onRemove={removeLineItem}
        />

        {/* Notes */}
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Notes & Terms</label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            maxLength={2000}
            rows={5}
            placeholder="Enter any notes or terms that should be included in quotations created from this template..."
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>

        {/* Actions */}
        <div className="flex items-center justify-end gap-4">
          <button
            type="button"
            onClick={() => router.back()}
            className="rounded border border-stroke px-6 py-2.5 font-medium hover:bg-gray-50 dark:border-strokedark dark:hover:bg-meta-4"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handlePreview}
            className="rounded bg-blue-500 px-6 py-2.5 font-medium text-white hover:bg-opacity-90"
          >
            Preview
          </button>
          <button
            type="submit"
            disabled={loading}
            className="rounded bg-primary px-6 py-2.5 font-medium text-white hover:bg-opacity-90 disabled:opacity-50"
          >
            {loading ? "Creating..." : "Create Template"}
          </button>
        </div>
      </form>
      </div>
    </TemplateErrorBoundary>
  );
}

