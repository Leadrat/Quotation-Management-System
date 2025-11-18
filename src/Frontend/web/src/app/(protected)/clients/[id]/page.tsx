"use client";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { ClientsApi } from "@/lib/api";

export default function ClientDetailsPage() {
  const params = useParams();
  const router = useRouter();
  const clientId = String(params?.id || "");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [c, setC] = useState<any | null>(null);

  useEffect(() => {
    (async () => {
      setLoading(true); setError(null);
      try {
        const res = await ClientsApi.get(clientId);
        setC(res.data);
      } catch (e: any) {
        setError(e.message || "Failed to load");
      } finally {
        setLoading(false);
      }
    })();
  }, [clientId]);

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-semibold">Client Details</h1>
        <div className="space-x-2 text-sm">
          <Link href={`/clients/${clientId}/history`} className="px-3 py-2 rounded border border-blue-600 text-blue-600 hover:bg-blue-50">
            View History
          </Link>
          <Link href={`/clients/${clientId}/edit`} className="px-3 py-2 rounded border">Edit</Link>
          <button
            onClick={async () => {
              if (!confirm("Delete this client?")) return;
              try {
                await ClientsApi.remove(clientId);
                router.replace("/clients");
              } catch (e: any) {
                alert(e.message || "Delete failed");
              }
            }}
            className="px-3 py-2 rounded border text-red-600"
          >Delete</button>
        </div>
      </div>
      {error && <div className="text-red-600 mb-3 text-sm">{error}</div>}
      {loading ? (
        <div>Loading...</div>
      ) : c ? (
        <div className="bg-white rounded border p-4 space-y-2">
          <div><span className="text-gray-500">Company:</span> {c.companyName}</div>
          <div><span className="text-gray-500">Email:</span> {c.email}</div>
          <div><span className="text-gray-500">Mobile:</span> {c.mobile}</div>
          <div><span className="text-gray-500">Contact:</span> {c.contactName}</div>
          <div><span className="text-gray-500">GSTIN:</span> {c.gstin || "-"}</div>
          <div><span className="text-gray-500">StateCode:</span> {c.stateCode || "-"}</div>
          <div><span className="text-gray-500">Address:</span> {c.address || "-"}</div>
          <div><span className="text-gray-500">City/State/Pin:</span> {c.city || "-"} / {c.state || "-"} / {c.pinCode || "-"}</div>
        </div>
      ) : (
        <div>Not found.</div>
      )}
    </div>
  );
}
