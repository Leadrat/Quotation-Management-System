-- Fix the Notifications table schema to match the Entity Framework configuration
-- Add missing columns to the Notifications table

DO $$
BEGIN
    -- Add UserId column if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'UserId') THEN
        ALTER TABLE "Notifications" ADD COLUMN "UserId" uuid NOT NULL DEFAULT gen_random_uuid();
        RAISE NOTICE 'Added UserId column';
    END IF;

    -- Add NotificationTypeId column if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'NotificationTypeId') THEN
        ALTER TABLE "Notifications" ADD COLUMN "NotificationTypeId" uuid NOT NULL DEFAULT gen_random_uuid();
        RAISE NOTICE 'Added NotificationTypeId column';
    END IF;

    -- Add Title column if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'Title') THEN
        ALTER TABLE "Notifications" ADD COLUMN "Title" character varying(255) NOT NULL DEFAULT 'Notification';
        RAISE NOTICE 'Added Title column';
    END IF;

    -- Add SentVia column if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'SentVia') THEN
        ALTER TABLE "Notifications" ADD COLUMN "SentVia" character varying(100) NOT NULL DEFAULT 'System';
        RAISE NOTICE 'Added SentVia column';
    END IF;

    -- Add UpdatedAt column if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'UpdatedAt') THEN
        ALTER TABLE "Notifications" ADD COLUMN "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
        RAISE NOTICE 'Added UpdatedAt column';
    END IF;

    -- Add Priority column if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'Priority') THEN
        ALTER TABLE "Notifications" ADD COLUMN "Priority" integer NOT NULL DEFAULT 1;
        RAISE NOTICE 'Added Priority column';
    END IF;

    -- Add Metadata column if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'Metadata') THEN
        ALTER TABLE "Notifications" ADD COLUMN "Metadata" character varying(4000);
        RAISE NOTICE 'Added Metadata column';
    END IF;

    -- Add Id column if it doesn't exist (for compatibility)
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'Id') THEN
        ALTER TABLE "Notifications" ADD COLUMN "Id" uuid NOT NULL DEFAULT gen_random_uuid();
        RAISE NOTICE 'Added Id column';
    END IF;

    -- Update Meta column to be character varying instead of jsonb for compatibility
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'Meta' AND data_type = 'jsonb') THEN
        ALTER TABLE "Notifications" ALTER COLUMN "Meta" TYPE character varying(4000) USING "Meta"::text;
        RAISE NOTICE 'Updated Meta column type to character varying';
    END IF;

    -- Update RelatedEntityType to allow NULL
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'RelatedEntityType' AND is_nullable = 'NO') THEN
        ALTER TABLE "Notifications" ALTER COLUMN "RelatedEntityType" DROP NOT NULL;
        RAISE NOTICE 'Made RelatedEntityType nullable';
    END IF;

    -- Update RelatedEntityId to allow NULL
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Notifications' AND column_name = 'RelatedEntityId' AND is_nullable = 'NO') THEN
        ALTER TABLE "Notifications" ALTER COLUMN "RelatedEntityId" DROP NOT NULL;
        RAISE NOTICE 'Made RelatedEntityId nullable';
    END IF;

    -- Update DeliveryStatus default value
    ALTER TABLE "Notifications" ALTER COLUMN "DeliveryStatus" SET DEFAULT 'PENDING';
    RAISE NOTICE 'Updated DeliveryStatus default value';

    -- Create missing indexes
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Notifications_UserId') THEN
        CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");
        RAISE NOTICE 'Created UserId index';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Notifications_NotificationTypeId') THEN
        CREATE INDEX "IX_Notifications_NotificationTypeId" ON "Notifications" ("NotificationTypeId");
        RAISE NOTICE 'Created NotificationTypeId index';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Notifications_UserId_IsRead') THEN
        CREATE INDEX "IX_Notifications_UserId_IsRead" ON "Notifications" ("UserId", "IsRead");
        RAISE NOTICE 'Created UserId_IsRead index';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Notifications_UserId_CreatedAt') THEN
        CREATE INDEX "IX_Notifications_UserId_CreatedAt" ON "Notifications" ("UserId", "CreatedAt");
        RAISE NOTICE 'Created UserId_CreatedAt index';
    END IF;

    -- Update existing records to have proper UserId values (set to RecipientUserId for compatibility)
    UPDATE "Notifications" SET "UserId" = "RecipientUserId" WHERE "UserId" IS NULL OR "UserId" = '00000000-0000-0000-0000-000000000000';
    RAISE NOTICE 'Updated existing records with proper UserId values';

END $$;

SELECT 'Notifications table schema fix completed successfully' as status;