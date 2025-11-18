"use client";

import { RefundTimelineDto, RefundTimelineEventType } from "@/types/refunds";

interface RefundTimelineProps {
  timeline: RefundTimelineDto[];
}

export default function RefundTimeline({ timeline }: RefundTimelineProps) {
  const getEventIcon = (eventType: RefundTimelineEventType) => {
    switch (eventType) {
      case RefundTimelineEventType.REQUESTED:
        return "📝";
      case RefundTimelineEventType.APPROVED:
        return "✅";
      case RefundTimelineEventType.REJECTED:
        return "❌";
      case RefundTimelineEventType.PROCESSING:
        return "⏳";
      case RefundTimelineEventType.COMPLETED:
        return "✓";
      case RefundTimelineEventType.FAILED:
        return "⚠️";
      case RefundTimelineEventType.REVERSED:
        return "↩️";
      default:
        return "•";
    }
  };

  const getEventLabel = (eventType: RefundTimelineEventType) => {
    switch (eventType) {
      case RefundTimelineEventType.REQUESTED:
        return "Requested";
      case RefundTimelineEventType.APPROVED:
        return "Approved";
      case RefundTimelineEventType.REJECTED:
        return "Rejected";
      case RefundTimelineEventType.PROCESSING:
        return "Processing";
      case RefundTimelineEventType.COMPLETED:
        return "Completed";
      case RefundTimelineEventType.FAILED:
        return "Failed";
      case RefundTimelineEventType.REVERSED:
        return "Reversed";
      default:
        return eventType;
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString("en-IN", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="space-y-4">
      {timeline.map((event, index) => (
        <div key={event.timelineId} className="flex items-start space-x-4">
          <div className="flex-shrink-0">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-100">
              <span className="text-sm">{getEventIcon(event.eventType)}</span>
            </div>
            {index < timeline.length - 1 && (
              <div className="mx-auto mt-2 h-8 w-0.5 bg-gray-200" />
            )}
          </div>
          <div className="flex-1 space-y-1">
            <div className="flex items-center justify-between">
              <p className="text-sm font-medium text-gray-900">
                {getEventLabel(event.eventType)}
              </p>
              <p className="text-xs text-gray-500">
                {formatDate(event.eventDate)}
              </p>
            </div>
            <p className="text-sm text-gray-600">
              by {event.actedByUserName}
            </p>
            {event.comments && (
              <p className="text-sm text-gray-500">{event.comments}</p>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}

