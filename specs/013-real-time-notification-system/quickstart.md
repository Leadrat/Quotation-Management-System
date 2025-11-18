# Quickstart: Spec-013 Real-Time Notification System

**Spec**: Spec-013  
**Last Updated**: 2025-11-15

## Prerequisites

- ✅ Backend solution with Spec-010 (Quotation Management) and Spec-012 (Discount Approval Workflow) applied
- ✅ PostgreSQL database running and accessible
- ✅ Frontend Next.js app set up with TailAdmin template
- ✅ User authentication and authorization working (JWT, RBAC)
- ✅ Email service configured (FluentEmail)

## Backend Setup

### 1. Database Migration

Run the migration to create new tables:

```bash
cd src/Backend/CRM.Infrastructure
dotnet ef migrations add CreateNotificationsTables --startup-project ../CRM.Api
dotnet ef database update --startup-project ../CRM.Api
```

**Verify:**
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('Notifications', 'NotificationPreferences', 'EmailNotificationLogs');
```

### 2. Application Wiring

Ensure in `Program.cs`:
- All notification command/query handlers registered
- All validators registered
- AutoMapper profiles registered
- NotificationsController registered
- Notification event handlers registered
- EmailNotificationDeliveryJob registered (if using background jobs)

### 3. Configuration

No additional configuration required. Uses existing email settings.

## Frontend Setup

### 1. API Integration

The `NotificationsApi` is already added to `src/Frontend/web/src/lib/api.ts` with all 9 methods.

### 2. Context Provider

The `NotificationProvider` is already wrapped in `src/Frontend/web/src/app/(protected)/layout.tsx`.

### 3. Components

All notification components are available:
- `NotificationInbox` - Main inbox component
- `NotificationItem` - Individual notification display
- `NotificationPreferencesModal` - Preferences management
- `NotificationBadge` - Badge for sidebar
- `NotificationToast` - Real-time toast notifications

### 4. Routes

- `/notifications` - Main notifications page

## Verification Checklist

### Backend

- [ ] Database tables created (`Notifications`, `NotificationPreferences`, `EmailNotificationLogs`)
- [ ] All handlers registered in `Program.cs`
- [ ] All validators registered in `Program.cs`
- [ ] `NotificationsController` accessible at `/api/v1/notifications`
- [ ] Can create notifications via domain events
- [ ] Email notifications are sent (check logs)

### Frontend

- [ ] `/notifications` page loads
- [ ] Notification badge appears in sidebar with unread count
- [ ] Can mark notifications as read
- [ ] Can archive/unarchive notifications
- [ ] Preferences modal opens and saves
- [ ] Real-time polling works (check every 30 seconds)

### Integration

- [ ] Quotation sent event creates notification
- [ ] Discount approval events create notifications
- [ ] Notifications appear in inbox
- [ ] Email notifications are sent (check email logs)
- [ ] Unread count updates correctly

## Testing

Run unit tests:
```bash
dotnet test tests/CRM.Tests/CRM.Tests.csproj --filter "FullyQualifiedName~Notifications"
```

Run integration tests:
```bash
dotnet test tests/CRM.Tests.Integration/CRM.Tests.Integration.csproj --filter "FullyQualifiedName~Notifications"
```

## Troubleshooting

### Notifications not appearing
- Check database for notification records
- Verify event handlers are registered
- Check application logs for errors

### Email not sending
- Verify email configuration in `appsettings.json`
- Check `EmailNotificationLogs` table for delivery status
- Review email queue processor logs

### Unread count not updating
- Verify `NotificationProvider` is wrapping the app
- Check browser console for API errors
- Verify polling interval (30 seconds)

## Next Steps

- Configure SignalR for true real-time updates (optional)
- Set up push notifications (optional)
- Customize notification templates
- Configure notification preferences defaults

