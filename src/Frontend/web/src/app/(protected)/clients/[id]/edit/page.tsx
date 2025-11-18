"use client";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ClientsApi } from "@/lib/api";

export default function EditClientPage() {
  const params = useParams();
  const router = useRouter();
  const clientId = String(params?.id || "");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<any>({});

  useEffect(() => {
    (async () => {
      setLoading(true); setError(null);
      try {
        const res = await ClientsApi.get(clientId);
        const c = (res as any).data ?? res; // support both shapes
        setForm({
          companyName: c.companyName || "",
          email: c.email || "",
          mobile: c.mobile || "",
          contactName: c.contactName || "",
          gstin: c.gstin || "",
          stateCode: c.stateCode || "",
          address: c.address || "",
          city: c.city || "",
          state: c.state || "",
          pinCode: c.pinCode || "",
          phoneCode: c.phoneCode || "",
        });
      } catch (e: any) {
        setError(e.message || "Failed to load");
      } finally { setLoading(false); }
    })();
  }, [clientId]);

  async function submit() {
    setSaving(true); setError(null);
    try {
      await ClientsApi.update(clientId, form);
      router.replace(`/clients/${clientId}`);
    } catch (e: any) {
      setError(e.message || "Failed to update client");
    } finally { setSaving(false); }
  }

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      <h1 className="text-2xl font-semibold mb-4">Edit Client</h1>
      {error && <div className="text-red-600 mb-3 text-sm">{error}</div>}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {([
          ["Company Name","companyName"],
          ["Email","email"],
          ["Mobile","mobile"],
          ["Contact Name","contactName"],
          ["GSTIN","gstin"],
          ["State Code","stateCode"],
          ["Address","address"],
          ["City","city"],
          ["State","state"],
          ["Pin Code","pinCode"],
          ["Phone Code","phoneCode"],
        ] as const).map(([label, key]) => (
          <div key={key}>
            <label className="block text-sm mb-1">{label}</label>
            <input
              className="w-full rounded border px-3 py-2"
              value={(form as any)[key] ?? ""}
              onChange={(e) => setForm((prev: any) => ({ ...prev, [key]: e.target.value }))}
            />
          </div>
        ))}
      </div>
      <div className="mt-4 space-x-2">
        <button onClick={submit} disabled={saving} className="rounded bg-blue-600 text-white px-4 py-2 disabled:opacity-50">{saving?"Saving...":"Save"}</button>
        <button onClick={() => router.back()} className="rounded border px-4 py-2">Cancel</button>
      </div>
    </div>
  );
}
