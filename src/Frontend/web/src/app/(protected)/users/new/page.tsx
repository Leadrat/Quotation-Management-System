"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { UsersApi, RolesApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Alert from "@/components/tailadmin/ui/alert/Alert";

const SALES_REP_ROLE_ID = "FAE6CEDB-42FD-497B-85F6-F2B14ECA0079";
const MANAGER_ROLE_ID = "8D38F43B-EB54-4E4A-9582-1C611F7B5DF6";

export default function AdminCreateUserPage() {
  const router = useRouter();
  const [role, setRole] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [roles, setRoles] = useState<Array<{ roleId: string; roleName: string }>>([]);
  const [managers, setManagers] = useState<Array<{ userId: string; firstName: string; lastName: string; email: string }>>([]);
  const [loadingRoles, setLoadingRoles] = useState(true);
  const [loadingManagers, setLoadingManagers] = useState(false);
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
    
    // Load roles
    if (token) {
      RolesApi.list({ isActive: true, pageSize: 100 }).then(res => {
        // Filter out Client role as per backend validation
        setRoles(res.data.filter((r: any) => r.roleName !== "Client"));
        setLoadingRoles(false);
      }).catch(() => {
        setLoadingRoles(false);
      });
    }
  }, []);

  // Load managers when SalesRep role is selected
  useEffect(() => {
    if (form.roleId === SALES_REP_ROLE_ID) {
      setLoadingManagers(true);
      UsersApi.list({ pageSize: 100 }).then(res => {
        // Filter to only managers
        const managerUsers = res.data.filter((u: any) => 
          u.roleId === MANAGER_ROLE_ID && u.isActive !== false
        );
        setManagers(managerUsers);
        setLoadingManagers(false);
      }).catch(() => {
        setManagers([]);
        setLoadingManagers(false);
      });
    } else {
      setManagers([]);
      setLoadingManagers(false);
      if (form.roleId !== SALES_REP_ROLE_ID) {
        setForm((prev: any) => ({ ...prev, reportingManagerId: "" }));
      }
    }
  }, [form.roleId]);

  async function submit() {
    if (role !== "Admin") { setError("Only Admin can create users"); return; }
    
    // Validate required fields
    if (!form.email || !form.password || !form.firstName || !form.lastName || !form.roleId) {
      setError("Please fill in all required fields");
      return;
    }

    // Validate roleId is a valid GUID
    const roleIdGuid = form.roleId.trim();
    if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(roleIdGuid)) {
      setError("Invalid role selected. Please select a valid role.");
      return;
    }

    // Validate password strength (backend requires: min 8 chars, uppercase, lowercase, number, special char)
    const password = form.password;
    if (password.length < 8) {
      setError("Password must be at least 8 characters long");
      return;
    }
    if (!/[A-Z]/.test(password)) {
      setError("Password must contain at least one uppercase letter");
      return;
    }
    if (!/[a-z]/.test(password)) {
      setError("Password must contain at least one lowercase letter");
      return;
    }
    if (!/[0-9]/.test(password)) {
      setError("Password must contain at least one number");
      return;
    }
    if (!/[!@#$%^&*(),.?\\"{}|<>]/.test(password)) {
      setError("Password must contain at least one special character (!@#$%^&*(),.?\"{}|<>)");
      return;
    }

    setSaving(true);
    setError(null);
    try {
      // Prepare payload with proper types
      const payload: any = {
        email: form.email.trim(),
        password: form.password,
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        roleId: roleIdGuid, // Ensure it's a valid GUID string
      };

      // Handle optional reportingManagerId - only include if provided
      if (form.reportingManagerId && form.reportingManagerId.trim()) {
        const managerId = form.reportingManagerId.trim();
        // Validate it's a GUID if provided
        if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(managerId)) {
          setError("Invalid Reporting Manager ID format. Must be a valid GUID.");
          setSaving(false);
          return;
        }
        payload.reportingManagerId = managerId;
      }

      // Only include phone fields if they have values
      if (form.mobile && form.mobile.trim()) {
        payload.mobile = form.mobile.trim();
      }
      if (form.phoneCode && form.phoneCode.trim()) {
        payload.phoneCode = form.phoneCode.trim();
      }

      // Debug: Log the payload being sent (remove in production)
      console.log("Creating user with payload:", JSON.stringify(payload, null, 2));

      await UsersApi.create(payload);
      alert("User created");
      router.replace("/dashboard");
    } catch (e: any) {
      // Debug: Log the full error for troubleshooting
      console.error("Error creating user:", e);
      console.error("Error status:", e.status);
      console.error("Error details:", e.details);
      console.error("Error errors:", e.errors);

      // Extract detailed error message from backend
      let errorMessage = "Failed to create user";
      if (e.message) {
        errorMessage = e.message;
      }
      // Check for validation errors in the response
      if (e.errors && Array.isArray(e.errors)) {
        const validationErrors = e.errors.map((err: any) => err.message || err).join(", ");
        errorMessage = `Validation errors: ${validationErrors}`;
      } else if (e.details) {
        errorMessage = `${errorMessage}: ${e.details}`;
      }
      // If it's a 400 error, provide more context
      if (e.status === 400) {
        errorMessage = `Bad Request (400): ${errorMessage}. Please check that all fields are valid (role ID must be a valid GUID, password meets requirements, etc.)`;
      }
      setError(errorMessage);
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      <PageBreadcrumb pageTitle="Create User" />
      
      <ComponentCard title="Create New User" desc="Only Admin users can create new user accounts">
        {error && <div className="mb-4"><Alert variant="error" title="Error" message={error} /></div>}
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label>Email <span className="text-error-500">*</span></Label>
            <Input
              type="email"
              value={form.email}
              onChange={(e)=>setForm((prev:any)=>({...prev,email:e.target.value}))}
              placeholder="user@example.com"
            />
          </div>
          <div>
            <Label>Password <span className="text-error-500">*</span></Label>
            <Input
              type="password"
              value={form.password}
              onChange={(e)=>setForm((prev:any)=>({...prev,password:e.target.value}))}
              placeholder="Enter password"
            />
          </div>
          <div>
            <Label>First Name <span className="text-error-500">*</span></Label>
            <Input
              value={form.firstName}
              onChange={(e)=>setForm((prev:any)=>({...prev,firstName:e.target.value}))}
              placeholder="First name"
            />
          </div>
          <div>
            <Label>Last Name <span className="text-error-500">*</span></Label>
            <Input
              value={form.lastName}
              onChange={(e)=>setForm((prev:any)=>({...prev,lastName:e.target.value}))}
              placeholder="Last name"
            />
          </div>
          <div>
            <Label>Role <span className="text-error-500">*</span></Label>
            {loadingRoles ? (
              <Input value="Loading roles..." disabled />
            ) : (
              <select
                value={form.roleId}
                onChange={(e) => setForm((prev: any) => ({ ...prev, roleId: e.target.value }))}
                className="h-11 w-full appearance-none rounded-lg border border-gray-300 px-4 py-2.5 pr-11 text-sm shadow-theme-xs focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
              >
                <option value="">Select a role</option>
                {roles.map((r) => (
                  <option key={r.roleId} value={r.roleId}>
                    {r.roleName}
                  </option>
                ))}
              </select>
            )}
          </div>
          <div>
            <Label>
              Reporting Manager
            </Label>
            {form.roleId === SALES_REP_ROLE_ID ? (
              <div>
                {loadingManagers ? (
                  <p className="text-xs text-gray-500 dark:text-gray-400 mb-2">Loading managers...</p>
                ) : managers.length > 0 ? (
                  <select
                    value={form.reportingManagerId || ""}
                    onChange={(e) => setForm((prev: any) => ({ ...prev, reportingManagerId: e.target.value }))}
                    className="h-11 w-full appearance-none rounded-lg border border-gray-300 px-4 py-2.5 pr-11 text-sm shadow-theme-xs focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 mb-2"
                  >
                    <option value="">Select a manager or enter ID below</option>
                    {managers.map((m) => (
                      <option key={m.userId} value={m.userId}>
                        {m.firstName} {m.lastName} ({m.email})
                      </option>
                    ))}
                  </select>
                ) : (
                  <p className="text-xs text-gray-500 dark:text-gray-400 mb-2">
                    No managers found. Please enter a Manager User ID below.
                  </p>
                )}
                <input
                  type="text"
                  value={form.reportingManagerId || ""}
                  onChange={(e) => {
                    setForm((prev: any) => ({ ...prev, reportingManagerId: e.target.value }));
                  }}
                  placeholder="Enter Manager User ID (GUID)"
                  className="h-11 w-full rounded-lg border border-gray-300 px-4 py-2.5 text-sm shadow-theme-xs placeholder:text-gray-400 focus:outline-hidden focus:ring-3 focus:border-brand-300 focus:ring-brand-500/10 dark:bg-gray-900 dark:text-white/90 dark:border-gray-700 dark:placeholder:text-white/30"
                />
              </div>
            ) : (
              <Input
                value={form.reportingManagerId}
                onChange={(e) => setForm((prev: any) => ({ ...prev, reportingManagerId: e.target.value }))}
                placeholder="Manager ID"
                
              />
            )}
          </div>
          <div>
            <Label>Phone Code</Label>
            <Input
              value={form.phoneCode}
              onChange={(e)=>setForm((prev:any)=>({...prev,phoneCode:e.target.value}))}
              placeholder="+91"
            />
          </div>
          <div>
            <Label>Mobile</Label>
            <Input
              value={form.mobile}
              onChange={(e)=>setForm((prev:any)=>({...prev,mobile:e.target.value}))}
              placeholder="Mobile number"
            />
          </div>
        </div>
        
        <div className="mt-6 flex gap-2">
          <Button onClick={submit} disabled={saving || role!=="Admin"}>
            {saving ? "Saving..." : "Create User"}
          </Button>
          <Button variant="outline" onClick={()=>router.back()}>
            Cancel
          </Button>
        </div>
      </ComponentCard>
    </>
  );
}
