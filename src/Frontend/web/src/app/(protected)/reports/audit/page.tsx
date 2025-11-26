"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import { DateRangePicker } from "@/components/reports/ui";
import { ExportButton } from "@/components/reports/ui";
import type { AuditReport } from "@/types/reports";

export default function AuditReportPage() {
  const [data, setData] = useState<AuditReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [entityType, setEntityType] = useState("");

  useEffect(() => {
    if (fromDate && toDate) {
      loadData();
    }
  }, [fromDate, toDate, entityType]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ReportsApi.getAuditReport(
        fromDate || undefined,
        toDate || undefined,
        entityType || undefined
      );
      if (response.success && response.data) {
        setData(response.data);
      }
    } catch (err: any) {
      console.error("Error loading audit report:", err);
      setError(err.message || "Failed to load report");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Audit & Compliance Report" />
      
      <div className="mb-6 grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="md:col-span-2">
          <DateRangePicker
            fromDate={fromDate}
            toDate={toDate}
            onChange={(from, to) => {
              setFromDate(from);
              setToDate(to);
            }}
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Entity Type
          </label>
          <select
            value={entityType}
            onChange={(e) => setEntityType(e.target.value)}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
          >
            <option value="">All</option>
            <option value="Quotation">Quotation</option>
            <option value="Payment">Payment</option>
            <option value="Client">Client</option>
            <option value="User">User</option>
          </select>
        </div>
      </div>

      {error && (
        <div className="mb-6 rounded-lg bg-red-50 border border-red-200 p-4 dark:bg-red-900/20 dark:border-red-800">
          <p className="text-sm text-red-800 dark:text-red-400">{error}</p>
        </div>
      )}

      {loading ? (
        <ComponentCard title="Audit & Compliance Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading audit report...</p>
          </div>
        </ComponentCard>
      ) : data ? (
        <div className="space-y-6">
          <ComponentCard title="Audit Trail">
            <div className="flex justify-end mb-4">
              <ExportButton reportId="audit" reportName="Audit & Compliance Report" />
            </div>

            {/* Changes */}
            {data.changes && data.changes.length > 0 && (
              <div className="mb-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Changes</h3>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-gray-200 dark:border-gray-700">
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Entity
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Action
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          User
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Timestamp
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {data.changes.slice(0, 50).map((change) => (
                        <tr key={change.entryId} className="border-b border-gray-100 dark:border-gray-800">
                          <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                            {change.entityType} ({change.entityId.substring(0, 8)}...)
                          </td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">{change.action}</td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">{change.userName}</td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                            {new Date(change.timestamp).toLocaleString()}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            {/* Approvals */}
            {data.approvals && data.approvals.length > 0 && (
              <div className="mb-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Approvals</h3>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-gray-200 dark:border-gray-700">
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Quotation
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Status
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Requested By
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Approved By
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Date
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {data.approvals.slice(0, 50).map((approval) => (
                        <tr key={approval.approvalId} className="border-b border-gray-100 dark:border-gray-800">
                          <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                            {approval.quotationNumber}
                          </td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">{approval.status}</td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                            {approval.requestedByUserName}
                          </td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                            {approval.approvedByUserName || "N/A"}
                          </td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                            {new Date(approval.requestedAt).toLocaleString()}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            {/* Payments */}
            {data.payments && data.payments.length > 0 && (
              <div className="mb-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Payments</h3>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-gray-200 dark:border-gray-700">
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Quotation
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Gateway
                        </th>
                        <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Amount
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Status
                        </th>
                        <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                          Date
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {data.payments.slice(0, 50).map((payment) => (
                        <tr key={payment.paymentId} className="border-b border-gray-100 dark:border-gray-800">
                          <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                            {payment.quotationNumber}
                          </td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                            {payment.paymentGateway}
                          </td>
                          <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                            ₹{payment.amount.toLocaleString()}
                          </td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">{payment.status}</td>
                          <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                            {new Date(payment.createdAt).toLocaleString()}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </ComponentCard>
        </div>
      ) : (
        <ComponentCard title="Audit & Compliance Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">
              Select a date range to generate the audit report
            </p>
          </div>
        </ComponentCard>
      )}
    </>
  );
}

