"use client";

import { useState, useEffect } from "react";
import { LocalizationApi } from "../../../../lib/api";
import type { LocalizationResource, SupportedLanguage, CreateLocalizationResourceRequest, UpdateLocalizationResourceRequest } from "../../../../types/localization";

export default function AdminLocalizationPage() {
  const [resources, setResources] = useState<LocalizationResource[]>([]);
  const [languages, setLanguages] = useState<SupportedLanguage[]>([]);
  const [selectedLanguage, setSelectedLanguage] = useState<string>("en");
  const [selectedCategory, setSelectedCategory] = useState<string>("");
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [loading, setLoading] = useState(true);
  const [editingResource, setEditingResource] = useState<LocalizationResource | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newResource, setNewResource] = useState<CreateLocalizationResourceRequest>({
    languageCode: "en",
    resourceKey: "",
    resourceValue: "",
    category: "",
  });

  useEffect(() => {
    loadData();
  }, [selectedLanguage]);

  async function loadData() {
    try {
      setLoading(true);
      const [langs, res] = await Promise.all([
        LocalizationApi.getSupportedLanguages(),
        LocalizationApi.getLocalizationResources(selectedLanguage),
      ]);
      setLanguages(langs.filter(l => l.isActive));
      setResources(res);
    } catch (error) {
      console.error("Failed to load data", error);
    } finally {
      setLoading(false);
    }
  }

  async function handleCreate() {
    try {
      await LocalizationApi.createLocalizationResource(newResource);
      setShowCreateModal(false);
      setNewResource({ languageCode: selectedLanguage, resourceKey: "", resourceValue: "", category: "" });
      await loadData();
    } catch (error) {
      console.error("Failed to create resource", error);
      alert("Failed to create resource");
    }
  }

  async function handleUpdate(resourceId: string, update: UpdateLocalizationResourceRequest) {
    try {
      await LocalizationApi.updateLocalizationResource(resourceId, update);
      setEditingResource(null);
      await loadData();
    } catch (error) {
      console.error("Failed to update resource", error);
      alert("Failed to update resource");
    }
  }

  async function handleDelete(resourceId: string) {
    if (!confirm("Are you sure you want to delete this resource?")) return;
    
    try {
      await LocalizationApi.deleteLocalizationResource(resourceId);
      await loadData();
    } catch (error) {
      console.error("Failed to delete resource", error);
      alert("Failed to delete resource");
    }
  }

  const filteredResources = resources.filter(r => {
    const matchesLanguage = r.languageCode === selectedLanguage;
    const matchesCategory = !selectedCategory || r.category === selectedCategory;
    const matchesSearch = !searchTerm || 
      r.resourceKey.toLowerCase().includes(searchTerm.toLowerCase()) ||
      r.resourceValue.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesLanguage && matchesCategory && matchesSearch;
  });

  const categories = Array.from(new Set(resources.map(r => r.category).filter(Boolean)));

  if (loading) {
    return <div className="p-6">Loading...</div>;
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Localization Management</h1>
        <button
          onClick={() => setShowCreateModal(true)}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          Add Resource
        </button>
      </div>

      <div className="mb-4 flex gap-4">
        <div>
          <label className="block text-sm font-medium mb-2">Language</label>
          <select
            value={selectedLanguage}
            onChange={(e) => setSelectedLanguage(e.target.value)}
            className="px-3 py-2 border rounded-md"
          >
            {languages.map((lang) => (
              <option key={lang.languageCode} value={lang.languageCode}>
                {lang.displayNameEn}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Category</label>
          <select
            value={selectedCategory}
            onChange={(e) => setSelectedCategory(e.target.value)}
            className="px-3 py-2 border rounded-md"
          >
            <option value="">All Categories</option>
            {categories.map((cat) => (
              <option key={cat} value={cat}>
                {cat}
              </option>
            ))}
          </select>
        </div>

        <div className="flex-1">
          <label className="block text-sm font-medium mb-2">Search</label>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search by key or value..."
            className="w-full px-3 py-2 border rounded-md"
          />
        </div>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead className="bg-gray-50 dark:bg-gray-900">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Key
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Value
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Category
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            {filteredResources.map((resource) => (
              <tr key={resource.resourceId}>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                  {resource.resourceKey}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                  {editingResource?.resourceId === resource.resourceId ? (
                    <input
                      type="text"
                      defaultValue={editingResource.resourceValue}
                      onBlur={(e) => {
                        handleUpdate(resource.resourceId, { resourceValue: e.target.value });
                      }}
                      onKeyDown={(e) => {
                        if (e.key === "Enter") {
                          handleUpdate(resource.resourceId, { resourceValue: e.currentTarget.value });
                        } else if (e.key === "Escape") {
                          setEditingResource(null);
                        }
                      }}
                      className="w-full px-2 py-1 border rounded"
                      autoFocus
                    />
                  ) : (
                    <span onClick={() => setEditingResource(resource)} className="cursor-pointer">
                      {resource.resourceValue}
                    </span>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                  {resource.category || "-"}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <button
                    onClick={() => handleDelete(resource.resourceId)}
                    className="text-red-600 hover:text-red-900"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 p-6 rounded-lg max-w-md w-full">
            <h2 className="text-xl font-bold mb-4">Create Resource</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-2">Key</label>
                <input
                  type="text"
                  value={newResource.resourceKey}
                  onChange={(e) => setNewResource({ ...newResource, resourceKey: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Value</label>
                <textarea
                  value={newResource.resourceValue}
                  onChange={(e) => setNewResource({ ...newResource, resourceValue: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                  rows={3}
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Category</label>
                <input
                  type="text"
                  value={newResource.category}
                  onChange={(e) => setNewResource({ ...newResource, category: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                />
              </div>
              <div className="flex gap-2">
                <button
                  onClick={handleCreate}
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                >
                  Create
                </button>
                <button
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 px-4 py-2 bg-gray-300 text-gray-700 rounded-md hover:bg-gray-400"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

