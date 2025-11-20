"use client";

import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { DollarLineIcon } from "@/icons";

export default function PaymentsPage() {
  return (
    <>
      <PageBreadcrumb pageTitle="Payments" />
      
      <ComponentCard>
        <div className="flex flex-col items-center justify-center py-16 px-4">
          <div className="mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-gray-100 dark:bg-gray-800">
            <DollarLineIcon className="h-10 w-10 text-gray-400 dark:text-gray-500" />
          </div>
          <h2 className="mb-2 text-2xl font-semibold text-gray-800 dark:text-white/90">
            Payments Coming Soon
          </h2>
          <p className="max-w-md text-center text-gray-500 dark:text-gray-400">
            We're working on building a comprehensive payment management system. 
            This feature will be available soon.
          </p>
        </div>
      </ComponentCard>
    </>
  );
}

