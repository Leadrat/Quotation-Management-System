"use client";

interface DateRangePickerProps {
  fromDate: string;
  toDate: string;
  onChange: (fromDate: string, toDate: string) => void;
  presets?: Array<{ label: string; fromDate: string; toDate: string }>;
}

export function DateRangePicker({ fromDate, toDate, onChange, presets }: DateRangePickerProps) {
  const defaultPresets = presets || [
    {
      label: "Today",
      fromDate: new Date().toISOString().split("T")[0],
      toDate: new Date().toISOString().split("T")[0],
    },
    {
      label: "Last 7 Days",
      fromDate: new Date(Date.now() - 6 * 24 * 60 * 60 * 1000).toISOString().split("T")[0],
      toDate: new Date().toISOString().split("T")[0],
    },
    {
      label: "Last 30 Days",
      fromDate: new Date(Date.now() - 29 * 24 * 60 * 60 * 1000).toISOString().split("T")[0],
      toDate: new Date().toISOString().split("T")[0],
    },
    {
      label: "This Month",
      fromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split("T")[0],
      toDate: new Date().toISOString().split("T")[0],
    },
    {
      label: "Last Month",
      fromDate: new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1).toISOString().split("T")[0],
      toDate: new Date(new Date().getFullYear(), new Date().getMonth(), 0).toISOString().split("T")[0],
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex gap-2 flex-wrap">
        {defaultPresets.map((preset) => (
          <button
            key={preset.label}
            onClick={() => onChange(preset.fromDate, preset.toDate)}
            className="px-3 py-1 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600 dark:hover:bg-gray-700"
          >
            {preset.label}
          </button>
        ))}
      </div>
      <div className="flex gap-4">
        <div className="flex-1">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">From Date</label>
          <input
            type="date"
            value={fromDate}
            onChange={(e) => onChange(e.target.value, toDate)}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
          />
        </div>
        <div className="flex-1">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">To Date</label>
          <input
            type="date"
            value={toDate}
            onChange={(e) => onChange(fromDate, e.target.value)}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
          />
        </div>
      </div>
    </div>
  );
}

