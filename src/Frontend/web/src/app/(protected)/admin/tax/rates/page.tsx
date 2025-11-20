"use client";
import { useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import TaxRateTable from "@/components/tax/TaxRateTable";
import TaxRateForm from "@/components/tax/TaxRateForm";
import { TaxRatesApi } from "@/lib/api";
import Button from "@/components/tailadmin/ui/button/Button";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import { useToast } from "@/components/quotations/Toast";

export default function TaxRatesPage() {
  const [showForm, setShowForm] = useState(false);
  const [editingRate, setEditingRate] = useState<any>(null);
  const [filters, setFilters] = useState({
    jurisdictionId: "",
    taxFrameworkId: "",
    productServiceCategoryId: "",
    asOfDate: "",
  });
  const [appliedFilters, setAppliedFilters] = useState(filters);
  const toast = useToast();

  const handleCreate = () => {
    setEditingRate(null);
    setShowForm(true);
  };

  const handleEdit = (rate: any) => {
    setEditingRate(rate);
    setShowForm(true);
  };

  const handleDelete = async (taxRateId: string) => {
    try {
      await TaxRatesApi.delete(taxRateId);
      toast.success("Tax rate deleted successfully");
      // Reload will happen automatically via useEffect in TaxRateTable
    } catch (e: any) {
      toast.error(e.message || "Failed to delete tax rate");
    }
  };

  const handleSave = async (data: any) => {
    try {
      if (editingRate) {
        await TaxRatesApi.update(editingRate.taxRateId, data);
        toast.success("Tax rate updated successfully");
      } else {
        await TaxRatesApi.create(data);
        toast.success("Tax rate created successfully");
      }
      setShowForm(false);
      setEditingRate(null);
      // Reload will happen automatically via useEffect in TaxRateTable
    } catch (e: any) {
      throw e; // Let form handle error display
    }
  };

  const applyFilters = () => {
    setAppliedFilters({ ...filters });
  };

  const clearFilters = () => {
    setFilters({
      jurisdictionId: "",
      taxFrameworkId: "",
      productServiceCategoryId: "",
      asOfDate: "",
    });
    setAppliedFilters({
      jurisdictionId: "",
      taxFrameworkId: "",
      productServiceCategoryId: "",
      asOfDate: "",
    });
  };

  return (
    <div>
      <PageBreadcrumb pageName="Tax Rates" />

      <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
        <div className="border-b border-stroke px-6.5 py-4 dark:border-strokedark">
          <div className="flex justify-between items-center">
            <div>
              <h3 className="font-medium text-black dark:text-white">
                Tax Rate Management
              </h3>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Configure tax rates by jurisdiction and product/service category
              </p>
            </div>
            <Button onClick={handleCreate}>Create Tax Rate</Button>
          </div>
        </div>

        <div className="p-6.5">
          {/* Filters */}
          <div className="mb-6 grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <Label htmlFor="jurisdictionId">Jurisdiction ID</Label>
              <Input
                id="jurisdictionId"
                type="text"
                value={filters.jurisdictionId}
                onChange={(e) => setFilters({ ...filters, jurisdictionId: e.target.value })}
                placeholder="Filter by jurisdiction"
              />
            </div>
            <div>
              <Label htmlFor="taxFrameworkId">Tax Framework ID</Label>
              <Input
                id="taxFrameworkId"
                type="text"
                value={filters.taxFrameworkId}
                onChange={(e) => setFilters({ ...filters, taxFrameworkId: e.target.value })}
                placeholder="Filter by framework"
              />
            </div>
            <div>
              <Label htmlFor="productServiceCategoryId">Category ID</Label>
              <Input
                id="productServiceCategoryId"
                type="text"
                value={filters.productServiceCategoryId}
                onChange={(e) => setFilters({ ...filters, productServiceCategoryId: e.target.value })}
                placeholder="Filter by category"
              />
            </div>
            <div>
              <Label htmlFor="asOfDate">As Of Date</Label>
              <Input
                id="asOfDate"
                type="date"
                value={filters.asOfDate}
                onChange={(e) => setFilters({ ...filters, asOfDate: e.target.value })}
              />
            </div>
            <div className="flex items-end gap-2">
              <Button onClick={applyFilters}>Apply Filters</Button>
              <Button variant="outline" onClick={clearFilters}>Clear</Button>
            </div>
          </div>

          {/* Tax Rate Table */}
          {!showForm ? (
            <TaxRateTable
              jurisdictionId={appliedFilters.jurisdictionId || undefined}
              taxFrameworkId={appliedFilters.taxFrameworkId || undefined}
              productServiceCategoryId={appliedFilters.productServiceCategoryId || undefined}
              asOfDate={appliedFilters.asOfDate || undefined}
              onEdit={handleEdit}
              onDelete={handleDelete}
            />
          ) : (
            <div className="bg-white dark:bg-gray-800 rounded-lg border p-6">
              <h4 className="text-lg font-semibold mb-4">
                {editingRate ? "Edit Tax Rate" : "Create Tax Rate"}
              </h4>
              <TaxRateForm
                taxRate={editingRate}
                onSave={handleSave}
                onCancel={() => {
                  setShowForm(false);
                  setEditingRate(null);
                }}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

