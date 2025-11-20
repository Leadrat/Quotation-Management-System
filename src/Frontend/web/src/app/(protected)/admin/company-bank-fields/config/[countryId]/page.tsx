"use client";
import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { CountryBankFieldConfigurationsApi } from "@/lib/api/countryBankFieldConfigurations";
import { BankFieldTypesApi } from "@/lib/api/bankFieldTypes";
import { CountriesApi } from "@/lib/api";

export default function CountryBankFieldConfigPage() {
  const params = useParams();
  const countryId = String(params?.countryId || "");
  const [country, setCountry] = useState<any>(null);
  const [configurations, setConfigurations] = useState<any[]>([]);
  const [bankFieldTypes, setBankFieldTypes] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [form, setForm] = useState({
    bankFieldTypeId: "",
    isRequired: false,
    validationRegex: "",
    minLength: "",
    maxLength: "",
    displayName: "",
    helpText: "",
    displayOrder: 0,
    isActive: true,
  });

  useEffect(() => {
    if (countryId) {
      loadData();
    }
  }, [countryId]);

  const loadData = async () => {
    setLoading(true);
    setError(null);
    try {
      const [countryRes, configsRes, typesRes] = await Promise.all([
        CountriesApi.getById(countryId).catch(() => ({ data: { countryName: countryId } })),
        CountryBankFieldConfigurationsApi.getByCountry(countryId, true),
        BankFieldTypesApi.list(true),
      ]);
      setCountry(countryRes.data || { countryName: countryId });
      setConfigurations(configsRes.data || []);
      setBankFieldTypes(typesRes.data || []);
    } catch (e: any) {
      setError(e.message || "Failed to load data");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      if (editing) {
        await CountryBankFieldConfigurationsApi.update(editing.configurationId, {
          isRequired: form.isRequired,
          validationRegex: form.validationRegex || undefined,
          minLength: form.minLength ? parseInt(form.minLength) : undefined,
          maxLength: form.maxLength ? parseInt(form.maxLength) : undefined,
          displayName: form.displayName || undefined,
          helpText: form.helpText || undefined,
          displayOrder: form.displayOrder,
          isActive: form.isActive,
        });
      } else {
        await CountryBankFieldConfigurationsApi.configure({
          countryId,
          bankFieldTypeId: form.bankFieldTypeId,
          isRequired: form.isRequired,
          validationRegex: form.validationRegex || undefined,
          minLength: form.minLength ? parseInt(form.minLength) : undefined,
          maxLength: form.maxLength ? parseInt(form.maxLength) : undefined,
          displayName: form.displayName || undefined,
          helpText: form.helpText || undefined,
          displayOrder: form.displayOrder,
        });
      }
      setShowForm(false);
      setEditing(null);
      resetForm();
      loadData();
    } catch (e: any) {
      setError(e.message || "Failed to save configuration");
    }
  };

  const resetForm = () => {
    setForm({
      bankFieldTypeId: "",
      isRequired: false,
      validationRegex: "",
      minLength: "",
      maxLength: "",
      displayName: "",
      helpText: "",
      displayOrder: 0,
      isActive: true,
    });
  };

  const handleEdit = (config: any) => {
    setEditing(config);
    setForm({
      bankFieldTypeId: config.bankFieldTypeId,
      isRequired: config.isRequired,
      validationRegex: config.validationRegex || "",
      minLength: config.minLength?.toString() || "",
      maxLength: config.maxLength?.toString() || "",
      displayName: config.displayName || "",
      helpText: config.helpText || "",
      displayOrder: config.displayOrder,
      isActive: config.isActive,
    });
    setShowForm(true);
  };

  if (loading) return <div className="p-6">Loading...</div>;

  const availableTypes = bankFieldTypes.filter(
    (type) =>
      !configurations.some(
        (c) => c.bankFieldTypeId === type.bankFieldTypeId && (!editing || c.configurationId !== editing.configurationId)
      )
  );

  return (
    <div className="p-6">
      <div className="mb-6">
        <Link href="/admin/company-bank-fields" className="text-sm text-blue-600 hover:text-blue-700 mb-4 inline-block">
          ← Back to Bank Field Types
        </Link>
        <h1 className="text-3xl font-bold mb-2">
          Country Bank Field Configuration: {country?.countryName || countryId}
        </h1>
        <p className="text-gray-600">Configure which bank fields are required for this country</p>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded text-red-700">
          {error}
        </div>
      )}

      {showForm && (
        <div className="mb-6 bg-white rounded border p-6">
          <h2 className="text-xl font-semibold mb-4">
            {editing ? "Edit" : "Add"} Configuration
          </h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            {!editing && (
              <div>
                <label className="block text-sm font-medium mb-1">Bank Field Type</label>
                <select
                  value={form.bankFieldTypeId}
                  onChange={(e) => setForm({ ...form, bankFieldTypeId: e.target.value })}
                  className="w-full px-3 py-2 border rounded"
                  required
                >
                  <option value="">Select bank field type</option>
                  {availableTypes.map((type) => (
                    <option key={type.bankFieldTypeId} value={type.bankFieldTypeId}>
                      {type.displayName} ({type.name})
                    </option>
                  ))}
                </select>
              </div>
            )}
            <div>
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={form.isRequired}
                  onChange={(e) => setForm({ ...form, isRequired: e.target.checked })}
                  className="mr-2"
                />
                Required
              </label>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-1">Min Length</label>
                <input
                  type="number"
                  value={form.minLength}
                  onChange={(e) => setForm({ ...form, minLength: e.target.value })}
                  className="w-full px-3 py-2 border rounded"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Max Length</label>
                <input
                  type="number"
                  value={form.maxLength}
                  onChange={(e) => setForm({ ...form, maxLength: e.target.value })}
                  className="w-full px-3 py-2 border rounded"
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Validation Regex</label>
              <input
                type="text"
                value={form.validationRegex}
                onChange={(e) => setForm({ ...form, validationRegex: e.target.value })}
                className="w-full px-3 py-2 border rounded"
                placeholder="e.g., ^[A-Z0-9]{11}$"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Display Name (Override)</label>
              <input
                type="text"
                value={form.displayName}
                onChange={(e) => setForm({ ...form, displayName: e.target.value })}
                className="w-full px-3 py-2 border rounded"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Help Text</label>
              <textarea
                value={form.helpText}
                onChange={(e) => setForm({ ...form, helpText: e.target.value })}
                className="w-full px-3 py-2 border rounded"
                rows={2}
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Display Order</label>
              <input
                type="number"
                value={form.displayOrder}
                onChange={(e) => setForm({ ...form, displayOrder: parseInt(e.target.value) || 0 })}
                className="w-full px-3 py-2 border rounded"
              />
            </div>
            {editing && (
              <div>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={form.isActive}
                    onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                    className="mr-2"
                  />
                  Active
                </label>
              </div>
            )}
            <div className="flex space-x-2">
              <button
                type="submit"
                className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
              >
                {editing ? "Update" : "Create"}
              </button>
              <button
                type="button"
                onClick={() => {
                  setShowForm(false);
                  setEditing(null);
                  resetForm();
                }}
                className="px-4 py-2 border rounded hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {!showForm && (
        <div className="mb-4">
          <button
            onClick={() => setShowForm(true)}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            disabled={availableTypes.length === 0}
          >
            Add Configuration
          </button>
          {availableTypes.length === 0 && (
            <p className="text-sm text-gray-500 mt-2">All bank field types are already configured</p>
          )}
        </div>
      )}

      <div className="bg-white rounded border overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium">Bank Field Type</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Required</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Validation</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Order</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Status</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {configurations
              .sort((a, b) => a.displayOrder - b.displayOrder)
              .map((config) => (
                <tr key={config.configurationId}>
                  <td className="px-4 py-3 text-sm">
                    {config.displayName || config.bankFieldTypeDisplayName || config.bankFieldTypeName}
                  </td>
                  <td className="px-4 py-3 text-sm">
                    {config.isRequired ? (
                      <span className="text-red-600 font-medium">Required</span>
                    ) : (
                      <span className="text-gray-500">Optional</span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">
                    {config.validationRegex || config.minLength || config.maxLength
                      ? `${config.minLength || ""}-${config.maxLength || ""} ${config.validationRegex ? "Regex" : ""}`
                      : "-"}
                  </td>
                  <td className="px-4 py-3 text-sm">{config.displayOrder}</td>
                  <td className="px-4 py-3 text-sm">
                    <span
                      className={`px-2 py-1 rounded text-xs ${
                        config.isActive ? "bg-green-100 text-green-800" : "bg-gray-100 text-gray-800"
                      }`}
                    >
                      {config.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm">
                    <button
                      onClick={() => handleEdit(config)}
                      className="text-blue-600 hover:text-blue-800"
                    >
                      Edit
                    </button>
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
        {configurations.length === 0 && (
          <div className="p-8 text-center text-gray-500">No configurations found</div>
        )}
      </div>
    </div>
  );
}

