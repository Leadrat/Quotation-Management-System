"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { TemplatesApi } from "@/lib/api";
import { TemplateErrorBoundary } from "@/components/templates";
import { useToast, ToastContainer } from "@/components/quotations/Toast";
import { useDropzone } from "react-dropzone";

export default function UploadTemplatePage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [visibility, setVisibility] = useState<"Public" | "Team" | "Private">("Private");
  const [templateType, setTemplateType] = useState<"Quotation" | "ProFormaInvoice">("Quotation");
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const toast = useToast();

  const onDrop = (acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      const file = acceptedFiles[0];
      // Validate file type
      const allowedTypes = [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/html",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      ];
      
      if (!allowedTypes.includes(file.type)) {
        setError("Please upload a valid template file (PDF, Word, Excel, or HTML)");
        return;
      }
      
      // Validate file size (max 10MB)
      if (file.size > 10 * 1024 * 1024) {
        setError("File size must be less than 10MB");
        return;
      }
      
      setUploadedFile(file);
      setError(null);
      
      // Auto-fill name if not set
      if (!name) {
        const fileName = file.name.replace(/\.[^/.]+$/, "");
        setName(fileName);
      }
    }
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      "application/pdf": [".pdf"],
      "application/msword": [".doc"],
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [".docx"],
      "text/html": [".html"],
      "application/vnd.ms-excel": [".xls"],
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": [".xlsx"],
    },
    maxFiles: 1,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!name.trim()) {
      setError("Template name is required");
      return;
    }
    
    if (!uploadedFile) {
      setError("Please upload a template file");
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      // Create FormData for file upload
      const formData = new FormData();
      formData.append("file", uploadedFile);
      formData.append("name", name.trim());
      if (description.trim()) {
        formData.append("description", description.trim());
      }
      formData.append("visibility", visibility);
      formData.append("templateType", templateType);

      toast.info("Uploading template...");
      
      // Call upload API
      const result = await TemplatesApi.upload(formData);
      
      toast.success("Template uploaded successfully!");
      router.push(`/templates/${result.data.templateId}`);
    } catch (err: any) {
      const errorMsg = err.message || "Failed to upload template";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const removeFile = () => {
    setUploadedFile(null);
  };

  return (
    <TemplateErrorBoundary>
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
        <div className="mb-6">
          <h4 className="text-title-md2 font-bold text-black dark:text-white">Upload Template</h4>
          <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
            Upload a template file (PDF, Word, Excel, or HTML) with placeholders for quotation or pro forma invoice generation.
            Supported placeholders: {"{"}CustomerName{"}"}, {"{"}CompanyName{"}"}, {"{"}NumberOfUsers{"}"}, {"{"}SubscriptionTenure{"}"}, {"{"}NetPrice{"}"}, {"{"}DiscountPercentage{"}"}, {"{"}GST{"}"}, {"{"}GrossAmount{"}"}, {"{"}QuotationDate{"}"}
          </p>
        </div>

        {error && (
          <div className="mb-4 rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
            <p className="text-red-700 dark:text-red-400">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Basic Info */}
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div className="md:col-span-2">
              <label className="mb-2.5 block text-black dark:text-white">Template Name *</label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                maxLength={100}
                placeholder="e.g., Standard Quotation Template"
                className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
              />
            </div>
            <div className="md:col-span-2">
              <label className="mb-2.5 block text-black dark:text-white">Description</label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                maxLength={255}
                rows={3}
                placeholder="Brief description of this template..."
                className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
              />
            </div>
            <div>
              <label className="mb-2.5 block text-black dark:text-white">Template Type *</label>
              <select
                value={templateType}
                onChange={(e) => setTemplateType(e.target.value as "Quotation" | "ProFormaInvoice")}
                required
                className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
              >
                <option value="Quotation">Quotation</option>
                <option value="ProFormaInvoice">Pro Forma Invoice</option>
              </select>
            </div>
            <div>
              <label className="mb-2.5 block text-black dark:text-white">Visibility *</label>
              <select
                value={visibility}
                onChange={(e) => setVisibility(e.target.value as "Public" | "Team" | "Private")}
                required
                className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
              >
                <option value="Private">Private (Only Me)</option>
                <option value="Team">Team (Same Role)</option>
                <option value="Public">Public (All Users - Requires Approval)</option>
              </select>
            </div>
          </div>

          {/* File Upload */}
          <div>
            <label className="mb-2.5 block text-black dark:text-white">Template File *</label>
            {!uploadedFile ? (
              <div
                {...getRootProps()}
                className={`rounded-xl border-2 border-dashed p-8 transition cursor-pointer ${
                  isDragActive
                    ? "border-blue-500 bg-blue-50 dark:bg-blue-900/20"
                    : "border-gray-300 bg-gray-50 dark:border-gray-700 dark:bg-gray-900"
                }`}
              >
                <input {...getInputProps()} />
                <div className="flex flex-col items-center text-center">
                  <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-200 text-gray-700 dark:bg-gray-800 dark:text-gray-400">
                    <svg
                      className="fill-current"
                      width="32"
                      height="32"
                      viewBox="0 0 32 32"
                      xmlns="http://www.w3.org/2000/svg"
                    >
                      <path
                        fillRule="evenodd"
                        clipRule="evenodd"
                        d="M16.5 4.5C16.3 4.5 16.1 4.6 16 4.7L10.5 10.2C10.2 10.5 10.2 11 10.5 11.3C10.8 11.6 11.3 11.6 11.6 11.3L15.5 7.4V20C15.5 20.4 15.9 20.8 16.3 20.8C16.7 20.8 17.1 20.4 17.1 20V7.4L21 11.3C21.3 11.6 21.8 11.6 22.1 11.3C22.4 11 22.4 10.5 22.1 10.2L16.6 4.7C16.5 4.6 16.3 4.5 16.5 4.5ZM6.5 20C6.5 19.6 6.1 19.2 5.7 19.2C5.3 19.2 4.9 19.6 4.9 20V24.5C4.9 25.6 5.8 26.5 6.9 26.5H25.1C26.2 26.5 27.1 25.6 27.1 24.5V20C27.1 19.6 26.7 19.2 26.3 19.2C25.9 19.2 25.5 19.6 25.5 20V24.5C25.5 24.9 25.1 25.3 24.7 25.3H6.9C6.5 25.3 6.1 24.9 6.1 24.5V20H6.5Z"
                      />
                    </svg>
                  </div>
                  <h4 className="mb-2 font-semibold text-gray-800 text-lg dark:text-white/90">
                    {isDragActive ? "Drop File Here" : "Drag & Drop File Here"}
                  </h4>
                  <span className="mb-4 block text-sm text-gray-700 dark:text-gray-400">
                    or click to browse
                  </span>
                  <span className="font-medium text-blue-500 underline">
                    Browse File
                  </span>
                  <p className="mt-4 text-xs text-gray-500 dark:text-gray-400">
                    Supported formats: PDF, Word (.doc, .docx), Excel (.xls, .xlsx), HTML
                    <br />
                    Max file size: 10MB
                  </p>
                </div>
              </div>
            ) : (
              <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-gray-800">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded bg-blue-100 dark:bg-blue-900">
                      <svg
                        className="h-6 w-6 text-blue-600 dark:text-blue-400"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                        />
                      </svg>
                    </div>
                    <div>
                      <p className="font-medium text-black dark:text-white">{uploadedFile.name}</p>
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        {(uploadedFile.size / 1024).toFixed(2)} KB
                      </p>
                    </div>
                  </div>
                  <button
                    type="button"
                    onClick={removeFile}
                    className="rounded bg-red-500 px-3 py-1.5 text-sm text-white hover:bg-red-600"
                  >
                    Remove
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* Placeholder Info */}
          <div className="rounded border border-blue-200 bg-blue-50 p-4 dark:border-blue-800 dark:bg-blue-900/20">
            <h5 className="mb-2 font-semibold text-blue-900 dark:text-blue-300">
              Available Placeholders
            </h5>
            <div className="grid grid-cols-2 gap-2 text-sm text-blue-800 dark:text-blue-200 md:grid-cols-3">
              <div>{"{"}CustomerName{"}"}</div>
              <div>{"{"}CompanyName{"}"}</div>
              <div>{"{"}NumberOfUsers{"}"}</div>
              <div>{"{"}SubscriptionTenure{"}"}</div>
              <div>{"{"}NetPrice{"}"}</div>
              <div>{"{"}DiscountPercentage{"}"}</div>
              <div>{"{"}GST{"}"}</div>
              <div>{"{"}GrossAmount{"}"}</div>
              <div>{"{"}QuotationDate{"}"}</div>
            </div>
            <p className="mt-3 text-xs text-blue-700 dark:text-blue-300">
              Use these placeholders in your template file. They will be replaced with actual values when generating quotations or pro forma invoices.
            </p>
          </div>

          {/* Actions */}
          <div className="flex items-center justify-end gap-4">
            <button
              type="button"
              onClick={() => router.back()}
              className="rounded border border-stroke px-6 py-2.5 font-medium hover:bg-gray-50 dark:border-strokedark dark:hover:bg-meta-4"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading || !uploadedFile}
              className="rounded bg-primary px-6 py-2.5 font-medium text-white hover:bg-opacity-90 disabled:opacity-50"
            >
              {loading ? "Uploading..." : "Upload Template"}
            </button>
          </div>
        </form>
      </div>
    </TemplateErrorBoundary>
  );
}

