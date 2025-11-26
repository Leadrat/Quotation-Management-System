"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import { TemplateErrorBoundary } from "@/components/templates";
import DocumentUploader from "@/components/templates/DocumentUploader";

export default function UploadTemplatePage() {
  const router = useRouter();

  useEffect(() => {
    const token = getAccessToken();
    const userRole = getRoleFromToken(token);

    if (userRole !== "Admin" && userRole !== "SalesRep") {
      router.replace("/dashboard");
    }
  }, [router]);

  return (
    <TemplateErrorBoundary>
      <DocumentUploader onSuccess={(templateId) => router.push(`/templates/${templateId}`)} onCancel={() => router.back()} />
    </TemplateErrorBoundary>
  );
}

