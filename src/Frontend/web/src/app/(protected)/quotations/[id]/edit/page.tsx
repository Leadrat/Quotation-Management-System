"use client";
import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { QuotationsApi, ClientsApi, DiscountApprovalsApi } from "@/lib/api";
import { calculateQuotationTotals } from "@/utils/taxCalculator";
import { formatCurrency } from "@/utils/quotationFormatter";
import { ApprovalSubmissionModal, LockedFormOverlay } from "@/components/approvals";
import { DiscountApproval } from "@/types/discount-approvals";

interface LineItem {
  lineItemId?: string;
  itemName: string;
  description: string;
  quantity: number;
  unitRate: number;
  amount: number;
}

export default function EditQuotationPage() {
  const params = useParams();
  const router = useRouter();
  const quotationId = params.id as string;
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [quotation, setQuotation] = useState<any>(null);
  const [quotationDate, setQuotationDate] = useState("");
  const [validUntil, setValidUntil] = useState("");
  const [discountPercentage, setDiscountPercentage] = useState(0);
  const [notes, setNotes] = useState("");
  const [lineItems, setLineItems] = useState<LineItem[]>([]);
  const [pendingApproval, setPendingApproval] = useState<DiscountApproval | null>(null);
  const [showApprovalModal, setShowApprovalModal] = useState(false);
  const [originalDiscount, setOriginalDiscount] = useState(0);

  useEffect(() => {
    if (quotationId) {
      loadQuotation();
    }
  }, [quotationId]);

  const loadQuotation = async () => {
    try {
      setLoadingData(true);
      setError(null);
      const result = await QuotationsApi.get(quotationId);
      const q = result.data;
      setQuotation(q);
      setQuotationDate(q.quotationDate?.split("T")[0] || "");
      setValidUntil(q.validUntil?.split("T")[0] || "");
      setDiscountPercentage(q.discountPercentage || 0);
      setOriginalDiscount(q.discountPercentage || 0);
      setNotes(q.notes || "");
      setLineItems(
        q.lineItems?.map((item: any) => ({
          lineItemId: item.lineItemId,
          itemName: item.itemName,
          description: item.description || "",
          quantity: item.quantity,
          unitRate: item.unitRate,
          amount: item.amount,
        })) || []
      );

      // Check for pending approval
      try {
        const approvalsRes = await DiscountApprovalsApi.getQuotationApprovals(quotationId);
        const approvals = approvalsRes.data || [];
        const pending = approvals.find((a: DiscountApproval) => a.status === "Pending");
        setPendingApproval(pending || null);
      } catch (err) {
        console.warn("Failed to load approvals", err);
      }
    } catch (err: any) {
      setError(err.message || "Failed to load quotation");
    } finally {
      setLoadingData(false);
    }
  };

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

  const totals = calculateQuotationTotals(
    lineItems,
    discountPercentage,
    quotation?.client?.stateCode,
    "27"
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (quotation?.status !== "DRAFT") {
      setError("Only draft quotations can be edited");
      return;
    }
    if (pendingApproval) {
      setError("This quotation is locked for approval and cannot be edited");
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
        quotationDate: quotationDate || undefined,
        validUntil: validUntil || undefined,
        discountPercentage: discountPercentage || undefined,
        notes: notes || undefined,
        lineItems: lineItems.map((item) => ({
          lineItemId: item.lineItemId,
          itemName: item.itemName,
          description: item.description || undefined,
          quantity: item.quantity,
          unitRate: item.unitRate,
        })),
      };

      await QuotationsApi.update(quotationId, payload);
      
      // Check if discount increased and requires approval
      const requiresApproval = discountPercentage >= 10;
      const discountIncreased = discountPercentage > originalDiscount;
      
      if (requiresApproval && discountIncreased) {
        setShowApprovalModal(true);
      } else {
        router.push(`/quotations/${quotationId}`);
      }
    } catch (err: any) {
      setError(err.message || "Failed to update quotation");
    } finally {
      setLoading(false);
    }
  };

  const requiresApproval = discountPercentage >= 10;
  const approvalThreshold = discountPercentage >= 20 ? 20 : 10;

  if (loadingData) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center">Loading...</div>
      </div>
    );
  }

  if (error && !quotation) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center text-red-500">{error}</div>
        <div className="text-center">
          <button onClick={() => router.back()} className="text-primary hover:underline">
            Go Back
          </button>
        </div>
      </div>
    );
  }

  if (quotation?.status !== "DRAFT") {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center text-red-500">Only draft quotations can be edited</div>
        <div className="text-center">
          <button onClick={() => router.push(`/quotations/${quotationId}`)} className="text-primary hover:underline">
            View Quotation
          </button>
        </div>
      </div>
    );
  }

  const isLocked = !!pendingApproval;

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <div className="mb-6">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Edit Quotation {quotation?.quotationNumber}</h4>
      </div>

      {error && (
        <div className="mb-4 rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      )}

      {isLocked && pendingApproval && (
        <div className="mb-6">
          <LockedFormOverlay approval={pendingApproval} />
        </div>
      )}

      <form onSubmit={handleSubmit} className={`space-y-6 ${isLocked ? "pointer-events-none opacity-50" : ""}`}>
        {/* Dates */}
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <div>
            <label className="mb-2.5 block text-black dark:text-white">Quotation Date</label>
            <input
              type="date"
              value={quotationDate}
              onChange={(e) => setQuotationDate(e.target.value)}
              className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            />
          </div>
          <div>
            <label className="mb-2.5 block text-black dark:text-white">Valid Until</label>
            <input
              type="date"
              value={validUntil}
              onChange={(e) => setValidUntil(e.target.value)}
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
            disabled={isLocked}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white disabled:opacity-50"
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
            <label className="block text-black dark:text-white">Line Items</label>
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
          <label className="mb-2.5 block text-black dark:text-white">Notes</label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={4}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
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
            disabled={loading || isLocked}
            className="rounded bg-primary px-6 py-3 font-medium text-white hover:bg-opacity-90 disabled:opacity-50"
          >
            {loading ? "Updating..." : "Update Quotation"}
          </button>
          <button
            type="button"
            onClick={() => router.push(`/quotations/${quotationId}`)}
            className="rounded border border-stroke px-6 py-3 font-medium text-black hover:bg-gray-50 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
          >
            Cancel
          </button>
        </div>
      </form>

      {showApprovalModal && (
        <ApprovalSubmissionModal
          isOpen={showApprovalModal}
          onClose={() => {
            setShowApprovalModal(false);
            router.push(`/quotations/${quotationId}`);
          }}
          quotationId={quotationId}
          discountPercentage={discountPercentage}
          threshold={approvalThreshold}
          onSuccess={() => {
            setShowApprovalModal(false);
            router.push(`/quotations/${quotationId}`);
          }}
        />
      )}
    </div>
  );
}

