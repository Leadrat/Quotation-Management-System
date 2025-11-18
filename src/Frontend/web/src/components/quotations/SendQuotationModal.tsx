import { useEffect, useMemo, useState } from "react";
import { QuotationsApi } from "@/lib/api";
import { validateEmail } from "@/utils/email";

type Props = {
  quotationId: string;
  open: boolean;
  onClose: () => void;
  defaultRecipient?: string;
  mode?: "send" | "resend";
  onSuccess?: () => void;
};

export function SendQuotationModal({
  quotationId,
  open,
  onClose,
  defaultRecipient,
  onSuccess,
  mode = "send",
}: Props) {
  const [recipientEmail, setRecipientEmail] = useState(defaultRecipient || "");
  const [ccEmails, setCcEmails] = useState<string>("");
  const [bccEmails, setBccEmails] = useState<string>("");
  const [customMessage, setCustomMessage] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setRecipientEmail(defaultRecipient || "");
      setCcEmails("");
      setBccEmails("");
      setCustomMessage("");
      setErrors({});
      setSuccessMessage(null);
    }
  }, [open, defaultRecipient]);

  const emailLists = useMemo(() => {
    const parse = (value: string) =>
      value
        .split(/[,\s;]/)
        .map((email) => email.trim())
        .filter(Boolean);
    return {
      cc: parse(ccEmails),
      bcc: parse(bccEmails),
    };
  }, [ccEmails, bccEmails]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    if (!recipientEmail || !validateEmail(recipientEmail)) {
      newErrors.recipientEmail = "A valid recipient email is required.";
    }
    [...emailLists.cc, ...emailLists.bcc].forEach((email) => {
      if (!validateEmail(email)) {
        newErrors.list = "Ensure all CC/BCC emails are valid.";
      }
    });
    if (customMessage.length > 2000) {
      newErrors.customMessage = "Message must be under 2000 characters.";
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;
    try {
      setSubmitting(true);
      setErrors({});
      const payload = {
        recipientEmail,
        ccEmails: emailLists.cc,
        bccEmails: emailLists.bcc,
        customMessage: customMessage.trim() || undefined,
      };

      if (mode === "resend") {
        await QuotationsApi.resend(quotationId, payload);
        setSuccessMessage("Quotation resent successfully.");
      } else {
        await QuotationsApi.send(quotationId, payload);
        setSuccessMessage("Quotation sent successfully.");
      }
      onSuccess?.();
    } catch (err: any) {
      setErrors({ submit: err.message || "Failed to send quotation." });
    } finally {
      setSubmitting(false);
    }
  };

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 px-4 py-8">
      <div className="w-full max-w-2xl rounded-lg bg-white shadow-lg dark:bg-boxdark">
        <div className="flex items-center justify-between border-b border-stroke px-6 py-4 dark:border-strokedark">
          <h3 className="text-lg font-semibold text-black dark:text-white">
            {mode === "resend" ? "Resend Quotation" : "Send Quotation"}
          </h3>
          <button onClick={onClose} className="text-gray-500 hover:text-black dark:text-gray-400">
            ✕
          </button>
        </div>

        <div className="max-h-[70vh] overflow-y-auto px-6 py-4">
          {successMessage && (
            <div className="mb-4 rounded border border-success bg-success/10 px-4 py-3 text-success">
              {successMessage}
            </div>
          )}

          <div className="space-y-4 text-sm">
            <div>
              <label className="mb-1 block font-medium text-black dark:text-white">To</label>
              <input
                className="w-full rounded border border-stroke px-3 py-2 text-black focus:border-primary focus:outline-none dark:border-strokedark dark:bg-boxdark dark:text-white"
                value={recipientEmail}
                onChange={(e) => setRecipientEmail(e.target.value)}
                placeholder="client@example.com"
                disabled={submitting}
              />
              {errors.recipientEmail && <p className="mt-1 text-xs text-meta-1">{errors.recipientEmail}</p>}
            </div>
            <div>
              <label className="mb-1 block font-medium text-black dark:text-white">CC</label>
              <input
                className="w-full rounded border border-stroke px-3 py-2 text-black focus:border-primary focus:outline-none dark:border-strokedark dark:bg-boxdark dark:text-white"
                value={ccEmails}
                onChange={(e) => setCcEmails(e.target.value)}
                placeholder="comma separated emails"
                disabled={submitting}
              />
            </div>
            <div>
              <label className="mb-1 block font-medium text-black dark:text-white">BCC</label>
              <input
                className="w-full rounded border border-stroke px-3 py-2 text-black focus:border-primary focus:outline-none dark:border-strokedark dark:bg-boxdark dark:text-white"
                value={bccEmails}
                onChange={(e) => setBccEmails(e.target.value)}
                placeholder="comma separated emails"
                disabled={submitting}
              />
            </div>
            <div>
              <label className="mb-1 block font-medium text-black dark:text-white">Custom Message</label>
              <textarea
                className="h-32 w-full rounded border border-stroke px-3 py-2 text-black focus:border-primary focus:outline-none dark:border-strokedark dark:bg-boxdark dark:text-white"
                value={customMessage}
                onChange={(e) => setCustomMessage(e.target.value)}
                placeholder="Add an optional personalized note to include in the email."
                disabled={submitting}
              />
              <p className="mt-1 text-xs text-gray-500">{customMessage.length}/2000</p>
              {errors.customMessage && <p className="mt-1 text-xs text-meta-1">{errors.customMessage}</p>}
            </div>
            {errors.list && <p className="text-xs text-meta-1">{errors.list}</p>}
            {errors.submit && <p className="text-xs text-meta-1">{errors.submit}</p>}
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
            disabled={submitting}
            className="rounded bg-primary px-6 py-2 text-sm font-semibold text-white hover:bg-opacity-90 disabled:opacity-50"
          >
            {submitting ? "Sending..." : mode === "resend" ? "Resend Quotation" : "Send Quotation"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default SendQuotationModal;

