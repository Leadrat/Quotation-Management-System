-- Create the main Notifications table if it doesn't exist
CREATE TABLE IF NOT EXISTS "Notifications" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "NotificationId" uuid NOT NULL DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL,
    "RecipientUserId" uuid NOT NULL,
    "NotificationTypeId" uuid NOT NULL,
    "EventType" character varying(100) NOT NULL,
    "Title" character varying(255) NOT NULL,
    "Message" character varying(10000) NOT NULL,
    "RelatedEntityId" uuid,
    "RelatedEntityType" character varying(100),
    "IsRead" boolean NOT NULL DEFAULT false,
    "ReadAt" timestamp with time zone,
    "IsArchived" boolean NOT NULL DEFAULT false,
    "ArchivedAt" timestamp with time zone,
    "SentVia" character varying(100) NOT NULL,
    "DeliveredChannels" character varying(500),
    "DeliveryStatus" character varying(50) NOT NULL DEFAULT 'PENDING',
    "Meta" character varying(4000),
    "Metadata" character varying(4000),
    "Priority" integer NOT NULL DEFAULT 1,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Notifications" PRIMARY KEY ("NotificationId")
);

-- Create NotificationTypes table if it doesn't exist
CREATE TABLE IF NOT EXISTS "NotificationTypes" (
    "NotificationTypeId" uuid NOT NULL DEFAULT gen_random_uuid(),
    "TypeName" character varying(100) NOT NULL,
    "Description" character varying(500),
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_NotificationTypes" PRIMARY KEY ("NotificationTypeId")
);

-- Create indexes for Notifications table
CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId" ON "Notifications" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_RecipientUserId" ON "Notifications" ("RecipientUserId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_NotificationTypeId" ON "Notifications" ("NotificationTypeId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_CreatedAt" ON "Notifications" ("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_Notifications_IsRead" ON "Notifications" ("IsRead");
CREATE INDEX IF NOT EXISTS "IX_Notifications_IsArchived" ON "Notifications" ("IsArchived");
CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId_IsRead" ON "Notifications" ("UserId", "IsRead");
CREATE INDEX IF NOT EXISTS "IX_Notifications_RecipientUserId_IsRead" ON "Notifications" ("RecipientUserId", "IsRead");
CREATE INDEX IF NOT EXISTS "IX_Notifications_RecipientUserId_IsArchived" ON "Notifications" ("RecipientUserId", "IsArchived");
CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId_CreatedAt" ON "Notifications" ("UserId", "CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_Notifications_RelatedEntityId" ON "Notifications" ("RelatedEntityId");

-- Create unique index for NotificationTypes
CREATE UNIQUE INDEX IF NOT EXISTS "IX_NotificationTypes_TypeName" ON "NotificationTypes" ("TypeName");

-- Insert default notification types if they don't exist
INSERT INTO "NotificationTypes" ("TypeName", "Description") 
SELECT 'QuotationApproved', 'Notification sent when a quotation is approved'
WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'QuotationApproved');

INSERT INTO "NotificationTypes" ("TypeName", "Description") 
SELECT 'PaymentRequest', 'Notification sent when a payment is requested'
WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'PaymentRequest');

INSERT INTO "NotificationTypes" ("TypeName", "Description") 
SELECT 'QuotationSent', 'Notification sent when a quotation is sent to client'
WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'QuotationSent');

INSERT INTO "NotificationTypes" ("TypeName", "Description") 
SELECT 'QuotationViewed', 'Notification sent when a quotation is viewed by client'
WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'QuotationViewed');

INSERT INTO "NotificationTypes" ("TypeName", "Description") 
SELECT 'QuotationResponseReceived', 'Notification sent when client responds to quotation'
WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'QuotationResponseReceived');

INSERT INTO "NotificationTypes" ("TypeName", "Description") 
SELECT 'QuotationExpiring', 'Notification sent when quotation is about to expire'
WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'QuotationExpiring');

INSERT INTO "NotificationTypes" ("TypeName", "Description") 
SELECT 'SystemAlert', 'General system alert notification'
WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'SystemAlert');

SELECT 'Notifications table creation completed successfully' as status;