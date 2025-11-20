"use client";
import { useEffect, useState } from "react";
import { TaxAuditLogApi } from "@/lib/api";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Pagination from "@/components/tailadmin/tables/Pagination";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface TaxAuditLogTableProps {
  quotationId?: string;
  countryId?: string;
  jurisdictionId?: string;
  fromDate?: string;
  toDate?: string;
}

export default function TaxAuditLogTable({
  quotationId,
  countryId,
  jurisdictionId,
  fromDate,
  toDate,
}: TaxAuditLogTableProps) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [logs, setLogs] = useState<any[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);

  useEffect(() => {
    loadData();
  }, [pageNumber, pageSize, quotationId, countryId, jurisdictionId, fromDate, toDate]);

  async function loadData() {
    setLoading(true);
    setError(null);
    try {
      const res = await TaxAuditLogApi.list({
        quotationId,
        countryId,
        jurisdictionId,
        fromDate,
        toDate,
        pageNumber,
        pageSize,
      });
      setLogs(Array.isArray(res.data) ? res.data : []);
      setTotal(res.totalCount || 0);
    } catch (e: any) {
      const errorMsg = e.message || "Failed to load audit log";
      setError(errorMsg);
      setLogs([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  }

  function formatDate(date: string) {
    return new Date(date).toLocaleString();
  }

  function getActionTypeColor(actionType: string) {
    if (actionType === "Calculation") return "bg-blue-100 text-blue-800";
    if (actionType === "ConfigurationChange") return "bg-green-100 text-green-800";
    return "bg-gray-100 text-gray-800";
  }

  if (loading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
        <p className="mt-2 text-gray-600">Loading audit log...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
        {error}
      </div>
    );
  }

  return (
    <div>
      <div className="overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Action Type</TableCell>
              <TableCell>Country</TableCell>
              <TableCell>Jurisdiction</TableCell>
              <TableCell>Quotation</TableCell>
              <TableCell>Changed By</TableCell>
              <TableCell>Details</TableCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {logs.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="text-center py-8 text-gray-500">
                  No audit log entries found
                </TableCell>
              </TableRow>
            ) : (
              logs.map((log) => (
                <TableRow key={log.logId}>
                  <TableCell>{formatDate(log.changedAt)}</TableCell>
                  <TableCell>
                    <Badge className={getActionTypeColor(log.actionType)}>
                      {log.actionType}
                    </Badge>
                  </TableCell>
                  <TableCell>{log.countryName || "N/A"}</TableCell>
                  <TableCell>{log.jurisdictionName || "N/A"}</TableCell>
                  <TableCell>
                    {log.quotationId ? (
                      <span className="text-blue-600">#{log.quotationId.substring(0, 8)}</span>
                    ) : (
                      "N/A"
                    )}
                  </TableCell>
                  <TableCell>{log.changedByUserName || "System"}</TableCell>
                  <TableCell>
                    <details className="cursor-pointer">
                      <summary className="text-blue-600 hover:underline">View</summary>
                      <pre className="mt-2 p-2 bg-gray-50 rounded text-xs overflow-auto max-w-md">
                        {JSON.stringify(log.calculationDetails, null, 2)}
                      </pre>
                    </details>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {total > 0 && (
        <div className="mt-4">
          <Pagination
            currentPage={pageNumber}
            totalPages={Math.ceil(total / pageSize)}
            onPageChange={setPageNumber}
            pageSize={pageSize}
            onPageSizeChange={setPageSize}
            totalItems={total}
          />
        </div>
      )}
    </div>
  );
}

