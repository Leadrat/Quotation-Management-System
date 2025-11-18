"use client";

interface HistoryEntryProps {
  entry: {
    historyId: string;
    actionType: string;
    actorDisplayName?: string;
    createdAt: string;
    reason?: string;
    changedFields?: string[];
    beforeSnapshot?: any;
    afterSnapshot?: any;
    suspicionScore?: number;
    metadata?: {
      ipAddress?: string;
      userAgent?: string;
    };
  };
  showClientLink?: boolean;
  clientId?: string;
}

export default function HistoryEntry({ entry, showClientLink = false, clientId }: HistoryEntryProps) {
  function formatDate(date: string) {
    return new Date(date).toLocaleString();
  }

  function formatActionType(type: string) {
    return type.charAt(0) + type.slice(1).toLowerCase();
  }

  function getActionColor(type: string) {
    switch (type) {
      case "CREATED":
        return "bg-green-100 text-green-800";
      case "UPDATED":
        return "bg-blue-100 text-blue-800";
      case "DELETED":
        return "bg-red-100 text-red-800";
      case "RESTORED":
        return "bg-purple-100 text-purple-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  }

  return (
    <div className="bg-white rounded border p-4">
      <div className="flex items-start justify-between mb-2">
        <div className="flex-1">
          <div className="flex items-center space-x-2">
            <span className={`px-2 py-1 rounded text-xs font-medium ${getActionColor(entry.actionType)}`}>
              {formatActionType(entry.actionType)}
            </span>
            {entry.suspicionScore && entry.suspicionScore > 0 && (
              <span className="px-2 py-1 rounded text-xs font-medium bg-yellow-100 text-yellow-800">
                Suspicious (Score: {entry.suspicionScore})
              </span>
            )}
            {showClientLink && clientId && (
              <a
                href={`/clients/${clientId}`}
                className="text-blue-600 hover:underline text-sm"
              >
                View Client
              </a>
            )}
          </div>
          <div className="text-sm text-gray-600 mt-1">
            by {entry.actorDisplayName || "System"} • {formatDate(entry.createdAt)}
          </div>
        </div>
      </div>

      {entry.reason && (
        <div className="text-sm text-gray-700 mt-2">
          <span className="font-medium">Reason:</span> {entry.reason}
        </div>
      )}

      {entry.changedFields && entry.changedFields.length > 0 && (
        <div className="text-sm text-gray-700 mt-2">
          <span className="font-medium">Changed Fields:</span> {entry.changedFields.join(", ")}
        </div>
      )}

      {entry.beforeSnapshot && entry.afterSnapshot && (
        <details className="mt-3">
          <summary className="text-sm text-blue-600 cursor-pointer hover:underline">
            View Before/After
          </summary>
          <div className="mt-2 grid grid-cols-1 md:grid-cols-2 gap-4 text-xs">
            <div>
              <div className="font-medium mb-1">Before:</div>
              <pre className="bg-gray-50 p-2 rounded overflow-auto max-h-40">
                {JSON.stringify(entry.beforeSnapshot, null, 2)}
              </pre>
            </div>
            <div>
              <div className="font-medium mb-1">After:</div>
              <pre className="bg-gray-50 p-2 rounded overflow-auto max-h-40">
                {JSON.stringify(entry.afterSnapshot, null, 2)}
              </pre>
            </div>
          </div>
        </details>
      )}

      {entry.metadata && (entry.metadata.ipAddress || entry.metadata.userAgent) && (
        <div className="text-xs text-gray-500 mt-2">
          {entry.metadata.ipAddress && `IP: ${entry.metadata.ipAddress}`}
          {entry.metadata.ipAddress && entry.metadata.userAgent && " • "}
          {entry.metadata.userAgent && `User Agent: ${entry.metadata.userAgent}`}
        </div>
      )}
    </div>
  );
}

