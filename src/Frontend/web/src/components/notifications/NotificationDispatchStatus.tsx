'use client';

import React from 'react';
import Badge from '@/components/tailadmin/ui/badge/Badge';
import Button from '@/components/tailadmin/ui/button/Button';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { 
  CheckCircle, 
  XCircle, 
  Clock, 
  AlertTriangle, 
  RefreshCw,
  Mail,
  MessageSquare,
  Smartphone
} from 'lucide-react';

interface NotificationDispatchStatusProps {
  dispatchStatuses: any[];
  onRetry?: (dispatchId: number) => void;
  showDetails?: boolean;
}

const getChannelIcon = (channel: string) => {
  switch (channel.toLowerCase()) {
    case 'email':
      return <Mail className="h-4 w-4" />;
    case 'sms':
      return <Smartphone className="h-4 w-4" />;
    case 'inapp':
    case 'in-app':
      return <MessageSquare className="h-4 w-4" />;
    default:
      return <MessageSquare className="h-4 w-4" />;
  }
};

const getStatusIcon = (status: string) => {
  switch (status.toLowerCase()) {
    case 'delivered':
      return <CheckCircle className="h-4 w-4 text-green-500" />;
    case 'failed':
      return <XCircle className="h-4 w-4 text-red-500" />;
    case 'permanently_failed':
      return <AlertTriangle className="h-4 w-4 text-red-600" />;
    case 'pending':
      return <Clock className="h-4 w-4 text-yellow-500" />;
    case 'cancelled':
      return <XCircle className="h-4 w-4 text-gray-500" />;
    default:
      return <Clock className="h-4 w-4 text-gray-400" />;
  }
};

const getStatusBadgeVariant = (status: string) => {
  switch (status.toLowerCase()) {
    case 'delivered':
      return 'default';
    case 'failed':
    case 'permanently_failed':
      return 'destructive';
    case 'pending':
      return 'secondary';
    case 'cancelled':
      return 'outline';
    default:
      return 'outline';
  }
};

const formatChannelName = (channel: string) => {
  switch (channel.toLowerCase()) {
    case 'email':
      return 'Email';
    case 'sms':
      return 'SMS';
    case 'inapp':
    case 'in-app':
      return 'In-App';
    default:
      return channel;
  }
};

export function NotificationDispatchStatus({
  dispatchStatuses,
  onRetry,
  showDetails = false
}: NotificationDispatchStatusProps) {
  if (!dispatchStatuses || dispatchStatuses.length === 0) {
    return (
      <div className="text-sm text-muted-foreground">
        No dispatch information available
      </div>
    );
  }

  if (!showDetails) {
    // Compact view - show only status icons
    return (
      <div className="flex items-center gap-2">
        {dispatchStatuses.map((dispatch) => (
          <TooltipProvider key={dispatch.id}>
            <Tooltip>
              <TooltipTrigger asChild>
                <div className="flex items-center gap-1">
                  {getChannelIcon(dispatch.channel)}
                  {getStatusIcon(dispatch.status)}
                </div>
              </TooltipTrigger>
              <TooltipContent>
                <div className="text-sm">
                  <div className="font-medium">
                    {formatChannelName(dispatch.channel)}
                  </div>
                  <div className="text-muted-foreground">
                    {dispatch.status.replace('_', ' ')}
                  </div>
                  {dispatch.attemptedAt && (
                    <div className="text-xs">
                      {formatDistanceToNow(new Date(dispatch.attemptedAt), { addSuffix: true })}
                    </div>
                  )}
                </div>
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
        ))}
      </div>
    );
  }

  // Detailed view
  return (
    <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
      <div className="border-b border-stroke px-5 py-4 dark:border-strokedark">
        <h3 className="text-sm font-semibold text-black dark:text-white">Dispatch Status</h3>
      </div>
      <div className="p-5 space-y-3">
        {dispatchStatuses.map((dispatch) => (
          <div key={dispatch.id} className="flex items-center justify-between p-3 border border-stroke rounded-lg dark:border-strokedark">
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2">
                {getChannelIcon(dispatch.channel)}
                <span className="font-medium text-sm text-black dark:text-white">
                  {formatChannelName(dispatch.channel)}
                </span>
              </div>
              <Badge variant="solid" color={dispatch.status.toLowerCase() === 'delivered' ? 'success' : dispatch.status.toLowerCase() === 'failed' ? 'error' : 'warning'}>
                <div className="flex items-center gap-1">
                  {getStatusIcon(dispatch.status)}
                  {dispatch.status.replace('_', ' ')}
                </div>
              </Badge>
              {dispatch.attemptNumber > 1 && (
                <Badge variant="light" color="info" className="text-xs">
                  Attempt {dispatch.attemptNumber}
                </Badge>
              )}
            </div>
            <div className="flex items-center gap-2">
              <div className="text-right text-sm text-gray-600 dark:text-gray-400">
                {dispatch.deliveredAt ? (
                  <div>Delivered recently</div>
                ) : dispatch.attemptedAt ? (
                  <div>Attempted recently</div>
                ) : null}
                {dispatch.nextRetryAt && (
                  <div className="text-xs">Next retry scheduled</div>
                )}
              </div>
              {dispatch.status.toLowerCase() === 'failed' && onRetry && (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => onRetry(dispatch.id)}
                >
                  <RefreshCw className="h-3 w-3 mr-1" />
                  Retry
                </Button>
              )}
            </div>
          </div>
        ))}
        {dispatchStatuses.some(d => d.errorMessage) && (
          <div className="mt-4">
            <h4 className="text-sm font-medium mb-2 text-black dark:text-white">Error Details</h4>
            <div className="space-y-2">
              {dispatchStatuses
                .filter(d => d.errorMessage)
                .map((dispatch) => (
                  <div key={dispatch.id} className="p-2 bg-red-50 border border-red-200 rounded text-sm dark:bg-red-900/20 dark:border-red-800">
                    <div className="font-medium text-red-800 dark:text-red-300">
                      {formatChannelName(dispatch.channel)} Error:
                    </div>
                    <div className="text-red-700 dark:text-red-400 mt-1">
                      {dispatch.errorMessage}
                    </div>
                  </div>
                ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
