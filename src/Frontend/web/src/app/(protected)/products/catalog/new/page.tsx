"use client";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import ProductForm from "@/components/products/ProductForm";

export default function CreateProductPage() {
  return (
    <div className="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
      <PageBreadcrumb pageTitle="Create Product" />
      <ComponentCard title="Create New Product">
        <ProductForm />
      </ComponentCard>
    </div>
  );
}

