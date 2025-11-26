-- Ensure NotificationTypes table exists and has default types
-- Also update existing notifications to have proper NotificationTypeId values

DO $$
DECLARE
    default_type_id uuid;
BEGIN
    -- Create NotificationTypes table if it doesn't exist
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
        RAISE NOTICE 'Created NotificationTypes table';
    END IF;

    -- Insert default notification types if they don't exist
    INSERT INTO "NotificationTypes" ("TypeName", "Description") 
    SELECT 'SystemAlert', 'General system alert notification'
    WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'SystemAlert');

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
    SELECT 'QuotationApproved', 'Notification sent when a quotation is approved'
    WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'QuotationApproved');

    INSERT INTO "NotificationTypes" ("TypeName", "Description") 
    SELECT 'PaymentRequest', 'Notification sent when a payment is requested'
    WHERE NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "TypeName" = 'PaymentRequest');

    -- Get the SystemAlert type ID for default notifications
    SELECT "NotificationTypeId" INTO default_type_id 
    FROM "NotificationTypes" 
    WHERE "TypeName" = 'SystemAlert' 
    LIMIT 1;

    -- Update existing notifications to have proper NotificationTypeId
    UPDATE "Notifications" 
    SET "NotificationTypeId" = default_type_id 
    WHERE "NotificationTypeId" IS NULL 
       OR "NotificationTypeId" = '00000000-0000-0000-0000-000000000000'
       OR NOT EXISTS (SELECT 1 FROM "NotificationTypes" WHERE "NotificationTypeId" = "Notifications"."NotificationTypeId");

    RAISE NOTICE 'Updated existing notifications with proper NotificationTypeId values';

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
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints 
                  WHERE constraint_name = 'FK_Notifications_NotificationTypes_NotificationTypeId') THEN
        ALTER TABLE "Notifications" 
        ADD CONSTRAINT "FK_Notifications_NotificationTypes_NotificationTypeId"
        FOREIGN KEY ("NotificationTypeId") 
        REFERENCES "NotificationTypes"("NotificationTypeId")
        ON DELETE RESTRICT;
        RAISE NOTICE 'Added foreign key constraint to NotificationTypes';
    END IF;

END $$;

SELECT 'Notification types setup completed successfully' as status;