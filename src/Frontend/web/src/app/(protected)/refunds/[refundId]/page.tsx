"use client";

import { useRouter } from "next/navigation";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ArrowDownIcon } from "@/icons";
import Link from "next/link";

export default function RefundDetailPage() {
  const router = useRouter();

  return (
    <>
      <PageBreadcrumb pageTitle="Refund Details" />
      
      <ComponentCard>
        <div className="flex flex-col items-center justify-center py-16 px-4">
          <div className="mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-gray-100 dark:bg-gray-800">
            <ArrowDownIcon className="h-10 w-10 text-gray-400 dark:text-gray-500" />
          </div>
          <h2 className="mb-2 text-2xl font-semibold text-gray-800 dark:text-white/90">
            Refunds Coming Soon
          </h2>
          <p className="max-w-md text-center text-gray-500 dark:text-gray-400 mb-6">
            We're working on building a comprehensive refund management system. 
            This feature will be available soon.
          </p>
          <div className="flex gap-4">
            <button
              onClick={() => router.back()}
              className="text-blue-600 hover:text-blue-800 dark:text-blue-400"
            >
              ← Go Back
            </button>
            <Link
              href="/refunds"
              className="text-blue-600 hover:text-blue-800 dark:text-blue-400"
            >
              View All Refunds
            </Link>
          </div>
        </div>
      </ComponentCard>
    </>
  );
}

