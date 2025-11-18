"use client";

export function TemplateListSkeleton() {
  return (
    <div className="space-y-4">
      {[1, 2, 3, 4, 5].map((i) => (
        <div key={i} className="animate-pulse rounded border border-stroke bg-white p-4 dark:border-strokedark dark:bg-boxdark">
          <div className="flex items-center justify-between">
            <div className="flex-1">
              <div className="h-4 w-48 bg-gray-300 rounded dark:bg-gray-600 mb-2"></div>
              <div className="h-3 w-64 bg-gray-200 rounded dark:bg-gray-700"></div>
            </div>
            <div className="h-6 w-20 bg-gray-300 rounded dark:bg-gray-600"></div>
          </div>
        </div>
      ))}
    </div>
  );
}

export function TemplateFormSkeleton() {
  return (
    <div className="space-y-6 animate-pulse">
      <div className="h-8 w-64 bg-gray-300 rounded dark:bg-gray-600"></div>
      <div className="space-y-4">
        <div className="h-10 bg-gray-200 rounded dark:bg-gray-700"></div>
        <div className="h-24 bg-gray-200 rounded dark:bg-gray-700"></div>
        <div className="h-10 bg-gray-200 rounded dark:bg-gray-700"></div>
      </div>
      <div className="h-32 bg-gray-200 rounded dark:bg-gray-700"></div>
    </div>
  );
}

