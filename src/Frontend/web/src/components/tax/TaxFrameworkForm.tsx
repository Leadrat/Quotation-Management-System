"use client";
import { useState, useEffect } from "react";
import { CountriesApi } from "@/lib/api";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Select from "@/components/tailadmin/form/Select";

interface TaxFrameworkFormProps {
  taxFramework?: any;
  countryId?: string;
  onSave: (data: any) => Promise<void>;
  onCancel: () => void;
}

export default function TaxFrameworkForm({
  taxFramework,
  countryId,
  onSave,
  onCancel,
}: TaxFrameworkFormProps) {
  const [form, setForm] = useState({
    countryId: taxFramework?.countryId || countryId || "",
    frameworkName: taxFramework?.frameworkName || "",
    frameworkType: taxFramework?.frameworkType || "GST",
    description: taxFramework?.description || "",
    taxComponents: taxFramework?.taxComponents || [] as Array<{ name: string; code: string; description?: string }>,
  });
  const [countries, setCountries] = useState<any[]>([]);
  const [newComponent, setNewComponent] = useState({ name: "", code: "", description: "" });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCountries();
  }, []);

  async function loadCountries() {
    try {
      const res = await CountriesApi.list({ isActive: true });
      setCountries(Array.isArray(res.data) ? res.data : []);
    } catch (e) {
      console.error("Failed to load countries", e);
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (form.taxComponents.length === 0) {
      setError("At least one tax component is required");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await onSave(form);
    } catch (e: any) {
      setError(e.message || "Failed to save tax framework");
    } finally {
      setLoading(false);
    }
  }

  function addComponent() {
    if (newComponent.name && newComponent.code) {
      setForm({
        ...form,
        taxComponents: [...form.taxComponents, { ...newComponent }],
      });
      setNewComponent({ name: "", code: "", description: "" });
    }
  }

  function removeComponent(index: number) {
    setForm({
      ...form,
      taxComponents: form.taxComponents.filter((_, i) => i !== index),
    });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="countryId">Country *</Label>
          <Select
            id="countryId"
            value={form.countryId}
            onChange={(e) => setForm({ ...form, countryId: e.target.value })}
            required
            disabled={!!taxFramework}
          >
            <option value="">Select Country</option>
            {countries.map((c) => (
              <option key={c.countryId} value={c.countryId}>
                {c.countryName} ({c.countryCode})
              </option>
            ))}
          </Select>
        </div>

        <div>
          <Label htmlFor="frameworkName">Framework Name *</Label>
          <Input
            id="frameworkName"
            type="text"
            value={form.frameworkName}
            onChange={(e) => setForm({ ...form, frameworkName: e.target.value })}
            required
            placeholder="e.g., Goods and Services Tax"
          />
        </div>

        <div>
          <Label htmlFor="frameworkType">Framework Type *</Label>
          <Select
            id="frameworkType"
            value={form.frameworkType}
            onChange={(e) => setForm({ ...form, frameworkType: e.target.value })}
            required
          >
            <option value="GST">GST</option>
            <option value="VAT">VAT</option>
          </Select>
        </div>
      </div>

      <div>
        <Label htmlFor="description">Description</Label>
        <textarea
          id="description"
          value={form.description}
          onChange={(e) => setForm({ ...form, description: e.target.value })}
          className="w-full px-3 py-2 border rounded-md"
          rows={3}
        />
      </div>

      <div className="space-y-2">
        <Label>Tax Components *</Label>
        <div className="flex gap-2">
          <Input
            type="text"
            placeholder="Component name (e.g., CGST)"
            value={newComponent.name}
            onChange={(e) => setNewComponent({ ...newComponent, name: e.target.value })}
            className="flex-1"
          />
          <Input
            type="text"
            placeholder="Code (e.g., CGST)"
            value={newComponent.code}
            onChange={(e) => setNewComponent({ ...newComponent, code: e.target.value })}
            className="w-32"
          />
          <Input
            type="text"
            placeholder="Description (optional)"
            value={newComponent.description}
            onChange={(e) => setNewComponent({ ...newComponent, description: e.target.value })}
            className="flex-1"
          />
          <Button type="button" onClick={addComponent} variant="outline">
            Add
          </Button>
        </div>
        {form.taxComponents.length > 0 && (
          <div className="mt-2 space-y-1">
            {form.taxComponents.map((comp, idx) => (
              <div key={idx} className="flex items-center justify-between p-2 bg-gray-50 rounded">
                <div className="text-sm">
                  <span className="font-medium">{comp.name}</span>
                  {comp.code && <span className="text-gray-500 ml-2">({comp.code})</span>}
                  {comp.description && <span className="text-gray-500 ml-2">- {comp.description}</span>}
                </div>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => removeComponent(idx)}
                >
                  Remove
                </Button>
              </div>
            ))}
          </div>
        )}
        {form.taxComponents.length === 0 && (
          <p className="text-xs text-gray-500">Add at least one tax component (e.g., CGST, SGST for GST)</p>
        )}
      </div>

      <div className="flex gap-2 justify-end">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={loading || form.taxComponents.length === 0}>
          {loading ? "Saving..." : taxFramework ? "Update" : "Create"}
        </Button>
      </div>
    </form>
  );
}

