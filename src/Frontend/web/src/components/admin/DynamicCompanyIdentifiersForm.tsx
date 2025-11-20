"use client";
import { useState, useEffect } from "react";
import Label from "../tailadmin/form/Label";
import Input from "../tailadmin/form/input/InputField";
import { CompanyIdentifiersApi, type CompanyIdentifierField } from "@/lib/api/companyIdentifiers";
import { CountriesApi } from "@/lib/api";

interface DynamicCompanyIdentifiersFormProps {
  selectedCountryId?: string;
  onValuesChange: (values: Record<string, string>) => void;
  initialValues?: Record<string, string>;
}

export default function DynamicCompanyIdentifiersForm({
  selectedCountryId,
  onValuesChange,
  initialValues = {},
}: DynamicCompanyIdentifiersFormProps) {
  const [fields, setFields] = useState<CompanyIdentifierField[]>([]);
  const [values, setValues] = useState<Record<string, string>>(initialValues);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});
  const [countries, setCountries] = useState<any[]>([]);

  useEffect(() => {
    loadCountries();
  }, []);

  useEffect(() => {
    if (selectedCountryId) {
      loadFields(selectedCountryId);
    } else {
      setFields([]);
      setValues({});
      onValuesChange({});
    }
  }, [selectedCountryId]);

  useEffect(() => {
    onValuesChange(values);
  }, [values]);

  useEffect(() => {
    if (initialValues) {
      setValues(initialValues);
    }
  }, [initialValues]);

  const loadCountries = async () => {
    try {
      const res = await CountriesApi.list({ isActive: true });
      setCountries(res.data || []);
    } catch (e: any) {
      console.error("Failed to load countries", e);
    }
  };

  const loadFields = async (countryId: string) => {
    setLoading(true);
    setError(null);
    setValidationErrors({});
    try {
      const res = await CompanyIdentifiersApi.getByCountry(countryId);
      const loadedFields = res.data?.fields || [];
      setFields(loadedFields);

      // Initialize values from loaded fields
      const initialVals: Record<string, string> = {};
      loadedFields.forEach((field) => {
        if (field.value) {
          initialVals[field.identifierTypeId] = field.value;
        } else if (initialValues[field.identifierTypeId]) {
          initialVals[field.identifierTypeId] = initialValues[field.identifierTypeId];
        }
      });
      setValues(initialVals);
    } catch (e: any) {
      setError(e.message || "Failed to load identifier fields");
      setFields([]);
    } finally {
      setLoading(false);
    }
  };

  const validateField = (field: CompanyIdentifierField, value: string): string | null => {
    if (field.isRequired && !value.trim()) {
      return `${field.displayName} is required`;
    }

    if (value) {
      if (field.minLength && value.length < field.minLength) {
        return `${field.displayName} must be at least ${field.minLength} characters`;
      }

      if (field.maxLength && value.length > field.maxLength) {
        return `${field.displayName} must be no more than ${field.maxLength} characters`;
      }

      if (field.validationRegex) {
        try {
          const regex = new RegExp(field.validationRegex);
          if (!regex.test(value)) {
            return `${field.displayName} format is invalid`;
          }
        } catch {
          // Invalid regex - skip validation
        }
      }
    }

    return null;
  };

  const handleFieldChange = (identifierTypeId: string, value: string) => {
    const field = fields.find((f) => f.identifierTypeId === identifierTypeId);
    if (field) {
      const error = validateField(field, value);
      setValidationErrors((prev) => {
        if (error) {
          return { ...prev, [identifierTypeId]: error };
        } else {
          const updated = { ...prev };
          delete updated[identifierTypeId];
          return updated;
        }
      });
    }

    setValues((prev) => ({
      ...prev,
      [identifierTypeId]: value,
    }));
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-brand-600"></div>
        <span className="ml-3 text-gray-600 dark:text-gray-400">Loading identifier fields...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4 text-red-700 dark:text-red-400">
        {error}
      </div>
    );
  }

  if (fields.length === 0) {
    return null; // Don't show anything if no fields configured
  }

  const sortedFields = [...fields].sort((a, b) => a.displayOrder - b.displayOrder);

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {sortedFields.map((field) => (
          <div key={field.identifierTypeId}>
            <Label>
              {field.displayName}
              {field.isRequired && <span className="text-red-600 ml-1">*</span>}
            </Label>
            <Input
              value={values[field.identifierTypeId] || ""}
              onChange={(e) => handleFieldChange(field.identifierTypeId, e.target.value)}
              placeholder={field.helpText || `Enter ${field.displayName.toLowerCase()}`}
              maxLength={field.maxLength || undefined}
              required={field.isRequired}
              className={
                validationErrors[field.identifierTypeId]
                  ? "border-red-500 focus:ring-red-500"
                  : ""
              }
            />
            {validationErrors[field.identifierTypeId] && (
              <p className="mt-1 text-sm text-red-600">
                {validationErrors[field.identifierTypeId]}
              </p>
            )}
            {field.helpText && !validationErrors[field.identifierTypeId] && (
              <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{field.helpText}</p>
            )}
            {field.validationRegex && (
              <p className="mt-1 text-xs text-gray-400 dark:text-gray-500">
                Pattern: {field.validationRegex}
              </p>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

