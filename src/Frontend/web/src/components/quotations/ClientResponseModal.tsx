import { useEffect, useState } from "react";
import { validateEmail } from "@/utils/email";

type ResponseType = "ACCEPTED" | "REJECTED" | "NEEDS_MODIFICATION";

type Props = {
  open: boolean;
  onClose: () => void;
  defaultType?: ResponseType;
  submitting?: boolean;
  onSubmit: (payload: { responseType: ResponseType; clientName?: string; clientEmail?: string; responseMessage?: string }) => Promise<void> | void;
};

export default function ClientResponseModal({ open, onClose, defaultType = "ACCEPTED", onSubmit }: Props) {
  const [responseType, setResponseType] = useState<ResponseType>(defaultType);
  const [clientName, setClientName] = useState("");
  const [clientEmail, setClientEmail] = useState("");
  const [responseMessage, setResponseMessage] = useState("");
  const [acceptTerms, setAcceptTerms] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (open) {
      setResponseType(defaultType);
      setClientName("");
      setClientEmail("");
      setResponseMessage("");
      setAcceptTerms(false);
      setErrors({});
    }
  }, [open, defaultType]);

  if (!open) return null;

  const validate = () => {
    const nextErrors: Record<string, string> = {};
    if (!clientName.trim()) nextErrors.clientName = "Your name is required.";
    if (!clientEmail || !validateEmail(clientEmail)) nextErrors.clientEmail = "A valid email is required.";
    if (!acceptTerms) nextErrors.acceptTerms = "Please confirm that the information is correct.";
    if (responseMessage.length > 2000) nextErrors.responseMessage = "Message must be under 2000 characters.";
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;
    try {
      setSubmitting(true);
      await onSubmit({
        responseType,
        clientName: clientName.trim(),
        clientEmail: clientEmail.trim(),
        responseMessage: responseMessage.trim() || undefined,
      });
    } finally {
      setSubmitting(false);
    }
  };

  const buttons: Array<{ label: string; value: ResponseType; color: string }> = [
    { label: "Accept Quotation", value: "ACCEPTED", color: "bg-emerald-500" },
    { label: "Request Modification", value: "NEEDS_MODIFICATION", color: "bg-amber-500" },
    { label: "Decline", value: "REJECTED", color: "bg-rose-500" },
  ];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 px-4 py-6">
      <div className="w-full max-w-lg rounded-lg bg-white shadow-lg dark:bg-boxdark">
        <div className="flex items-center justify-between border-b border-stroke px-6 py-4 dark:border-strokedark">
          <h3 className="text-lg font-semibold text-black dark:text-white">Please confirm your decision</h3>
          <button onClick={onClose} className="text-gray-500 hover:text-black dark:text-gray-400">
            ✕
          </button>
        </div>
        <div className="px-6 py-4">
          <div className="mb-4 flex flex-wrap gap-2">
            {buttons.map((btn) => (
              <button
                key={btn.value}
                onClick={() => setResponseType(btn.value)}
                className={`rounded px-3 py-2 text-xs font-semibold text-white ${btn.color} ${
                  responseType === btn.value ? "opacity-100" : "opacity-70"
                }`}
              >
                {btn.label}
              </button>
            ))}
          </div>

          <div className="space-y-4 text-sm">
            <div>
              <label className="mb-1 block font-medium text-black dark:text-white">Your Name</label>
              <input
                className="w-full rounded border border-stroke px-3 py-2 text-black focus:border-primary focus:outline-none dark:border-strokedark dark:bg-boxdark dark:text-white"
                value={clientName}
                onChange={(e) => setClientName(e.target.value)}
              />
              {errors.clientName && <p className="mt-1 text-xs text-meta-1">{errors.clientName}</p>}
            </div>
            <div>
              <label className="mb-1 block font-medium text-black dark:text-white">Email</label>
              <input
                className="w-full rounded border border-stroke px-3 py-2 text-black focus:border-primary focus:outline-none dark:border-strokedark dark:bg-boxdark dark:text-white"
                value={clientEmail}
                onChange={(e) => setClientEmail(e.target.value)}
                placeholder="you@example.com"
              />
              {errors.clientEmail && <p className="mt-1 text-xs text-meta-1">{errors.clientEmail}</p>}
            </div>
            <div>
              <label className="mb-1 block font-medium text-black dark:text-white">Comments</label>
              <textarea
                className="h-28 w-full rounded border border-stroke px-3 py-2 text-black focus:border-primary focus:outline-none dark:border-strokedark dark:bg-boxdark dark:text-white"
                value={responseMessage}
                onChange={(e) => setResponseMessage(e.target.value)}
                placeholder="Add optional notes, questions, or feedback here."
              />
              <p className="mt-1 text-xs text-gray-500">{responseMessage.length}/2000</p>
              {errors.responseMessage && <p className="text-xs text-meta-1">{errors.responseMessage}</p>}
            </div>
            <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-300">
              <input
                type="checkbox"
                checked={acceptTerms}
                onChange={(e) => setAcceptTerms(e.target.checked)}
                className="h-4 w-4"
              />
              I confirm the information above is correct.
            </label>
            {errors.acceptTerms && <p className="text-xs text-meta-1">{errors.acceptTerms}</p>}
          </div>
        </div>
        <div className="flex items-center justify-between border-t border-stroke px-6 py-4 dark:border-strokedark">
          <button
            onClick={onClose}
            className="rounded border border-stroke px-4 py-2 text-sm text-black hover:bg-gray-50 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
            disabled={submitting}
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            className="rounded bg-primary px-6 py-2 text-sm font-semibold text-white hover:bg-opacity-90 disabled:opacity-50"
            disabled={submitting}
          >
            {submitting ? "Submitting..." : "Submit Response"}
          </button>
        </div>
      </div>
    </div>
  );
}

