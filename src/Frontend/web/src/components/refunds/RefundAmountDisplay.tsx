"use client";

interface RefundAmountDisplayProps {
  amount: number;
  currency?: string;
  className?: string;
}

export default function RefundAmountDisplay({
  amount,
  currency = "INR",
  className = "",
}: RefundAmountDisplayProps) {
  const formatCurrency = (amount: number, currency: string) => {
    return new Intl.NumberFormat("en-IN", {
      style: "currency",
      currency: currency,
      minimumFractionDigits: 2,
    }).format(amount);
  };

  return (
    <span className={`font-semibold ${className}`}>
      {formatCurrency(amount, currency)}
    </span>
  );
}

