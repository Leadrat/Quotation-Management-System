import { useState, useEffect } from "react";
import { RefundDto } from "@/types/refunds";
import { RefundsApi } from "@/lib/api";

export function useRefunds(filters?: { status?: string; approvalLevel?: string }) {
  const [refunds, setRefunds] = useState<RefundDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadRefunds();
  }, [filters?.status, filters?.approvalLevel]);

  const loadRefunds = async () => {
    setLoading(true);
    setError(null);
    try {
      let response;
      if (filters?.status === "pending") {
        response = await RefundsApi.getPending(filters.approvalLevel);
      } else {
        // For now, get pending - in production, you'd have a getAll endpoint
        response = await RefundsApi.getPending();
      }
      setRefunds(response.data || []);
    } catch (err: any) {
      setError(err.message || "Failed to load refunds");
    } finally {
      setLoading(false);
    }
  };

  return { refunds, loading, error, refetch: loadRefunds };
}

export function useRefund(refundId: string) {
  const [refund, setRefund] = useState<RefundDto | null>(null);
  const [timeline, setTimeline] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (refundId) {
      loadRefund();
    }
  }, [refundId]);

  const loadRefund = async () => {
    setLoading(true);
    setError(null);
    try {
      const [refundRes, timelineRes] = await Promise.all([
        RefundsApi.getById(refundId),
        RefundsApi.getTimeline(refundId).catch(() => ({ data: [] })),
      ]);
      setRefund(refundRes.data);
      setTimeline(timelineRes.data || []);
    } catch (err: any) {
      setError(err.message || "Failed to load refund");
    } finally {
      setLoading(false);
    }
  };

  return { refund, timeline, loading, error, refetch: loadRefund };
}

