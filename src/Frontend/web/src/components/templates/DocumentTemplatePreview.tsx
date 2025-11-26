"use client";
import React, { useState } from 'react';

interface Placeholder {
    placeholderName: string;
    placeholderType: string;
    defaultValue: string;
}

interface DocumentTemplatePreviewProps {
    placeholders: Placeholder[];
    onSave: (placeholders: Placeholder[]) => Promise<void>;
    onCancel: () => void;
}

export default function DocumentTemplatePreview({ placeholders, onSave, onCancel }: DocumentTemplatePreviewProps) {
    const [editedPlaceholders, setEditedPlaceholders] = useState<Placeholder[]>(placeholders);

    const handleTypeChange = (index: number, newType: string) => {
        const updated = [...editedPlaceholders];
        updated[index].placeholderType = newType;
        setEditedPlaceholders(updated);
    };

    const handleDefaultValueChange = (index: number, newValue: string) => {
        const updated = [...editedPlaceholders];
        updated[index].defaultValue = newValue;
        setEditedPlaceholders(updated);
    };

    return (
        <div className="bg-white shadow rounded-lg p-6 max-w-4xl mx-auto">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Review Detected Placeholders</h2>
            <p className="text-sm text-gray-600 mb-6">
                The following placeholders were detected in your document. Please review and categorize them correctly.
            </p>

            <div className="overflow-x-auto mb-6">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                        <tr>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Placeholder Name
                            </th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Type
                            </th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Default Value (Optional)
                            </th>
                        </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                        {editedPlaceholders.map((placeholder, index) => (
                            <tr key={index}>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                                    {placeholder.placeholderName}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                    <select
                                        value={placeholder.placeholderType}
                                        onChange={(e) => handleTypeChange(index, e.target.value)}
                                        className="mt-1 block w-full py-2 px-3 border border-gray-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                                    >
                                        <option value="Company">Company Details</option>
                                        <option value="Customer">Customer Details</option>
                                        <option value="Quotation">Quotation Details</option>
                                        <option value="Other">Other</option>
                                    </select>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                    <input
                                        type="text"
                                        value={placeholder.defaultValue}
                                        onChange={(e) => handleDefaultValueChange(index, e.target.value)}
                                        className="shadow-sm focus:ring-indigo-500 focus:border-indigo-500 block w-full sm:text-sm border-gray-300 rounded-md"
                                        placeholder="Default value"
                                    />
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <div className="flex justify-end space-x-3">
                <button
                    type="button"
                    onClick={onCancel}
                    className="bg-white py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                >
                    Cancel
                </button>
                <button
                    type="button"
                    onClick={() => onSave(editedPlaceholders)}
                    className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                >
                    Save Template
                </button>
            </div>
        </div>
    );
}
