"use client";
import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import JurisdictionTree from "@/components/tax/JurisdictionTree";
import JurisdictionForm from "@/components/tax/JurisdictionForm";
import { JurisdictionsApi, CountriesApi } from "@/lib/api";
import Button from "@/components/tailadmin/ui/button/Button";
import { useToast, ToastContainer } from "@/components/quotations/Toast";

export default function JurisdictionsPage() {
  const params = useParams();
  const router = useRouter();
  const countryId = params.countryId as string;
  const [showForm, setShowForm] = useState(false);
  const [editingJurisdiction, setEditingJurisdiction] = useState<any>(null);
  const [parentJurisdictionId, setParentJurisdictionId] = useState<string | undefined>();
  const [jurisdictions, setJurisdictions] = useState<any[]>([]);
  const [country, setCountry] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const toast = useToast();

  useEffect(() => {
    if (countryId) {
      loadData();
    }
  }, [countryId]);

  async function loadData() {
    try {
      setLoading(true);
      const [countryRes, jurisdictionsRes] = await Promise.all([
        CountriesApi.getById(countryId).catch(() => ({ data: null })),
        JurisdictionsApi.listByCountry(countryId).catch(() => ({ data: [] })),
      ]);
      setCountry(countryRes.data);
      setJurisdictions(Array.isArray(jurisdictionsRes.data) ? jurisdictionsRes.data : []);
    } catch (e: any) {
      toast.error(e.message || "Failed to load data");
    } finally {
      setLoading(false);
    }
  }

  const handleCreate = () => {
    setEditingJurisdiction(null);
    setParentJurisdictionId(undefined);
    setShowForm(true);
  };

  const handleAddChild = (parentId: string) => {
    setEditingJurisdiction(null);
    setParentJurisdictionId(parentId);
    setShowForm(true);
  };

  const handleEdit = (jurisdiction: any) => {
    setEditingJurisdiction(jurisdiction);
    setParentJurisdictionId(undefined);
    setShowForm(true);
  };

  const handleDelete = async (jurisdictionId: string) => {
    try {
      await JurisdictionsApi.delete(jurisdictionId);
      toast.success("Jurisdiction deleted successfully");
      await loadData();
    } catch (e: any) {
      toast.error(e.message || "Failed to delete jurisdiction");
    }
  };

  const handleSave = async (data: any) => {
    try {
      if (editingJurisdiction) {
        await JurisdictionsApi.update(editingJurisdiction.jurisdictionId, data);
        toast.success("Jurisdiction updated successfully");
      } else {
        await JurisdictionsApi.create(data);
        toast.success("Jurisdiction created successfully");
      }
      setShowForm(false);
      setEditingJurisdiction(null);
      setParentJurisdictionId(undefined);
      await loadData();
    } catch (e: any) {
      throw e; // Let form handle error display
    }
  };

  if (loading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
        <p className="mt-2 text-gray-600">Loading jurisdictions...</p>
      </div>
    );
  }

  return (
    <div>
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <PageBreadcrumb pageName={`Jurisdictions - ${country?.countryName || "Country"}`} />

      <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
        <div className="border-b border-stroke px-6.5 py-4 dark:border-strokedark">
          <div className="flex justify-between items-center">
            <div>
              <h3 className="font-medium text-black dark:text-white">
                Jurisdiction Management
              </h3>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Configure jurisdictions for {country?.countryName || "country"}
              </p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => router.push("/admin/tax/countries")}>
                Back to Countries
              </Button>
              {!showForm && (
                <Button onClick={handleCreate}>Create Jurisdiction</Button>
              )}
            </div>
          </div>
        </div>

        <div className="p-6.5">
          {!showForm ? (
            <JurisdictionTree
              jurisdictions={jurisdictions}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onAddChild={handleAddChild}
            />
          ) : (
            <div className="bg-white dark:bg-gray-800 rounded-lg border p-6">
              <h4 className="text-lg font-semibold mb-4">
                {editingJurisdiction ? "Edit Jurisdiction" : parentJurisdictionId ? "Create Child Jurisdiction" : "Create Jurisdiction"}
              </h4>
              <JurisdictionForm
                jurisdiction={editingJurisdiction}
                countryId={countryId}
                parentJurisdictionId={parentJurisdictionId}
                onSave={handleSave}
                onCancel={() => {
                  setShowForm(false);
                  setEditingJurisdiction(null);
                  setParentJurisdictionId(undefined);
                }}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
