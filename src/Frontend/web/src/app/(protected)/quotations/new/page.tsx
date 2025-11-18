"use client";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { QuotationsApi, ClientsApi, TemplatesApi } from "@/lib/api";
import { calculateQuotationTotals } from "@/utils/taxCalculator";
import { formatCurrency } from "@/utils/quotationFormatter";
import { QuotationErrorBoundary } from "@/components/quotations/ErrorBoundary";
import { QuotationFormSkeleton } from "@/components/quotations/LoadingSkeleton";
import { useToast, ToastContainer } from "@/components/quotations/Toast";
import { ApplyTemplateModal } from "@/components/templates";
import { ApprovalSubmissionModal } from "@/components/approvals";
import type { QuotationTemplate } from "@/types/templates";

interface LineItem {
  itemName: string;
  description: string;
  quantity: number;
  unitRate: number;
  amount: number;
}

export default function CreateQuotationPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [clients, setClients] = useState<any[]>([]);
  const [selectedClientId, setSelectedClientId] = useState("");
  const [selectedClient, setSelectedClient] = useState<any>(null);
  const [quotationDate, setQuotationDate] = useState(new Date().toISOString().split("T")[0]);
  const [validUntil, setValidUntil] = useState(() => {
    const date = new Date();
    date.setDate(date.getDate() + 30);
    return date.toISOString().split("T")[0];
  });
  const [discountPercentage, setDiscountPercentage] = useState(0);
  const [notes, setNotes] = useState("");
  const [lineItems, setLineItems] = useState<LineItem[]>([
    { itemName: "", description: "", quantity: 1, unitRate: 0, amount: 0 },
  ]);
  const [showTemplateModal, setShowTemplateModal] = useState(false);
  const [showApprovalModal, setShowApprovalModal] = useState(false);
  const [createdQuotationId, setCreatedQuotationId] = useState<string | null>(null);
  const toast = useToast();

  const requiresApproval = discountPercentage >= 10;
  const approvalThreshold = discountPercentage >= 20 ? 20 : 10;

  useEffect(() => {
    // Load clients for dropdown
    ClientsApi.list(1, 100).then((res) => {
      setClients(res.data || []);
      setInitialLoading(false);
    }).catch((err) => {
      toast.error("Failed to load clients");
      setInitialLoading(false);
    });
  }, []);

  useEffect(() => {
    // Find selected client
    if (selectedClientId) {
      const client = clients.find((c) => c.clientId === selectedClientId);
      setSelectedClient(client);
    } else {
      setSelectedClient(null);
    }
  }, [selectedClientId, clients]);

  const updateLineItem = (index: number, field: keyof LineItem, value: any) => {
    const updated = [...lineItems];
    updated[index] = { ...updated[index], [field]: value };
    if (field === "quantity" || field === "unitRate") {
      updated[index].amount = updated[index].quantity * updated[index].unitRate;
    }
    setLineItems(updated);
  };

  const addLineItem = () => {
    setLineItems([...lineItems, { itemName: "", description: "", quantity: 1, unitRate: 0, amount: 0 }]);
  };

  const removeLineItem = (index: number) => {
    if (lineItems.length > 1) {
      setLineItems(lineItems.filter((_, i) => i !== index));
    }
  };

  const handleApplyTemplate = async (template: QuotationTemplate) => {
    if (!selectedClientId) {
      toast.error("Please select a client first");
      return;
    }

    try {
      const result = await TemplatesApi.apply(template.templateId, selectedClientId);
      const appliedData = result.data;

      // Apply template data to form
      if (appliedData.quotationDate) setQuotationDate(appliedData.quotationDate);
      if (appliedData.validUntil) setValidUntil(appliedData.validUntil);
      setDiscountPercentage(appliedData.discountPercentage || 0);
      if (appliedData.notes) setNotes(appliedData.notes);

      // Apply line items
      const newLineItems: LineItem[] = appliedData.lineItems.map((item) => ({
        itemName: item.itemName,
        description: item.description || "",
        quantity: item.quantity,
        unitRate: item.unitRate,
        amount: item.quantity * item.unitRate,
      }));
      setLineItems(newLineItems);

      toast.success(`Template "${template.name}" applied successfully!`);
      setShowTemplateModal(false);
    } catch (err: any) {
      toast.error(err.message || "Failed to apply template");
    }
  };

  const totals = calculateQuotationTotals(
    lineItems,
    discountPercentage,
    selectedClient?.stateCode,
    "27" // Company state code
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedClientId) {
      setError("Please select a client");
      return;
    }
    if (lineItems.some((item) => !item.itemName || item.quantity <= 0 || item.unitRate <= 0)) {
      setError("Please fill in all line items with valid values");
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const payload = {
        clientId: selectedClientId,
        quotationDate,
        validUntil,
        discountPercentage,
        notes: notes || undefined,
        lineItems: lineItems.map((item) => ({
          itemName: item.itemName,
          description: item.description || undefined,
          quantity: item.quantity,
          unitRate: item.unitRate,
        })),
      };

      const result = await QuotationsApi.create(payload);
      const newQuotationId = result.data.quotationId;
      setCreatedQuotationId(newQuotationId);
      
      // Check if approval is required
      if (requiresApproval) {
        setShowApprovalModal(true);
      } else {
        toast.success("Quotation created successfully!");
        router.push(`/quotations/${newQuotationId}`);
      }
    } catch (err: any) {
      console.error("Quotation creation error:", err);
      let errorMsg = "Failed to create quotation";
      
      if (err.message) {
        errorMsg = err.message;
      } else if (err.error) {
        errorMsg = typeof err.error === 'string' ? err.error : err.error.error || errorMsg;
      }
      
      // Check for detailed error from backend
      if (err.details) {
        errorMsg += `: ${err.details}`;
      } else if (err.errors) {
        // Handle validation errors
        const validationErrors = Object.values(err.errors).flat();
        if (validationErrors.length > 0) {
          errorMsg = validationErrors.join(", ");
        }
      }
      
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Cmd+S or Ctrl+S to save
      if ((e.metaKey || e.ctrlKey) && e.key === "s") {
        e.preventDefault();
        const form = document.querySelector("form");
        if (form) {
          form.requestSubmit();
        }
      }
      // Esc to cancel
      if (e.key === "Escape") {
        router.back();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [router]);

  if (initialLoading) {
    return (
      <QuotationErrorBoundary>
        <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
          <QuotationFormSkeleton />
        </div>
      </QuotationErrorBoundary>
    );
  }

  return (
    <QuotationErrorBoundary>
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <div className="mb-6">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Create Quotation</h4>
      </div>

      {error && (
        <div className="mb-4 rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Client Selection */}
        <div>
          <div className="mb-2.5 flex items-center justify-between">
            <label className="block text-black dark:text-black">Client *</label>
            {selectedClientId && (
              <button
                type="button"
                onClick={() => setShowTemplateModal(true)}
                className="rounded bg-blue-500 px-4 py-2 text-sm text-black border-2 border-blue-500 hover:bg-opacity-90"
              >
                Apply Template
              </button>
            )}
          </div>
          <select
            value={selectedClientId}
            onChange={(e) => setSelectedClientId(e.target.value)}
            required
            className="w-full rounded border-[1.5px] border-stroke bg-white px-5 py-3 font-medium text-black outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-black"
          >
            <option value="" className="text-black border-2 border-black dark:text-black">Select a client</option>
            {clients.map((client) => (
              <option key={client.clientId} value={client.clientId} className="text-black dark:black-white">
                {client.companyName} - {client.email}
              </option>
            ))}
          </select>
          {selectedClient && (
            <div className="mt-2 text-sm text-black border-2 border-black dark:text-black">
              <p>State: {selectedClient.state || "N/A"} ({selectedClient.stateCode || "N/A"})</p>
            </div>
          )}
        </div>

        {/* Dates */}
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <div>
            <label className="mb-2.5 block text-black dark:text-white">Quotation Date *</label>
            <input
              type="date"
              value={quotationDate}
              onChange={(e) => setQuotationDate(e.target.value)}
              required
              className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            />
          </div>
          <div>
            <label className="mb-2.5 block text-black dark:text-white">Valid Until *</label>
            <input
              type="date"
              value={validUntil}
              onChange={(e) => setValidUntil(e.target.value)}
              required
              className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            />
          </div>
        </div>

        {/* Discount */}
        <div>
          <div className="mb-2.5 flex items-center justify-between">
            <label className="block text-black dark:text-white">Discount Percentage</label>
            {requiresApproval && (
              <span className="text-xs text-yellow-600 dark:text-yellow-400">
                ⚠️ Requires {discountPercentage >= 20 ? "Admin" : "Manager"} approval
              </span>
            )}
          </div>
          <input
            type="number"
            min="0"
            max="100"
            step="0.01"
            value={discountPercentage}
            onChange={(e) => setDiscountPercentage(parseFloat(e.target.value) || 0)}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
          {requiresApproval && (
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Discounts ≥ {approvalThreshold}% require approval before the quotation can be sent.
            </p>
          )}
        </div>

        {/* Line Items */}
        <div>
          <div className="mb-4 flex items-center justify-between">
            <label className="block text-black dark:text-white">Line Items *</label>
            <button
              type="button"
              onClick={addLineItem}
              className="rounded bg-primary px-4 py-2 text-sm text-white hover:bg-opacity-90"
            >
              Add Line Item
            </button>
          </div>
          <div className="space-y-4">
            {lineItems.map((item, index) => (
              <div key={index} className="rounded border border-stroke p-4 dark:border-strokedark">
                <div className="mb-4 flex items-center justify-between">
                  <span className="font-medium text-black dark:text-white">Item {index + 1}</span>
                  {lineItems.length > 1 && (
                    <button
                      type="button"
                      onClick={() => removeLineItem(index)}
                      className="rounded bg-red-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                    >
                      Remove
                    </button>
                  )}
                </div>
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  <div>
                    <label className="mb-2.5 block text-sm text-black dark:text-white">Item Name *</label>
                    <input
                      type="text"
                      value={item.itemName}
                      onChange={(e) => updateLineItem(index, "itemName", e.target.value)}
                      required
                      className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
                    />
                  </div>
                  <div>
                    <label className="mb-2.5 block text-sm text-black dark:text-white">Description</label>
                    <input
                      type="text"
                      value={item.description}
                      onChange={(e) => updateLineItem(index, "description", e.target.value)}
                      className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
                    />
                  </div>
                  <div>
                    <label className="mb-2.5 block text-sm text-black dark:text-white">Quantity *</label>
                    <input
                      type="number"
                      min="0.01"
                      step="0.01"
                      value={item.quantity}
                      onChange={(e) => updateLineItem(index, "quantity", parseFloat(e.target.value) || 0)}
                      required
                      className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
                    />
                  </div>
                  <div>
                    <label className="mb-2.5 block text-sm text-black dark:text-white">Unit Rate *</label>
                    <input
                      type="number"
                      min="0.01"
                      step="0.01"
                      value={item.unitRate}
                      onChange={(e) => updateLineItem(index, "unitRate", parseFloat(e.target.value) || 0)}
                      required
                      className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
                    />
                  </div>
                </div>
                <div className="mt-2 text-right text-sm font-medium text-black dark:text-white">
                  Amount: {formatCurrency(item.amount)}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Notes */}
        <div>
          <label className="mb- block text-black dark:text-white">Notes</label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={4}
            className="w-full rounded-lg border-2 border-gray-300 bg-white px-5 py-3 text-base font-normal text-gray-900 placeholder:text-gray-400 outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
            placeholder="Add any additional notes or comments here..."
            style={{ color: '#111827' }}
          />
        </div>

        {/* Totals Summary */}
        <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
          <h5 className="mb-3 font-semibold text-black dark:text-white">Summary</h5>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-gray-600 dark:text-gray-400">Subtotal:</span>
              <span className="font-medium text-black dark:text-white">{formatCurrency(totals.subtotal)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600 dark:text-gray-400">Discount ({discountPercentage}%):</span>
              <span className="font-medium text-black dark:text-white">-{formatCurrency(totals.discountAmount)}</span>
            </div>
            {totals.cgstAmount > 0 && (
              <>
                <div className="flex justify-between">
                  <span className="text-gray-600 dark:text-gray-400">CGST (9%):</span>
                  <span className="font-medium text-black dark:text-white">{formatCurrency(totals.cgstAmount)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600 dark:text-gray-400">SGST (9%):</span>
                  <span className="font-medium text-black dark:text-white">{formatCurrency(totals.sgstAmount)}</span>
                </div>
              </>
            )}
            {totals.igstAmount > 0 && (
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">IGST (18%):</span>
                <span className="font-medium text-black dark:text-white">{formatCurrency(totals.igstAmount)}</span>
              </div>
            )}
            <div className="mt-3 flex justify-between border-t border-stroke pt-2 dark:border-strokedark">
              <span className="font-semibold text-black dark:text-white">Total Amount:</span>
              <span className="text-lg font-bold text-primary">{formatCurrency(totals.totalAmount)}</span>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-4">
          <button
            type="submit"
            disabled={loading}
            className="rounded-lg border-2 border-primary bg-primary px-6 py-3 font-medium text-white shadow-md transition-all hover:bg-opacity-90 hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? "Creating..." : "Create Quotation"}
          </button>
          <button
            type="button"
            onClick={() => router.back()}
            className="rounded-lg border-2 border-gray-300 bg-white px-6 py-3 font-medium text-black shadow-sm transition-all hover:bg-gray-50 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 dark:border-strokedark dark:white dark:text-white dark:hover:bg-meta-4"
          >
            Cancel
          </button>
        </div>
      </form>

      {showTemplateModal && selectedClientId && (
        <ApplyTemplateModal
          clientId={selectedClientId}
          onSelect={handleApplyTemplate}
          onClose={() => setShowTemplateModal(false)}
        />
      )}

      {showApprovalModal && createdQuotationId && (
        <ApprovalSubmissionModal
          isOpen={showApprovalModal}
          onClose={() => {
            setShowApprovalModal(false);
            toast.success("Quotation created successfully!");
            router.push(`/quotations/${createdQuotationId}`);
          }}
          quotationId={createdQuotationId}
          discountPercentage={discountPercentage}
          threshold={approvalThreshold}
          onSuccess={() => {
            setShowApprovalModal(false);
            toast.success("Quotation created and approval requested!");
            router.push(`/quotations/${createdQuotationId}`);
          }}
        />
      )}
      </div>
    </QuotationErrorBoundary>
  );
}

