// TypeScript types for Quotation Templates matching backend DTOs

export type TemplateVisibility = "Public" | "Team" | "Private";

export interface TemplateLineItem {
  lineItemId: string;
  templateId: string;
  sequenceNumber: number;
  itemName: string;
  description?: string;
  quantity: number;
  unitRate: number;
  amount: number;
  createdAt: string;
}

export interface QuotationTemplate {
  templateId: string;
  name: string;
  description?: string;
  ownerUserId: string;
  ownerUserName: string;
  ownerRole: string;
  visibility: TemplateVisibility;
  isApproved: boolean;
  approvedByUserId?: string;
  approvedByUserName?: string;
  approvedAt?: string;
  version: number;
  previousVersionId?: string;
  usageCount: number;
  lastUsedAt?: string;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string;
  discountDefault?: number;
  notes?: string;
  lineItems: TemplateLineItem[];
  isActive: boolean;
  isEditable: boolean;
}

export interface CreateQuotationTemplateRequest {
  name: string;
  description?: string;
  visibility: TemplateVisibility;
  discountDefault?: number;
  notes?: string;
  lineItems: CreateTemplateLineItemRequest[];
}

export interface CreateTemplateLineItemRequest {
  itemName: string;
  description?: string;
  quantity: number;
  unitRate: number;
}

export interface UpdateQuotationTemplateRequest {
  name?: string;
  description?: string;
  visibility?: TemplateVisibility;
  discountDefault?: number;
  notes?: string;
  lineItems?: UpdateTemplateLineItemRequest[];
}

export interface UpdateTemplateLineItemRequest {
  lineItemId?: string;
  itemName: string;
  description?: string;
  quantity: number;
  unitRate: number;
}

export interface QuotationTemplateVersion {
  templateId: string;
  name: string;
  description?: string;
  version: number;
  previousVersionId?: string;
  updatedByUserName: string;
  updatedAt: string;
  isCurrentVersion: boolean;
}

export interface TemplateUsageStats {
  totalTemplates: number;
  totalUsage: number;
  approvedTemplates: number;
  pendingApprovalTemplates: number;
  mostUsedTemplates: MostUsedTemplate[];
  usageByVisibility: Record<string, number>;
  usageByRole: Record<string, number>;
}

export interface MostUsedTemplate {
  templateId: string;
  name: string;
  usageCount: number;
  lastUsedAt?: string;
}

export interface PagedTemplatesResult {
  success: boolean;
  data: {
    data: QuotationTemplate[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
  };
}

export interface TemplateResponse {
  success: boolean;
  data: QuotationTemplate;
}

export interface TemplatesListResponse {
  success: boolean;
  data: QuotationTemplate[];
}

export interface TemplateVersionsResponse {
  success: boolean;
  data: QuotationTemplateVersion[];
}

export interface TemplateUsageStatsResponse {
  success: boolean;
  data: TemplateUsageStats;
}

export interface ApplyTemplateResponse {
  success: boolean;
  data: {
    clientId: string;
    quotationDate?: string;
    validUntil?: string;
    discountPercentage: number;
    notes?: string;
    lineItems: Array<{
      itemName: string;
      description?: string;
      quantity: number;
      unitRate: number;
    }>;
  };
}

