-- Create the main Notifications table based on UserNotification entity configuration
-- This script creates the table if it doesn't exist

DO $$
BEGIN
    -- Check if the Notifications table exists
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Notifications') THEN
        -- Create the Notifications table
        CREATE TABLE "Notifications" (
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
        
        -- Create indexes
        CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");
        CREATE INDEX "IX_Notifications_RecipientUserId" ON "Notifications" ("RecipientUserId");
        CREATE INDEX "IX_Notifications_NotificationTypeId" ON "Notifications" ("NotificationTypeId");
        CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications" ("CreatedAt");
        CREATE INDEX "IX_Notifications_IsRead" ON "Notifications" ("IsRead");
        CREATE INDEX "IX_Notifications_IsArchived" ON "Notifications" ("IsArchived");
        CREATE INDEX "IX_Notifications_UserId_IsRead" ON "Notifications" ("UserId", "IsRead");
        CREATE INDEX "IX_Notifications_RecipientUserId_IsRead" ON "Notifications" ("RecipientUserId", "IsRead");
        CREATE INDEX "IX_Notifications_RecipientUserId_IsArchived" ON "Notifications" ("RecipientUserId", "IsArchived");
        CREATE INDEX "IX_Notifications_UserId_CreatedAt" ON "Notifications" ("UserId", "CreatedAt");
        CREATE INDEX "IX_Notifications_RelatedEntityId" ON "Notifications" ("RelatedEntityId");
        
        RAISE NOTICE 'Notifications table created successfully';
    ELSE
        RAISE NOTICE 'Notifications table already exists';
    END IF;
    
    -- Check if NotificationTypes table exists, if not create it
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'NotificationTypes') THEN
        CREATE TABLE "NotificationTypes" (
            "NotificationTypeId" uuid NOT NULL DEFAULT gen_random_uuid(),
            "TypeName" character varying(100) NOT NULL,
            "Description" character varying(500),
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
            "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
            CONSTRAINT "PK_NotificationTypes" PRIMARY KEY ("NotificationTypeId")
        );
        
        CREATE UNIQUE INDEX "IX_NotificationTypes_TypeName" ON "NotificationTypes" ("TypeName");
        
        -- Insert some default notification types
        INSERT INTO "NotificationTypes" ("TypeName", "Description") VALUES
        ('QuotationApproved', 'Notification sent when a quotation is approved'),
        ('PaymentRequest', 'Notification sent when a payment is requested'),
        ('QuotationSent', 'Notification sent when a quotation is sent to client'),
        ('QuotationViewed', 'Notification sent when a quotation is viewed by client'),
        ('QuotationResponseReceived', 'Notification sent when client responds to quotation'),
        ('QuotationExpiring', 'Notification sent when quotation is about to expire'),
        ('SystemAlert', 'General system alert notification');
        
        RAISE NOTICE 'NotificationTypes table created with default types';
    END IF;
    
    -- Add foreign key constraints if they don't exist
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Users') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints 
                      WHERE constraint_name = 'FK_Notifications_Users_UserId') THEN
            ALTER TABLE "Notifications" 
            ADD CONSTRAINT "FK_Notifications_Users_UserId"
            FOREIGN KEY ("UserId") 
            REFERENCES "Users"("UserId")
            ON DELETE RESTRICT;
            RAISE NOTICE 'Added foreign key constraint to Users for UserId';
        END IF;
        
        IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints 
                      WHERE constraint_name = 'FK_Notifications_Users_RecipientUserId') THEN
            ALTER TABLE "Notifications" 
            ADD CONSTRAINT "FK_Notifications_Users_RecipientUserId"
            FOREIGN KEY ("RecipientUserId") 
            REFERENCES "Users"("UserId")
            ON DELETE RESTRICT;
            RAISE NOTICE 'Added foreign key constraint to Users for RecipientUserId';
        END IF;
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'NotificationTypes') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints 
                      WHERE constraint_name = 'FK_Notifications_NotificationTypes_NotificationTypeId') THEN
            ALTER TABLE "Notifications" 
            ADD CONSTRAINT "FK_Notifications_NotificationTypes_NotificationTypeId"
            FOREIGN KEY ("NotificationTypeId") 
            REFERENCES "NotificationTypes"("NotificationTypeId")
            ON DELETE RESTRICT;
            RAISE NOTICE 'Added foreign key constraint to NotificationTypes';
        END IF;
    END IF;
    
END $$;

-- Verify the table structure
SELECT 'Notifications table creation completed successfully' as status;