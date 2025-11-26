"use client";
import { useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import TaxAuditLogTable from "@/components/tax/TaxAuditLogTable";
import Button from "@/components/tailadmin/ui/button/Button";

export default function TaxAuditLogPage() {
  const [quotationId, setQuotationId] = useState("");
  const [countryId, setCountryId] = useState("");
  const [jurisdictionId, setJurisdictionId] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [filters, setFilters] = useState({
    quotationId: "",
    countryId: "",
    jurisdictionId: "",
    fromDate: "",
    toDate: "",
  });

  const applyFilters = () => {
    setFilters({
      quotationId,
      countryId,
      jurisdictionId,
      fromDate,
      toDate,
    });
  };

  const clearFilters = () => {
    setQuotationId("");
    setCountryId("");
    setJurisdictionId("");
    setFromDate("");
    setToDate("");
    setFilters({
      quotationId: "",
      countryId: "",
      jurisdictionId: "",
      fromDate: "",
      toDate: "",
    });
  };

  return (
    <div>
      <PageBreadcrumb pageTitle="Tax Audit Log" />
      
      <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
        <div className="border-b border-stroke px-6.5 py-4 dark:border-strokedark">
          <h3 className="font-medium text-black dark:text-white">
            Tax Calculation Audit Trail
          </h3>
          <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
            View history of tax calculations and configuration changes
          </p>
        </div>

        <div className="p-6.5">
          {/* Filters */}
          <div className="mb-6 grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label htmlFor="quotationId">Quotation ID</Label>
              <Input
                id="quotationId"
                type="text"
                value={quotationId}
                onChange={(e) => setQuotationId(e.target.value)}
                placeholder="Filter by quotation ID"
              />
            </div>
            <div>
              <Label htmlFor="countryId">Country ID</Label>
              <Input
                id="countryId"
                type="text"
                value={countryId}
                onChange={(e) => setCountryId(e.target.value)}
                placeholder="Filter by country ID"
              />
            </div>
            <div>
              <Label htmlFor="jurisdictionId">Jurisdiction ID</Label>
              <Input
                id="jurisdictionId"
                type="text"
                value={jurisdictionId}
                onChange={(e) => setJurisdictionId(e.target.value)}
                placeholder="Filter by jurisdiction ID"
              />
            </div>
            <div>
              <Label htmlFor="fromDate">From Date</Label>
              <Input
                id="fromDate"
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
              />
            </div>
            <div>
              <Label htmlFor="toDate">To Date</Label>
              <Input
                id="toDate"
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
              />
            </div>
            <div className="flex items-end gap-2">
              <Button onClick={applyFilters}>Apply Filters</Button>
              <Button variant="outline" onClick={clearFilters}>Clear</Button>
            </div>
          </div>

          {/* Audit Log Table */}
          <TaxAuditLogTable
            quotationId={filters.quotationId || undefined}
            countryId={filters.countryId || undefined}
            jurisdictionId={filters.jurisdictionId || undefined}
            fromDate={filters.fromDate || undefined}
            toDate={filters.toDate || undefined}
          />
        </div>
      </div>
    </div>
  );
}

