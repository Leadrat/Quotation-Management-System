"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { ProductsApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Input from "@/components/tailadmin/form/input/InputField";
import Button from "@/components/tailadmin/ui/button/Button";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Pagination from "@/components/tailadmin/tables/Pagination";
import { useToast, ToastContainer } from "@/components/quotations/Toast";
import { ProductErrorBoundary } from "@/components/products/ErrorBoundary";
import type { Product, ProductType } from "@/types/products";

export default function ProductCatalogPage() {
  const toast = useToast();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [products, setProducts] = useState<Product[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState("");
  const [productTypeFilter, setProductTypeFilter] = useState<ProductType | "">("");
  const [isActiveFilter, setIsActiveFilter] = useState<boolean | null>(null);

  async function load(p = 1) {
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const res = await ProductsApi.list({
        pageNumber: p,
        pageSize,
        productType: productTypeFilter || undefined,
        isActive: isActiveFilter !== null ? isActiveFilter : undefined,
        search: search || undefined,
      });
      setProducts(res.data || []);
      setPageNumber(res.pageNumber);
      setPageSize(res.pageSize);
      setTotal(res.totalCount);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setProducts([]);
        setTotal(0);
        return;
      }
      setError(e.message || "Failed to load products");
      toast.error(e.message || "Failed to load products");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load(pageNumber);
  }, [pageNumber, pageSize, productTypeFilter, isActiveFilter, search]);

  const handleSearch = () => {
    setPageNumber(1);
    load(1);
  };

  return (
    <ProductErrorBoundary>
      <div className="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
        <PageBreadcrumb pageName="Product Catalog" />
        <ComponentCard title="Product Catalog">
        <div className="mb-4 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div className="flex flex-col gap-4 md:flex-row md:items-center">
            <Input
              type="text"
              placeholder="Search products..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyPress={(e) => e.key === "Enter" && handleSearch()}
              className="w-full md:w-64"
            />
            <select
              value={productTypeFilter}
              onChange={(e) => setProductTypeFilter(e.target.value as ProductType | "")}
              className="rounded border border-stroke bg-white px-3 py-2 text-black focus:border-primary focus-visible:outline-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
            >
              <option value="">All Types</option>
              <option value="Subscription">Subscription</option>
              <option value="AddOnSubscription">Add-On (Subscription)</option>
              <option value="AddOnOneTime">Add-On (One-Time)</option>
              <option value="CustomDevelopment">Custom Development</option>
            </select>
            <select
              value={isActiveFilter === null ? "" : isActiveFilter.toString()}
              onChange={(e) => setIsActiveFilter(e.target.value === "" ? null : e.target.value === "true")}
              className="rounded border border-stroke bg-white px-3 py-2 text-black focus:border-primary focus-visible:outline-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
            >
              <option value="">All Status</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
            <Button onClick={handleSearch}>Search</Button>
          </div>
          <Link href="/products/catalog/new">
            <Button>Add Product</Button>
          </Link>
        </div>

        {error && (
          <div className="mb-4 rounded border border-red-500 bg-red-50 p-4 text-red-700 dark:bg-red-900/20 dark:text-red-400">
            {error}
          </div>
        )}

        {loading ? (
          <div className="py-8 text-center">Loading...</div>
        ) : products.length === 0 ? (
          <div className="py-8 text-center text-gray-500">No products found</div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableCell>Product Name</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell>Price</TableCell>
                    <TableCell>Currency</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {products.map((product) => (
                    <TableRow key={product.productId}>
                      <TableCell className="font-medium">{product.productName}</TableCell>
                      <TableCell>{product.productType}</TableCell>
                      <TableCell>{product.categoryName || "Uncategorized"}</TableCell>
                      <TableCell>
                        {product.basePricePerUserPerMonth
                          ? `${product.basePricePerUserPerMonth.toFixed(2)}/user/month`
                          : "N/A"}
                      </TableCell>
                      <TableCell>{product.currency}</TableCell>
                      <TableCell>
                        <span
                          className={`rounded px-2 py-1 text-xs ${
                            product.isActive
                              ? "bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400"
                              : "bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400"
                          }`}
                        >
                          {product.isActive ? "Active" : "Inactive"}
                        </span>
                      </TableCell>
                      <TableCell>
                        <Link href={`/products/catalog/${product.productId}`}>
                          <Button variant="outline" size="sm">
                            View
                          </Button>
                        </Link>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
            <Pagination
              currentPage={pageNumber}
              totalPages={Math.ceil(total / pageSize)}
              onPageChange={(page) => setPageNumber(page)}
            />
          </>
        )}
        </ComponentCard>
        <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      </div>
    </ProductErrorBoundary>
  );
}

