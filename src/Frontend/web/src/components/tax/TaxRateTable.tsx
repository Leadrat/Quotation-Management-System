"use client";
import { useState, useEffect } from "react";
import { TaxRatesApi } from "@/lib/api";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Badge from "@/components/tailadmin/ui/badge/Badge";
import Button from "@/components/tailadmin/ui/button/Button";

interface TaxRateTableProps {
  jurisdictionId?: string;
  taxFrameworkId?: string;
  productServiceCategoryId?: string;
  asOfDate?: string;
  onEdit?: (taxRate: any) => void;
  onDelete?: (taxRateId: string) => void;
}

export default function TaxRateTable({
  jurisdictionId,
  taxFrameworkId,
  productServiceCategoryId,
  asOfDate,
  onEdit,
  onDelete,
}: TaxRateTableProps) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [taxRates, setTaxRates] = useState<any[]>([]);

  useEffect(() => {
    loadData();
  }, [jurisdictionId, taxFrameworkId, productServiceCategoryId, asOfDate]);

  async function loadData() {
    setLoading(true);
    setError(null);
    try {
      const res = await TaxRatesApi.list({
        jurisdictionId,
        taxFrameworkId,
        productServiceCategoryId,
        asOfDate,
      });
      setTaxRates(Array.isArray(res.data) ? res.data : []);
    } catch (e: any) {
      const errorMsg = e.message || "Failed to load tax rates";
      setError(errorMsg);
      setTaxRates([]);
    } finally {
      setLoading(false);
    }
  }

  function formatDate(date: string) {
    return new Date(date).toLocaleDateString();
  }

  if (loading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
        <p className="mt-2 text-gray-600">Loading tax rates...</p>
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
    <div className="overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableCell>Jurisdiction</TableCell>
            <TableCell>Category</TableCell>
            <TableCell>Rate</TableCell>
            <TableCell>Components</TableCell>
            <TableCell>Effective From</TableCell>
            <TableCell>Effective To</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {taxRates.length === 0 ? (
            <TableRow>
              <TableCell colSpan={8} className="text-center py-8 text-gray-500">
                No tax rates found
              </TableCell>
            </TableRow>
          ) : (
            taxRates.map((rate) => (
              <TableRow key={rate.taxRateId}>
                <TableCell>{rate.jurisdictionName || "Country Default"}</TableCell>
                <TableCell>{rate.categoryName || "All Categories"}</TableCell>
                <TableCell className="font-medium">{rate.taxRate}%</TableCell>
                <TableCell>
                  {rate.taxComponents && rate.taxComponents.length > 0 ? (
                    <div className="flex flex-wrap gap-1">
                      {rate.taxComponents.map((comp: any, idx: number) => (
                        <Badge key={idx} className="bg-blue-100 text-blue-800 text-xs">
                          {comp.component}: {comp.rate}%
                        </Badge>
                      ))}
                    </div>
                  ) : (
                    <span className="text-gray-400">-</span>
                  )}
                </TableCell>
                <TableCell>{formatDate(rate.effectiveFrom)}</TableCell>
                <TableCell>{rate.effectiveTo ? formatDate(rate.effectiveTo) : "Ongoing"}</TableCell>
                <TableCell>
                  {rate.isExempt ? (
                    <Badge className="bg-yellow-100 text-yellow-800">Exempt</Badge>
                  ) : rate.isZeroRated ? (
                    <Badge className="bg-green-100 text-green-800">Zero Rated</Badge>
                  ) : (
                    <Badge className="bg-gray-100 text-gray-800">Active</Badge>
                  )}
                </TableCell>
                <TableCell>
                  <div className="flex gap-2">
                    {onEdit && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onEdit(rate)}
                      >
                        Edit
                      </Button>
                    )}
                    {onDelete && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => {
                          if (confirm("Are you sure you want to delete this tax rate?")) {
                            onDelete(rate.taxRateId);
                          }
                        }}
                      >
                        Delete
                      </Button>
                    )}
                  </div>
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}

