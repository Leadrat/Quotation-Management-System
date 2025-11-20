import { apiFetch } from "../api";

export interface CountryIdentifierConfiguration {
  configurationId: string;
  countryId: string;
  countryName?: string;
  identifierTypeId: string;
  identifierTypeName?: string;
  identifierTypeDisplayName?: string;
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

export interface ConfigureCountryIdentifierRequest {
  countryId: string;
  identifierTypeId: string;
  isRequired?: boolean;
  validationRegex?: string;
  minLength?: number;
  maxLength?: number;
  displayName?: string;
  helpText?: string;
  displayOrder?: number;
}

export interface UpdateCountryIdentifierConfigurationRequest {
  isRequired: boolean;
  validationRegex?: string;
  minLength?: number;
  maxLength?: number;
  displayName?: string;
  helpText?: string;
  displayOrder: number;
  isActive: boolean;
}

export const CountryIdentifierConfigurationsApi = {
  list: (params?: { countryId?: string; identifierTypeId?: string; includeInactive?: boolean }) => {
    const q = new URLSearchParams();
    if (params?.countryId) q.append("countryId", params.countryId);
    if (params?.identifierTypeId) q.append("identifierTypeId", params.identifierTypeId);
    if (params?.includeInactive) q.append("includeInactive", "true");
    return apiFetch<{ success: boolean; data: CountryIdentifierConfiguration[] }>(
      `/api/v1/admin/country-identifier-configurations?${q.toString()}`,
      { auth: true }
    );
  },
  
  getByCountry: (countryId: string, includeInactive = false) =>
    apiFetch<{ success: boolean; data: CountryIdentifierConfiguration[] }>(
      `/api/v1/admin/country-identifier-configurations/countries/${countryId}?includeInactive=${includeInactive}`,
      { auth: true }
    ),
  
  configure: (payload: ConfigureCountryIdentifierRequest) =>
    apiFetch<{ success: boolean; data: CountryIdentifierConfiguration; message?: string }>(
      "/api/v1/admin/country-identifier-configurations",
      { method: "POST", body: JSON.stringify(payload), auth: true }
    ),
  
  update: (configurationId: string, payload: UpdateCountryIdentifierConfigurationRequest) =>
    apiFetch<{ success: boolean; data: CountryIdentifierConfiguration; message?: string }>(
      `/api/v1/admin/country-identifier-configurations/${configurationId}`,
      { method: "PUT", body: JSON.stringify(payload), auth: true }
    ),
};

