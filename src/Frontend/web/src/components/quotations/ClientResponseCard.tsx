import { formatDateTime } from "@/utils/quotationFormatter";

type Props = {
  response: {
    responseType: string;
    clientName?: string;
    clientEmail?: string;
    responseMessage?: string;
    responseDate: string;
    ipAddress?: string;
  };
};

const responseColors: Record<string, string> = {
  ACCEPTED: "text-emerald-600 bg-emerald-50",
  REJECTED: "text-rose-600 bg-rose-50",
  NEEDS_MODIFICATION: "text-amber-600 bg-amber-50",
};

export default function ClientResponseCard({ response }: Props) {
  if (!response) return null;
  const type = response.responseType?.toUpperCase?.() || "RECEIVED";
  const color = responseColors[type] || "text-primary bg-primary/10";

  return (
    <div className="rounded border border-stroke p-4 text-sm dark:border-strokedark">
      <div className={`mb-3 inline-flex rounded-full px-3 py-1 text-xs font-semibold ${color}`}>{type}</div>
      <div className="space-y-1 text-gray-700 dark:text-gray-300">
        {response.clientName && (
          <p>
            <span className="font-medium text-black dark:text-white">Client:</span> {response.clientName}
          </p>
        )}
        {response.clientEmail && (
          <p>
            <span className="font-medium text-black dark:text-white">Email:</span> {response.clientEmail}
          </p>
        )}
        <p>
          <span className="font-medium text-black dark:text-white">Responded:</span>{" "}
          {formatDateTime(response.responseDate)}
        </p>
        {response.ipAddress && (
          <p>
            <span className="font-medium text-black dark:text-white">IP:</span> {response.ipAddress}
          </p>
        )}
      </div>
      {response.responseMessage && (
        <div className="mt-4 rounded-md bg-gray-50 p-3 text-gray-700 dark:bg-meta-4 dark:text-gray-200">
          <p className="text-xs uppercase text-gray-500">Client message</p>
          <p className="whitespace-pre-wrap">{response.responseMessage}</p>
        </div>
      )}
    </div>
  );
}

