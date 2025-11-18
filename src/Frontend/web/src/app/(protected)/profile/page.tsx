"use client";
import { useEffect, useState } from "react";
import { PasswordApi, UsersApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken, parseJwt } from "@/lib/session";

export default function ProfilePage() {
  const [userId, setUserId] = useState<string>("");
  const [role, setRole] = useState<string | null>(null);
  const [profile, setProfile] = useState({ firstName: "", lastName: "", phoneCode: "", mobile: "" });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = getAccessToken();
    const payload = parseJwt(token);
    if (payload?.sub) setUserId(payload.sub);
    setRole(getRoleFromToken(token));
  }, []);

  async function saveProfile() {
    if (!userId) return;
    setSaving(true); setError(null);
    try {
      await UsersApi.updateProfile(userId, {
        firstName: profile.firstName,
        lastName: profile.lastName,
        phoneCode: profile.phoneCode,
        mobile: profile.mobile,
      });
      alert("Profile updated");
    } catch (e: any) {
      setError(e.message || "Failed to update profile");
    } finally { setSaving(false); }
  }

  async function changePassword(formData: FormData) {
    setSaving(true); setError(null);
    try {
      await PasswordApi.change({
        currentPassword: String(formData.get("currentPassword") || ""),
        newPassword: String(formData.get("newPassword") || ""),
        confirmPassword: String(formData.get("confirmPassword") || ""),
      });
      alert("Password changed");
      (document.getElementById("pwd-form") as HTMLFormElement)?.reset();
    } catch (e: any) {
      setError(e.message || "Failed to change password");
    } finally { setSaving(false); }
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-semibold mb-4">My Profile</h1>
        {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {([
            ["First Name","firstName"],
            ["Last Name","lastName"],
            ["Phone Code","phoneCode"],
            ["Mobile","mobile"],
          ] as const).map(([label, key]) => (
            <div key={key}>
              <label className="block text-sm mb-1">{label}</label>
              <input className="w-full rounded border px-3 py-2" value={(profile as any)[key]}
                onChange={(e)=>setProfile(prev=>({...prev,[key]:e.target.value}))}/>
            </div>
          ))}
        </div>
        <div className="mt-4">
          <button onClick={saveProfile} disabled={saving} className="rounded bg-blue-600 text-white px-4 py-2 disabled:opacity-50">{saving?"Saving...":"Save"}</button>
        </div>
      </div>

      <div>
        <h2 className="text-xl font-semibold mb-2">Change Password</h2>
        <form id="pwd-form" action={(formData)=>changePassword(formData)} className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm mb-1">Current Password</label>
            <input name="currentPassword" type="password" className="w-full rounded border px-3 py-2" required/>
          </div>
          <div>
            <label className="block text-sm mb-1">New Password</label>
            <input name="newPassword" type="password" className="w-full rounded border px-3 py-2" required/>
          </div>
          <div>
            <label className="block text-sm mb-1">Confirm Password</label>
            <input name="confirmPassword" type="password" className="w-full rounded border px-3 py-2" required/>
          </div>
          <div className="col-span-full">
            <button type="submit" disabled={saving} className="rounded bg-blue-600 text-white px-4 py-2 disabled:opacity-50">{saving?"Saving...":"Change Password"}</button>
          </div>
        </form>
      </div>
    </div>
  );
}
