'use client';

import React, { useState, useEffect } from 'react';
import Button from '@/components/tailadmin/ui/button/Button';
import Badge from '@/components/tailadmin/ui/badge/Badge';
import { 
  RefreshCw, 
  TrendingUp, 
  AlertTriangle, 
  CheckCircle, 
  Clock,
  Mail,
  MessageSquare,
  Smartphone,
  Download
} from 'lucide-react';

interface DispatchStatusDashboardProps {
  className?: string;
}

// Mock data for demonstration
const mockStatistics = {
  totalAttempts: 1250,
  successRate: 94.2,
  failedAttempts: 73,
  averageDeliveryTime: 2.3
};

const mockDispatchHistory = [
  {
    id: '1',
    notificationTitle: 'Quotation Sent',
    notificationId: 'N001',
    userId: 'U001',
    userEmail: 'john@example.com',
    channel: 'EMAIL',
    status: 'DELIVERED',
    priority: 'NORMAL',
    attemptNumber: 1,
    attemptedAt: new Date().toISOString()
  },
  {
    id: '2',
    notificationTitle: 'Payment Reminder',
    notificationId: 'N002',
    userId: 'U002',
    userEmail: 'jane@example.com',
    channel: 'SMS',
    status: 'FAILED',
    priority: 'HIGH',
    attemptNumber: 2,
    attemptedAt: new Date().toISOString()
  }
];

export function DispatchStatusDashboard({ className }: DispatchStatusDashboardProps) {
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState('history');

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'DELIVERED':
        return <CheckCircle className="h-4 w-4 text-green-500" />;
      case 'FAILED':
        return <AlertTriangle className="h-4 w-4 text-red-500" />;
      case 'PENDING':
        return <Clock className="h-4 w-4 text-yellow-500" />;
      default:
        return <Clock className="h-4 w-4 text-gray-400" />;
    }
  };

  const getChannelIcon = (channel: string) => {
    switch (channel) {
      case 'EMAIL':
        return <Mail className="h-4 w-4" />;
      case 'SMS':
        return <Smartphone className="h-4 w-4" />;
      case 'IN_APP':
        return <MessageSquare className="h-4 w-4" />;
      default:
        return <MessageSquare className="h-4 w-4" />;
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
      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="rounded-sm border border-stroke bg-white px-5 py-6 shadow-default dark:border-strokedark dark:bg-boxdark">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Total Attempts</p>
              <p className="text-2xl font-bold text-black dark:text-white">{mockStatistics.totalAttempts.toLocaleString()}</p>
            </div>
            <TrendingUp className="h-8 w-8 text-blue-500" />
          </div>
        </div>
        
        <div className="rounded-sm border border-stroke bg-white px-5 py-6 shadow-default dark:border-strokedark dark:bg-boxdark">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Success Rate</p>
              <p className="text-2xl font-bold text-black dark:text-white">{mockStatistics.successRate.toFixed(1)}%</p>
            </div>
            <CheckCircle className="h-8 w-8 text-green-500" />
          </div>
        </div>
        
        <div className="rounded-sm border border-stroke bg-white px-5 py-6 shadow-default dark:border-strokedark dark:bg-boxdark">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Failed Attempts</p>
              <p className="text-2xl font-bold text-black dark:text-white">{mockStatistics.failedAttempts.toLocaleString()}</p>
            </div>
            <AlertTriangle className="h-8 w-8 text-red-500" />
          </div>
        </div>
        
        <div className="rounded-sm border border-stroke bg-white px-5 py-6 shadow-default dark:border-strokedark dark:bg-boxdark">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Avg Delivery Time</p>
              <p className="text-2xl font-bold text-black dark:text-white">{Math.round(mockStatistics.averageDeliveryTime)}min</p>
            </div>
            <Clock className="h-8 w-8 text-yellow-500" />
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
        <div className="border-b border-stroke px-5 py-4 dark:border-strokedark">
          <div className="flex items-center justify-between">
            <div className="flex gap-4">
              {['history', 'analytics', 'failed'].map((tab) => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(tab)}
                  className={`px-4 py-2 font-medium text-sm border-b-2 transition-colors ${
                    activeTab === tab
                      ? "border-primary text-primary"
                      : "border-transparent text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white"
                  }`}
                >
                  {tab === 'history' ? 'Dispatch History' : tab === 'analytics' ? 'Analytics' : 'Failed Dispatches'}
                </button>
              ))}
            </div>
            
            <div className="flex items-center gap-2">
              <Button variant="outline" onClick={refresh} size="sm">
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh
              </Button>
              <Button variant="outline" size="sm">
                <Download className="h-4 w-4 mr-2" />
                Export
              </Button>
            </div>
          </div>
        </div>

        <div className="p-5">
          {activeTab === 'history' && (
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-black dark:text-white">Recent Dispatch History</h3>
              <div className="overflow-x-auto">
                <table className="w-full table-auto">
                  <thead>
                    <tr className="bg-gray-2 text-left dark:bg-meta-4">
                      <th className="px-4 py-4 font-medium text-black dark:text-white">Notification</th>
                      <th className="px-4 py-4 font-medium text-black dark:text-white">User</th>
                      <th className="px-4 py-4 font-medium text-black dark:text-white">Channel</th>
                      <th className="px-4 py-4 font-medium text-black dark:text-white">Status</th>
                      <th className="px-4 py-4 font-medium text-black dark:text-white">Priority</th>
                      <th className="px-4 py-4 font-medium text-black dark:text-white">Attempts</th>
                      <th className="px-4 py-4 font-medium text-black dark:text-white">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {mockDispatchHistory.map((item) => (
                      <tr key={item.id} className="border-b border-stroke dark:border-strokedark">
                        <td className="px-4 py-5">
                          <div>
                            <div className="font-medium text-black dark:text-white">{item.notificationTitle}</div>
                            <div className="text-sm text-gray-600 dark:text-gray-400">ID: {item.notificationId}</div>
                          </div>
                        </td>
                        <td className="px-4 py-5">
                          <div>
                            <div className="font-medium text-black dark:text-white">User {item.userId}</div>
                            <div className="text-sm text-gray-600 dark:text-gray-400">{item.userEmail}</div>
                          </div>
                        </td>
                        <td className="px-4 py-5">
                          <div className="flex items-center gap-2">
                            {getChannelIcon(item.channel)}
                            <Badge variant="light" color="info">
                              {item.channel.toUpperCase()}
                            </Badge>
                          </div>
                        </td>
                        <td className="px-4 py-5">
                          <div className="flex items-center gap-2">
                            {getStatusIcon(item.status)}
                            <Badge variant="solid" color={item.status === 'DELIVERED' ? 'success' : item.status === 'FAILED' ? 'error' : 'warning'}>
                              {item.status.replace('_', ' ')}
                            </Badge>
                          </div>
                        </td>
                        <td className="px-4 py-5">
                          <Badge variant="solid" color={item.priority === 'HIGH' ? 'error' : 'light'}>
                            {item.priority}
                          </Badge>
                        </td>
                        <td className="px-4 py-5">{item.attemptNumber}</td>
                        <td className="px-4 py-5">
                          {item.status === 'FAILED' && (
                            <Button size="sm" variant="outline">
                              Retry
                            </Button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {activeTab === 'analytics' && (
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-black dark:text-white">Analytics</h3>
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="rounded-sm border border-stroke bg-white p-6 shadow-default dark:border-strokedark dark:bg-boxdark">
                  <h4 className="text-md font-semibold text-black dark:text-white mb-4">Dispatch by Channel</h4>
                  <div className="space-y-3">
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600 dark:text-gray-400">Email</span>
                      <span className="font-medium">65%</span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600 dark:text-gray-400">SMS</span>
                      <span className="font-medium">25%</span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600 dark:text-gray-400">In-App</span>
                      <span className="font-medium">10%</span>
                    </div>
                  </div>
                </div>
                
                <div className="rounded-sm border border-stroke bg-white p-6 shadow-default dark:border-strokedark dark:bg-boxdark">
                  <h4 className="text-md font-semibold text-black dark:text-white mb-4">Dispatch by Status</h4>
                  <div className="space-y-3">
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600 dark:text-gray-400">Delivered</span>
                      <span className="font-medium text-green-600">94.2%</span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600 dark:text-gray-400">Failed</span>
                      <span className="font-medium text-red-600">5.8%</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'failed' && (
            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <AlertTriangle className="h-5 w-5 text-red-500" />
                <h3 className="text-lg font-semibold text-black dark:text-white">Failed Dispatches</h3>
              </div>
              <div className="text-center py-8 text-gray-600 dark:text-gray-400">
                No failed dispatches found
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
