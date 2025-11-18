// Admin Types (Spec-018)

// System Settings
export interface SystemSettingsDto {
  companyName: string;
  defaultCurrency: string;
  dateFormat: string;
  timeFormat: string;
  timeZone: string;
  emailNotificationsEnabled: boolean;
  smsNotificationsEnabled: boolean;
  updatedAt: string;
}

export interface UpdateSystemSettingsRequest {
  settings: {
    companyName?: string;
    defaultCurrency?: string;
    dateFormat?: string;
    timeFormat?: string;
    timeZone?: string;
    emailNotificationsEnabled?: boolean;
    smsNotificationsEnabled?: boolean;
  };
}

// Integration Keys
export interface IntegrationKeyDto {
  id: string;
  keyName: string;
  provider: string;
  keyValue?: string; // Masked or decrypted value
  isMasked?: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
}

export interface CreateIntegrationKeyRequest {
  keyName: string;
  keyValue: string;
  provider: string;
}

export interface UpdateIntegrationKeyRequest {
  keyName?: string;
  keyValue?: string;
  provider?: string;
}

// Audit Logs
export interface AuditLogDto {
  id: string;
  actionType: string;
  entity: string;
  entityId?: string;
  performedBy: string;
  performedByName?: string;
  ipAddress?: string;
  timestamp: string;
  changes?: any;
}

// Branding
export interface CustomBrandingDto {
  id: string;
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  logoUrl?: string;
  footerHtml?: string;
  updatedAt: string;
  updatedBy: string;
}

export interface UpdateBrandingRequest {
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  footerHtml?: string;
}

// Data Retention
export interface DataRetentionPolicyDto {
  id: string;
  entityType: string;
  retentionPeriodMonths: number;
  isActive: boolean;
  autoPurgeEnabled: boolean;
  updatedAt: string;
  updatedBy: string;
}

export interface UpdateDataRetentionPolicyRequest {
  entityType: string;
  retentionPeriodMonths: number;
  isActive: boolean;
  autoPurgeEnabled: boolean;
}

// Notification Settings
export interface NotificationSettingsDto {
  id: string;
  bannerMessage?: string;
  bannerType: "info" | "warning" | "error" | "success";
  isVisible: boolean;
  updatedAt: string;
  updatedBy: string;
}

export interface UpdateNotificationSettingsRequest {
  bannerMessage?: string;
  bannerType?: "info" | "warning" | "error" | "success";
  isVisible?: boolean;
}

