"use client";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { TemplatesApi } from "@/lib/api";
import { LineItemsEditor, TemplatePreview } from "@/components/templates";
import type {
  QuotationTemplate,
  UpdateQuotationTemplateRequest,
  UpdateTemplateLineItemRequest,
} from "@/types/templates";

export default function EditTemplatePage() {
  const params = useParams();
  const router = useRouter();
  const templateId = String(params?.id || "");
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [template, setTemplate] = useState<QuotationTemplate | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [visibility, setVisibility] = useState<"Public" | "Team" | "Private">("Private");
  const [discountDefault, setDiscountDefault] = useState<number | undefined>(undefined);
  const [notes, setNotes] = useState("");
  const [lineItems, setLineItems] = useState<UpdateTemplateLineItemRequest[]>([]);
  const [showPreview, setShowPreview] = useState(false);
  const [previewTemplate, setPreviewTemplate] = useState<QuotationTemplate | null>(null);

  useEffect(() => {
    if (templateId) {
      loadTemplate();
    }
  }, [templateId]);

  const loadTemplate = async () => {
    try {
      setInitialLoading(true);
      setError(null);
      const result = await TemplatesApi.get(templateId);
      const t = result.data;
      setTemplate(t);
      setName(t.name);
      setDescription(t.description || "");
      setVisibility(t.visibility);
      setDiscountDefault(t.discountDefault);
      setNotes(t.notes || "");
      setLineItems(
        t.lineItems.map((item) => ({
          lineItemId: item.lineItemId,
          itemName: item.itemName,
          description: item.description,
          quantity: item.quantity,
          unitRate: item.unitRate,
        }))
      );
    } catch (err: any) {
      setError(err.message || "Failed to load template");
    } finally {
      setInitialLoading(false);
    }
  };

  const updateLineItem = (index: number, field: keyof UpdateTemplateLineItemRequest, value: any) => {
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
    if (!template) return;
    const preview: QuotationTemplate = {
      ...template,
      name: name || template.name,
      description: description || template.description,
      visibility,
      discountDefault,
      notes: notes || template.notes,
      lineItems: lineItems.map((item, idx) => ({
        lineItemId: item.lineItemId || `preview-${idx}`,
        templateId: template.templateId,
        sequenceNumber: idx + 1,
        itemName: item.itemName,
        description: item.description,
        quantity: item.quantity,
        unitRate: item.unitRate,
        amount: item.quantity * item.unitRate,
        createdAt: new Date().toISOString(),
      })),
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
      const payload: UpdateQuotationTemplateRequest = {
        name: name.trim(),
        description: description.trim() || undefined,
        visibility,
        discountDefault,
        notes: notes.trim() || undefined,
        lineItems: lineItems.map((item) => ({
          lineItemId: item.lineItemId,
          itemName: item.itemName.trim(),
          description: item.description?.trim() || undefined,
          quantity: item.quantity,
          unitRate: item.unitRate,
        })),
      };

      const result = await TemplatesApi.update(templateId, payload);
      router.push(`/templates/${result.data.templateId}`);
    } catch (err: any) {
      const errorMsg = err.message || "Failed to update template";
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  if (initialLoading) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center">Loading template...</div>
      </div>
    );
  }

  if (error && !template) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      </div>
    );
  }

  if (showPreview && previewTemplate) {
    return <TemplatePreview template={previewTemplate} onClose={() => setShowPreview(false)} />;
  }

  if (!template) return null;

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <div className="mb-6">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Edit Template: {template.name}</h4>
        <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
          Note: Editing will create a new version. Current version: v{template.version}
        </p>
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
            {loading ? "Updating..." : "Update Template"}
          </button>
        </div>
      </form>
    </div>
  );
}

