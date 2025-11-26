"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { ProductCategoriesApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import type { ProductCategory } from "@/types/products";

export default function ProductCategoriesPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [categories, setCategories] = useState<ProductCategory[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    categoryName: "",
    categoryCode: "",
    description: "",
    parentCategoryId: "",
    isActive: true,
  });
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    loadCategories();
  }, []);

  async function loadCategories() {
    setLoading(true);
    setError(null);
    try {
      const res = await ProductCategoriesApi.list({ isActive: true });
      setCategories(res.data || []);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setCategories([]);
        return;
      }
      setError(e.message || "Failed to load categories");
    } finally {
      setLoading(false);
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await ProductCategoriesApi.create({
        categoryName: formData.categoryName,
        categoryCode: formData.categoryCode.toUpperCase(),
        description: formData.description || undefined,
        parentCategoryId: formData.parentCategoryId || undefined,
        isActive: formData.isActive,
      });
      setShowForm(false);
      setFormData({
        categoryName: "",
        categoryCode: "",
        description: "",
        parentCategoryId: "",
        isActive: true,
      });
      loadCategories();
    } catch (e: any) {
      setError(e.message || "Failed to create category");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
      <PageBreadcrumb pageTitle="Product Categories" />
      <ComponentCard title="Product Categories">
        <div className="mb-4 flex justify-between">
          <Button onClick={() => setShowForm(!showForm)}>
            {showForm ? "Cancel" : "Add Category"}
          </Button>
        </div>

        {showForm && (
          <form onSubmit={handleSubmit} className="mb-6 space-y-4 rounded-lg border border-stroke bg-white p-4 dark:border-form-strokedark dark:bg-form-input">
            <h3 className="text-lg font-semibold">Create New Category</h3>
            {error && (
              <div className="rounded border border-red-500 bg-red-50 p-2 text-sm text-red-700 dark:bg-red-900/20 dark:text-red-400">
                {error}
              </div>
            )}
            <div>
              <Label htmlFor="categoryName" required>
                Category Name
              </Label>
              <Input
                id="categoryName"
                type="text"
                required
                value={formData.categoryName}
                onChange={(e) => setFormData({ ...formData, categoryName: e.target.value })}
                placeholder="e.g., Cloud Services"
              />
            </div>
            <div>
              <Label htmlFor="categoryCode" required>
                Category Code
              </Label>
              <Input
                id="categoryCode"
                type="text"
                required
                value={formData.categoryCode}
                onChange={(e) => setFormData({ ...formData, categoryCode: e.target.value.toUpperCase() })}
                placeholder="CLOUD"
                maxLength={50}
              />
              <p className="mt-1 text-xs text-gray-500">Uppercase letters, numbers, and underscores only</p>
            </div>
            <div>
              <Label htmlFor="description">Description</Label>
              <textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="h-24 w-full rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90"
                placeholder="Category description"
              />
            </div>
            <div>
              <Label htmlFor="parentCategoryId">Parent Category</Label>
              <select
                id="parentCategoryId"
                value={formData.parentCategoryId}
                onChange={(e) => setFormData({ ...formData, parentCategoryId: e.target.value })}
                className="h-11 w-full rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90"
              >
                <option value="">None (Top Level)</option>
                {categories.map((cat) => (
                  <option key={cat.categoryId} value={cat.categoryId}>
                    {cat.categoryName}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isActive"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
              />
              <Label htmlFor="isActive">Active</Label>
            </div>
            <div className="flex gap-4">
              <button
                type="submit"
                disabled={submitting}
                className="inline-flex items-center justify-center gap-2 rounded-lg bg-brand-500 px-5 py-3.5 text-sm font-medium text-white shadow-theme-xs transition hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {submitting ? "Creating..." : "Create Category"}
              </button>
              <Button type="button" variant="outline" onClick={() => setShowForm(false)}>
                Cancel
              </Button>
            </div>
          </form>
        )}

        {error && !showForm && (
          <div className="mb-4 rounded border border-red-500 bg-red-50 p-4 text-red-700 dark:bg-red-900/20 dark:text-red-400">
            {error}
          </div>
        )}

        {loading ? (
          <div className="py-8 text-center">Loading...</div>
        ) : categories.length === 0 ? (
          <div className="py-8 text-center text-gray-500">No categories found</div>
        ) : (
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableCell>Category Code</TableCell>
                  <TableCell>Category Name</TableCell>
                  <TableCell>Parent Category</TableCell>
                  <TableCell>Status</TableCell>
                </TableRow>
              </TableHeader>
              <TableBody>
                {categories.map((category) => (
                  <TableRow key={category.categoryId}>
                    <TableCell className="font-mono font-medium">{category.categoryCode}</TableCell>
                    <TableCell>{category.categoryName}</TableCell>
                    <TableCell>{category.parentCategoryName || "—"}</TableCell>
                    <TableCell>
                      <span
                        className={`rounded px-2 py-1 text-xs ${
                          category.isActive
                            ? "bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400"
                            : "bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400"
                        }`}
                      >
                        {category.isActive ? "Active" : "Inactive"}
                      </span>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </ComponentCard>
    </div>
  );
}

