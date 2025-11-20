"use client";
import { useState, useEffect } from "react";
import Link from "next/link";
import { IdentifierTypesApi, type IdentifierType } from "@/lib/api/identifierTypes";

export default function IdentifierTypesPage() {
  const [types, setTypes] = useState<IdentifierType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<IdentifierType | null>(null);
  const [form, setForm] = useState({ name: "", displayName: "", description: "", isActive: true });

  useEffect(() => {
    loadTypes();
  }, []);

  const loadTypes = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await IdentifierTypesApi.list(true);
      setTypes(res.data || []);
    } catch (e: any) {
      setError(e.message || "Failed to load identifier types");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      if (editing) {
        await IdentifierTypesApi.update(editing.identifierTypeId, {
          displayName: form.displayName,
          description: form.description,
          isActive: form.isActive,
        });
      } else {
        await IdentifierTypesApi.create({
          name: form.name,
          displayName: form.displayName,
          description: form.description,
        });
      }
      setShowForm(false);
      setEditing(null);
      setForm({ name: "", displayName: "", description: "", isActive: true });
      loadTypes();
    } catch (e: any) {
      setError(e.message || "Failed to save identifier type");
    }
  };

  const handleEdit = (type: IdentifierType) => {
    setEditing(type);
    setForm({
      name: type.name,
      displayName: type.displayName,
      description: type.description || "",
      isActive: type.isActive,
    });
    setShowForm(true);
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditing(null);
    setForm({ name: "", displayName: "", description: "", isActive: true });
  };

  if (loading) return <div className="p-6">Loading...</div>;

  return (
    <div className="p-6">
      <div className="mb-6">
        <Link href="/admin" className="text-sm text-blue-600 hover:text-blue-700 mb-4 inline-block">
          ← Back to Admin Console
        </Link>
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold mb-2">Identifier Types</h1>
            <p className="text-gray-600">Manage company identifier types (PAN, VAT, GST, etc.)</p>
          </div>
          {!showForm && (
            <button
              onClick={() => setShowForm(true)}
              className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              Add Identifier Type
            </button>
          )}
        </div>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded text-red-700">
          {error}
        </div>
      )}

      {showForm && (
        <div className="mb-6 bg-white rounded border p-6">
          <h2 className="text-xl font-semibold mb-4">
            {editing ? "Edit" : "Add"} Identifier Type
          </h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            {!editing && (
              <div>
                <label className="block text-sm font-medium mb-1">Name (Code)</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value.toUpperCase() })}
                  className="w-full px-3 py-2 border rounded"
                  placeholder="e.g., PAN, VAT, GST"
                  required
                  pattern="[A-Z_][A-Z0-9_]*"
                  title="Uppercase letters, numbers, and underscores only"
                />
              </div>
            )}
            <div>
              <label className="block text-sm font-medium mb-1">Display Name</label>
              <input
                type="text"
                value={form.displayName}
                onChange={(e) => setForm({ ...form, displayName: e.target.value })}
                className="w-full px-3 py-2 border rounded"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Description</label>
              <textarea
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                className="w-full px-3 py-2 border rounded"
                rows={3}
              />
            </div>
            {editing && (
              <div>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={form.isActive}
                    onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                    className="mr-2"
                  />
                  Active
                </label>
              </div>
            )}
            <div className="flex space-x-2">
              <button
                type="submit"
                className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
              >
                {editing ? "Update" : "Create"}
              </button>
              <button
                type="button"
                onClick={handleCancel}
                className="px-4 py-2 border rounded hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="bg-white rounded border overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium">Name</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Display Name</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Description</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Status</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {types.map((type) => (
              <tr key={type.identifierTypeId}>
                <td className="px-4 py-3 text-sm">{type.name}</td>
                <td className="px-4 py-3 text-sm">{type.displayName}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{type.description || "-"}</td>
                <td className="px-4 py-3 text-sm">
                  <span
                    className={`px-2 py-1 rounded text-xs ${
                      type.isActive ? "bg-green-100 text-green-800" : "bg-gray-100 text-gray-800"
                    }`}
                  >
                    {type.isActive ? "Active" : "Inactive"}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm">
                  <div className="flex space-x-2">
                    <button
                      onClick={() => handleEdit(type)}
                      className="text-blue-600 hover:text-blue-800"
                    >
                      Edit
                    </button>
                    <Link
                      href="/admin/company-identifiers/config"
                      className="text-green-600 hover:text-green-800"
                    >
                      View Configs
                    </Link>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {types.length === 0 && (
          <div className="p-8 text-center text-gray-500">No identifier types found</div>
        )}
      </div>
    </div>
  );
}

