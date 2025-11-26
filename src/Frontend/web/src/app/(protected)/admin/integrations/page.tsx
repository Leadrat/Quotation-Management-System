"use client";
import { useState } from "react";
import { useIntegrationKeys } from "@/hooks";
import Link from "next/link";

export default function IntegrationKeysPage() {
  const { keys, loading, error, saving, createKey, updateKey, deleteKey, getKeyWithValue } =
    useIntegrationKeys();
  const [showDialog, setShowDialog] = useState(false);
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [showValue, setShowValue] = useState<Record<string, boolean>>({});
  const [form, setForm] = useState({
    keyName: "",
    keyValue: "",
    provider: "",
  });

  const handleCreate = () => {
    setForm({ keyName: "", keyValue: "", provider: "" });
    setEditingKey(null);
    setShowDialog(true);
  };

  const handleEdit = (key: any) => {
    setForm({
      keyName: key.keyName,
      keyValue: "",
      provider: key.provider,
    });
    setEditingKey(key.id);
    setShowDialog(true);
  };

  const handleShowValue = async (id: string) => {
    if (!showValue[id]) {
      const result = await getKeyWithValue(id);
      if (result.success) {
        setShowValue({ ...showValue, [id]: true });
      }
    } else {
      setShowValue({ ...showValue, [id]: false });
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (editingKey) {
      await updateKey(editingKey, form);
    } else {
      await createKey(form);
    }
    setShowDialog(false);
    setForm({ keyName: "", keyValue: "", provider: "" });
    setEditingKey(null);
  };

  const handleDelete = async (id: string) => {
    if (confirm("Are you sure you want to delete this integration key?")) {
      await deleteKey(id);
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <div className="text-center py-12">Loading integration keys...</div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <Link
            href="/admin"
            className="text-sm text-brand-600 hover:text-brand-700 mb-4 inline-block"
          >
            ← Back to Admin Console
          </Link>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            Integration Keys
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            Manage API keys and credentials for third-party services
          </p>
        </div>
        <button
          onClick={handleCreate}
          className="px-4 py-2 text-sm font-medium text-white bg-brand-600 rounded-lg hover:bg-brand-700"
        >
          + Add Key
        </button>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg text-red-700 dark:text-red-400">
          {error}
          {error.includes("409") && (
            <p className="mt-2 text-sm">This may indicate a duplicate key or conflict. Please check your integration keys.</p>
          )}
        </div>
      )}

      <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-900">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Key Name
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Provider
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Key Value
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {!keys || !Array.isArray(keys) || keys.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-6 py-12 text-center text-gray-500 dark:text-gray-400">
                  No integration keys found. Click "Add Key" to create one.
                </td>
              </tr>
            ) : (
              keys.map((key) => (
                <tr key={key.id} className="hover:bg-gray-50 dark:hover:bg-gray-700/50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                    {key.keyName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                    {key.provider}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                    {showValue[key.id] ? (
                      <code className="text-xs bg-gray-100 dark:bg-gray-900 px-2 py-1 rounded">
                        {key.keyValue || "••••••••"}
                      </code>
                    ) : (
                      <code className="text-xs bg-gray-100 dark:bg-gray-900 px-2 py-1 rounded">
                        ••••••••
                      </code>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm space-x-2">
                    <button
                      onClick={() => handleShowValue(key.id)}
                      className="text-brand-600 hover:text-brand-700"
                    >
                      {showValue[key.id] ? "Hide" : "Show"}
                    </button>
                    <button
                      onClick={() => handleEdit(key)}
                      className="text-blue-600 hover:text-blue-700"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => handleDelete(key.id)}
                      className="text-red-600 hover:text-red-700"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {showDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">
              {editingKey ? "Edit Integration Key" : "Create Integration Key"}
            </h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Key Name
                </label>
                <input
                  type="text"
                  value={form.keyName}
                  onChange={(e) => setForm({ ...form, keyName: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Provider
                </label>
                <input
                  type="text"
                  value={form.provider}
                  onChange={(e) => setForm({ ...form, provider: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Key Value
                </label>
                <input
                  type="password"
                  value={form.keyValue}
                  onChange={(e) => setForm({ ...form, keyValue: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
                  required={!editingKey}
                  placeholder={editingKey ? "Leave blank to keep current value" : ""}
                />
              </div>
              <div className="flex justify-end gap-4 pt-4">
                <button
                  type="button"
                  onClick={() => setShowDialog(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="px-4 py-2 text-sm font-medium text-white bg-brand-600 rounded-lg hover:bg-brand-700 disabled:opacity-50"
                >
                  {saving ? "Saving..." : editingKey ? "Update" : "Create"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

