"use client";
import { useState, useEffect } from "react";
import Label from "../tailadmin/form/Label";
import Input from "../tailadmin/form/input/InputField";
import { CompanyBankDetailsApi, type CompanyBankField } from "@/lib/api/companyBankDetails";

interface DynamicCompanyBankDetailsFormProps {
  selectedCountryId?: string;
  onValuesChange: (values: Record<string, string>) => void;
  initialValues?: Record<string, string>;
}

export default function DynamicCompanyBankDetailsForm({
  selectedCountryId,
  onValuesChange,
  initialValues = {},
}: DynamicCompanyBankDetailsFormProps) {
  const [fields, setFields] = useState<CompanyBankField[]>([]);
  const [values, setValues] = useState<Record<string, string>>(initialValues);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

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

  const loadFields = async (countryId: string) => {
    setLoading(true);
    setError(null);
    setValidationErrors({});
    try {
      const res = await CompanyBankDetailsApi.getByCountry(countryId);
      const loadedFields = res.data?.fields || [];
      setFields(loadedFields);

      // Initialize values from loaded fields
      const initialVals: Record<string, string> = {};
      loadedFields.forEach((field) => {
        if (field.value) {
          initialVals[field.bankFieldTypeId] = field.value;
        } else if (initialValues[field.bankFieldTypeId]) {
          initialVals[field.bankFieldTypeId] = initialValues[field.bankFieldTypeId];
        }
      });
      setValues(initialVals);
    } catch (e: any) {
      setError(e.message || "Failed to load bank detail fields");
      setFields([]);
    } finally {
      setLoading(false);
    }
  };

  const validateField = (field: CompanyBankField, value: string): string | null => {
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

  const handleFieldChange = (bankFieldTypeId: string, value: string) => {
    const field = fields.find((f) => f.bankFieldTypeId === bankFieldTypeId);
    if (field) {
      const error = validateField(field, value);
      setValidationErrors((prev) => {
        if (error) {
          return { ...prev, [bankFieldTypeId]: error };
        } else {
          const updated = { ...prev };
          delete updated[bankFieldTypeId];
          return updated;
        }
      });
    }

    setValues((prev) => ({
      ...prev,
      [bankFieldTypeId]: value,
    }));
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-brand-600"></div>
        <span className="ml-3 text-gray-600 dark:text-gray-400">Loading bank detail fields...</span>
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
          <div key={field.bankFieldTypeId}>
            <Label>
              {field.displayName}
              {field.isRequired && <span className="text-red-600 ml-1">*</span>}
            </Label>
            <Input
              value={values[field.bankFieldTypeId] || ""}
              onChange={(e) => handleFieldChange(field.bankFieldTypeId, e.target.value)}
              placeholder={field.helpText || `Enter ${field.displayName.toLowerCase()}`}
              maxLength={field.maxLength || undefined}
              required={field.isRequired}
              className={
                validationErrors[field.bankFieldTypeId]
                  ? "border-red-500 focus:ring-red-500"
                  : ""
              }
            />
            {validationErrors[field.bankFieldTypeId] && (
              <p className="mt-1 text-sm text-red-600">
                {validationErrors[field.bankFieldTypeId]}
              </p>
            )}
            {field.helpText && !validationErrors[field.bankFieldTypeId] && (
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

