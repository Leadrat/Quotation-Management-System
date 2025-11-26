"use client";
import { useState, useEffect } from "react";
import { ProductsApi } from "@/lib/api";
import { formatCurrency } from "@/utils/quotationFormatter";
import type { ProductUsageStats } from "@/types/products";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { FaFileInvoice, FaDollarSign } from "react-icons/fa";

interface ProductUsageStatsProps {
  productId: string;
}

export default function ProductUsageStatsComponent({ productId }: ProductUsageStatsProps) {
  const [stats, setStats] = useState<ProductUsageStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (productId) {
      loadStats();
    }
  }, [productId]);

  async function loadStats() {
    setLoading(true);
    setError(null);
    try {
      const res = await ProductsApi.getUsageStats(productId);
      if (res.success && res.data) {
        setStats(res.data);
      }
    } catch (e: any) {
      setError(e.message || "Failed to load usage statistics");
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return (
      <ComponentCard title="Usage Statistics">
        <div className="py-8 text-center text-gray-500">Loading statistics...</div>
      </ComponentCard>
    );
  }

  if (error) {
    return (
      <ComponentCard title="Usage Statistics">
        <div className="py-4 text-center text-red-500">
          {error}
          <button onClick={loadStats} className="mt-2 text-primary hover:underline">
            Retry
          </button>
        </div>
      </ComponentCard>
    );
  }

  if (!stats) {
    return (
      <ComponentCard title="Usage Statistics">
        <div className="py-4 text-center text-gray-500">No usage data available</div>
      </ComponentCard>
    );
  }

  return (
    <ComponentCard title="Usage Statistics">
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-lg border border-stroke bg-white p-4 dark:border-strokedark dark:bg-form-input">
          <div className="flex items-center gap-3">
            <div className="rounded-full bg-primary/10 p-3">
              <FaFileInvoice className="w-5 h-5 text-primary" />
            </div>
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Quotations</p>
              <p className="text-2xl font-bold text-black dark:text-white">{stats.totalQuotationsUsedIn}</p>
            </div>
          </div>
        </div>

        <div className="rounded-lg border border-stroke bg-white p-4 dark:border-strokedark dark:bg-form-input">
          <div className="flex items-center gap-3">
            <div className="rounded-full bg-green-100 p-3 dark:bg-green-900/20">
              <FaDollarSign className="w-5 h-5 text-green-600 dark:text-green-400" />
            </div>
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Total Revenue</p>
              <p className="text-2xl font-bold text-black dark:text-white">
                {formatCurrency(stats.totalRevenueGenerated, stats.currency)}
              </p>
            </div>
          </div>
        </div>

      </div>

    </ComponentCard>
  );
}

