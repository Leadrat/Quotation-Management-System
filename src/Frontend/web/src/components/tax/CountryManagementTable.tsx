"use client";
import { useState, useEffect } from "react";
import { CountriesApi } from "@/lib/api";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Badge from "@/components/tailadmin/ui/badge/Badge";
import Button from "@/components/tailadmin/ui/button/Button";

interface CountryManagementTableProps {
  onEdit?: (country: any) => void;
  onDelete?: (countryId: string) => void;
  onViewJurisdictions?: (countryId: string) => void;
}

export default function CountryManagementTable({
  onEdit,
  onDelete,
  onViewJurisdictions,
}: CountryManagementTableProps) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [countries, setCountries] = useState<any[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    setLoading(true);
    setError(null);
    try {
      const res = await CountriesApi.list();
      setCountries(Array.isArray(res.data) ? res.data : []);
    } catch (e: any) {
      const errorMsg = e.message || "Failed to load countries";
      setError(errorMsg);
      setCountries([]);
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
        <p className="mt-2 text-gray-600">Loading countries...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
        {error}
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableCell>Country Name</TableCell>
            <TableCell>Country Code</TableCell>
            <TableCell>Tax Framework</TableCell>
            <TableCell>Currency</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Default</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {countries.length === 0 ? (
            <TableRow>
              <TableCell colSpan={7} className="text-center py-8 text-gray-500">
                No countries found
              </TableCell>
            </TableRow>
          ) : (
            countries.map((country) => (
              <TableRow key={country.countryId}>
                <TableCell className="font-medium">{country.countryName}</TableCell>
                <TableCell>{country.countryCode}</TableCell>
                <TableCell>
                  <Badge className="bg-blue-100 text-blue-800">
                    {country.taxFrameworkType || "N/A"}
                  </Badge>
                </TableCell>
                <TableCell>{country.defaultCurrency}</TableCell>
                <TableCell>
                  {country.isActive ? (
                    <Badge className="bg-green-100 text-green-800">Active</Badge>
                  ) : (
                    <Badge className="bg-gray-100 text-gray-800">Inactive</Badge>
                  )}
                </TableCell>
                <TableCell>
                  {country.isDefault ? (
                    <Badge className="bg-yellow-100 text-yellow-800">Default</Badge>
                  ) : (
                    <span className="text-gray-400">-</span>
                  )}
                </TableCell>
                <TableCell>
                  <div className="flex gap-2">
                    {onViewJurisdictions && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onViewJurisdictions(country.countryId)}
                      >
                        Jurisdictions
                      </Button>
                    )}
                    {onEdit && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onEdit(country)}
                      >
                        Edit
                      </Button>
                    )}
                    {onDelete && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => {
                          if (confirm("Are you sure you want to delete this country?")) {
                            onDelete(country.countryId);
                          }
                        }}
                      >
                        Delete
                      </Button>
                    )}
                  </div>
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}

