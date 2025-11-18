"use client";

import type { PaymentSummaryDto } from "@/types/payments";

interface PaymentSummaryCardsProps {
  summary: PaymentSummaryDto;
}

export function PaymentSummaryCards({ summary }: PaymentSummaryCardsProps) {
  const cards = [
    {
      title: "Total Pending",
      amount: summary.totalPending,
      count: summary.pendingCount,
      color: "yellow",
      icon: "⏳",
    },
    {
      title: "Total Paid",
      amount: summary.totalPaid,
      count: summary.paidCount,
      color: "green",
      icon: "✅",
    },
    {
      title: "Total Refunded",
      amount: summary.totalRefunded,
      count: summary.refundedCount,
      color: "blue",
      icon: "↩️",
    },
    {
      title: "Total Failed",
      amount: summary.totalFailed,
      count: summary.failedCount,
      color: "red",
      icon: "❌",
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      {cards.map((card) => (
        <div
          key={card.title}
          className="bg-white rounded-lg shadow p-6 border-l-4"
          style={{
            borderLeftColor:
              card.color === "yellow"
                ? "#eab308"
                : card.color === "green"
                ? "#22c55e"
                : card.color === "blue"
                ? "#3b82f6"
                : "#ef4444",
          }}
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">{card.title}</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">
                ₹{card.amount.toLocaleString("en-IN", { minimumFractionDigits: 2 })}
              </p>
              <p className="text-xs text-gray-500 mt-1">{card.count} payments</p>
            </div>
            <div className="text-4xl">{card.icon}</div>
          </div>
        </div>
      ))}
    </div>
  );
}

