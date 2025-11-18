"use client";

import type { QuotationTrendData } from "@/types/reports";

interface QuotationTrendChartProps {
  data: QuotationTrendData[];
}

export function QuotationTrendChart({ data }: QuotationTrendChartProps) {
  if (!data || data.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Quotation Trend</h3>
        <p className="text-gray-500 text-center py-8">No data available</p>
      </div>
    );
  }

  const maxValue = Math.max(...data.flatMap(d => [d.created, d.sent]));

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">Quotation Trend (Last 30 Days)</h3>
      <div className="h-64 flex items-end justify-between gap-2">
        {data.map((item, index) => {
          const createdHeight = (item.created / maxValue) * 100;
          const sentHeight = (item.sent / maxValue) * 100;
          const date = new Date(item.date);
          return (
            <div key={index} className="flex-1 flex flex-col items-center gap-1">
              <div className="w-full flex gap-0.5 justify-center items-end h-full">
                <div
                  className="bg-blue-500 rounded-t w-full"
                  style={{ height: `${createdHeight}%` }}
                  title={`Created: ${item.created}`}
                />
                <div
                  className="bg-green-500 rounded-t w-full"
                  style={{ height: `${sentHeight}%` }}
                  title={`Sent: ${item.sent}`}
                />
              </div>
              <span className="text-xs text-gray-500 mt-2 transform -rotate-45 origin-top-left">
                {date.getDate()}/{date.getMonth() + 1}
              </span>
            </div>
          );
        })}
      </div>
      <div className="flex gap-4 mt-4 justify-center">
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 bg-blue-500 rounded"></div>
          <span className="text-sm text-gray-600">Created</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 bg-green-500 rounded"></div>
          <span className="text-sm text-gray-600">Sent</span>
        </div>
      </div>
    </div>
  );
}

