import { useEffect, useRef, useState, useCallback } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useNotificationStore } from '@/store/notificationStore';
import type { Notification } from '@/lib/types/notification.types';

export interface UseRealTimeNotificationsResult {
  isConnected: boolean;
  connectionState: string;
  error: string | null;
  connect: () => Promise<void>;
  disconnect: () => Promise<void>;
  reconnect: () => Promise<void>;
}

export function useRealTimeNotifications(): UseRealTimeNotificationsResult {
  const [isConnected, setIsConnected] = useState(false);
  const [connectionState, setConnectionState] = useState('Disconnected');
  const [error, setError] = useState<string | null>(null);
  const connectionRef = useRef<HubConnection | null>(null);
  const { addNotification, updateNotification, refreshUnreadCount } = useNotificationStore();

  const connect = useCallback(async () => {
    if (connectionRef.current?.state === 'Connected') {
      return;
    }

    try {
      setError(null);
      
      const connection = new HubConnectionBuilder()
        .withUrl('/api/hubs/notifications', {
          withCredentials: true,
          headers: {
            Authorization: `Bearer ${localStorage.getItem('authToken')}`
          }
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff: 0, 2, 10, 30 seconds, then 30 seconds
            if (retryContext.previousRetryCount === 0) return 0;
            if (retryContext.previousRetryCount === 1) return 2000;
            if (retryContext.previousRetryCount === 2) return 10000;
            return 30000;
          }
        })
        .configureLogging(LogLevel.Information)
        .build();

      // Set up event handlers
      connection.on('NotificationReceived', (notification: Notification) => {
        console.log('Real-time notification received:', notification);
        addNotification(notification);
        
        // Show browser notification if permission granted
        if (Notification.permission === 'granted') {
          new Notification(notification.title, {
            body: notification.message,
            icon: '/favicon.ico',
            tag: notification.notificationId
          });
        }
      });

      connection.on('NotificationRead', (notificationId: string) => {
        console.log('Notification marked as read:', notificationId);
        updateNotification(notificationId, { isRead: true, readAt: new Date().toISOString() });
        refreshUnreadCount();
      });

      connection.on('NotificationStatusUpdated', (notificationId: string, dispatchStatus: any) => {
        console.log('Notification dispatch status updated:', notificationId, dispatchStatus);
        // Update the notification with new dispatch status
        updateNotification(notificationId, { dispatchStatus: [dispatchStatus] });
      });

      connection.on('UnreadCountUpdated', (count: number) => {
        console.log('Unread count updated:', count);
        useNotificationStore.getState().setUnreadCount(count);
      });

      // Connection state handlers
      connection.onclose((error) => {
        console.log('SignalR connection closed:', error);
        setIsConnected(false);
        setConnectionState('Disconnected');
        if (error) {
          setError(error.message);
        }
      });

      connection.onreconnecting((error) => {
        console.log('SignalR reconnecting:', error);
        setIsConnected(false);
        setConnectionState('Reconnecting');
        setError(null);
      });

      connection.onreconnected((connectionId) => {
        console.log('SignalR reconnected:', connectionId);
        setIsConnected(true);
        setConnectionState('Connected');
        setError(null);
        
        // Refresh notifications after reconnection to sync any missed updates
        refreshUnreadCount();
      });

      // Start the connection
      await connection.start();
      
      connectionRef.current = connection;
      setIsConnected(true);
      setConnectionState('Connected');
      
      console.log('SignalR connection established');
      
      // Join user-specific group
      await connection.invoke('JoinUserGroup');
      
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to connect to notification hub';
      console.error('SignalR connection error:', err);
      setError(errorMessage);
      setIsConnected(false);
      setConnectionState('Disconnected');
    }
  }, [addNotification, updateNotification, refreshUnreadCount]);

  const disconnect = useCallback(async () => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.stop();
        connectionRef.current = null;
        setIsConnected(false);
        setConnectionState('Disconnected');
        setError(null);
        console.log('SignalR connection disconnected');
      } catch (err) {
        console.error('Error disconnecting SignalR:', err);
      }
    }
  }, []);

  const reconnect = useCallback(async () => {
    await disconnect();
    await connect();
  }, [connect, disconnect]);

  // Auto-connect on mount and handle cleanup
  useEffect(() => {
    // Request notification permission
    if ('Notification' in window && Notification.permission === 'default') {
      Notification.requestPermission();
    }

    // Auto-connect
    connect();

    // Cleanup on unmount
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
      }
    };
  }, [connect]);

  // Handle page visibility changes
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible' && !isConnected) {
        // Reconnect when page becomes visible
        connect();
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
  }, [isConnected, connect]);

  // Handle online/offline events
  useEffect(() => {
    const handleOnline = () => {
      if (!isConnected) {
        connect();
      }
    };

    const handleOffline = () => {
      setError('Connection lost - you are offline');
    };

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
    
    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, [isConnected, connect]);

  return {
    isConnected,
    connectionState,
    error,
    connect,
    disconnect,
    reconnect
  };
}