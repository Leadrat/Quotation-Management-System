"use client";
import { useState, useEffect } from "react";
import { TemplatesApi } from "@/lib/api";
import type { QuotationTemplate } from "@/types/templates";
import TemplatePreview from "./TemplatePreview";

interface ApplyTemplateModalProps {
  clientId: string;
  onSelect: (template: QuotationTemplate) => void;
  onClose: () => void;
}

export default function ApplyTemplateModal({ clientId, onSelect, onClose }: ApplyTemplateModalProps) {
  const [loading, setLoading] = useState(true);
  const [templates, setTemplates] = useState<QuotationTemplate[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedTemplate, setSelectedTemplate] = useState<QuotationTemplate | null>(null);
  const [showPreview, setShowPreview] = useState(false);

  useEffect(() => {
    loadTemplates();
  }, []);

  const loadTemplates = async () => {
    try {
      setLoading(true);
      const result = await TemplatesApi.getPublic();
      setTemplates(result.data || []);
    } catch (err: any) {
      console.error("Failed to load templates:", err);
    } finally {
      setLoading(false);
    }
  };

  const filteredTemplates = templates.filter(
    (t) =>
      t.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (t.description && t.description.toLowerCase().includes(searchTerm.toLowerCase()))
  );

  const handleApply = () => {
    if (selectedTemplate) {
      onSelect(selectedTemplate);
      onClose();
    }
  };

  if (showPreview && selectedTemplate) {
    return (
      <TemplatePreview
        template={selectedTemplate}
        onClose={() => setShowPreview(false)}
      />
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="relative w-full max-w-3xl rounded-lg bg-white p-6 shadow-lg dark:bg-boxdark">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-xl font-bold text-black dark:text-white">Apply Template</h3>
          <button
            onClick={onClose}
            className="rounded bg-gray-500 px-4 py-2 text-white hover:bg-opacity-90"
          >
            Close
          </button>
        </div>

        {/* Search */}
        <div className="mb-4">
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search templates..."
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-4 py-2 text-black outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>

        {/* Template List */}
        <div className="max-h-96 overflow-y-auto">
          {loading ? (
            <div className="py-8 text-center text-black dark:text-white">Loading templates...</div>
          ) : filteredTemplates.length === 0 ? (
            <div className="py-8 text-center text-gray-500 dark:text-gray-400">No templates found</div>
          ) : (
            <div className="space-y-2">
              {filteredTemplates.map((template) => (
                <div
                  key={template.templateId}
                  className={`cursor-pointer rounded border p-4 transition ${
                    selectedTemplate?.templateId === template.templateId
                      ? "border-primary bg-primary/10"
                      : "border-stroke hover:border-primary dark:border-strokedark"
                  }`}
                  onClick={() => setSelectedTemplate(template)}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <h4 className="font-semibold text-black dark:text-white">{template.name}</h4>
                      {template.description && (
                        <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">{template.description}</p>
                      )}
                      <div className="mt-2 flex items-center gap-4 text-xs text-gray-500 dark:text-gray-400">
                        <span className="text-black dark:text-white">{template.lineItems.length} items</span>
                        <span className="text-black dark:text-white">Used {template.usageCount} times</span>
                        {template.isApproved && (
                          <span className="rounded bg-green-100 px-2 py-1 text-green-800 dark:bg-green-900 dark:text-green-300">
                            Approved
                          </span>
                        )}
                      </div>
                    </div>
                    <div className="ml-4 flex gap-2">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          setSelectedTemplate(template);
                          setShowPreview(true);
                        }}
                        className="rounded bg-blue-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                      >
                        Preview
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Actions */}
        <div className="mt-4 flex items-center justify-end gap-2">
          <button
            onClick={onClose}
            className="rounded border border-stroke px-4 py-2 text-sm text-black hover:bg-gray-50 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
          >
            Cancel
          </button>
          <button
            onClick={handleApply}
            disabled={!selectedTemplate}
            className="rounded bg-primary px-4 py-2 text-sm text-white hover:bg-opacity-90 disabled:opacity-50"
          >
            Apply Template
          </button>
        </div>
      </div>
    </div>
  );
}

