"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { UsersApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken } from "@/lib/session";

export default function AdminCreateUserPage() {
  const router = useRouter();
  const [role, setRole] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<any>({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    roleId: "",
    reportingManagerId: "",
    phoneCode: "+91",
    mobile: "",
  });

  useEffect(() => {
    const token = getAccessToken();
    setRole(getRoleFromToken(token));
  }, []);

  async function submit() {
    if (role !== "Admin") { setError("Only Admin can create users"); return; }
    setSaving(true); setError(null);
    try {
      await UsersApi.create(form);
      alert("User created");
      router.replace("/dashboard");
    } catch (e: any) {
      setError(e.message || "Failed to create user");
    } finally { setSaving(false); }
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold mb-4">Create User (Admin)</h1>
      {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {([
          ["Email","email"],
          ["Password","password"],
          ["First Name","firstName"],
          ["Last Name","lastName"],
          ["Role Id","roleId"],
          ["Reporting Manager Id","reportingManagerId"],
          ["Phone Code","phoneCode"],
          ["Mobile","mobile"],
        ] as const).map(([label, key]) => (
          <div key={key}>
            <label className="block text-sm mb-1">{label}</label>
            <input
              className="w-full rounded border px-3 py-2"
              type={key === "password" ? "password" : "text"}
              value={(form as any)[key] ?? ""}
              onChange={(e)=>setForm((prev:any)=>({...prev,[key]:e.target.value}))}
            />
          </div>
        ))}
      </div>
      <div className="mt-4 space-x-2">
        <button onClick={submit} disabled={saving || role!=="Admin"} className="rounded bg-blue-600 text-white px-4 py-2 disabled:opacity-50">{saving?"Saving...":"Create"}</button>
        <button onClick={()=>router.back()} className="rounded border px-4 py-2">Cancel</button>
      </div>
    </div>
  );
}
