import { apiFetch } from "../api";

export interface CompanyIdentifierField {
  identifierTypeId: string;
  identifierTypeName: string;
  displayName: string;
  value?: string;
  isRequired: boolean;
  validationRegex?: string;
  minLength?: number;
  maxLength?: number;
  helpText?: string;
  displayOrder: number;
}

export interface CompanyIdentifierValues {
  countryId: string;
  countryName?: string;
  fields: CompanyIdentifierField[];
}

export interface SaveCompanyIdentifierValuesRequest {
  countryId: string;
  values: Record<string, string>; // Key: identifierTypeId (as string), Value: identifier value
}

export const CompanyIdentifiersApi = {
  getByCountry: (countryId: string) =>
    apiFetch<{ success: boolean; data: CompanyIdentifierValues }>(
      `/api/v1/company-details/identifiers/countries/${countryId}`,
      { auth: true }
    ),
  
  save: (countryId: string, payload: SaveCompanyIdentifierValuesRequest) =>
    apiFetch<{ success: boolean; data: CompanyIdentifierValues; message?: string }>(
      `/api/v1/company-details/identifiers/countries/${countryId}`,
      { method: "PUT", body: JSON.stringify(payload), auth: true }
    ),
};

