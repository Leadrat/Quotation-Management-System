"use client";
import { useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import CountryManagementTable from "@/components/tax/CountryManagementTable";
import CountryForm from "@/components/tax/CountryForm";
import { CountriesApi } from "@/lib/api";
import Button from "@/components/tailadmin/ui/button/Button";
import { useRouter } from "next/navigation";
import { useToast, ToastContainer } from "@/components/quotations/Toast";

export default function CountriesPage() {
  const router = useRouter();
  const toast = useToast();
  const [showForm, setShowForm] = useState(false);
  const [editingCountry, setEditingCountry] = useState<any>(null);

  const handleCreate = () => {
    setEditingCountry(null);
    setShowForm(true);
  };

  const handleEdit = (country: any) => {
    setEditingCountry(country);
    setShowForm(true);
  };

  const handleDelete = async (countryId: string) => {
    try {
      // Note: Delete endpoint may not exist, handle gracefully
      toast.error("Delete functionality requires backend endpoint");
    } catch (e: any) {
      toast.error(e.message || "Failed to delete country");
    }
  };

  const handleViewJurisdictions = (countryId: string) => {
    router.push(`/admin/tax/countries/${countryId}/jurisdictions`);
  };

  const handleSave = async (data: any) => {
    try {
      if (editingCountry) {
        await CountriesApi.update(editingCountry.countryId, data);
        toast.success("Country updated successfully");
      } else {
        await CountriesApi.create(data);
        toast.success("Country created successfully");
      }
      setShowForm(false);
      setEditingCountry(null);
      window.location.reload(); // Simple reload for now
    } catch (e: any) {
      throw e; // Let form handle error display
    }
  };

  const toast = useToast();

  return (
    <div>
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <PageBreadcrumb pageName="Countries" />

      <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
        <div className="border-b border-stroke px-6.5 py-4 dark:border-strokedark">
          <div className="flex justify-between items-center">
            <div>
              <h3 className="font-medium text-black dark:text-white">
                Country Management
              </h3>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Configure countries and their tax framework types
              </p>
            </div>
            {!showForm && (
              <Button onClick={handleCreate}>Create Country</Button>
            )}
          </div>
        </div>

        <div className="p-6.5">
          {!showForm ? (
            <CountryManagementTable
              onEdit={handleEdit}
              onDelete={handleDelete}
              onViewJurisdictions={handleViewJurisdictions}
            />
          ) : (
            <div className="bg-white dark:bg-gray-800 rounded-lg border p-6">
              <h4 className="text-lg font-semibold mb-4">
                {editingCountry ? "Edit Country" : "Create Country"}
              </h4>
              <CountryForm
                country={editingCountry}
                onSave={handleSave}
                onCancel={() => {
                  setShowForm(false);
                  setEditingCountry(null);
                }}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

