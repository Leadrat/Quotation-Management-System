import { apiFetch } from "../api";

export interface CountryBankFieldConfiguration {
  configurationId: string;
  countryId: string;
  countryName?: string;
  bankFieldTypeId: string;
  bankFieldTypeName?: string;
  bankFieldTypeDisplayName?: string;
  isRequired: boolean;
  validationRegex?: string;
  minLength?: number;
  maxLength?: number;
  displayName?: string;
  helpText?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ConfigureCountryBankFieldRequest {
  countryId: string;
  bankFieldTypeId: string;
  isRequired?: boolean;
  validationRegex?: string;
  minLength?: number;
  maxLength?: number;
  displayName?: string;
  helpText?: string;
  displayOrder?: number;
}

export interface UpdateCountryBankFieldConfigurationRequest {
  isRequired: boolean;
  validationRegex?: string;
  minLength?: number;
  maxLength?: number;
  displayName?: string;
  helpText?: string;
  displayOrder: number;
  isActive: boolean;
}

export const CountryBankFieldConfigurationsApi = {
  list: (params?: { countryId?: string; bankFieldTypeId?: string; includeInactive?: boolean }) => {
    const q = new URLSearchParams();
    if (params?.countryId) q.append("countryId", params.countryId);
    if (params?.bankFieldTypeId) q.append("bankFieldTypeId", params.bankFieldTypeId);
    if (params?.includeInactive) q.append("includeInactive", "true");
    return apiFetch<{ success: boolean; data: CountryBankFieldConfiguration[] }>(
      `/api/v1/admin/country-bank-field-configurations?${q.toString()}`,
      { auth: true }
    );
  },
  
  getByCountry: (countryId: string, includeInactive = false) =>
    apiFetch<{ success: boolean; data: CountryBankFieldConfiguration[] }>(
      `/api/v1/admin/country-bank-field-configurations/countries/${countryId}?includeInactive=${includeInactive}`,
      { auth: true }
    ),
  
  configure: (payload: ConfigureCountryBankFieldRequest) =>
    apiFetch<{ success: boolean; data: CountryBankFieldConfiguration; message?: string }>(
      "/api/v1/admin/country-bank-field-configurations",
      { method: "POST", body: JSON.stringify(payload), auth: true }
    ),
  
  update: (configurationId: string, payload: UpdateCountryBankFieldConfigurationRequest) =>
    apiFetch<{ success: boolean; data: CountryBankFieldConfiguration; message?: string }>(
      `/api/v1/admin/country-bank-field-configurations/${configurationId}`,
      { method: "PUT", body: JSON.stringify(payload), auth: true }
    ),
};

