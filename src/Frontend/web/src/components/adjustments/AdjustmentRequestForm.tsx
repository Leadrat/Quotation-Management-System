"use client";

import { useState } from "react";
import { CreateAdjustmentRequest, AdjustmentType } from "@/types/refunds";
import { AdjustmentsApi } from "@/lib/api";

interface AdjustmentRequestFormProps {
  quotationId: string;
  onSuccess: () => void;
  onCancel: () => void;
}

export default function AdjustmentRequestForm({
  quotationId,
  onSuccess,
  onCancel,
}: AdjustmentRequestFormProps) {
  const [formData, setFormData] = useState<CreateAdjustmentRequest>({
    quotationId,
    adjustmentType: AdjustmentType.AMOUNT_CORRECTION,
    originalAmount: 0,
    adjustedAmount: 0,
    reason: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await AdjustmentsApi.create(formData);
      onSuccess();
    } catch (err: any) {
      setError(err.message || "Failed to create adjustment request");
    } finally {
      setLoading(false);
    }
  };

  const difference = formData.adjustedAmount - formData.originalAmount;
  const hasDifference = difference !== 0;

  const getAdjustmentTypeLabel = (type: string) => {
    return type
      .split("_")
      .map((word) => word.charAt(0) + word.slice(1).toLowerCase())
      .join(" ");
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Error Message */}
      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4">
          <div className="flex items-start">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm font-medium text-red-800">{error}</p>
            </div>
          </div>
        </div>
      )}

      {/* Adjustment Type */}
      <div>
        <label className="mb-2 block text-sm font-semibold text-gray-700">
          Adjustment Type <span className="text-red-500">*</span>
        </label>
        <div className="relative">
          <select
            value={formData.adjustmentType}
            onChange={(e) =>
              setFormData({
                ...formData,
                adjustmentType: e.target.value as AdjustmentType,
              })
            }
            className="w-full appearance-none rounded-lg border border-gray-300 bg-white px-4 py-3 pr-10 text-sm font-medium text-gray-900 shadow-sm transition-all focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
            required
          >
            <option value={AdjustmentType.DISCOUNT_CHANGE}>Discount Change</option>
            <option value={AdjustmentType.AMOUNT_CORRECTION}>Amount Correction</option>
            <option value={AdjustmentType.TAX_CORRECTION}>Tax Correction</option>
          </select>
          <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3">
            <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </div>
        </div>
      </div>

      {/* Amount Fields Grid */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {/* Original Amount */}
        <div>
          <label className="mb-2 block text-sm font-semibold text-gray-700">
            Original Amount <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm font-medium text-gray-500">₹</span>
            <input
              type="number"
              step="0.01"
              min="0"
              value={formData.originalAmount || ""}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  originalAmount: parseFloat(e.target.value) || 0,
                })
              }
              className="w-full rounded-lg border border-gray-300 bg-white px-4 py-3 pl-8 text-sm font-medium text-gray-900 shadow-sm transition-all focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
              placeholder="0.00"
              required
            />
          </div>
        </div>

        {/* Adjusted Amount */}
        <div>
          <label className="mb-2 block text-sm font-semibold text-gray-700">
            Adjusted Amount <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm font-medium text-gray-500">₹</span>
            <input
              type="number"
              step="0.01"
              min="0.01"
              value={formData.adjustedAmount || ""}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  adjustedAmount: parseFloat(e.target.value) || 0,
                })
              }
              className="w-full rounded-lg border border-gray-300 bg-white px-4 py-3 pl-8 text-sm font-medium text-gray-900 shadow-sm transition-all focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
              placeholder="0.00"
              required
            />
          </div>
        </div>
      </div>

      {/* Difference Display */}
      {hasDifference && (
        <div className={`rounded-lg border p-4 ${
          difference > 0 
            ? "border-green-200 bg-green-50" 
            : "border-orange-200 bg-orange-50"
        }`}>
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-gray-700">Adjustment Difference:</span>
            <span className={`text-lg font-bold ${
              difference > 0 
                ? "text-green-700" 
                : "text-orange-700"
            }`}>
              {difference > 0 ? "+" : ""}₹{Math.abs(difference).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </span>
          </div>
        </div>
      )}

      {/* Reason */}
      <div>
        <label className="mb-2 block text-sm font-semibold text-gray-700">
          Reason for Adjustment <span className="text-red-500">*</span>
        </label>
        <textarea
          value={formData.reason}
          onChange={(e) =>
            setFormData({ ...formData, reason: e.target.value })
          }
          className="w-full rounded-lg border border-gray-300 bg-white px-4 py-3 text-sm text-gray-900 shadow-sm transition-all focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
          rows={4}
          placeholder="Please provide a detailed reason for this adjustment..."
          required
          maxLength={500}
        />
        <p className="mt-1 text-xs text-gray-500">
          {formData.reason.length}/500 characters
        </p>
      </div>

      {/* Action Buttons */}
      <div className="flex gap-3 pt-2">
        <button
          type="submit"
          disabled={loading}
          className="flex-1 rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-3 text-sm font-semibold text-white shadow-md transition-all hover:from-blue-700 hover:to-blue-800 hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:from-blue-600 disabled:hover:to-blue-700"
        >
          {loading ? (
            <span className="flex items-center justify-center">
              <svg className="mr-2 h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Submitting...
            </span>
          ) : (
            "Submit Adjustment Request"
          )}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="rounded-lg border-2 border-gray-300 bg-white px-6 py-3 text-sm font-semibold text-gray-700 shadow-sm transition-all hover:bg-gray-50 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2"
        >
          Cancel
        </button>
      </div>
    </form>
  );
}

