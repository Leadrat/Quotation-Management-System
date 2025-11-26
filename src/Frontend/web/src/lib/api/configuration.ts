import { apiClient } from './client';
import { 
  NotificationTemplate,
  ChannelConfiguration,
  CreateNotificationTemplateRequest,
  UpdateNotificationTemplateRequest,
  TestTemplateRequest,
  TestTemplateResponse,
  ConfigurationTestResult
} from '@/lib/types/configuration.types';

export const configurationApi = {
  /**
   * Get all notification templates
   */
  async getTemplates(params?: {
    channel?: string;
    isActive?: boolean;
  }): Promise<NotificationTemplate[]> {
    const searchParams = new URLSearchParams();
    
    if (params?.channel) searchParams.set('channel', params.channel);
    if (params?.isActive !== undefined) searchParams.set('isActive', params.isActive.toString());

    const response = await apiClient.get(`/notification-configuration/templates?${searchParams.toString()}`);
    return response.data;
  },

  /**
   * Get a specific notification template
   */
  async getTemplate(templateKey: string): Promise<NotificationTemplate> {
    const response = await apiClient.get(`/notification-configuration/templates/${templateKey}`);
    return response.data;
  },

  /**
   * Create a new notification template
   */
  async createTemplate(template: CreateNotificationTemplateRequest): Promise<NotificationTemplate> {
    const response = await apiClient.post('/notification-configuration/templates', template);
    return response.data;
  },

  /**
   * Update an existing notification template
   */
  async updateTemplate(
    templateKey: string, 
    template: UpdateNotificationTemplateRequest
  ): Promise<NotificationTemplate> {
    const response = await apiClient.put(`/notification-configuration/templates/${templateKey}`, template);
    return response.data;
  },

  /**
   * Delete a notification template
   */
  async deleteTemplate(templateKey: string): Promise<void> {
    await apiClient.delete(`/notification-configuration/templates/${templateKey}`);
  },

  /**
   * Test a notification template with sample data
   */
  async testTemplate(
    templateKey: string, 
    request: TestTemplateRequest
  ): Promise<TestTemplateResponse> {
    const response = await apiClient.post(
      `/notification-configuration/templates/${templateKey}/test`, 
      request
    );
    return response.data;
  },

  /**
   * Get available template variables for a specific template type
   */
  async getTemplateVariables(eventType: string): Promise<string[]> {
    const response = await apiClient.get(`/notification-configuration/templates/variables/${eventType}`);
    return response.data;
  },

  /**
   * Get all channel configurations
   */
  async getChannelConfigurations(): Promise<ChannelConfiguration[]> {
    const response = await apiClient.get('/notification-configuration/channels');
    return response.data;
  },

  /**
   * Get configuration for a specific channel
   */
  async getChannelConfiguration(channel: string): Promise<ChannelConfiguration> {
    const response = await apiClient.get(`/notification-configuration/channels/${channel}`);
    return response.data;
  },

  /**
   * Update channel configuration
   */
  async updateChannelConfiguration(
    channel: string, 
    configuration: ChannelConfiguration
  ): Promise<ChannelConfiguration> {
    const response = await apiClient.put(
      `/notification-configuration/channels/${channel}`, 
      configuration
    );
    return response.data;
  },

  /**
   * Test a channel configuration
   */
  async testChannelConfiguration(
    channel: string, 
    configuration: ChannelConfiguration
  ): Promise<ConfigurationTestResult> {
    const response = await apiClient.post(
      `/notification-configuration/channels/${channel}/test`, 
      configuration
    );
    return response.data;
  },

  /**
   * Reload all channel configurations (hot reload)
   */
  async reloadConfigurations(): Promise<void> {
    await apiClient.post('/notification-configuration/reload');
  }
};