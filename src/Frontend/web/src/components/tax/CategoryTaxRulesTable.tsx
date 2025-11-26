"use client";
import { useState, useEffect } from "react";
import { ProductServiceCategoriesApi, TaxRatesApi } from "@/lib/api";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Badge from "@/components/tailadmin/ui/badge/Badge";
import Button from "@/components/tailadmin/ui/button/Button";

interface CategoryTaxRulesTableProps {
  onEditCategory?: (category: any) => void;
}

export default function CategoryTaxRulesTable({ onEditCategory }: CategoryTaxRulesTableProps) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [categories, setCategories] = useState<any[]>([]);
  const [taxRatesByCategory, setTaxRatesByCategory] = useState<Record<string, any[]>>({});

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    setLoading(true);
    setError(null);
    try {
      const categoriesRes = await ProductServiceCategoriesApi.list();
      const categoriesList = Array.isArray(categoriesRes.data) ? categoriesRes.data : [];
      setCategories(categoriesList);

      // Load tax rates for each category
      const ratesMap: Record<string, any[]> = {};
      for (const category of categoriesList) {
        try {
          const ratesRes = await TaxRatesApi.list({
            productServiceCategoryId: category.categoryId,
          });
          ratesMap[category.categoryId] = Array.isArray(ratesRes.data) ? ratesRes.data : [];
        } catch (e) {
          ratesMap[category.categoryId] = [];
        }
      }
      setTaxRatesByCategory(ratesMap);
    } catch (e: any) {
      const errorMsg = e.message || "Failed to load categories";
      setError(errorMsg);
      setCategories([]);
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
        <p className="mt-2 text-gray-600">Loading categories...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
        {error}
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableCell>Category Name</TableCell>
            <TableCell>Category Code</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Tax Rates</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {categories.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} className="text-center py-8 text-gray-500">
                No categories found
              </TableCell>
            </TableRow>
          ) : (
            categories.map((category) => {
              const rates = taxRatesByCategory[category.categoryId] || [];
              return (
                <TableRow key={category.categoryId}>
                  <TableCell className="font-medium">{category.categoryName}</TableCell>
                  <TableCell>{category.categoryCode || "-"}</TableCell>
                  <TableCell>
                    {category.isActive ? (
                      <Badge className="bg-green-100 text-green-800">Active</Badge>
                    ) : (
                      <Badge className="bg-gray-100 text-gray-800">Inactive</Badge>
                    )}
                  </TableCell>
                  <TableCell>
                    {rates.length > 0 ? (
                      <div className="flex flex-wrap gap-1">
                        {rates.slice(0, 3).map((rate: any) => (
                          <Badge key={rate.taxRateId} className="bg-blue-100 text-blue-800 text-xs">
                            {rate.jurisdictionName || "Default"}: {rate.taxRate}%
                          </Badge>
                        ))}
                        {rates.length > 3 && (
                          <Badge className="bg-gray-100 text-gray-800 text-xs">
                            +{rates.length - 3} more
                          </Badge>
                        )}
                      </div>
                    ) : (
                      <span className="text-gray-400 text-sm">No rates configured</span>
                    )}
                  </TableCell>
                  <TableCell>
                    {onEditCategory && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onEditCategory(category)}
                      >
                        Edit
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              );
            })
          )}
        </TableBody>
      </Table>
    </div>
  );
}

