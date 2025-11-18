export function ApprovalListSkeleton() {
  return (
    <div className="space-y-4">
      {[1, 2, 3, 4, 5].map((i) => (
        <div key={i} className="animate-pulse rounded-lg border border-stroke bg-white p-4 dark:border-strokedark dark:bg-boxdark">
          <div className="flex justify-between items-center mb-3">
            <div className="h-5 bg-gray-200 rounded w-40 dark:bg-gray-700"></div>
            <div className="h-6 bg-gray-200 rounded w-24 dark:bg-gray-700"></div>
          </div>
          <div className="space-y-2">
            <div className="h-4 bg-gray-200 rounded w-full dark:bg-gray-700"></div>
            <div className="h-4 bg-gray-200 rounded w-3/4 dark:bg-gray-700"></div>
            <div className="h-4 bg-gray-200 rounded w-1/2 dark:bg-gray-700"></div>
          </div>
          <div className="mt-4 flex gap-2">
            <div className="h-8 bg-gray-200 rounded w-20 dark:bg-gray-700"></div>
            <div className="h-8 bg-gray-200 rounded w-20 dark:bg-gray-700"></div>
          </div>
        </div>
      ))}
    </div>
  );
}

export function ApprovalDetailSkeleton() {
  return (
    <div className="animate-pulse space-y-6">
      <div className="h-8 bg-gray-200 rounded w-64 dark:bg-gray-700"></div>
      <div className="rounded-lg border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
        <div className="space-y-4">
          <div className="h-5 bg-gray-200 rounded w-32 dark:bg-gray-700"></div>
          <div className="h-4 bg-gray-200 rounded w-full dark:bg-gray-700"></div>
          <div className="h-4 bg-gray-200 rounded w-3/4 dark:bg-gray-700"></div>
        </div>
      </div>
      <div className="rounded-lg border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="flex items-center gap-3">
              <div className="h-10 w-10 bg-gray-200 rounded-full dark:bg-gray-700"></div>
              <div className="flex-1 space-y-2">
                <div className="h-4 bg-gray-200 rounded w-1/2 dark:bg-gray-700"></div>
                <div className="h-3 bg-gray-200 rounded w-1/3 dark:bg-gray-700"></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

export function ApprovalMetricsSkeleton() {
  return (
    <div className="animate-pulse space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="rounded-lg border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
            <div className="h-4 bg-gray-200 rounded w-24 mb-2 dark:bg-gray-700"></div>
            <div className="h-8 bg-gray-200 rounded w-16 dark:bg-gray-700"></div>
          </div>
        ))}
      </div>
      <div className="rounded-lg border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
        <div className="h-6 bg-gray-200 rounded w-48 mb-4 dark:bg-gray-700"></div>
        <div className="h-64 bg-gray-200 rounded dark:bg-gray-700"></div>
      </div>
    </div>
  );
}

