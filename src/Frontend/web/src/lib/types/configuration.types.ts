import { NotificationChannel } from './dispatch.types';

export interface NotificationTemplate {
  templateKey: string;
  eventType: string;
  channel: NotificationChannel;
  subject?: string;
  bodyTemplate: string;
  isActive: boolean;
  requiredVariables: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateNotificationTemplateRequest {
  templateKey: string;
  eventType: string;
  channel: NotificationChannel;
  subject?: string;
  bodyTemplate: string;
  isActive: boolean;
  requiredVariables: string[];
}

export interface UpdateNotificationTemplateRequest {
  subject?: string;
  bodyTemplate: string;
  isActive: boolean;
  requiredVariables: string[];
}

export interface TestTemplateRequest {
  templateData: Record<string, any>;
  recipientEmail?: string;
  recipientPhone?: string;
}

export interface TestTemplateResponse {
  success: boolean;
  renderedSubject?: string;
  renderedBody: string;
  errors: string[];
}

export interface ChannelConfiguration {
  channel: NotificationChannel;
  isEnabled: boolean;
  maxRetryAttempts: number;
  retryDelaySeconds: number;
  timeoutSeconds: number;
  settings: Record<string, any>;
}

export interface ConfigurationTestResult {
  channel: NotificationChannel;
  isValid: boolean;
  testResults: string[];
  errors: string[];
}

export interface TemplateVariable {
  name: string;
  description: string;
  type: string;
  required: boolean;
  example?: string;
}