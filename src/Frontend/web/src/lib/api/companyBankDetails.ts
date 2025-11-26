import { apiFetch } from "../api";

export interface CompanyBankField {
  bankFieldTypeId: string;
  bankFieldTypeName: string;
  displayName: string;
  value?: string;
  isRequired: boolean;
  validationRegex?: string;
  minLength?: number;
  maxLength?: number;
  helpText?: string;
  displayOrder: number;
}

export interface CompanyBankDetails {
  countryId: string;
  countryName?: string;
  fields: CompanyBankField[];
}

export interface SaveCompanyBankDetailsRequest {
  countryId: string;
  values: Record<string, string>; // Key: bankFieldTypeId (as string), Value: field value
}

export const CompanyBankDetailsApi = {
  getByCountry: (countryId: string) =>
    apiFetch<{ success: boolean; data: CompanyBankDetails }>(
      `/api/v1/company-details/bank-details/countries/${countryId}`,
      { auth: true }
    ),
  
  save: (countryId: string, payload: SaveCompanyBankDetailsRequest) =>
    apiFetch<{ success: boolean; data: CompanyBankDetails; message?: string }>(
      `/api/v1/company-details/bank-details/countries/${countryId}`,
      { method: "PUT", body: JSON.stringify(payload), auth: true }
    ),
};

