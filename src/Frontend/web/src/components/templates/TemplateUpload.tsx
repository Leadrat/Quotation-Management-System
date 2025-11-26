import React, { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { ArrowUpTrayIcon, DocumentIcon, XMarkIcon } from '@heroicons/react/24/outline';

interface TemplateUploadProps {
  onUpload: (file: File) => void;
}

export default function TemplateUpload({ onUpload }: TemplateUploadProps) {
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const selectedFile = acceptedFiles[0];
    if (selectedFile) {
      if (!selectedFile.name.endsWith('.docx')) {
        setError('Only .docx files are supported.');
        return;
      }
      setFile(selectedFile);
      setError(null);
      onUpload(selectedFile);
    }
  }, [onUpload]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': ['.docx']
    },
    maxFiles: 1
  });

  const removeFile = (e: React.MouseEvent) => {
    e.stopPropagation();
    setFile(null);
    setError(null);
  };

  return (
    <div className="w-full">
      <div
        {...getRootProps()}
        className={`border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors
          ${isDragActive ? 'border-indigo-500 bg-indigo-50' : 'border-gray-300 hover:border-indigo-400'}
          ${error ? 'border-red-500 bg-red-50' : ''}
        `}
      >
        <input {...getInputProps()} />
        
        {file ? (
          <div className="flex items-center justify-center space-x-4">
            <DocumentIcon className="h-10 w-10 text-indigo-600" />
            <div className="text-left">
              <p className="text-sm font-medium text-gray-900">{file.name}</p>
              <p className="text-xs text-gray-500">{(file.size / 1024).toFixed(2)} KB</p>
            </div>
            <button
              onClick={removeFile}
              className="p-1 rounded-full hover:bg-gray-200 text-gray-500"
            >
              <XMarkIcon className="h-5 w-5" />
            </button>
          </div>
        ) : (
          <div className="space-y-2">
            <ArrowUpTrayIcon className="mx-auto h-12 w-12 text-gray-400" />
            <p className="text-base text-gray-600">
              <span className="font-medium text-indigo-600 hover:text-indigo-500">Upload a file</span>
              {' '}or drag and drop
            </p>
            <p className="text-xs text-gray-500">Word Documents (.docx) up to 10MB</p>
          </div>
        )}
      </div>
      {error && <p className="mt-2 text-sm text-red-600">{error}</p>}
    </div>
  );
}
