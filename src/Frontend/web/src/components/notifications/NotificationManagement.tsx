'use client';

import React, { useState } from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { 
  Bell, 
  Settings, 
  BarChart3, 
  MessageSquare, 
  Users,
  Activity
} from 'lucide-react';
import { NotificationList } from './NotificationList';
import { DispatchStatusDashboard } from './DispatchStatusDashboard';
import { NotificationConfigurationPanel } from './NotificationConfigurationPanel';
import { useNotifications } from '@/hooks/useNotifications';
import { useRealTimeNotifications } from '@/hooks/useRealTimeNotifications';

interface NotificationManagementProps {
  className?: string;
  defaultTab?: string;
}

export function NotificationManagement({ 
  className, 
  defaultTab = 'notifications' 
}: NotificationManagementProps) {
  const [activeTab, setActiveTab] = useState(defaultTab);
  const { unreadCount, totalCount } = useNotifications();
  const { isConnected, connectionState } = useRealTimeNotifications();

  const getConnectionStatusBadge = () => {
    switch (connectionState) {
      case 'Connected':
        return (
          <Badge variant="solid" color="success" className="bg-green-100 text-green-800">
            <Activity className="h-3 w-3 mr-1" />
            Live
          </Badge>
        );
      case 'Reconnecting':
        return (
          <Badge variant="solid" color="warning" className="bg-yellow-100 text-yellow-800">
            <Activity className="h-3 w-3 mr-1 animate-pulse" />
            Reconnecting
          </Badge>
        );
      case 'Disconnected':
        return (
          <Badge variant="solid" color="error" className="bg-red-100 text-red-800">
            <Activity className="h-3 w-3 mr-1" />
            Offline
          </Badge>
        );
      default:
        return null;
    }
  };

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div>
            <h1 className="text-3xl font-bold">Notification Management</h1>
            <p className="text-muted-foreground">
              Manage notifications, monitor delivery, and configure channels
            </p>
          </div>
          <div className="flex items-center gap-2">
            {unreadCount > 0 && (
              <Badge variant="solid" color="error">
                {unreadCount} unread
              </Badge>
            )}
            {getConnectionStatusBadge()}
          </div>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Notifications</p>
                <p className="text-2xl font-bold">{totalCount}</p>
              </div>
              <Bell className="h-8 w-8 text-blue-500" />
            </div>
          </CardContent>
        </Card>
        
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Unread</p>
                <p className="text-2xl font-bold">{unreadCount}</p>
              </div>
              <MessageSquare className="h-8 w-8 text-orange-500" />
            </div>
          </CardContent>
        </Card>
        
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Connection Status</p>
                <p className="text-2xl font-bold">{isConnected ? 'Online' : 'Offline'}</p>
              </div>
              <Activity className={`h-8 w-8 ${isConnected ? 'text-green-500' : 'text-red-500'}`} />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Main Content Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-4">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="notifications" className="flex items-center gap-2">
            <Bell className="h-4 w-4" />
            Notifications
            {unreadCount > 0 && (
              <Badge variant="solid" color="error" className="ml-1 h-5 w-5 p-0 text-xs">
                {unreadCount}
              </Badge>
            )}
          </TabsTrigger>
          <TabsTrigger value="dispatch" className="flex items-center gap-2">
            <BarChart3 className="h-4 w-4" />
            Dispatch Status
          </TabsTrigger>
          <TabsTrigger value="configuration" className="flex items-center gap-2">
            <Settings className="h-4 w-4" />
            Configuration
          </TabsTrigger>
        </TabsList>

        <TabsContent value="notifications" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Bell className="h-5 w-5" />
                User Notifications
              </CardTitle>
            </CardHeader>
            <CardContent>
              <NotificationList 
                showFilters={true}
                onNotificationClick={(notification) => {
                  console.log('Notification clicked:', notification);
                  // Handle navigation based on notification type
                }}
              />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="dispatch" className="space-y-4">
          <DispatchStatusDashboard />
        </TabsContent>

        <TabsContent value="configuration" className="space-y-4">
          <NotificationConfigurationPanel />
        </TabsContent>
      </Tabs>
    </div>
  );
}
