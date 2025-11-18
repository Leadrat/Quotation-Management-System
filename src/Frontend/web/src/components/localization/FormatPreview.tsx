"use client";

import { useLocale } from "../../context/LocaleContext";

export function FormatPreview() {
  const { formatCurrency, formatDate, formatNumber, formatDateTime, preferences } = useLocale();

  if (!preferences) {
    return null;
  }

  const sampleAmount = 12345.67;
  const sampleDate = new Date();
  const sampleNumber = 9876.54;

  return (
    <div className="mt-6 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
      <h3 className="text-lg font-semibold mb-4">Format Preview</h3>
      
      <div className="space-y-3">
        <div>
          <label className="text-sm font-medium text-gray-600 dark:text-gray-400">Currency:</label>
          <div className="text-lg font-semibold mt-1">
            {formatCurrency(sampleAmount)}
          </div>
        </div>

        <div>
          <label className="text-sm font-medium text-gray-600 dark:text-gray-400">Date:</label>
          <div className="text-lg font-semibold mt-1">
            {formatDate(sampleDate)}
          </div>
        </div>

        <div>
          <label className="text-sm font-medium text-gray-600 dark:text-gray-400">Number:</label>
          <div className="text-lg font-semibold mt-1">
            {formatNumber(sampleNumber)}
          </div>
        </div>

        <div>
          <label className="text-sm font-medium text-gray-600 dark:text-gray-400">Date & Time:</label>
          <div className="text-lg font-semibold mt-1">
            {formatDateTime(sampleDate)}
          </div>
        </div>
      </div>
    </div>
  );
}

