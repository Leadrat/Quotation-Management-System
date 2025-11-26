"use client";
import { useState, useEffect } from "react";
import Label from "../form/Label";
import Button from "../ui/button/Button";
import { API_BASE } from "@/lib/api";

interface LogoUploadProps {
  logoUrl?: string;
  onUpload: (file: File) => Promise<void>;
  uploading?: boolean;
}

export default function LogoUpload({ logoUrl, onUpload, uploading = false }: LogoUploadProps) {
  const [preview, setPreview] = useState<string | null>(null);
  const [file, setFile] = useState<File | null>(null);

  // Update preview when logoUrl changes
  useEffect(() => {
    if (logoUrl) {
      // Convert relative URL to absolute URL for preview
      const absoluteUrl = logoUrl.startsWith('http') 
        ? logoUrl 
        : `${API_BASE}${logoUrl}`;
      setPreview(absoluteUrl);
    } else {
      setPreview(null);
    }
  }, [logoUrl]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (selectedFile) {
      // Validate file type
      const allowedTypes = ["image/png", "image/jpeg", "image/jpg", "image/svg+xml", "image/webp"];
      if (!allowedTypes.includes(selectedFile.type)) {
        alert("Please select a valid image file (PNG, JPG, SVG, or WEBP)");
        return;
      }

      // Validate file size (5MB)
      if (selectedFile.size > 5 * 1024 * 1024) {
        alert("File size must be less than 5MB");
        return;
      }

      setFile(selectedFile);
      const reader = new FileReader();
      reader.onloadend = () => {
        setPreview(reader.result as string);
      };
      reader.readAsDataURL(selectedFile);
    }
  };

  const handleUpload = async () => {
    if (file) {
      await onUpload(file);
      setFile(null);
      // Preview will be updated via useEffect when logoUrl prop changes
    }
  };

  return (
    <div className="space-y-4">
      <Label>Company Logo</Label>
      <div className="flex items-start gap-4">
        <div className="flex-shrink-0">
          <div className="w-32 h-32 border border-gray-300 dark:border-gray-700 rounded-lg p-2 bg-white dark:bg-gray-800 flex items-center justify-center">
            {preview ? (
              <img
                src={preview}
                alt="Company logo preview"
                className="max-w-full max-h-full object-contain"
                onError={(e) => {
                  // If image fails to load, show placeholder
                  (e.target as HTMLImageElement).style.display = 'none';
                  const parent = (e.target as HTMLImageElement).parentElement;
                  if (parent && !parent.querySelector('.placeholder')) {
                    const placeholder = document.createElement('div');
                    placeholder.className = 'placeholder text-gray-400 text-xs text-center';
                    placeholder.innerHTML = 'Logo<br/>Preview';
                    parent.appendChild(placeholder);
                  }
                }}
              />
            ) : (
              <div className="text-gray-400 text-xs text-center">
                Logo<br/>Preview
              </div>
            )}
          </div>
        </div>
        <div className="flex-1 space-y-2">
          <input
            type="file"
            accept="image/png,image/jpeg,image/jpg,image/svg+xml,image/webp"
            onChange={handleFileChange}
            className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-brand-50 file:text-brand-700 hover:file:bg-brand-100 dark:file:bg-brand-900 dark:file:text-brand-300 focus:outline-none focus:ring-2 focus:ring-brand-500"
            aria-label="Upload company logo"
            id="logo-upload"
          />
          <p className="text-xs text-gray-500 dark:text-gray-400">
            Supported formats: PNG, JPG, SVG, WEBP. Max size: 5MB
          </p>
          {file && (
            <Button
              type="button"
              onClick={handleUpload}
              disabled={uploading}
              className="bg-brand-600 hover:bg-brand-700"
            >
              {uploading ? "Uploading..." : "Upload Logo"}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

