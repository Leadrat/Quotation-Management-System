-- Check if there are any notifications and their data integrity
SELECT 
    COUNT(*) as total_notifications,
    COUNT(CASE WHEN "UserId" IS NOT NULL THEN 1 END) as notifications_with_userid,
    COUNT(CASE WHEN "NotificationTypeId" IS NOT NULL THEN 1 END) as notifications_with_typeid
FROM "Notifications";

-- Check if we have users in the system
SELECT COUNT(*) as total_users FROM "Users";

-- Check notification types
SELECT "NotificationTypeId", "TypeName" FROM "NotificationTypes" ORDER BY "TypeName";