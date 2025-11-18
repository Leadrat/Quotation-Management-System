"use client";
import { ApprovalTimeline as ApprovalTimelineType } from "@/types/discount-approvals";
import { formatDateTime } from "@/utils/quotationFormatter";

interface ApprovalTimelineProps {
  timeline: ApprovalTimelineType[];
  className?: string;
}

export function ApprovalTimeline({ timeline, className = "" }: ApprovalTimelineProps) {
  if (timeline.length === 0) {
    return (
      <div className={`text-center text-gray-500 dark:text-gray-400 ${className}`}>
        No approval history available.
      </div>
    );
  }

  const getEventIcon = (eventType: string) => {
    switch (eventType) {
      case "Requested":
        return (
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-100 dark:bg-blue-900">
            <svg className="h-4 w-4 text-blue-600 dark:text-blue-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
          </div>
        );
      case "Approved":
        return (
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-green-100 dark:bg-green-900">
            <svg className="h-4 w-4 text-green-600 dark:text-green-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          </div>
        );
      case "Rejected":
        return (
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-red-100 dark:bg-red-900">
            <svg className="h-4 w-4 text-red-600 dark:text-red-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </div>
        );
      case "Escalated":
        return (
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-orange-100 dark:bg-orange-900">
            <svg className="h-4 w-4 text-orange-600 dark:text-orange-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
            </svg>
          </div>
        );
      case "Resubmitted":
        return (
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-purple-100 dark:bg-purple-900">
            <svg className="h-4 w-4 text-purple-600 dark:text-purple-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          </div>
        );
      default:
        return (
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-100 dark:bg-gray-800">
            <div className="h-2 w-2 rounded-full bg-gray-400"></div>
          </div>
        );
    }
  };

  return (
    <div className={`space-y-4 ${className}`}>
      {timeline.map((event, index) => (
        <div key={`${event.approvalId}-${event.eventType}-${index}`} className="relative flex gap-4">
          {/* Timeline line */}
          {index < timeline.length - 1 && (
            <div className="absolute left-4 top-8 h-full w-0.5 bg-gray-200 dark:bg-gray-700"></div>
          )}

          {/* Icon */}
          <div className="relative z-10">{getEventIcon(event.eventType)}</div>

          {/* Content */}
          <div className="flex-1 pb-4">
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <h5 className="text-sm font-semibold text-gray-900 dark:text-white">
                  {event.eventType}
                </h5>
                <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                  <span className="font-medium">{event.userName}</span> ({event.userRole})
                </p>
                <p className="mt-1 text-sm text-gray-500 dark:text-gray-500">
                  {formatDateTime(event.timestamp)}
                </p>
              </div>
              <span className="text-xs text-gray-500 dark:text-gray-400">
                {event.status}
              </span>
            </div>

            {event.reason && (
              <div className="mt-2 rounded-md bg-gray-50 p-3 dark:bg-gray-800">
                <p className="text-sm font-medium text-gray-900 dark:text-white">Reason:</p>
                <p className="mt-1 text-sm text-gray-700 dark:text-gray-300">{event.reason}</p>
              </div>
            )}

            {event.comments && (
              <details className="mt-2">
                <summary className="cursor-pointer text-sm font-medium text-gray-700 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white">
                  View Comments
                </summary>
                <div className="mt-2 rounded-md bg-gray-50 p-3 dark:bg-gray-800">
                  <p className="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
                    {event.comments}
                  </p>
                </div>
              </details>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}

