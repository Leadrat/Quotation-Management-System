import { apiFetch } from "../api";

export interface BankFieldType {
  bankFieldTypeId: string;
  name: string;
  displayName: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateBankFieldTypeRequest {
  name: string;
  displayName: string;
  description?: string;
}

export interface UpdateBankFieldTypeRequest {
  displayName: string;
  description?: string;
  isActive: boolean;
}

export const BankFieldTypesApi = {
  list: (includeInactive = false) =>
    apiFetch<{ success: boolean; data: BankFieldType[] }>(
      `/api/v1/admin/bank-field-types?includeInactive=${includeInactive}`,
      { auth: true }
    ),
  
  create: (payload: CreateBankFieldTypeRequest) =>
    apiFetch<{ success: boolean; data: BankFieldType; message?: string }>(
      "/api/v1/admin/bank-field-types",
      { method: "POST", body: JSON.stringify(payload), auth: true }
    ),
  
  update: (bankFieldTypeId: string, payload: UpdateBankFieldTypeRequest) =>
    apiFetch<{ success: boolean; data: BankFieldType; message?: string }>(
      `/api/v1/admin/bank-field-types/${bankFieldTypeId}`,
      { method: "PUT", body: JSON.stringify(payload), auth: true }
    ),
};

