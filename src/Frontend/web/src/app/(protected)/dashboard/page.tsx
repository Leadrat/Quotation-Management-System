"use client";
import { useEffect, useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { FolderIcon, FileIcon, DollarLineIcon, CheckCircleIcon } from "@/icons";
import { ReportsApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken } from "@/lib/session";

export default function DashboardPage() {
  const [stats, setStats] = useState<{
    totalClients: number;
    totalQuotations: number;
    totalPayments: number;
    pendingApprovals: number;
  } | null>(null);
  const [salesStats, setSalesStats] = useState<{
    quotationsCreatedThisMonth: number;
    quotationsAcceptedThisMonth: number;
    quotationsSentThisMonth: number;
  } | null>(null);
  const [loading, setLoading] = useState(true);
  const [userRole, setUserRole] = useState<string | null>(null);

  useEffect(() => {
    const token = getAccessToken();
    const role = getRoleFromToken(token);
    setUserRole(role);
    loadStats(role);
  }, []);

  const loadStats = async (role: string | null) => {
    try {
      setLoading(true);
      
      // Load basic dashboard stats
      const response = await ReportsApi.getDashboardStats();
      if (response.success) {
        setStats(response.data);
      }

      // If user is SalesRep, load sales dashboard metrics
      if (role === "SalesRep") {
        try {
          const salesResponse = await ReportsApi.getSalesDashboard();
          if (salesResponse.success && salesResponse.data) {
            setSalesStats({
              quotationsCreatedThisMonth: salesResponse.data.quotationsCreatedThisMonth || 0,
              quotationsAcceptedThisMonth: salesResponse.data.quotationsAcceptedThisMonth || 0,
              quotationsSentThisMonth: salesResponse.data.quotationsSentThisMonth || 0,
            });
          }
        } catch (err) {
          console.error("Error loading sales dashboard stats:", err);
        }
      }
    } catch (err) {
      console.error("Error loading dashboard stats:", err);
    } finally {
      setLoading(false);
    }
  };

  const formatNumber = (num: number | null | undefined) => {
    if (num === null || num === undefined) return "0";
    return num.toLocaleString();
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Dashboard" />
      
      <div className={`grid grid-cols-1 gap-4 sm:grid-cols-2 ${userRole === "SalesRep" ? "lg:grid-cols-6" : "lg:grid-cols-4"} mb-6`}>
        {/* Total Clients Card */}
        <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] md:p-6">
          <div className="flex items-center justify-center w-12 h-12 bg-gray-100 rounded-xl dark:bg-gray-800">
            <FolderIcon className="text-gray-800 size-6 dark:text-white/90" />
          </div>
          <div className="flex items-end justify-between mt-5">
            <div>
              <span className="text-sm text-gray-500 dark:text-gray-400">Total Clients</span>
              <h4 className="mt-2 font-bold text-gray-800 text-title-sm dark:text-white/90">
                {loading ? "Loading..." : formatNumber(stats?.totalClients)}
              </h4>
            </div>
          </div>
        </div>

        {/* Total Quotations Card */}
        <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] md:p-6">
          <div className="flex items-center justify-center w-12 h-12 bg-gray-100 rounded-xl dark:bg-gray-800">
            <FileIcon className="text-gray-800 size-6 dark:text-white/90" />
          </div>
          <div className="flex items-end justify-between mt-5">
            <div>
              <span className="text-sm text-gray-500 dark:text-gray-400">Total Quotations</span>
              <h4 className="mt-2 font-bold text-gray-800 text-title-sm dark:text-white/90">
                {loading ? "Loading..." : formatNumber(stats?.totalQuotations)}
              </h4>
            </div>
          </div>
        </div>

        {/* Sales Rep Specific Cards */}
        {userRole === "SalesRep" && (
          <>
            {/* Quotations Created This Month Card */}
            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] md:p-6">
              <div className="flex items-center justify-center w-12 h-12 bg-blue-100 rounded-xl dark:bg-blue-900/20">
                <FileIcon className="text-blue-600 size-6 dark:text-blue-400" />
              </div>
              <div className="flex items-end justify-between mt-5">
                <div>
                  <span className="text-sm text-gray-500 dark:text-gray-400">Quotations Created (This Month)</span>
                  <h4 className="mt-2 font-bold text-gray-800 text-title-sm dark:text-white/90">
                    {loading ? "Loading..." : formatNumber(salesStats?.quotationsCreatedThisMonth)}
                  </h4>
                </div>
              </div>
            </div>

            {/* Quotations Accepted This Month Card */}
            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] md:p-6">
              <div className="flex items-center justify-center w-12 h-12 bg-green-100 rounded-xl dark:bg-green-900/20">
                <CheckCircleIcon className="text-green-600 size-6 dark:text-green-400" />
              </div>
              <div className="flex items-end justify-between mt-5">
                <div>
                  <span className="text-sm text-gray-500 dark:text-gray-400">Quotations Accepted (This Month)</span>
                  <h4 className="mt-2 font-bold text-gray-800 text-title-sm dark:text-white/90">
                    {loading ? "Loading..." : formatNumber(salesStats?.quotationsAcceptedThisMonth)}
                  </h4>
                </div>
              </div>
            </div>

            {/* Quotations Sent This Month Card */}
            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] md:p-6">
              <div className="flex items-center justify-center w-12 h-12 bg-purple-100 rounded-xl dark:bg-purple-900/20">
                <FileIcon className="text-purple-600 size-6 dark:text-purple-400" />
              </div>
              <div className="flex items-end justify-between mt-5">
                <div>
                  <span className="text-sm text-gray-500 dark:text-gray-400">Quotations Sent (This Month)</span>
                  <h4 className="mt-2 font-bold text-gray-800 text-title-sm dark:text-white/90">
                    {loading ? "Loading..." : formatNumber(salesStats?.quotationsSentThisMonth)}
                  </h4>
                </div>
              </div>
            </div>
          </>
        )}

        {/* Total Payments Card */}
        <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] md:p-6">
          <div className="flex items-center justify-center w-12 h-12 bg-gray-100 rounded-xl dark:bg-gray-800">
            <DollarLineIcon className="text-gray-800 size-6 dark:text-white/90" />
          </div>
          <div className="flex items-end justify-between mt-5">
            <div>
              <span className="text-sm text-gray-500 dark:text-gray-400">Total Payments</span>
              <h4 className="mt-2 font-bold text-gray-800 text-title-sm dark:text-white/90">
                {loading ? "Loading..." : formatNumber(stats?.totalPayments)}
              </h4>
            </div>
          </div>
        </div>

        {/* Pending Approvals Card */}
        <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] md:p-6">
          <div className="flex items-center justify-center w-12 h-12 bg-gray-100 rounded-xl dark:bg-gray-800">
            <CheckCircleIcon className="text-gray-800 size-6 dark:text-white/90" />
          </div>
          <div className="flex items-end justify-between mt-5">
            <div>
              <span className="text-sm text-gray-500 dark:text-gray-400">Pending Approvals</span>
              <h4 className="mt-2 font-bold text-gray-800 text-title-sm dark:text-white/90">
                {loading ? "Loading..." : formatNumber(stats?.pendingApprovals)}
              </h4>
            </div>
          </div>
        </div>
      </div>

      <ComponentCard title="Welcome to CRM Quotation Management System" desc="Use the navigation to manage clients, quotations, payments, and more.">
        <p className="text-gray-500 dark:text-gray-400">
          This dashboard displays real-time statistics and metrics from your CRM system.
        </p>
      </ComponentCard>
    </>
  );
}
