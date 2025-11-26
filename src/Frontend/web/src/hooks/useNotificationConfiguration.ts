import { useState, useEffect, useCallback } from 'react';
import { configurationApi } from '@/lib/api/configuration';
import { 
  NotificationTemplate, 
  ChannelConfiguration, 
  CreateNotificationTemplateRequest,
  UpdateNotificationTemplateRequest,
  TestTemplateRequest,
  TestTemplateResponse,
  ConfigurationTestResult
} from '@/lib/types/configuration.types';
import { NotificationChannel } from '@/lib/types/dispatch.types';

export interface UseNotificationTemplatesResult {
  templates: NotificationTemplate[];
  loading: boolean;
  error: string | null;
  createTemplate: (template: CreateNotificationTemplateRequest) => Promise<NotificationTemplate>;
  updateTemplate: (templateKey: string, template: UpdateNotificationTemplateRequest) => Promise<NotificationTemplate>;
  deleteTemplate: (templateKey: string) => Promise<void>;
  testTemplate: (templateKey: string, request: TestTemplateRequest) => Promise<TestTemplateResponse>;
  refresh: () => Promise<void>;
}

export function useNotificationTemplates(
  channel?: NotificationChannel,
  isActive?: boolean
): UseNotificationTemplatesResult {
  const [templates, setTemplates] = useState<NotificationTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadTemplates = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const data = await configurationApi.getTemplates({
        channel: channel || undefined,
        isActive
      });
      
      setTemplates(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load templates');
      console.error('Error loading templates:', err);
    } finally {
      setLoading(false);
    }
  }, [channel, isActive]);

  const createTemplate = useCallback(async (template: CreateNotificationTemplateRequest) => {
    try {
      const newTemplate = await configurationApi.createTemplate(template);
      setTemplates(prev => [...prev, newTemplate]);
      return newTemplate;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to create template';
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, []);

  const updateTemplate = useCallback(async (
    templateKey: string, 
    template: UpdateNotificationTemplateRequest
  ) => {
    try {
      const updatedTemplate = await configurationApi.updateTemplate(templateKey, template);
      setTemplates(prev => 
        prev.map(t => t.templateKey === templateKey ? updatedTemplate : t)
      );
      return updatedTemplate;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update template';
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, []);

  const deleteTemplate = useCallback(async (templateKey: string) => {
    try {
      await configurationApi.deleteTemplate(templateKey);
      setTemplates(prev => prev.filter(t => t.templateKey !== templateKey));
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete template';
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, []);

  const testTemplate = useCallback(async (
    templateKey: string, 
    request: TestTemplateRequest
  ): Promise<TestTemplateResponse> => {
    try {
      return await configurationApi.testTemplate(templateKey, request);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to test template';
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, []);

  useEffect(() => {
    loadTemplates();
  }, [loadTemplates]);

  return {
    templates,
    loading,
    error,
    createTemplate,
    updateTemplate,
    deleteTemplate,
    testTemplate,
    refresh: loadTemplates
  };
}

export interface UseChannelConfigurationResult {
  configurations: ChannelConfiguration[];
  loading: boolean;
  error: string | null;
  updateConfiguration: (channel: NotificationChannel, config: ChannelConfiguration) => Promise<ChannelConfiguration>;
  testConfiguration: (channel: NotificationChannel, config: ChannelConfiguration) => Promise<ConfigurationTestResult>;
  reloadConfigurations: () => Promise<void>;
  refresh: () => Promise<void>;
}

export function useChannelConfiguration(): UseChannelConfigurationResult {
  const [configurations, setConfigurations] = useState<ChannelConfiguration[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadConfigurations = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const data = await configurationApi.getChannelConfigurations();
      setConfigurations(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load configurations');
      console.error('Error loading configurations:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  const updateConfiguration = useCallback(async (
    channel: NotificationChannel, 
    config: ChannelConfiguration
  ) => {
    try {
      const updatedConfig = await configurationApi.updateChannelConfiguration(channel, config);
      setConfigurations(prev => 
        prev.map(c => c.channel === channel ? updatedConfig : c)
      );
      return updatedConfig;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update configuration';
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, []);

  const testConfiguration = useCallback(async (
    channel: NotificationChannel, 
    config: ChannelConfiguration
  ): Promise<ConfigurationTestResult> => {
    try {
      return await configurationApi.testChannelConfiguration(channel, config);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to test configuration';
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, []);

  const reloadConfigurations = useCallback(async () => {
    try {
      await configurationApi.reloadConfigurations();
      await loadConfigurations(); // Refresh after reload
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to reload configurations';
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, [loadConfigurations]);

  useEffect(() => {
    loadConfigurations();
  }, [loadConfigurations]);

  return {
    configurations,
    loading,
    error,
    updateConfiguration,
    testConfiguration,
    reloadConfigurations,
    refresh: loadConfigurations
  };
}