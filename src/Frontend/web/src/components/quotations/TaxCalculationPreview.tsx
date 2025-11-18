import { formatCurrency } from "@/utils/quotationFormatter";
import { QuotationTotals } from "@/utils/taxCalculator";

interface TaxCalculationPreviewProps {
  totals: QuotationTotals;
}

export default function TaxCalculationPreview({ totals }: TaxCalculationPreviewProps) {
  return (
    <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
      <h5 className="mb-3 font-semibold text-black dark:text-white">Tax Calculation</h5>
      <div className="space-y-2 text-sm">
        <div className="flex justify-between">
          <span className="text-gray-600 dark:text-gray-400">Subtotal:</span>
          <span className="font-medium text-black dark:text-white">{formatCurrency(totals.subtotal)}</span>
        </div>
        {totals.discountAmount > 0 && (
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">Discount ({totals.discountPercentage}%):</span>
            <span className="font-medium text-black dark:text-white">-{formatCurrency(totals.discountAmount)}</span>
          </div>
        )}
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
            <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
              Intra-state (CGST + SGST)
            </div>
          </>
        )}
        {totals.igstAmount > 0 && (
          <>
            <div className="flex justify-between">
              <span className="text-gray-600 dark:text-gray-400">IGST (18%):</span>
              <span className="font-medium text-black dark:text-white">{formatCurrency(totals.igstAmount)}</span>
            </div>
            <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
              Inter-state (IGST)
            </div>
          </>
        )}
        <div className="mt-3 flex justify-between border-t border-stroke pt-2 dark:border-strokedark">
          <span className="font-semibold text-black dark:text-white">Total Amount:</span>
          <span className="text-lg font-bold text-primary">{formatCurrency(totals.totalAmount)}</span>
        </div>
      </div>
    </div>
  );
}

