"use client";
import { useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { useScheduledReports } from "@/hooks/useScheduledReports";
import type { ScheduleReportRequest, RecurrencePattern } from "@/types/reports";

export default function ScheduledReportsPage() {
  const { reports, loading, error, createReport, deleteReport, sendTestEmail } = useScheduledReports();
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [formData, setFormData] = useState<ScheduleReportRequest>({
    reportName: "",
    reportType: "sales-pipeline",
    reportConfig: {},
    recurrencePattern: "daily" as RecurrencePattern,
    emailRecipients: "",
  });

  const handleCreate = async () => {
    const result = await createReport(formData);
    if (result) {
      setShowCreateModal(false);
      setFormData({
        reportName: "",
        reportType: "sales-pipeline",
        reportConfig: {},
        recurrencePattern: "daily" as RecurrencePattern,
        emailRecipients: "",
      });
    }
  };

  const handleDelete = async (reportId: string) => {
    if (!confirm("Are you sure you want to delete this scheduled report?")) return;
    await deleteReport(reportId);
  };

  const handleSendTest = async (reportId: string, emailRecipients: string) => {
    await sendTestEmail(reportId, emailRecipients);
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Scheduled Reports" />
      
      <div className="mb-6 flex justify-end">
        <button
          onClick={() => setShowCreateModal(true)}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600"
        >
          Create Scheduled Report
        </button>
      </div>

      {error && (
        <div className="mb-6 rounded-lg bg-red-50 border border-red-200 p-4 dark:bg-red-900/20 dark:border-red-800">
          <p className="text-sm text-red-800 dark:text-red-400">{error}</p>
        </div>
      )}

      <ComponentCard title="Scheduled Reports">
        {loading ? (
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading scheduled reports...</p>
          </div>
        ) : reports.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No scheduled reports found</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200 dark:border-gray-700">
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                    Report Name
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                    Type
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                    Frequency
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                    Recipients
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                    Last Sent
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                    Next Scheduled
                  </th>
                  <th className="text-center py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                    Status
                  </th>
                  <th className="text-center py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {reports.map((report) => (
                  <tr key={report.reportId} className="border-b border-gray-100 dark:border-gray-800">
                    <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">{report.reportName}</td>
                    <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">{report.reportType}</td>
                    <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300 capitalize">
                      {report.recurrencePattern}
                    </td>
                    <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                      {report.emailRecipients}
                    </td>
                    <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                      {report.lastSentAt ? new Date(report.lastSentAt).toLocaleString() : "Never"}
                    </td>
                    <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                      {new Date(report.nextScheduledAt).toLocaleString()}
                    </td>
                    <td className="py-3 px-4 text-center">
                      <span
                        className={`inline-block px-2 py-1 text-xs font-medium rounded-full ${
                          report.isActive
                            ? "bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400"
                            : "bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400"
                        }`}
                      >
                        {report.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-center">
                      <div className="flex items-center justify-center gap-2">
                        <button
                          onClick={() => handleSendTest(report.reportId, report.emailRecipients)}
                          className="px-3 py-1 text-sm font-medium text-blue-600 hover:text-blue-700 dark:text-blue-400"
                        >
                          Test
                        </button>
                        <button
                          onClick={() => handleDelete(report.reportId)}
                          className="px-3 py-1 text-sm font-medium text-red-600 hover:text-red-700 dark:text-red-400"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </ComponentCard>

      {/* Create Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-md dark:bg-gray-800">
            <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">Create Scheduled Report</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Report Name
                </label>
                <input
                  type="text"
                  value={formData.reportName}
                  onChange={(e) => setFormData({ ...formData, reportName: e.target.value })}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-700 dark:text-gray-300"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Report Type
                </label>
                <select
                  value={formData.reportType}
                  onChange={(e) => setFormData({ ...formData, reportType: e.target.value })}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-700 dark:text-gray-300"
                >
                  <option value="sales-pipeline">Sales Pipeline</option>
                  <option value="team-performance">Team Performance</option>
                  <option value="payment-status">Payment Status</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Frequency
                </label>
                <select
                  value={formData.recurrencePattern}
                  onChange={(e) =>
                    setFormData({ ...formData, recurrencePattern: e.target.value as RecurrencePattern })
                  }
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-700 dark:text-gray-300"
                >
                  <option value="daily">Daily</option>
                  <option value="weekly">Weekly</option>
                  <option value="monthly">Monthly</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Email Recipients (comma-separated)
                </label>
                <input
                  type="text"
                  value={formData.emailRecipients}
                  onChange={(e) => setFormData({ ...formData, emailRecipients: e.target.value })}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-700 dark:text-gray-300"
                  placeholder="email1@example.com, email2@example.com"
                />
              </div>
              <div className="flex gap-2 justify-end">
                <button
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 dark:bg-gray-700 dark:text-gray-300 dark:border-gray-600"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreate}
                  className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600"
                >
                  Create
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

