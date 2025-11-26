"use client";

interface QuotationPaymentSectionProps {
  quotationId: string;
  totalAmount: number;
  currency?: string;
}

export function QuotationPaymentSection({
  quotationId,
  totalAmount,
  currency = "INR",
}: QuotationPaymentSectionProps) {
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
      <div className="flex flex-col items-center justify-center py-8">
        <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100 dark:bg-gray-700">
          <svg
            className="h-8 w-8 text-gray-400 dark:text-gray-500"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        </div>
        <h3 className="text-lg font-semibold text-gray-800 dark:text-white/90 mb-2">
          Payment Coming Soon
        </h3>
        <p className="text-sm text-center text-gray-500 dark:text-gray-400 max-w-sm">
          Payment processing will be available soon. For now, please contact us directly to complete your payment.
        </p>
      </div>
    </div>
  );
}

