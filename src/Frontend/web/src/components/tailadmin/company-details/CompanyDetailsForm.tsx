"use client";
import { useState, useEffect } from "react";
import Label from "../form/Label";
import Input from "../form/input/InputField";
import LogoUpload from "./LogoUpload";
import DynamicCompanyIdentifiersForm from "../../admin/DynamicCompanyIdentifiersForm";
import DynamicCompanyBankDetailsForm from "../../admin/DynamicCompanyBankDetailsForm";
import { CompanyIdentifiersApi } from "@/lib/api/companyIdentifiers";
import { CompanyBankDetailsApi } from "@/lib/api/companyBankDetails";
import { CountriesApi } from "@/lib/api";

interface CompanyDetailsFormData {
  panNumber?: string;
  tanNumber?: string;
  gstNumber?: string;
  companyName?: string;
  companyAddress?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  countryId?: string;
  contactEmail?: string;
  contactPhone?: string;
  website?: string;
  legalDisclaimer?: string;
  logoUrl?: string;
  bankDetails: Array<{
    bankDetailsId?: string;
    country: string;
    accountNumber: string;
    ifscCode?: string;
    iban?: string;
    swiftCode?: string;
    bankName: string;
    branchName?: string;
  }>;
}

interface CompanyDetailsFormProps {
  initialData?: CompanyDetailsFormData;
  onSubmit: (data: CompanyDetailsFormData) => Promise<void>;
  onLogoUpload: (file: File) => Promise<void>;
  saving?: boolean;
  uploadingLogo?: boolean;
}

export default function CompanyDetailsForm({
  initialData,
  onSubmit,
  onLogoUpload,
  saving = false,
  uploadingLogo = false,
}: CompanyDetailsFormProps) {
  const [form, setForm] = useState<CompanyDetailsFormData>(() => {
    const data = initialData || { bankDetails: [] };
    return {
      panNumber: data.panNumber || "",
      tanNumber: data.tanNumber || "",
      gstNumber: data.gstNumber || "",
      companyName: data.companyName || "",
      companyAddress: data.companyAddress || "",
      city: data.city || "",
      state: data.state || "",
      postalCode: data.postalCode || "",
      country: data.country || "",
      countryId: data.countryId,
      contactEmail: data.contactEmail || "",
      contactPhone: data.contactPhone || "",
      website: data.website || "",
      legalDisclaimer: data.legalDisclaimer || "",
      logoUrl: data.logoUrl || "",
      bankDetails: data.bankDetails || [],
    };
  });

  const [countries, setCountries] = useState<any[]>([]);
  const [identifierValues, setIdentifierValues] = useState<Record<string, string>>({});
  const [bankDetailValues, setBankDetailValues] = useState<Record<string, string>>({});
  const [loadingCountryData, setLoadingCountryData] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCountries();
  }, []);

  // Update form when initialData changes
  useEffect(() => {
    if (initialData) {
      setForm({
        panNumber: initialData.panNumber || "",
        tanNumber: initialData.tanNumber || "",
        gstNumber: initialData.gstNumber || "",
        companyName: initialData.companyName || "",
        companyAddress: initialData.companyAddress || "",
        city: initialData.city || "",
        state: initialData.state || "",
        postalCode: initialData.postalCode || "",
        country: initialData.country || "",
        countryId: initialData.countryId,
        contactEmail: initialData.contactEmail || "",
        contactPhone: initialData.contactPhone || "",
        website: initialData.website || "",
        legalDisclaimer: initialData.legalDisclaimer || "",
        logoUrl: initialData.logoUrl || "",
        bankDetails: initialData.bankDetails || [],
      });
    }
  }, [initialData]);

  const loadCountries = async () => {
    try {
      // Load all countries (not just active ones) for company details
      // Request with large pageSize to get all countries
      const res = await CountriesApi.list({ pageNumber: 1, pageSize: 1000 });
      
      // If we got paginated results and there are more, fetch all pages
      let allCountries = res.data || [];
      const totalCount = res.totalCount || allCountries.length;
      
      if (totalCount > allCountries.length) {
        // Fetch remaining pages
        const remainingPages = Math.ceil((totalCount - allCountries.length) / 1000);
        const additionalRequests = [];
        
        for (let page = 2; page <= remainingPages + 1; page++) {
          additionalRequests.push(
            CountriesApi.list({ pageNumber: page, pageSize: 1000 })
          );
        }
        
        const additionalResults = await Promise.all(additionalRequests);
        additionalResults.forEach((result: any) => {
          if (result.data) {
            allCountries = [...allCountries, ...result.data];
          }
        });
      }
      
      // Sort countries alphabetically by name for easier selection
      allCountries.sort((a: any, b: any) => 
        (a.countryName || "").localeCompare(b.countryName || "")
      );
      
      setCountries(allCountries);
      console.log(`Loaded ${allCountries.length} countries (Total in DB: ${totalCount})`);
      
      if (allCountries.length === 0) {
        setError("No countries found in the database. Please add countries first.");
      } else if (allCountries.length === 1) {
        console.warn("Only one country found in the database. You may need to add more countries via Admin > Tax Management.");
      }
    } catch (e: any) {
      console.error("Failed to load countries", e);
      setError(e.message || "Failed to load countries");
    }
  };

  const handleIdentifierValuesChange = (values: Record<string, string>) => {
    setIdentifierValues(values);
  };

  const handleBankDetailValuesChange = (values: Record<string, string>) => {
    setBankDetailValues(values);
  };

  const handleCountryChange = async (countryId: string) => {
    const country = countries.find((c) => c.countryId === countryId);
    
    // Clear all fields first
    setForm((prev) => ({
      ...prev,
      countryId: countryId,
      country: country?.countryName || "",
      companyName: "",
      companyAddress: "",
      city: "",
      state: "",
      postalCode: "",
      contactEmail: "",
      contactPhone: "",
      website: "",
      logoUrl: "",
      legalDisclaimer: "",
    }));
    
    // Clear identifier and bank detail values
    setIdentifierValues({});
    setBankDetailValues({});
    setError(null);
    
    // Load existing data for this country from API and localStorage
    if (countryId) {
      setLoadingCountryData(true);
      try {
        console.log(`Loading company details for country: ${country?.countryName || countryId}`);
        
        // Load identifier and bank detail values from API
        const [identifierRes, bankRes, companyDetailsRes] = await Promise.all([
          CompanyIdentifiersApi.getByCountry(countryId).catch(() => ({ data: { fields: [] } })),
          CompanyBankDetailsApi.getByCountry(countryId).catch(() => ({ data: { fields: [] } })),
          // Try to get company details if current country matches
          import("@/lib/api").then(api => api.CompanyDetailsApi.get().catch(() => ({ success: false, data: null }))),
        ]);

        // Extract identifier values
        const idValues: Record<string, string> = {};
        if (identifierRes.data?.fields) {
          identifierRes.data.fields.forEach((field: any) => {
            if (field.value) {
              idValues[field.identifierTypeId] = field.value;
            }
          });
        }
        setIdentifierValues(idValues);

        // Extract bank detail values
        const bankValues: Record<string, string> = {};
        if (bankRes.data?.fields) {
          bankRes.data.fields.forEach((field: any) => {
            if (field.value) {
              bankValues[field.bankFieldTypeId] = field.value;
            }
          });
        }
        setBankDetailValues(bankValues);

        // Load company details from API if current country matches
        const companyDetails = companyDetailsRes.data;
        if (companyDetails && companyDetails.countryId === countryId) {
          setForm((prev) => ({
            ...prev,
            companyName: companyDetails.companyName || "",
            companyAddress: companyDetails.companyAddress || "",
            city: companyDetails.city || "",
            state: companyDetails.state || "",
            postalCode: companyDetails.postalCode || "",
            contactEmail: companyDetails.contactEmail || "",
            contactPhone: companyDetails.contactPhone || "",
            website: companyDetails.website || "",
            logoUrl: companyDetails.logoUrl || "",
            legalDisclaimer: companyDetails.legalDisclaimer || "",
          }));
          console.log(`Loaded saved company details for ${country?.countryName} from API`);
        } else {
          // Load from localStorage as fallback
          const savedDataKey = `company_details_${countryId}`;
          const savedData = typeof window !== 'undefined' ? localStorage.getItem(savedDataKey) : null;
          if (savedData) {
            try {
              const parsed = JSON.parse(savedData);
              setForm((prev) => ({
                ...prev,
                companyName: parsed.companyName || "",
                companyAddress: parsed.companyAddress || "",
                city: parsed.city || "",
                state: parsed.state || "",
                postalCode: parsed.postalCode || "",
                contactEmail: parsed.contactEmail || "",
                contactPhone: parsed.contactPhone || "",
                website: parsed.website || "",
                logoUrl: parsed.logoUrl || "",
                legalDisclaimer: parsed.legalDisclaimer || "",
              }));
              console.log(`Loaded saved company details for ${country?.countryName} from localStorage`);
            } catch (e) {
              console.warn("Failed to parse saved company details from localStorage", e);
            }
          }
        }

        console.log(`Loaded ${identifierRes.data?.fields?.length || 0} identifier fields and ${bankRes.data?.fields?.length || 0} bank detail fields for ${country?.countryName}`);
      } catch (e: any) {
        console.error("Failed to load country-specific values", e);
        setError(e.message || "Failed to load country-specific fields");
      } finally {
        setLoadingCountryData(false);
      }
    }
  };

  const updateField = (field: keyof CompanyDetailsFormData, value: any) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!form.countryId) {
      alert("Please select a country first");
      return;
    }

    // Save identifier values
    if (form.countryId && Object.keys(identifierValues).length > 0) {
      try {
        await CompanyIdentifiersApi.save(form.countryId, {
          countryId: form.countryId,
          values: identifierValues,
        });
      } catch (e: any) {
        console.error("Failed to save identifier values:", e);
      }
    }

    // Save bank detail values
    if (form.countryId && Object.keys(bankDetailValues).length > 0) {
      try {
        await CompanyBankDetailsApi.save(form.countryId, {
          countryId: form.countryId,
          values: bankDetailValues,
        });
      } catch (e: any) {
        console.error("Failed to save bank detail values:", e);
      }
    }

    // Save main company details to API
    await onSubmit(form);

    // Also save to localStorage for this specific country (as backup/fallback)
    if (typeof window !== 'undefined' && form.countryId) {
      const savedDataKey = `company_details_${form.countryId}`;
      const dataToSave = {
        companyName: form.companyName || "",
        companyAddress: form.companyAddress || "",
        city: form.city || "",
        state: form.state || "",
        postalCode: form.postalCode || "",
        contactEmail: form.contactEmail || "",
        contactPhone: form.contactPhone || "",
        website: form.website || "",
        logoUrl: form.logoUrl || "",
        legalDisclaimer: form.legalDisclaimer || "",
        countryId: form.countryId,
        country: form.country || "",
      };
      try {
        localStorage.setItem(savedDataKey, JSON.stringify(dataToSave));
        console.log(`Saved company details for ${form.country} to localStorage`);
      } catch (e) {
        console.warn("Failed to save company details to localStorage", e);
      }
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-8">
      {/* Country Selector - At the Top */}
      <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Select Country
        </h2>
        <div>
          <Label>Country *</Label>
          <select
            value={form.countryId || ""}
            onChange={(e) => {
              const countryId = e.target.value;
              if (countryId) {
                handleCountryChange(countryId);
              } else {
                // Clear everything when no country is selected
                setForm((prev) => ({
                  ...prev,
                  countryId: undefined,
                  country: "",
                  companyName: "",
                  companyAddress: "",
                  city: "",
                  state: "",
                  postalCode: "",
                }));
                setIdentifierValues({});
                setBankDetailValues({});
              }
            }}
            required
            className="w-full rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-sm"
          >
            <option value="">Select a country</option>
            {countries.length === 0 ? (
              <option disabled>Loading countries...</option>
            ) : (
              countries.map((country) => (
                <option key={country.countryId} value={country.countryId}>
                  {country.countryName} {country.countryCode ? `(${country.countryCode})` : ""}
                </option>
              ))
            )}
          </select>
          {countries.length > 0 && (
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              {countries.length} {countries.length === 1 ? "country" : "countries"} available
            </p>
          )}
          {form.country && (
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Selected: {form.country}</p>
          )}
        </div>
      </div>

      {/* Company Information Section */}
      {form.countryId && (
        <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Company Information
          </h2>
          <div className="space-y-4">
            <div>
              <Label htmlFor="companyName">Company Name</Label>
              <Input
                id="companyName"
                value={form.companyName || ""}
                onChange={(e) => updateField("companyName", e.target.value)}
                placeholder="Enter company name"
                aria-label="Company name"
              />
            </div>
            <div>
              <Label>Company Address</Label>
              <textarea
                value={form.companyAddress || ""}
                onChange={(e) => updateField("companyAddress", e.target.value)}
                placeholder="Enter full company address"
                rows={3}
                className="w-full rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-sm"
              />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <Label>City</Label>
                <Input
                  value={form.city || ""}
                  onChange={(e) => updateField("city", e.target.value)}
                  placeholder="Enter city"
                />
              </div>
              <div>
                <Label>State</Label>
                <Input
                  value={form.state || ""}
                  onChange={(e) => updateField("state", e.target.value)}
                  placeholder="Enter state"
                />
              </div>
              <div>
                <Label>Postal Code</Label>
                <Input
                  value={form.postalCode || ""}
                  onChange={(e) => updateField("postalCode", e.target.value)}
                  placeholder="Enter postal code"
                />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Contact Information Section */}
      <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Contact Information
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label>Contact Email</Label>
            <Input
              type="email"
              value={form.contactEmail || ""}
              onChange={(e) => updateField("contactEmail", e.target.value)}
              placeholder="contact@company.com"
            />
          </div>
          <div>
            <Label>Contact Phone</Label>
            <Input
              value={form.contactPhone || ""}
              onChange={(e) => updateField("contactPhone", e.target.value)}
              placeholder="+91-22-12345678"
            />
          </div>
          <div className="md:col-span-2">
            <Label>Website</Label>
            <Input
              type="url"
              value={form.website || ""}
              onChange={(e) => updateField("website", e.target.value)}
              placeholder="https://www.company.com"
            />
          </div>
        </div>
      </div>

      {/* Logo Upload Section */}
      <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
        <LogoUpload
          logoUrl={form.logoUrl}
          onUpload={onLogoUpload}
          uploading={uploadingLogo}
        />
      </div>

      {/* Error message */}
      {error && !loadingCountryData && form.countryId && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
          <div className="flex items-start">
            <svg className="w-5 h-5 text-red-600 dark:text-red-400 mt-0.5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            </svg>
            <p className="text-red-700 dark:text-red-400 text-sm">{error}</p>
          </div>
        </div>
      )}

      {/* Dynamic Company Identifiers Section (Country-Specific) */}
      {form.countryId && (
        <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-1">
              Company Identifiers
            </h2>
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Country-specific company identifiers for <strong className="text-brand-600">{form.country}</strong>
            </p>
          </div>
          {loadingCountryData ? (
            <div className="flex items-center justify-center py-8">
              <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-brand-600"></div>
              <span className="ml-3 text-gray-600 dark:text-gray-400">Loading identifier fields...</span>
            </div>
          ) : (
            <DynamicCompanyIdentifiersForm
              selectedCountryId={form.countryId}
              onValuesChange={handleIdentifierValuesChange}
              initialValues={identifierValues}
            />
          )}
        </div>
      )}

      {/* Dynamic Company Bank Details Section (Country-Specific) */}
      {form.countryId && (
        <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-1">
              Bank Details
            </h2>
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Country-specific bank details for <strong className="text-brand-600">{form.country}</strong>
            </p>
          </div>
          {loadingCountryData ? (
            <div className="flex items-center justify-center py-8">
              <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-brand-600"></div>
              <span className="ml-3 text-gray-600 dark:text-gray-400">Loading bank detail fields...</span>
            </div>
          ) : (
            <DynamicCompanyBankDetailsForm
              selectedCountryId={form.countryId}
              onValuesChange={handleBankDetailValuesChange}
              initialValues={bankDetailValues}
            />
          )}
        </div>
      )}

      {/* Legal Disclaimer Section */}
      <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Legal Disclaimer
        </h2>
        <div>
          <Label>Disclaimer Text</Label>
          <textarea
            value={form.legalDisclaimer || ""}
            onChange={(e) => updateField("legalDisclaimer", e.target.value)}
            placeholder="Enter legal disclaimer text for quotations"
            rows={4}
            className="w-full rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-sm"
          />
        </div>
      </div>

      {/* Submit Button - Only show when country is selected */}
      {form.countryId && (
        <div className="flex justify-end gap-4">
          <button
            type="submit"
            disabled={saving}
            className="px-6 py-2 bg-brand-600 hover:bg-brand-700 text-white rounded-lg font-medium disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2"
            aria-label="Save company details"
          >
            {saving ? "Saving..." : "Save Company Details"}
          </button>
        </div>
      )}
    </form>
  );
}

