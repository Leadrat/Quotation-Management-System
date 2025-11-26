"use client";
import { useEffect, useState } from "react";
import { useRouter, useParams } from "next/navigation";
import { ProductsApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import ProductForm from "@/components/products/ProductForm";
import ProductUsageStats from "@/components/products/ProductUsageStats";
import Button from "@/components/tailadmin/ui/button/Button";
import { useToast, ToastContainer } from "@/components/quotations/Toast";
import type { Product } from "@/types/products";

export default function EditProductPage() {
  const router = useRouter();
  const params = useParams();
  const toast = useToast();
  const productId = params.productId as string;
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [product, setProduct] = useState<Product | null>(null);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (!getAccessToken()) {
      router.push("/login");
      return;
    }

    loadProduct();
  }, [productId]);

  async function loadProduct() {
    if (!productId) return;
    setLoading(true);
    setError(null);
    try {
      const res = await ProductsApi.getById(productId);
      if (res.success && res.data) {
        // Parse JSONB fields
        const productData = res.data;
        const parsedProduct: any = {
          ...productData,
          billingCycleMultipliers: productData.billingCycleMultipliers
            ? JSON.parse(productData.billingCycleMultipliers as string)
            : undefined,
          addOnPricing: productData.addOnPricing
            ? JSON.parse(productData.addOnPricing as unknown as string)
            : undefined,
          customDevelopmentPricing: productData.customDevelopmentPricing
            ? JSON.parse(productData.customDevelopmentPricing as unknown as string)
            : undefined,
        };
        setProduct(parsedProduct);
      } else {
        setError("Product not found");
      }
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        router.push("/login");
        return;
      }
      setError(e.message || "Failed to load product");
    } finally {
      setLoading(false);
    }
  }

  const handleDelete = async () => {
    if (!confirm("Are you sure you want to delete this product? This action cannot be undone.")) {
      return;
    }

    setDeleting(true);
    try {
      await ProductsApi.delete(productId);
      toast.success("Product deleted successfully!");
      router.push("/products/catalog");
    } catch (e: any) {
      const errorMsg = e.message || "Failed to delete product";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setDeleting(false);
    }
  };

  if (loading) {
    return (
      <div className="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
        <div className="py-8 text-center">Loading...</div>
      </div>
    );
  }

  if (error && !product) {
    return (
      <div className="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
        <PageBreadcrumb pageTitle="Edit Product" />
        <ComponentCard title="Error">
          <div className="rounded border border-red-500 bg-red-50 p-4 text-red-700 dark:bg-red-900/20 dark:text-red-400">
            {error}
          </div>
          <div className="mt-4">
            <Button onClick={() => router.push("/products/catalog")}>Back to Catalog</Button>
          </div>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
      <PageBreadcrumb pageTitle="Edit Product" />
      <ComponentCard
        title={`Edit Product: ${product?.productName || ""}`}
      >
        <div className="flex justify-end mb-4">
          <Button variant="outline" onClick={handleDelete} disabled={deleting}>
            {deleting ? "Deleting..." : "Delete"}
          </Button>
        </div>
        {error && (
          <div className="mb-4 rounded border border-red-500 bg-red-50 p-4 text-red-700 dark:bg-red-900/20 dark:text-red-400">
            {error}
          </div>
        )}
        {product && <ProductForm productId={productId} initialData={product} />}
      </ComponentCard>
      
      {product && (
        <div className="mt-6">
          <ProductUsageStats productId={productId} />
        </div>
      )}
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
    </div>
  );
}

