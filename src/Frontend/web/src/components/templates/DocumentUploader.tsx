"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useDropzone } from "react-dropzone";
import { DocumentTemplatesApi } from "@/lib/api";
import { useToast, ToastContainer } from "@/components/quotations/Toast";

type Props = {
  onSuccess?: (templateId: string) => void;
  onCancel?: () => void;
};

const allowedTypes = [
  "application/pdf",
  "application/msword",
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
];

export default function DocumentUploader({ onSuccess, onCancel }: Props) {
  const router = useRouter();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [templateType, setTemplateType] = useState<"Quotation" | "ProFormaInvoice">("Quotation");
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const toast = useToast();

  const onDrop = (acceptedFiles: File[]) => {
    if (acceptedFiles.length === 0) return;
    const file = acceptedFiles[0];

    if (!allowedTypes.includes(file.type)) {
      setError("Please upload a valid template file (PDF or Word)");
      return;
    }

    if (file.size > 50 * 1024 * 1024) {
      setError("File size must be less than 50MB");
      return;
    }

    setUploadedFile(file);
    setError(null);

    if (!name) {
      const fileName = file.name.replace(/\.[^/.]+$/, "");
      setName(fileName);
    }
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      "application/pdf": [".pdf"],
      "application/msword": [".doc"],
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [".docx"],
    },
    maxFiles: 1,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!uploadedFile) {
      setError("Please upload a template file");
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const formData = new FormData();
      formData.append("file", uploadedFile);
      formData.append("name", name.trim());
      if (description.trim()) {
        formData.append("description", description.trim());
      }
      formData.append("templateType", templateType);

      toast.info("Uploading template...");
      const result = await DocumentTemplatesApi.upload(formData);
      toast.success("Template uploaded successfully!");

      if (onSuccess) {
        onSuccess(result.data.templateId);
      } else {
        router.push(`/templates/${result.data.templateId}`);
      }
    } catch (err: any) {
      const errorMsg = err?.message || "Failed to upload template";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const removeFile = () => setUploadedFile(null);

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <div className="mb-6">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Upload Template</h4>
        <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
          Upload a PDF or Word document that the system will convert into an editable template with placeholders.
        </p>
      </div>

      {error && (
        <div className="mb-4 rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
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
        </div>

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
                  📄
                </div>
                <h4 className="mb-2 font-semibold text-gray-800 text-lg dark:text-white/90">
                  {isDragActive ? "Drop File Here" : "Drag & Drop File Here"}
                </h4>
                <span className="mb-4 block text-sm text-gray-700 dark:text-gray-400">or click to browse</span>
                <span className="font-medium text-blue-500 underline">Browse File</span>
                <p className="mt-4 text-xs text-gray-500 dark:text-gray-400">
                  Supported formats: PDF, Word (.doc, .docx). Max file size: 50MB.
                </p>
              </div>
            </div>
          ) : (
            <div className="rounded border border-stroke bg-gray-50 p-4 dark:border-strokedark dark:bg-gray-800">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-black dark:text-white">{uploadedFile.name}</p>
                  <p className="text-sm text-gray-500 dark:text-gray-400">{(uploadedFile.size / 1024).toFixed(2)} KB</p>
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

        <div className="rounded border border-blue-200 bg-blue-50 p-4 dark:border-blue-800 dark:bg-blue-900/20">
          <h5 className="mb-2 font-semibold text-blue-900 dark:text-blue-300">Recommended Placeholders</h5>
          <div className="grid grid-cols-2 gap-2 text-sm text-blue-800 dark:text-blue-200 md:grid-cols-3">
            <div>{"{{CompanyName}}"}</div>
            <div>{"{{CompanyAddress}}"}</div>
            <div>{"{{CompanyPhone}}"}</div>
            <div>{"{{CustomerCompanyName}}"}</div>
            <div>{"{{CustomerAddress}}"}</div>
            <div>{"{{QuotationNumber}}"}</div>
            <div>{"{{QuotationDate}}"}</div>
            <div>{"{{SubTotal}}"}</div>
            <div>{"{{TotalAmount}}"}</div>
          </div>
          <p className="mt-3 text-xs text-blue-700 dark:text-blue-300">
            These placeholders will be automatically replaced with actual data during quotation creation.
          </p>
        </div>

        <div className="flex items-center justify-end gap-4">
          <button
            type="button"
            onClick={() => {
              if (onCancel) onCancel();
              else router.back();
            }}
            className="rounded border border-stroke px-6 py-2.5 font-medium hover:bg-gray-50 dark:border-strokedark dark:hover:bg-meta-4"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading || !uploadedFile}
            className="rounded border-2 border-blue-500 bg-white px-6 py-2.5 font-medium text-black hover:bg-blue-50 disabled:opacity-50 dark:bg-white dark:text-black dark:border-blue-500 dark:hover:bg-blue-50"
          >
            {loading ? "Uploading..." : "Upload Template"}
          </button>
        </div>
      </form>
    </div>
  );
}

