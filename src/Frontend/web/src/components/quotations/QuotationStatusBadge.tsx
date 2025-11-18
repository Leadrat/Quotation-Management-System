import { getStatusColor, getStatusLabel } from "@/utils/quotationFormatter";

interface QuotationStatusBadgeProps {
  status: string;
  className?: string;
}

export default function QuotationStatusBadge({ status, className = "" }: QuotationStatusBadgeProps) {
  return (
    <span className={`inline-flex rounded-full px-3 py-1 text-xs font-medium ${getStatusColor(status)} ${className}`}>
      {getStatusLabel(status)}
    </span>
  );
}

