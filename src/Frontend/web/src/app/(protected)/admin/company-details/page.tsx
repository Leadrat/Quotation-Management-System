"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { CompanyDetailsApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import CompanyDetailsForm from "@/components/tailadmin/company-details/CompanyDetailsForm";
import Link from "next/link";

export default function CompanyDetailsPage() {
  const router = useRouter();
  const [role, setRole] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [uploadingLogo, setUploadingLogo] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [companyDetails, setCompanyDetails] = useState<any>(null);

  useEffect(() => {
    const token = getAccessToken();
    const userRole = getRoleFromToken(token);
    setRole(userRole);

    if (userRole !== "Admin") {
      router.push("/dashboard");
      return;
    }

    loadCompanyDetails();
  }, [router]);

  const loadCompanyDetails = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await CompanyDetailsApi.get();
      if (response.success) {
        // Ensure we have a valid object with all required fields
        const data = response.data || {};
        setCompanyDetails({
          ...data,
          bankDetails: data.bankDetails || [],
          // Ensure all string fields are strings, not undefined/null
          panNumber: data.panNumber || "",
          tanNumber: data.tanNumber || "",
          gstNumber: data.gstNumber || "",
          companyName: data.companyName || "",
          companyAddress: data.companyAddress || "",
          city: data.city || "",
          state: data.state || "",
          postalCode: data.postalCode || "",
          country: data.country || "",
          contactEmail: data.contactEmail || "",
          contactPhone: data.contactPhone || "",
          website: data.website || "",
          legalDisclaimer: data.legalDisclaimer || "",
          logoUrl: data.logoUrl || "",
        });
      }
    } catch (err: any) {
      console.error("Failed to load company details:", err);
      setError(err.message || "Failed to load company details");
      // Set empty data on error to prevent form issues
      setCompanyDetails({ bankDetails: [] });
    } finally {
      setLoading(false);
    }
  };

  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [pendingFormData, setPendingFormData] = useState<any>(null);

  const handleSubmit = async (formData: any) => {
    if (role !== "Admin") {
      setError("Only Admin can update company details");
      return;
    }

    // Show confirmation modal
    setPendingFormData(formData);
    setShowConfirmModal(true);
  };

  const confirmSubmit = async () => {
    if (!pendingFormData) return;

    setShowConfirmModal(false);
    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      const response = await CompanyDetailsApi.update(pendingFormData);
      if (response.success) {
        setSuccess("Company details updated successfully!");
        setCompanyDetails(response.data);
        setTimeout(() => setSuccess(null), 5000);
      } else {
        setError("Failed to update company details");
      }
    } catch (err: any) {
      console.error("Failed to update company details:", err);
      setError(err.message || "Failed to update company details");
    } finally {
      setSaving(false);
      setPendingFormData(null);
    }
  };

  const handleLogoUpload = async (file: File) => {
    setUploadingLogo(true);
    setError(null);
    setSuccess(null);

    try {
      const response = await CompanyDetailsApi.uploadLogo(file);
      if (response.success) {
        setSuccess("Logo uploaded successfully!");
        // Reload company details to get updated logo URL
        await loadCompanyDetails();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError("Failed to upload logo");
      }
    } catch (err: any) {
      console.error("Failed to upload logo:", err);
      setError(err.message || "Failed to upload logo");
    } finally {
      setUploadingLogo(false);
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <div className="text-center py-12">Loading company details...</div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb
        items={[
          { label: "Admin", path: "/admin" },
          { label: "Company Details", path: "/admin/company-details" },
        ]}
      />

      <div className="mb-6">
        <Link
          href="/admin"
          className="text-sm text-brand-600 hover:text-brand-700 mb-4 inline-block"
        >
          ← Back to Admin Console
        </Link>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
          Company Details
        </h1>
        <p className="text-gray-600 dark:text-gray-400">
          Configure company information including tax identification numbers, banking details, and branding
        </p>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg text-red-700 dark:text-red-400">
          {error}
        </div>
      )}

      {success && (
        <div className="mb-4 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg text-green-700 dark:text-green-400 animate-fade-in">
          {success}
        </div>
      )}

      {/* Confirmation Modal */}
      {showConfirmModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Confirm Changes
            </h3>
            <p className="text-gray-600 dark:text-gray-400 mb-6">
              Are you sure you want to save these changes to company details? This will update all future quotations.
            </p>
            <div className="flex justify-end gap-4">
              <button
                onClick={() => {
                  setShowConfirmModal(false);
                  setPendingFormData(null);
                }}
                className="px-4 py-2 text-gray-700 dark:text-gray-300 bg-gray-200 dark:bg-gray-700 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-gray-500"
              >
                Cancel
              </button>
              <button
                onClick={confirmSubmit}
                className="px-4 py-2 bg-brand-600 hover:bg-brand-700 text-white rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500"
              >
                Confirm Save
              </button>
            </div>
          </div>
        </div>
      )}

      <ComponentCard>
        <CompanyDetailsForm
          initialData={companyDetails || { bankDetails: [] }}
          onSubmit={handleSubmit}
          onLogoUpload={handleLogoUpload}
          saving={saving}
          uploadingLogo={uploadingLogo}
        />
      </ComponentCard>
    </div>
  );
}

