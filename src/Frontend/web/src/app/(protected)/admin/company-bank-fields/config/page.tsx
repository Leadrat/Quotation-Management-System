"use client";
import { useState, useEffect } from "react";
import Link from "next/link";
import { CountriesApi } from "@/lib/api";

export default function BankFieldConfigCountrySelectorPage() {
  const [countries, setCountries] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCountries();
  }, []);

  const loadCountries = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await CountriesApi.list({ isActive: true });
      setCountries(res.data || []);
    } catch (e: any) {
      setError(e.message || "Failed to load countries");
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="p-6">Loading...</div>;

  return (
    <div className="p-6">
      <div className="mb-6">
        <Link href="/admin/company-bank-fields" className="text-sm text-blue-600 hover:text-blue-700 mb-4 inline-block">
          ← Back to Bank Field Types
        </Link>
        <h1 className="text-3xl font-bold mb-2">Select Country for Configuration</h1>
        <p className="text-gray-600">Choose a country to configure bank field types</p>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded text-red-700">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {countries.map((country) => (
          <Link
            key={country.countryId}
            href={`/admin/company-bank-fields/config/${country.countryId}`}
            className="bg-white rounded border p-4 hover:shadow-md transition-shadow"
          >
            <h3 className="font-semibold text-lg">{country.countryName}</h3>
            <p className="text-sm text-gray-500 mt-1">{country.countryCode}</p>
          </Link>
        ))}
      </div>
      {countries.length === 0 && (
        <div className="p-8 text-center text-gray-500">No countries found</div>
      )}
    </div>
  );
}

