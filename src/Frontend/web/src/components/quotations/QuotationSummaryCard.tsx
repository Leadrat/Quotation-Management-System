import { formatCurrency } from "@/utils/quotationFormatter";

interface QuotationSummaryCardProps {
  subtotal: number;
  discountAmount: number;
  discountPercentage: number;
  taxAmount: number;
  cgstAmount?: number;
  sgstAmount?: number;
  igstAmount?: number;
  totalAmount: number;
}

export default function QuotationSummaryCard({
  subtotal,
  discountAmount,
  discountPercentage,
  taxAmount,
  cgstAmount = 0,
  sgstAmount = 0,
  igstAmount = 0,
  totalAmount,
}: QuotationSummaryCardProps) {
  return (
    <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
      <h5 className="mb-3 font-semibold text-black dark:text-white">Summary</h5>
      <div className="space-y-2 text-sm">
        <div className="flex justify-between">
          <span className="text-gray-600 dark:text-gray-400">Subtotal:</span>
          <span className="font-medium text-black dark:text-white">{formatCurrency(subtotal)}</span>
        </div>
        {discountAmount > 0 && (
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">Discount ({discountPercentage}%):</span>
            <span className="font-medium text-black dark:text-white">-{formatCurrency(discountAmount)}</span>
          </div>
        )}
        {cgstAmount > 0 && (
          <>
            <div className="flex justify-between">
              <span className="text-gray-600 dark:text-gray-400">CGST (9%):</span>
              <span className="font-medium text-black dark:text-white">{formatCurrency(cgstAmount)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600 dark:text-gray-400">SGST (9%):</span>
              <span className="font-medium text-black dark:text-white">{formatCurrency(sgstAmount)}</span>
            </div>
          </>
        )}
        {igstAmount > 0 && (
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">IGST (18%):</span>
            <span className="font-medium text-black dark:text-white">{formatCurrency(igstAmount)}</span>
          </div>
        )}
        <div className="mt-3 flex justify-between border-t border-stroke pt-2 dark:border-strokedark">
          <span className="font-semibold text-black dark:text-white">Total Amount:</span>
          <span className="text-lg font-bold text-primary">{formatCurrency(totalAmount)}</span>
        </div>
      </div>
    </div>
  );
}

