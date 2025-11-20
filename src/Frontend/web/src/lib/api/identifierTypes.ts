import { apiFetch } from "../api";

export interface IdentifierType {
  identifierTypeId: string;
  name: string;
  displayName: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateIdentifierTypeRequest {
  name: string;
  displayName: string;
  description?: string;
}

export interface UpdateIdentifierTypeRequest {
  displayName: string;
  description?: string;
  isActive: boolean;
}

export const IdentifierTypesApi = {
  list: (includeInactive = false) =>
    apiFetch<{ success: boolean; data: IdentifierType[] }>(
      `/api/v1/admin/identifier-types?includeInactive=${includeInactive}`,
      { auth: true }
    ),
  
  create: (payload: CreateIdentifierTypeRequest) =>
    apiFetch<{ success: boolean; data: IdentifierType; message?: string }>(
      "/api/v1/admin/identifier-types",
      { method: "POST", body: JSON.stringify(payload), auth: true }
    ),
  
  update: (identifierTypeId: string, payload: UpdateIdentifierTypeRequest) =>
    apiFetch<{ success: boolean; data: IdentifierType; message?: string }>(
      `/api/v1/admin/identifier-types/${identifierTypeId}`,
      { method: "PUT", body: JSON.stringify(payload), auth: true }
    ),
};

