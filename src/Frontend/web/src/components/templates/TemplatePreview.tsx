"use client";
import { formatCurrency } from "@/utils/quotationFormatter";
import type { QuotationTemplate } from "@/types/templates";

interface TemplatePreviewProps {
  template: QuotationTemplate;
  onClose: () => void;
}

export default function TemplatePreview({ template, onClose }: TemplatePreviewProps) {
  const subtotal = template.lineItems.reduce((sum, item) => sum + item.amount, 0);
  const discountAmount = template.discountDefault
    ? (subtotal * template.discountDefault) / 100
    : 0;
  const afterDiscount = subtotal - discountAmount;
  // Simplified tax calculation (18% GST)
  const taxAmount = afterDiscount * 0.18;
  const total = afterDiscount + taxAmount;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="relative w-full max-w-4xl rounded-lg bg-white p-6 shadow-lg dark:bg-boxdark">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-xl font-bold text-black dark:text-white">Template Preview: {template.name}</h3>
          <button
            onClick={onClose}
            className="rounded bg-gray-500 px-4 py-2 text-white hover:bg-opacity-90"
          >
            Close
          </button>
        </div>

        <div className="max-h-[70vh] overflow-y-auto">
          {/* Template Header */}
          <div className="mb-6 border-b border-stroke pb-4 dark:border-strokedark">
            <h4 className="text-lg font-semibold text-black dark:text-white">{template.name}</h4>
            {template.description && (
              <p className="mt-2 text-gray-600 dark:text-gray-400">{template.description}</p>
            )}
          </div>

          {/* Line Items */}
          <div className="mb-6">
            <table className="w-full table-auto">
              <thead>
                <tr className="bg-gray-2 text-left dark:bg-meta-4">
                  <th className="px-4 py-3 font-medium text-black dark:text-white">#</th>
                  <th className="px-4 py-3 font-medium text-black dark:text-white">Item</th>
                  <th className="px-4 py-3 font-medium text-black dark:text-white">Description</th>
                  <th className="px-4 py-3 text-right font-medium text-black dark:text-white">Qty</th>
                  <th className="px-4 py-3 text-right font-medium text-black dark:text-white">Rate</th>
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

          {/* Totals */}
          <div className="ml-auto w-full max-w-md space-y-2 border-t border-stroke pt-4 dark:border-strokedark">
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
            <div className="mt-6 rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
              <h5 className="mb-2 font-semibold text-black dark:text-white">Notes & Terms</h5>
              <p className="whitespace-pre-wrap text-sm text-gray-700 dark:text-gray-300">{template.notes}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

