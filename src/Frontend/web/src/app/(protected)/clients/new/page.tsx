"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { ClientsApi } from "@/lib/api";

export default function NewClientPage() {
  const router = useRouter();
  const [form, setForm] = useState({
    companyName: "",
    email: "",
    mobile: "",
    contactName: "",
    gstin: "",
    address: "",
    city: "",
    state: "",
    pinCode: "",
    phoneCode: "+91",
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  async function submit() {
    setSaving(true);
    setError(null);
    setFieldErrors({});

    try {
      // Combine phoneCode and mobile for backend validation
      let mobileWithCode: string | undefined;
      if (form.phoneCode && form.mobile) {
        mobileWithCode = `${form.phoneCode}${form.mobile.replace(/^\+/, '')}`;
      } else {
        mobileWithCode = form.mobile || form.phoneCode || undefined;
      }

      const payload = {
        companyName: form.companyName,
        email: form.email,
        mobile: mobileWithCode,
        contactName: form.contactName || undefined,
        gstin: form.gstin || undefined,
        address: form.address || undefined,
        city: form.city || undefined,
        state: form.state || undefined,
        pinCode: form.pinCode || undefined,
        phoneCode: form.phoneCode || undefined,
      };

      const res = await ClientsApi.create(payload);
      const id = (res as any)?.data?.clientId;
      router.replace(`/clients/${id}`);
    } catch (e: any) {
      // Handle validation errors
      if (e.status === 400 && e.errors) {
        const errors: Record<string, string> = {};
        // Map backend PascalCase property names to frontend camelCase field names
        const fieldNameMap: Record<string, string> = {
          'CompanyName': 'companyName',
          'Email': 'email',
          'Mobile': 'mobile',
          'ContactName': 'contactName',
          'Gstin': 'gstin',
          'Address': 'address',
          'City': 'city',
          'State': 'state',
          'PinCode': 'pinCode',
          'PhoneCode': 'phoneCode'
        };

        Object.keys(e.errors).forEach(key => {
          const frontendKey = fieldNameMap[key] || key.toLowerCase();
          if (Array.isArray(e.errors[key])) {
            errors[frontendKey] = e.errors[key][0];
          } else {
            errors[frontendKey] = e.errors[key];
          }
        });
        setFieldErrors(errors);
        setError("Please fix the validation errors below");
      } else {
        setError(e.message || "Failed to create client");
      }
    } finally {
      setSaving(false);
    }
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold mb-4">New Client</h1>
      {error && <div className="text-red-600 mb-3 text-sm">{error}</div>}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {([
          ["Company Name", "companyName", true],
          ["Email", "email", true],
          ["Mobile", "mobile", true],
          ["Contact Name", "contactName", false],
          ["GSTIN", "gstin", false],
          ["Address", "address", false],
          ["City", "city", false],
          ["State", "state", false],
          ["Pin Code (6 digits)", "pinCode", false],
          ["Phone Code", "phoneCode", false],
        ] as const).map(([label, key, required]) => (
          <div key={key}>
            <label className="block text-sm mb-1">
              {label} {required && <span className="text-red-500">*</span>}
            </label>
            <input
              className={`w-full rounded border px-3 py-2 ${fieldErrors[key] ? 'border-red-500' : ''
                }`}
              value={(form as any)[key]}
              onChange={(e) => {
                setForm(prev => ({ ...prev, [key]: e.target.value }));
                if (fieldErrors[key]) {
                  setFieldErrors(prev => {
                    const newErrors = { ...prev };
                    delete newErrors[key];
                    return newErrors;
                  });
                }
              }}
              placeholder={key === 'mobile' ? 'Enter number without +' : ''}
            />
            {fieldErrors[key] && (
              <div className="text-red-500 text-xs mt-1">{fieldErrors[key]}</div>
            )}
          </div>
        ))}
      </div>
      <div className="mt-4 space-x-2">
        <button onClick={submit} disabled={saving} className="rounded bg-blue-600 text-white px-4 py-2 disabled:opacity-50">{saving ? "Saving..." : "Save"}</button>
        <button onClick={() => router.back()} className="rounded border px-4 py-2">Cancel</button>
      </div>
    </div>
  );
}
