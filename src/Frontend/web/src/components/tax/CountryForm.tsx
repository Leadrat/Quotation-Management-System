"use client";
import { useState } from "react";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Select from "@/components/tailadmin/form/Select";

interface CountryFormProps {
  country?: any;
  onSave: (data: any) => Promise<void>;
  onCancel: () => void;
}

export default function CountryForm({ country, onSave, onCancel }: CountryFormProps) {
  const [form, setForm] = useState({
    countryName: country?.countryName || "",
    countryCode: country?.countryCode || "",
    taxFrameworkType: country?.taxFrameworkType || "GST",
    defaultCurrency: country?.defaultCurrency || "",
    isActive: country?.isActive !== false,
    isDefault: country?.isDefault || false,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      await onSave(form);
    } catch (e: any) {
      setError(e.message || "Failed to save country");
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
          <Label htmlFor="countryName">Country Name *</Label>
          <Input
            id="countryName"
            type="text"
            value={form.countryName}
            onChange={(e) => setForm({ ...form, countryName: e.target.value })}
            required
            placeholder="e.g., India, United Arab Emirates"
          />
        </div>

        <div>
          <Label htmlFor="countryCode">Country Code (ISO 3166-1 alpha-2) *</Label>
          <Input
            id="countryCode"
            type="text"
            value={form.countryCode}
            onChange={(e) => setForm({ ...form, countryCode: e.target.value.toUpperCase().slice(0, 2) })}
            required
            maxLength={2}
            placeholder="e.g., IN, AE"
          />
          <p className="text-xs text-gray-500 mt-1">Must be 2 uppercase letters</p>
        </div>

        <div>
          <Label htmlFor="taxFrameworkType">Tax Framework Type *</Label>
          <Select
            id="taxFrameworkType"
            value={form.taxFrameworkType}
            onChange={(e) => setForm({ ...form, taxFrameworkType: e.target.value })}
            required
          >
            <option value="GST">GST (Goods and Services Tax)</option>
            <option value="VAT">VAT (Value Added Tax)</option>
          </Select>
        </div>

        <div>
          <Label htmlFor="defaultCurrency">Default Currency (ISO 4217) *</Label>
          <Input
            id="defaultCurrency"
            type="text"
            value={form.defaultCurrency}
            onChange={(e) => setForm({ ...form, defaultCurrency: e.target.value.toUpperCase().slice(0, 3) })}
            required
            maxLength={3}
            placeholder="e.g., INR, AED"
          />
          <p className="text-xs text-gray-500 mt-1">Must be 3 uppercase letters</p>
        </div>
      </div>

      <div className="space-y-2">
        <label className="flex items-center">
          <input
            type="checkbox"
            checked={form.isActive}
            onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
            className="mr-2"
          />
          Active
        </label>
        <label className="flex items-center">
          <input
            type="checkbox"
            checked={form.isDefault}
            onChange={(e) => setForm({ ...form, isDefault: e.target.checked })}
            className="mr-2"
          />
          Set as Default Country
        </label>
      </div>

      <div className="flex gap-2 justify-end">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={loading}>
          {loading ? "Saving..." : country ? "Update" : "Create"}
        </Button>
      </div>
    </form>
  );
}

