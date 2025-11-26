'use client';

import { useState } from 'react';
import Button from '@/components/tailadmin/ui/button/Button';
import Badge from '@/components/tailadmin/ui/badge/Badge';
import Label from '@/components/tailadmin/form/Label';
import { 
  Settings, 
  Save, 
  TestTube, 
  Mail, 
  MessageSquare, 
  Smartphone,
  RefreshCw,
  Plus,
  Trash2,
  Eye
} from 'lucide-react';

interface NotificationConfigurationPanelProps {
  className?: string;
}

interface ChannelSettings {
  SmtpServer?: string;
  SmtpPort?: number;
  FromAddress?: string;
  FromName?: string;
  UseSsl?: boolean;
  Provider?: string;
  FromNumber?: string;
  ConnectionTimeout?: number;
  MaxConnections?: number;
}

interface ChannelConfig {
  channel: string;
  isEnabled: boolean;
  maxRetryAttempts: number;
  retryDelaySeconds: number;
  timeoutSeconds: number;
  settings: ChannelSettings;
}

// Mock data for demonstration
const mockTemplates = [
  {
    templateKey: 'quotation-sent',
    eventType: 'QuotationSent',
    channel: 'EMAIL',
    subject: 'Your quotation has been sent',
    bodyTemplate: 'Hello {{userName}}, your quotation {{quotationId}} has been sent.',
    isActive: true,
    requiredVariables: ['userName', 'quotationId']
  },
  {
    templateKey: 'payment-reminder',
    eventType: 'PaymentReminder',
    channel: 'SMS',
    subject: '',
    bodyTemplate: 'Payment reminder for {{amount}} due on {{dueDate}}',
    isActive: true,
    requiredVariables: ['amount', 'dueDate']
  }
];

const mockConfigurations: ChannelConfig[] = [
  {
    channel: 'EMAIL',
    isEnabled: true,
    maxRetryAttempts: 3,
    retryDelaySeconds: 60,
    timeoutSeconds: 30,
    settings: {
      SmtpServer: 'smtp.example.com',
      SmtpPort: 587,
      FromAddress: 'noreply@example.com',
      FromName: 'CRM System',
      UseSsl: true
    }
  },
  {
    channel: 'SMS',
    isEnabled: true,
    maxRetryAttempts: 2,
    retryDelaySeconds: 30,
    timeoutSeconds: 15,
    settings: {
      Provider: 'Twilio',
      FromNumber: '+1234567890'
    }
  },
  {
    channel: 'IN_APP',
    isEnabled: true,
    maxRetryAttempts: 1,
    retryDelaySeconds: 5,
    timeoutSeconds: 10,
    settings: {
      ConnectionTimeout: 30,
      MaxConnections: 1000
    }
  }
];

export function NotificationConfigurationPanel({ className }: NotificationConfigurationPanelProps) {
  const [activeTab, setActiveTab] = useState('templates');
  const [loading, setLoading] = useState(false);
  
  // Form state for new template
  const [newTemplate, setNewTemplate] = useState({
    templateKey: '',
    eventType: '',
    bodyTemplate: ''
  });
  
  // Form state for channel configurations
  const [channelConfigs, setChannelConfigs] = useState<ChannelConfig[]>(mockConfigurations);

  const getChannelIcon = (channel: string) => {
    switch (channel) {
      case 'EMAIL':
        return <Mail className="h-5 w-5" />;
      case 'SMS':
        return <Smartphone className="h-5 w-5" />;
      case 'IN_APP':
        return <MessageSquare className="h-5 w-5" />;
      default:
        return <Settings className="h-5 w-5" />;
    }
  };

  const refresh = () => {
    setLoading(true);
    setTimeout(() => setLoading(false), 1000);
  };

  if (loading) {
    return (
      <div className={`flex items-center justify-center h-64 ${className}`}>
        <RefreshCw className="h-8 w-8 animate-spin" />
      </div>
    );
  }

  return (
    <div className={`space-y-6 ${className}`}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-black dark:text-white">Notification Configuration</h2>
          <p className="text-gray-600 dark:text-gray-400">
            Manage notification templates and channel settings
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={refresh} size="sm">
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Button variant="outline" size="sm">
            <Settings className="h-4 w-4 mr-2" />
            Reload Config
          </Button>
        </div>
      </div>

      {/* Tabs */}
      <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
        <div className="border-b border-stroke px-5 py-4 dark:border-strokedark">
          <div className="flex gap-4">
            {['templates', 'channels'].map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`px-4 py-2 font-medium text-sm border-b-2 transition-colors ${
                  activeTab === tab
                    ? "border-primary text-primary"
                    : "border-transparent text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white"
                }`}
              >
                {tab === 'templates' ? 'Templates' : 'Channel Settings'}
              </button>
            ))}
          </div>
        </div>

        <div className="p-5">
          {activeTab === 'templates' && (
            <div className="space-y-6">
              {/* Create New Template */}
              <div className="rounded-sm border border-stroke bg-white p-6 shadow-default dark:border-strokedark dark:bg-boxdark">
                <div className="flex items-center gap-2 mb-4">
                  <Plus className="h-5 w-5" />
                  <h3 className="text-lg font-semibold text-black dark:text-white">Create New Template</h3>
                </div>
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <Label htmlFor="templateKey">Template Key</Label>
                      <input
                        id="templateKey"
                        value={newTemplate.templateKey}
                        onChange={(e) => setNewTemplate(prev => ({ ...prev, templateKey: e.target.value }))}
                        className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                        placeholder="quotation-approved"
                      />
                    </div>
                    <div>
                      <Label htmlFor="eventType">Event Type</Label>
                      <input
                        id="eventType"
                        value={newTemplate.eventType}
                        onChange={(e) => setNewTemplate(prev => ({ ...prev, eventType: e.target.value }))}
                        className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                        placeholder="QuotationApproved"
                      />
                    </div>
                  </div>
                  
                  <div>
                    <Label htmlFor="bodyTemplate">Body Template</Label>
                    <textarea
                      id="bodyTemplate"
                      rows={4}
                      value={newTemplate.bodyTemplate}
                      onChange={(e) => setNewTemplate(prev => ({ ...prev, bodyTemplate: e.target.value }))}
                      className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                      placeholder="Hello {{userName}}, your quotation {{quotationId}} has been approved."
                    />
                  </div>

                  <div className="flex justify-end">
                    <Button>
                      <Plus className="h-4 w-4 mr-2" />
                      Create Template
                    </Button>
                  </div>
                </div>
              </div>

              {/* Existing Templates */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                {mockTemplates.map((template) => (
                  <div key={template.templateKey} className="rounded-sm border border-stroke bg-white p-6 shadow-default dark:border-strokedark dark:bg-boxdark">
                    <div className="flex items-center justify-between mb-4">
                      <div className="flex items-center gap-2">
                        {getChannelIcon(template.channel)}
                        <h4 className="font-semibold text-black dark:text-white">{template.templateKey}</h4>
                      </div>
                      <div className="flex items-center gap-2">
                        <Badge variant="solid" color={template.isActive ? 'success' : 'light'}>
                          {template.isActive ? 'Active' : 'Inactive'}
                        </Badge>
                        <Badge variant="light" color="info">
                          {template.channel.toUpperCase()}
                        </Badge>
                      </div>
                    </div>
                    
                    <div className="space-y-3">
                      <div>
                        <Label className="text-sm font-medium">Event Type</Label>
                        <p className="text-sm text-gray-600 dark:text-gray-400">{template.eventType}</p>
                      </div>

                      {template.subject && (
                        <div>
                          <Label className="text-sm font-medium">Subject</Label>
                          <p className="text-sm text-gray-600 dark:text-gray-400">{template.subject}</p>
                        </div>
                      )}

                      <div>
                        <Label className="text-sm font-medium">Body Template</Label>
                        <p className="text-sm text-gray-600 dark:text-gray-400 line-clamp-3">
                          {template.bodyTemplate}
                        </p>
                      </div>

                      {template.requiredVariables.length > 0 && (
                        <div>
                          <Label className="text-sm font-medium">Required Variables</Label>
                          <div className="flex flex-wrap gap-1 mt-1">
                            {template.requiredVariables.map((variable) => (
                              <Badge key={variable} variant="light" color="dark">
                                {variable}
                              </Badge>
                            ))}
                          </div>
                        </div>
                      )}

                      <div className="flex gap-2 pt-2">
                        <Button size="sm" variant="outline" className="flex-1">
                          <TestTube className="h-4 w-4 mr-2" />
                          Test
                        </Button>
                        <Button size="sm" variant="outline" className="flex-1">
                          <Eye className="h-4 w-4 mr-2" />
                          Edit
                        </Button>
                        <Button size="sm" variant="outline">
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {activeTab === 'channels' && (
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              {channelConfigs.map((config) => (
                <div key={config.channel} className="rounded-sm border border-stroke bg-white p-6 shadow-default dark:border-strokedark dark:bg-boxdark">
                  <div className="flex items-center gap-2 mb-4">
                    {getChannelIcon(config.channel)}
                    <h4 className="font-semibold text-black dark:text-white">{config.channel.toUpperCase()} Configuration</h4>
                    <Badge variant="solid" color={config.isEnabled ? 'success' : 'light'}>
                      {config.isEnabled ? 'Enabled' : 'Disabled'}
                    </Badge>
                  </div>

                  <div className="space-y-4">
                    {/* Retry Settings */}
                    <div>
                      <Label htmlFor={`${config.channel}-retries`}>Max Retry Attempts</Label>
                      <input
                        id={`${config.channel}-retries`}
                        type="number"
                        value={config.maxRetryAttempts}
                        onChange={(e) => {
                          const newConfigs = channelConfigs.map(c => 
                            c.channel === config.channel 
                              ? { ...c, maxRetryAttempts: parseInt(e.target.value) || 0 }
                              : c
                          );
                          setChannelConfigs(newConfigs);
                        }}
                        className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                        min="0"
                        max="10"
                      />
                    </div>
                    <div>
                      <Label htmlFor={`${config.channel}-delay`}>Retry Delay (seconds)</Label>
                      <input
                        id={`${config.channel}-delay`}
                        type="number"
                        value={config.retryDelaySeconds}
                        onChange={(e) => {
                          const newConfigs = channelConfigs.map(c => 
                            c.channel === config.channel 
                              ? { ...c, retryDelaySeconds: parseInt(e.target.value) || 1 }
                              : c
                          );
                          setChannelConfigs(newConfigs);
                        }}
                        className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                        min="1"
                        max="3600"
                      />
                    </div>

                    {/* Channel-specific Settings */}
                    {config.channel === 'EMAIL' && (
                      <div className="space-y-3">
                        <div>
                          <Label>SMTP Server</Label>
                          <input
                            value={config.settings.SmtpServer || ''}
                            onChange={(e) => {
                              const newConfigs = channelConfigs.map(c => 
                                c.channel === config.channel 
                                  ? { ...c, settings: { ...c.settings, SmtpServer: e.target.value } }
                                  : c
                              );
                              setChannelConfigs(newConfigs);
                            }}
                            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                            placeholder="smtp.example.com"
                          />
                        </div>
                        <div>
                          <Label>From Address</Label>
                          <input
                            value={config.settings.FromAddress || ''}
                            onChange={(e) => {
                              const newConfigs = channelConfigs.map(c => 
                                c.channel === config.channel 
                                  ? { ...c, settings: { ...c.settings, FromAddress: e.target.value } }
                                  : c
                              );
                              setChannelConfigs(newConfigs);
                            }}
                            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                            placeholder="noreply@example.com"
                          />
                        </div>
                      </div>
                    )}

                    {config.channel === 'SMS' && (
                      <div className="space-y-3">
                        <div>
                          <Label>SMS Provider</Label>
                          <input
                            value={config.settings.Provider || ''}
                            onChange={(e) => {
                              const newConfigs = channelConfigs.map(c => 
                                c.channel === config.channel 
                                  ? { ...c, settings: { ...c.settings, Provider: e.target.value } }
                                  : c
                              );
                              setChannelConfigs(newConfigs);
                            }}
                            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                            placeholder="Twilio"
                          />
                        </div>
                        <div>
                          <Label>From Number</Label>
                          <input
                            value={config.settings.FromNumber || ''}
                            onChange={(e) => {
                              const newConfigs = channelConfigs.map(c => 
                                c.channel === config.channel 
                                  ? { ...c, settings: { ...c.settings, FromNumber: e.target.value } }
                                  : c
                              );
                              setChannelConfigs(newConfigs);
                            }}
                            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 text-black outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
                            placeholder="+1234567890"
                          />
                        </div>
                      </div>
                    )}

                    <div className="flex gap-2 pt-4">
                      <Button variant="outline" size="sm" className="flex-1">
                        <TestTube className="h-4 w-4 mr-2" />
                        Test
                      </Button>
                      <Button size="sm" className="flex-1">
                        <Save className="h-4 w-4 mr-2" />
                        Save
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
