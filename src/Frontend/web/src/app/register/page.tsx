"use client";
import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { RegistrationApi } from "@/lib/api";

export default function RegisterClientPage() {
  const router = useRouter();
  const [form, setForm] = useState<any>({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    phoneCode: "+91",
    mobile: "",
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await RegistrationApi.registerClient(form);
      alert("Registration successful. Please login.");
      router.replace("/login");
    } catch (e: any) {
      setError(e.message || "Registration failed");
    } finally {
      setSaving(false);
    }
  }

  return (
    <main className="min-h-screen bg-white">
      <div className="mx-auto flex min-h-screen max-w-7xl items-center justify-center px-4">
        <div className="w-full max-w-xl">
          <h1 className="mb-1 text-2xl font-semibold text-black">Client Registration</h1>
          <p className="mb-6 text-sm text-black">Create your account to get started.</p>

          {error && (
            <div className="mb-4 rounded-md border border-black/20 bg-white px-3 py-2 text-sm text-red-600">
              {error}
            </div>
          )}

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 md:gap-5">
            <div className="md:col-span-2">
              <label className="mb-1 block text-sm font-medium text-black">Email</label>
              <input
                type="email"
                className="h-11 w-full rounded-md border border-black bg-white px-3 text-sm text-black placeholder:text-black/50 focus:outline-0"
                placeholder="you@example.com"
                value={form.email}
                onChange={(e)=>setForm((prev:any)=>({...prev,email:e.target.value}))}
              />
            </div>
            <div className="md:col-span-2">
              <label className="mb-1 block text-sm font-medium text-black">Password</label>
              <input
                type="password"
                className="h-11 w-full rounded-md border border-black bg-white px-3 text-sm text-black placeholder:text-black/50 focus:outline-0"
                placeholder="••••••••"
                value={form.password}
                onChange={(e)=>setForm((prev:any)=>({...prev,password:e.target.value}))}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-black">First Name</label>
              <input
                type="text"
                className="h-11 w-full rounded-md border border-black bg-white px-3 text-sm text-black placeholder:text-black/50 focus:outline-0"
                value={form.firstName}
                onChange={(e)=>setForm((prev:any)=>({...prev,firstName:e.target.value}))}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-black">Last Name</label>
              <input
                type="text"
                className="h-11 w-full rounded-md border border-black bg-white px-3 text-sm text-black placeholder:text-black/50 focus:outline-0"
                value={form.lastName}
                onChange={(e)=>setForm((prev:any)=>({...prev,lastName:e.target.value}))}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-black">Phone Code</label>
              <input
                type="text"
                className="h-11 w-full rounded-md border border-black bg-white px-3 text-sm text-black placeholder:text-black/50 focus:outline-0"
                value={form.phoneCode}
                onChange={(e)=>setForm((prev:any)=>({...prev,phoneCode:e.target.value}))}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-black">Mobile</label>
              <input
                type="text"
                className="h-11 w-full rounded-md border border-black bg-white px-3 text-sm text-black placeholder:text-black/50 focus:outline-0"
                value={form.mobile}
                onChange={(e)=>setForm((prev:any)=>({...prev,mobile:e.target.value}))}
              />
            </div>
          </div>

          <div className="mt-6 flex items-center justify-between gap-3">
            <button
              onClick={submit}
              disabled={saving}
              className="inline-flex h-11 w-full items-center justify-center rounded-md bg-black px-4 text-sm font-medium text-white disabled:opacity-60 md:w-auto"
            >
              {saving ? "Registering..." : "Register"}
            </button>
            <Link href="/login" className="inline-flex h-11 items-center justify-center rounded-md border border-black bg-white px-4 text-sm font-medium text-black">
              Back to Login
            </Link>
          </div>
        </div>
      </div>
    </main>
  );
}
