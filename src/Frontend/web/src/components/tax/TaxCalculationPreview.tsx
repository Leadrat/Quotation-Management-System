"use client";
import { useState, useEffect } from "react";
import { TaxCalculationApi } from "@/lib/api";
import { formatCurrency } from "@/utils/quotationFormatter";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface TaxCalculationPreviewProps {
  clientId: string;
  lineItems: Array<{
    lineItemId: string;
    productServiceCategoryId?: string;
    amount: number;
  }>;
  subtotal: number;
  discountAmount: number;
  calculationDate?: string;
  countryId?: string;
}

export default function TaxCalculationPreview({
  clientId,
  lineItems,
  subtotal,
  discountAmount,
  calculationDate,
  countryId,
}: TaxCalculationPreviewProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [taxResult, setTaxResult] = useState<any>(null);

  useEffect(() => {
    if (clientId && lineItems.length > 0 && subtotal > 0) {
      calculateTax();
    } else {
      setTaxResult(null);
    }
  }, [clientId, lineItems, subtotal, discountAmount, calculationDate, countryId]);

  async function calculateTax() {
    if (!clientId || lineItems.length === 0) return;

    setLoading(true);
    setError(null);

    try {
      const result = await TaxCalculationApi.preview({
        clientId,
        lineItems,
        subtotal,
        discountAmount,
        calculationDate: calculationDate || new Date().toISOString(),
        countryId: countryId || undefined,
      });
      setTaxResult(result.data);
    } catch (e: any) {
      setError(e.message || "Failed to calculate tax");
      setTaxResult(null);
    } finally {
      setLoading(false);
    }
  }

  if (!clientId || lineItems.length === 0) {
    return (
      <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
        <h5 className="mb-3 font-semibold text-black dark:text-white">Tax Calculation</h5>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          Select a client and add line items to see tax calculation
        </p>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
        <h5 className="mb-3 font-semibold text-black dark:text-white">Tax Calculation</h5>
        <div className="text-center py-4">
          <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-gray-900"></div>
          <p className="mt-2 text-sm text-gray-500">Calculating tax...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded border border-red-200 bg-red-50 p-4 dark:border-red-800 dark:bg-red-900/20">
        <h5 className="mb-2 font-semibold text-red-800 dark:text-red-400">Tax Calculation Error</h5>
        <p className="text-sm text-red-700 dark:text-red-300">{error}</p>
        <p className="text-xs text-red-600 dark:text-red-400 mt-2">
          Tax calculation will use default rates
        </p>
      </div>
    );
  }

  if (!taxResult) {
    return (
      <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
        <h5 className="mb-3 font-semibold text-black dark:text-white">Tax Calculation</h5>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          No tax calculation available
        </p>
      </div>
    );
  }

  return (
    <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-meta-4">
      <div className="mb-3 flex items-center justify-between">
        <h5 className="font-semibold text-black dark:text-white">Tax Calculation</h5>
        {taxResult.frameworkName && (
          <Badge className="bg-blue-100 text-blue-800 text-xs">
            {taxResult.frameworkName}
          </Badge>
        )}
      </div>

      {taxResult.countryName && (
        <div className="mb-2 text-xs text-gray-600 dark:text-gray-400">
          <span className="font-medium">Country:</span> {taxResult.countryName}
          {taxResult.jurisdictionName && (
            <> | <span className="font-medium">Jurisdiction:</span> {taxResult.jurisdictionName}</>
          )}
        </div>
      )}

      <div className="space-y-2 text-sm">
        <div className="flex justify-between">
          <span className="text-gray-600 dark:text-gray-400">Subtotal:</span>
          <span className="font-medium text-black dark:text-white">{formatCurrency(taxResult.subtotal)}</span>
        </div>
        {taxResult.discountAmount > 0 && (
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">Discount:</span>
            <span className="font-medium text-black dark:text-white">-{formatCurrency(taxResult.discountAmount)}</span>
          </div>
        )}
        <div className="flex justify-between">
          <span className="text-gray-600 dark:text-gray-400">Taxable Amount:</span>
          <span className="font-medium text-black dark:text-white">{formatCurrency(taxResult.taxableAmount)}</span>
        </div>

        {/* Tax Breakdown by Component */}
        {taxResult.taxBreakdown && taxResult.taxBreakdown.length > 0 && (
          <div className="mt-3 pt-2 border-t border-stroke dark:border-strokedark">
            <div className="mb-2 text-xs font-medium text-gray-700 dark:text-gray-300">Tax Breakdown:</div>
            {taxResult.taxBreakdown.map((component: any, idx: number) => (
              <div key={idx} className="flex justify-between text-xs">
                <span className="text-gray-600 dark:text-gray-400">
                  {component.component} ({component.rate}%):
                </span>
                <span className="font-medium text-black dark:text-white">
                  {formatCurrency(component.amount)}
                </span>
              </div>
            ))}
          </div>
        )}

        {/* Fallback to legacy format if no breakdown */}
        {(!taxResult.taxBreakdown || taxResult.taxBreakdown.length === 0) && taxResult.totalTax > 0 && (
          <div className="mt-3 pt-2 border-t border-stroke dark:border-strokedark">
            <div className="flex justify-between">
              <span className="text-gray-600 dark:text-gray-400">Tax:</span>
              <span className="font-medium text-black dark:text-white">{formatCurrency(taxResult.totalTax)}</span>
            </div>
          </div>
        )}

        <div className="mt-3 flex justify-between border-t border-stroke pt-2 dark:border-strokedark">
          <span className="font-semibold text-black dark:text-white">Total Amount:</span>
          <span className="text-lg font-bold text-primary">{formatCurrency(taxResult.totalAmount)}</span>
        </div>
      </div>

      {/* Line Item Breakdown (Collapsible) */}
      {taxResult.lineItemBreakdown && taxResult.lineItemBreakdown.length > 0 && (
        <details className="mt-4 pt-3 border-t border-stroke dark:border-strokedark">
          <summary className="cursor-pointer text-sm font-medium text-gray-700 dark:text-gray-300 hover:text-primary">
            View Line Item Breakdown
          </summary>
          <div className="mt-2 space-y-2">
            {taxResult.lineItemBreakdown.map((item: any, idx: number) => (
              <div key={idx} className="rounded bg-white dark:bg-gray-800 p-2 text-xs">
                <div className="flex justify-between mb-1">
                  <span className="font-medium">
                    {item.categoryName || "Uncategorized"}
                  </span>
                  <span>{formatCurrency(item.amount)}</span>
                </div>
                {item.componentBreakdown && item.componentBreakdown.length > 0 && (
                  <div className="ml-2 space-y-1 text-gray-600 dark:text-gray-400">
                    {item.componentBreakdown.map((comp: any, compIdx: number) => (
                      <div key={compIdx} className="flex justify-between">
                        <span>{comp.component}:</span>
                        <span>{formatCurrency(comp.amount)}</span>
                      </div>
                    ))}
                  </div>
                )}
                <div className="mt-1 pt-1 border-t border-gray-200 dark:border-gray-700 flex justify-between">
                  <span className="font-medium">Tax:</span>
                  <span className="font-medium">{formatCurrency(item.taxAmount)}</span>
                </div>
              </div>
            ))}
          </div>
        </details>
      )}
    </div>
  );
}

