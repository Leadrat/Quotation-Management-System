/**
 * Utility functions for formatting quotation data
 */

export function formatCurrency(amount: number, currency: string = "INR"): string {
  return new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatDate(date: string | Date): string {
  const d = typeof date === "string" ? new Date(date) : date;
  return new Intl.DateTimeFormat("en-IN", {
    year: "numeric",
    month: "short",
    day: "numeric",
  }).format(d);
}

export function formatDateTime(date: string | Date): string {
  const d = typeof date === "string" ? new Date(date) : date;
  return new Intl.DateTimeFormat("en-IN", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(d);
}

export function getStatusColor(status: string | null | undefined): string {
  if (!status) {
    return "bg-gray-100 text-gray-800";
  }
  const statusLower = status.toLowerCase();
  switch (statusLower) {
    case "draft":
      return "bg-gray-100 text-gray-800";
    case "sent":
      return "bg-blue-100 text-blue-800";
    case "viewed":
      return "bg-yellow-100 text-yellow-800";
    case "accepted":
      return "bg-green-100 text-green-800";
    case "rejected":
      return "bg-red-100 text-red-800";
    case "expired":
      return "bg-orange-100 text-orange-800";
    case "cancelled":
      return "bg-gray-100 text-gray-800";
    default:
      return "bg-gray-100 text-gray-800";
  }
}

export function getStatusLabel(status: string | null | undefined): string {
  if (!status) {
    return "Unknown";
  }
  const statusLower = status.toLowerCase();
  switch (statusLower) {
    case "draft":
      return "Draft";
    case "sent":
      return "Sent";
    case "viewed":
      return "Viewed";
    case "accepted":
      return "Accepted";
    case "rejected":
      return "Rejected";
    case "expired":
      return "Expired";
    case "cancelled":
      return "Cancelled";
    default:
      return status;
  }
}

export function formatQuotationNumber(number: string): string {
  return number.toUpperCase();
}

