# Notification System Frontend

This directory contains the complete frontend implementation for the CRM Notification System (Spec-025).

## Components

### Core Components

- **`NotificationBadge`** - Shows unread notification count
- **`NotificationItem`** - Individual notification display with dispatch status
- **`NotificationList`** - Full notification list with pagination and filtering
- **`NotificationFilters`** - Filter controls for notifications
- **`NotificationDropdown`** - Dropdown notification menu for navigation

### Dispatch Management Components

- **`DispatchStatusDashboard`** - Comprehensive dashboard for monitoring notification delivery
- **`NotificationDispatchStatus`** - Shows delivery status for individual notifications

### Configuration Management Components

- **`NotificationConfigurationPanel`** - Panel for managing templates and channel settings
- **`NotificationManagement`** - Unified management interface combining all features

### Usage Examples

#### Basic Notification Badge
```tsx
import { NotificationBadge } from '@/components/notifications';

function NavBar() {
  return (
    <div className="nav-item">
      <NotificationBadge />
    </div>
  );
}
```

#### Notification Dropdown in Header
```tsx
import { NotificationDropdown } from '@/components/notifications';

function Header() {
  return (
    <header>
      <NotificationDropdown />
    </header>
  );
}
```

#### Full Notification Page
```tsx
import { NotificationList } from '@/components/notifications';

function NotificationsPage() {
  const handleNotificationClick = (notification) => {
    // Handle navigation based on notification type
    console.log('Clicked:', notification);
  };

  return (
    <NotificationList 
      onNotificationClick={handleNotificationClick}
      showFilters={true}
    />
  );
}
```

#### Dispatch Status Dashboard
```tsx
import { DispatchStatusDashboard } from '@/components/notifications';

function AdminDashboard() {
  return (
    <div>
      <h1>Notification Delivery Status</h1>
      <DispatchStatusDashboard />
    </div>
  );
}
```

#### Configuration Management
```tsx
import { NotificationConfigurationPanel } from '@/components/notifications';

function ConfigPage() {
  return (
    <div>
      <h1>Notification Settings</h1>
      <NotificationConfigurationPanel />
    </div>
  );
}
```

#### Complete Management Interface
```tsx
import { NotificationManagement } from '@/components/notifications';

function AdminNotifications() {
  return (
    <NotificationManagement 
      defaultTab="dispatch"
      className="container mx-auto p-6"
    />
  );
}
```

## Hooks

### `useNotifications`
Main hook for notification operations:

```tsx
import { useNotifications } from '@/hooks/useNotifications';

function MyComponent() {
  const {
    notifications,
    unreadCount,
    isLoading,
    loadNotifications,
    markAsRead,
    markAllAsRead,
    setFilters,
  } = useNotifications();

  // Use the notification data and actions
}
```

### `useRealTimeNotifications`
Hook for real-time SignalR notifications:

```tsx
import { useRealTimeNotifications } from '@/hooks/useRealTimeNotifications';

function App() {
  const { isConnected, connectionState } = useRealTimeNotifications();

  return (
    <div>
      {isConnected && <span>🟢 Live notifications enabled</span>}
      <span>Status: {connectionState}</span>
    </div>
  );
}
```

### `useDispatchHistory`
Hook for dispatch monitoring and management:

```tsx
import { useDispatchHistory } from '@/hooks/useDispatchHistory';

function DispatchPage() {
  const {
    items,
    statistics,
    loading,
    refresh,
    retryDispatch
  } = useDispatchHistory();

  return (
    <div>
      <p>Success Rate: {statistics?.successRate}%</p>
      {/* Display dispatch history */}
    </div>
  );
}
```

### `useNotificationConfiguration`
Hook for template and channel management:

```tsx
import { useNotificationTemplates, useChannelConfiguration } from '@/hooks/useNotificationConfiguration';

function ConfigPage() {
  const { templates, createTemplate, updateTemplate } = useNotificationTemplates();
  const { configurations, updateConfiguration } = useChannelConfiguration();

  // Manage templates and channel settings
}
```

## Store

The notification system uses Zustand for state management. The store automatically handles:

- Loading and caching notifications
- Pagination and filtering
- Real-time updates
- Unread count tracking

## API Integration

The system integrates with the backend API endpoints:

- `GET /api/v1/notifications` - List notifications
- `PUT /api/v1/notifications/{id}/read` - Mark as read
- `GET /api/v1/notifications/unread-count` - Get unread count
- `POST /api/v1/notifications` - Create notification (admin)

## Real-time Features

### SignalR Integration
- Automatic connection to `/hubs/notifications`
- Real-time notification delivery
- Automatic reconnection on connection loss
- User group management for targeted notifications

### Browser Notifications
- Requests permission for browser notifications
- Shows desktop notifications for new messages
- Respects user's notification preferences

## Styling

All components use Tailwind CSS classes and are designed to be:
- Responsive (mobile-first)
- Accessible (ARIA labels, keyboard navigation)
- Consistent with the CRM design system
- Customizable via className props

## Features

### Filtering and Search
- Filter by read/unread status
- Filter by notification type
- Date range filtering
- Real-time filter application

### Pagination
- Infinite scroll support
- Load more button
- Efficient data loading
- Proper loading states

### Accessibility
- Screen reader support
- Keyboard navigation
- Focus management
- ARIA labels and descriptions

### Performance
- Optimized re-renders with Zustand
- Efficient API calls
- Proper loading states
- Error handling and retry logic

## Dependencies

Required packages (already included in package.json):
- `@microsoft/signalr` - Real-time notifications
- `zustand` - State management
- `date-fns` - Date formatting
- `next` - React framework
- `tailwindcss` - Styling

## Integration Checklist

To integrate the notification system into your app:

1. ✅ Install dependencies (already done)
2. ✅ Add notification components to your pages
3. ✅ Set up real-time notifications with access token
4. ✅ Configure SignalR hub URL in your environment
5. ✅ Add notification routes to your navigation
6. ✅ Test notification creation from backend
7. ✅ Test real-time delivery
8. ✅ Test browser notification permissions

## Customization

### Theming
Components accept `className` props for custom styling:

```tsx
<NotificationList className="custom-shadow custom-border" />
<NotificationBadge className="custom-badge-style" />
```

### Icons
Notification icons can be customized by modifying the `getNotificationIcon` function in `NotificationItem.tsx`.

### Channels
Delivery channel badges can be customized in the `getChannelBadges` function.

## Troubleshooting

### Common Issues

1. **SignalR not connecting**
   - Check access token is valid
   - Verify hub URL is correct
   - Check network connectivity

2. **Notifications not updating**
   - Ensure real-time hook is properly initialized
   - Check browser console for errors
   - Verify backend is sending events

3. **Browser notifications not showing**
   - Check notification permissions
   - Verify HTTPS connection (required for notifications)
   - Check browser notification settings

### Debug Mode

Enable debug logging by setting:
```tsx
// In useRealTimeNotifications hook
.configureLogging(LogLevel.Debug)
```

This will show detailed SignalR connection information in the browser console.