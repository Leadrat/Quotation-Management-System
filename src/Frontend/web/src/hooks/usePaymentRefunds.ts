import { useState, useEffect } from "react";
import { RefundDto } from "@/types/refunds";
import { RefundsApi } from "@/lib/api";

export function usePaymentRefunds(paymentId: string) {
  const [refunds, setRefunds] = useState<RefundDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (paymentId) {
      loadRefunds();
    }
  }, [paymentId]);

  const loadRefunds = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await RefundsApi.getByPayment(paymentId);
      setRefunds(response.data || []);
    } catch (err: any) {
      setError(err.message || "Failed to load refunds");
    } finally {
      setLoading(false);
    }
  };

  return { refunds, loading, error, refetch: loadRefunds };
}

