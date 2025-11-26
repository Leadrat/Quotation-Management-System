"use client";
import { useState, useEffect } from "react";
import { CountriesApi, JurisdictionsApi } from "@/lib/api";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Select from "@/components/tailadmin/form/Select";

interface JurisdictionFormProps {
  jurisdiction?: any;
  countryId?: string;
  parentJurisdictionId?: string;
  onSave: (data: any) => Promise<void>;
  onCancel: () => void;
}

export default function JurisdictionForm({
  jurisdiction,
  countryId,
  parentJurisdictionId,
  onSave,
  onCancel,
}: JurisdictionFormProps) {
  const [form, setForm] = useState({
    countryId: jurisdiction?.countryId || countryId || "",
    parentJurisdictionId: jurisdiction?.parentJurisdictionId || parentJurisdictionId || "",
    jurisdictionName: jurisdiction?.jurisdictionName || "",
    jurisdictionCode: jurisdiction?.jurisdictionCode || "",
    jurisdictionType: jurisdiction?.jurisdictionType || "",
    isActive: jurisdiction?.isActive !== false,
  });
  const [countries, setCountries] = useState<any[]>([]);
  const [parentJurisdictions, setParentJurisdictions] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCountries();
    if (form.countryId) {
      loadParentJurisdictions();
    }
  }, [form.countryId]);

  async function loadCountries() {
    try {
      const res = await CountriesApi.list({ isActive: true });
      setCountries(Array.isArray(res.data) ? res.data : []);
    } catch (e) {
      console.error("Failed to load countries", e);
    }
  }

  async function loadParentJurisdictions() {
    if (!form.countryId) return;
    try {
      const res = await JurisdictionsApi.listByCountry(form.countryId);
      const allJurisdictions = Array.isArray(res.data) ? res.data : [];
      // Filter out self and descendants if editing
      const filtered = jurisdiction
        ? allJurisdictions.filter(j => j.jurisdictionId !== jurisdiction.jurisdictionId)
        : allJurisdictions;
      setParentJurisdictions(filtered);
    } catch (e) {
      console.error("Failed to load parent jurisdictions", e);
      setParentJurisdictions([]);
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const payload = {
        ...form,
        parentJurisdictionId: form.parentJurisdictionId || null,
        jurisdictionCode: form.jurisdictionCode || null,
        jurisdictionType: form.jurisdictionType || null,
      };
      await onSave(payload);
    } catch (e: any) {
      setError(e.message || "Failed to save jurisdiction");
    } finally {
      setLoading(false);
    }
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
            onChange={(e) => setForm({ ...form, countryId: e.target.value, parentJurisdictionId: "" })}
            required
            disabled={!!jurisdiction}
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
          <Label htmlFor="parentJurisdictionId">Parent Jurisdiction</Label>
          <Select
            id="parentJurisdictionId"
            value={form.parentJurisdictionId}
            onChange={(e) => setForm({ ...form, parentJurisdictionId: e.target.value })}
            disabled={!form.countryId}
          >
            <option value="">None (Root Level)</option>
            {parentJurisdictions.map((j) => (
              <option key={j.jurisdictionId} value={j.jurisdictionId}>
                {j.jurisdictionName}
                {j.jurisdictionCode && ` (${j.jurisdictionCode})`}
              </option>
            ))}
          </Select>
          <p className="text-xs text-gray-500 mt-1">
            Select a parent jurisdiction to create a hierarchical structure
          </p>
        </div>

        <div>
          <Label htmlFor="jurisdictionName">Jurisdiction Name *</Label>
          <Input
            id="jurisdictionName"
            type="text"
            value={form.jurisdictionName}
            onChange={(e) => setForm({ ...form, jurisdictionName: e.target.value })}
            required
            placeholder="e.g., Maharashtra, Dubai"
          />
        </div>

        <div>
          <Label htmlFor="jurisdictionCode">Jurisdiction Code</Label>
          <Input
            id="jurisdictionCode"
            type="text"
            value={form.jurisdictionCode}
            onChange={(e) => setForm({ ...form, jurisdictionCode: e.target.value.toUpperCase() })}
            placeholder="e.g., 27, DXB"
          />
        </div>

        <div>
          <Label htmlFor="jurisdictionType">Jurisdiction Type</Label>
          <Select
            id="jurisdictionType"
            value={form.jurisdictionType}
            onChange={(e) => setForm({ ...form, jurisdictionType: e.target.value })}
          >
            <option value="">Select Type</option>
            <option value="State">State</option>
            <option value="Province">Province</option>
            <option value="Emirate">Emirate</option>
            <option value="City">City</option>
            <option value="Region">Region</option>
          </Select>
        </div>
      </div>

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

      <div className="flex gap-2 justify-end">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={loading}>
          {loading ? "Saving..." : jurisdiction ? "Update" : "Create"}
        </Button>
      </div>
    </form>
  );
}

