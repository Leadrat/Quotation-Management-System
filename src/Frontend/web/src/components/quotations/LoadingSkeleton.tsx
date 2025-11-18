export default function LoadingSkeleton() {
  return (
    <div className="animate-pulse space-y-4">
      <div className="h-4 bg-gray-200 rounded w-3/4"></div>
      <div className="h-4 bg-gray-200 rounded w-1/2"></div>
      <div className="h-4 bg-gray-200 rounded w-5/6"></div>
    </div>
  );
}

export function QuotationListSkeleton() {
  return (
    <div className="space-y-4">
      {[1, 2, 3, 4, 5].map((i) => (
        <div key={i} className="animate-pulse bg-white rounded-lg border p-4">
          <div className="flex justify-between items-center">
            <div className="h-4 bg-gray-200 rounded w-32"></div>
            <div className="h-6 bg-gray-200 rounded w-20"></div>
          </div>
          <div className="mt-2 h-4 bg-gray-200 rounded w-48"></div>
          <div className="mt-2 h-4 bg-gray-200 rounded w-24"></div>
        </div>
      ))}
    </div>
  );
}

export function QuotationFormSkeleton() {
  return (
    <div className="animate-pulse space-y-6">
      <div className="h-8 bg-gray-200 rounded w-64"></div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="h-10 bg-gray-200 rounded"></div>
        <div className="h-10 bg-gray-200 rounded"></div>
        <div className="h-10 bg-gray-200 rounded"></div>
        <div className="h-10 bg-gray-200 rounded"></div>
      </div>
      <div className="h-32 bg-gray-200 rounded"></div>
    </div>
  );
}

