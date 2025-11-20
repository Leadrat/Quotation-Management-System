"use client";
import { useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import CategoryTaxRulesTable from "@/components/tax/CategoryTaxRulesTable";
import { ProductServiceCategoriesApi } from "@/lib/api";
import Button from "@/components/tailadmin/ui/button/Button";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import { useToast } from "@/components/quotations/Toast";

export default function CategoriesPage() {
  const [showForm, setShowForm] = useState(false);
  const [editingCategory, setEditingCategory] = useState<any>(null);
  const [form, setForm] = useState({
    categoryName: "",
    categoryCode: "",
    description: "",
    isActive: true,
  });
  const [loading, setLoading] = useState(false);
  const toast = useToast();

  const handleCreate = () => {
    setEditingCategory(null);
    setForm({
      categoryName: "",
      categoryCode: "",
      description: "",
      isActive: true,
    });
    setShowForm(true);
  };

  const handleEdit = (category: any) => {
    setEditingCategory(category);
    setForm({
      categoryName: category.categoryName || "",
      categoryCode: category.categoryCode || "",
      description: category.description || "",
      isActive: category.isActive !== false,
    });
    setShowForm(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (editingCategory) {
        await ProductServiceCategoriesApi.update(editingCategory.categoryId, form);
        toast.success("Category updated successfully");
      } else {
        await ProductServiceCategoriesApi.create(form);
        toast.success("Category created successfully");
      }
      setShowForm(false);
      setEditingCategory(null);
      // Reload will happen automatically via useEffect in CategoryTaxRulesTable
      window.location.reload(); // Simple reload for now
    } catch (e: any) {
      toast.error(e.message || "Failed to save category");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageBreadcrumb pageName="Product/Service Categories" />

      <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
        <div className="border-b border-stroke px-6.5 py-4 dark:border-strokedark">
          <div className="flex justify-between items-center">
            <div>
              <h3 className="font-medium text-black dark:text-white">
                Product/Service Category Management
              </h3>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Manage product and service categories for tax rate configuration
              </p>
            </div>
            {!showForm && (
              <Button onClick={handleCreate}>Create Category</Button>
            )}
          </div>
        </div>

        <div className="p-6.5">
          {!showForm ? (
            <CategoryTaxRulesTable onEditCategory={handleEdit} />
          ) : (
            <div className="bg-white dark:bg-gray-800 rounded-lg border p-6">
              <h4 className="text-lg font-semibold mb-4">
                {editingCategory ? "Edit Category" : "Create Category"}
              </h4>
              <form onSubmit={handleSave} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="categoryName">Category Name *</Label>
                    <Input
                      id="categoryName"
                      type="text"
                      value={form.categoryName}
                      onChange={(e) => setForm({ ...form, categoryName: e.target.value })}
                      required
                    />
                  </div>
                  <div>
                    <Label htmlFor="categoryCode">Category Code</Label>
                    <Input
                      id="categoryCode"
                      type="text"
                      value={form.categoryCode}
                      onChange={(e) => setForm({ ...form, categoryCode: e.target.value })}
                    />
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
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => {
                      setShowForm(false);
                      setEditingCategory(null);
                    }}
                  >
                    Cancel
                  </Button>
                  <Button type="submit" disabled={loading}>
                    {loading ? "Saving..." : editingCategory ? "Update" : "Create"}
                  </Button>
                </div>
              </form>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

