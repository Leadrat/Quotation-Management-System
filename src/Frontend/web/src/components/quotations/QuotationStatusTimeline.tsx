import { formatDateTime } from "@/utils/quotationFormatter";

type HistoryEntry = {
  historyId: string;
  previousStatus?: string;
  newStatus: string;
  changedAt: string;
  changedByUserName?: string;
  reason?: string;
};

type Props = {
  history: HistoryEntry[];
  loading?: boolean;
};

const statusColors: Record<string, string> = {
  DRAFT: "bg-gray-400",
  SENT: "bg-sky-500",
  VIEWED: "bg-amber-500",
  ACCEPTED: "bg-emerald-500",
  REJECTED: "bg-rose-500",
  EXPIRED: "bg-orange-500",
  CANCELLED: "bg-slate-500",
};

export default function QuotationStatusTimeline({ history, loading }: Props) {
  if (loading) {
    return <div className="py-8 text-center text-sm text-gray-500">Loading status history...</div>;
  }

  if (!history || history.length === 0) {
    return <div className="py-8 text-center text-sm text-gray-500">No status changes recorded yet.</div>;
  }

  return (
    <div className="space-y-6">
      {history.map((entry) => {
        const status = entry.newStatus?.toUpperCase?.() || "";
        return (
          <div key={entry.historyId} className="relative pl-6">
            <span
              className={`absolute left-0 top-1.5 h-3 w-3 rounded-full ${statusColors[status] || "bg-primary"}`}
            />
            <div className="rounded border border-stroke px-4 py-3 text-sm dark:border-strokedark">
              <div className="flex flex-wrap items-center gap-2">
                <span className="font-semibold text-black dark:text-white">{status}</span>
                <span className="text-xs text-gray-500">{formatDateTime(entry.changedAt)}</span>
              </div>
              <p className="text-xs text-gray-500">
                Updated by {entry.changedByUserName || "System"}
                {entry.previousStatus && ` • from ${entry.previousStatus}`}
              </p>
              {entry.reason && <p className="mt-2 text-gray-700 dark:text-gray-300">{entry.reason}</p>}
            </div>
          </div>
        );
      })}
    </div>
  );
}

