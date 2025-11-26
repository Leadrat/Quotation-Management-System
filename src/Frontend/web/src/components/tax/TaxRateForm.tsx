"use client";
import { useState, useEffect } from "react";
import { TaxRatesApi, JurisdictionsApi, TaxFrameworksApi, ProductServiceCategoriesApi } from "@/lib/api";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Select from "@/components/tailadmin/form/Select";

interface TaxRateFormProps {
  taxRate?: any;
  onSave: (data: any) => Promise<void>;
  onCancel: () => void;
}

export default function TaxRateForm({ taxRate, onSave, onCancel }: TaxRateFormProps) {
  const [form, setForm] = useState({
    jurisdictionId: "",
    taxFrameworkId: "",
    productServiceCategoryId: "",
    taxRate: 0,
    effectiveFrom: new Date().toISOString().split("T")[0],
    effectiveTo: "",
    isExempt: false,
    isZeroRated: false,
    description: "",
    taxComponents: [] as Array<{ component: string; rate: number }>,
  });
  const [jurisdictions, setJurisdictions] = useState<any[]>([]);
  const [frameworks, setFrameworks] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [newComponent, setNewComponent] = useState({ component: "", rate: 0 });

  useEffect(() => {
    if (taxRate) {
      setForm({
        jurisdictionId: taxRate.jurisdictionId || "",
        taxFrameworkId: taxRate.taxFrameworkId || "",
        productServiceCategoryId: taxRate.productServiceCategoryId || "",
        taxRate: taxRate.taxRate || 0,
        effectiveFrom: taxRate.effectiveFrom || new Date().toISOString().split("T")[0],
        effectiveTo: taxRate.effectiveTo || "",
        isExempt: taxRate.isExempt || false,
        isZeroRated: taxRate.isZeroRated || false,
        description: taxRate.description || "",
        taxComponents: taxRate.taxComponents || [],
      });
    }
    loadOptions();
  }, [taxRate]);

  async function loadOptions() {
    try {
      const [jurisdictionsRes, frameworksRes, categoriesRes] = await Promise.all([
        JurisdictionsApi.listByCountry("").catch(() => ({ data: [] })),
        TaxFrameworksApi.list().catch(() => ({ data: [] })),
        ProductServiceCategoriesApi.list().catch(() => ({ data: [] })),
      ]);
      setJurisdictions(Array.isArray(jurisdictionsRes.data) ? jurisdictionsRes.data : []);
      setFrameworks(Array.isArray(frameworksRes.data) ? frameworksRes.data : []);
      setCategories(Array.isArray(categoriesRes.data) ? categoriesRes.data : []);
    } catch (e) {
      console.error("Failed to load options", e);
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const payload = {
        ...form,
        jurisdictionId: form.jurisdictionId || null,
        productServiceCategoryId: form.productServiceCategoryId || null,
        effectiveTo: form.effectiveTo || null,
        description: form.description || null,
      };
      await onSave(payload);
    } catch (e: any) {
      setError(e.message || "Failed to save tax rate");
    } finally {
      setLoading(false);
    }
  }

  function addComponent() {
    if (newComponent.component && newComponent.rate > 0) {
      setForm({
        ...form,
        taxComponents: [...form.taxComponents, { ...newComponent }],
      });
      setNewComponent({ component: "", rate: 0 });
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
          <Label htmlFor="taxFrameworkId">Tax Framework *</Label>
          <Select
            id="taxFrameworkId"
            value={form.taxFrameworkId}
            onChange={(e) => setForm({ ...form, taxFrameworkId: e.target.value })}
            required
          >
            <option value="">Select Framework</option>
            {frameworks.map((f) => (
              <option key={f.taxFrameworkId} value={f.taxFrameworkId}>
                {f.frameworkName}
              </option>
            ))}
          </Select>
        </div>

        <div>
          <Label htmlFor="jurisdictionId">Jurisdiction</Label>
          <Select
            id="jurisdictionId"
            value={form.jurisdictionId}
            onChange={(e) => setForm({ ...form, jurisdictionId: e.target.value })}
          >
            <option value="">Country Default</option>
            {jurisdictions.map((j) => (
              <option key={j.jurisdictionId} value={j.jurisdictionId}>
                {j.jurisdictionName}
              </option>
            ))}
          </Select>
        </div>

        <div>
          <Label htmlFor="productServiceCategoryId">Product/Service Category</Label>
          <Select
            id="productServiceCategoryId"
            value={form.productServiceCategoryId}
            onChange={(e) => setForm({ ...form, productServiceCategoryId: e.target.value })}
          >
            <option value="">All Categories</option>
            {categories.map((c) => (
              <option key={c.categoryId} value={c.categoryId}>
                {c.categoryName}
              </option>
            ))}
          </Select>
        </div>

        <div>
          <Label htmlFor="taxRate">Tax Rate (%) *</Label>
          <Input
            id="taxRate"
            type="number"
            step="0.01"
            min="0"
            max="100"
            value={form.taxRate}
            onChange={(e) => setForm({ ...form, taxRate: parseFloat(e.target.value) || 0 })}
            required
          />
        </div>

        <div>
          <Label htmlFor="effectiveFrom">Effective From *</Label>
          <Input
            id="effectiveFrom"
            type="date"
            value={form.effectiveFrom}
            onChange={(e) => setForm({ ...form, effectiveFrom: e.target.value })}
            required
          />
        </div>

        <div>
          <Label htmlFor="effectiveTo">Effective To</Label>
          <Input
            id="effectiveTo"
            type="date"
            value={form.effectiveTo}
            onChange={(e) => setForm({ ...form, effectiveTo: e.target.value })}
          />
        </div>
      </div>

      <div className="space-y-2">
        <Label>Tax Components (Optional)</Label>
        <div className="flex gap-2">
          <Input
            type="text"
            placeholder="Component name (e.g., CGST)"
            value={newComponent.component}
            onChange={(e) => setNewComponent({ ...newComponent, component: e.target.value })}
            className="flex-1"
          />
          <Input
            type="number"
            step="0.01"
            placeholder="Rate %"
            value={newComponent.rate}
            onChange={(e) => setNewComponent({ ...newComponent, rate: parseFloat(e.target.value) || 0 })}
            className="w-24"
          />
          <Button type="button" onClick={addComponent} variant="outline">
            Add
          </Button>
        </div>
        {form.taxComponents.length > 0 && (
          <div className="mt-2 space-y-1">
            {form.taxComponents.map((comp, idx) => (
              <div key={idx} className="flex items-center justify-between p-2 bg-gray-50 rounded">
                <span className="text-sm">
                  {comp.component}: {comp.rate}%
                </span>
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
      </div>

      <div className="space-y-2">
        <Label htmlFor="description">Description</Label>
        <textarea
          id="description"
          value={form.description}
          onChange={(e) => setForm({ ...form, description: e.target.value })}
          className="w-full px-3 py-2 border rounded-md"
          rows={3}
        />
      </div>

      <div className="flex items-center gap-4">
        <label className="flex items-center">
          <input
            type="checkbox"
            checked={form.isExempt}
            onChange={(e) => setForm({ ...form, isExempt: e.target.checked })}
            className="mr-2"
          />
          Exempt
        </label>
        <label className="flex items-center">
          <input
            type="checkbox"
            checked={form.isZeroRated}
            onChange={(e) => setForm({ ...form, isZeroRated: e.target.checked })}
            className="mr-2"
          />
          Zero Rated
        </label>
      </div>

      <div className="flex gap-2 justify-end">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={loading}>
          {loading ? "Saving..." : taxRate ? "Update" : "Create"}
        </Button>
      </div>
    </form>
  );
}

