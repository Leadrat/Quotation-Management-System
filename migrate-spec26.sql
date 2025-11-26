-- Migration for Spec 26: Notification Creation and Dispatch Infrastructure

-- Create NotificationTemplates table
CREATE TABLE IF NOT EXISTS "NotificationTemplates" (
    "Id" SERIAL PRIMARY KEY,
    "TemplateKey" VARCHAR(100) NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(1000),
    "Channel" INTEGER NOT NULL,
    "Subject" VARCHAR(500) NOT NULL,
    "BodyTemplate" TEXT NOT NULL,
    "Variables" JSONB,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    "UpdatedAt" TIMESTAMPTZ NOT NULL
);

-- Create NotificationChannelConfigurations table
CREATE TABLE IF NOT EXISTS "NotificationChannelConfigurations" (
    "Id" SERIAL PRIMARY KEY,
    "Channel" INTEGER NOT NULL,
    "IsEnabled" BOOLEAN NOT NULL DEFAULT true,
    "Configuration" JSONB NOT NULL,
    "MaxRetryAttempts" INTEGER NOT NULL DEFAULT 3,
    "RetryDelay" INTERVAL NOT NULL DEFAULT INTERVAL '5 minutes',
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    "UpdatedAt" TIMESTAMPTZ NOT NULL
);

-- Create NotificationDispatchAttempts table
CREATE TABLE IF NOT EXISTS "NotificationDispatchAttempts" (
    "Id" SERIAL PRIMARY KEY,
    "NotificationId" UUID NOT NULL,
    "Channel" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "AttemptedAt" TIMESTAMPTZ NOT NULL,
    "CompletedAt" TIMESTAMPTZ,
    "ErrorMessage" VARCHAR(2000),
    "ExternalReference" VARCHAR(500),
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "NextRetryAt" TIMESTAMPTZ
);

-- Create indexes for NotificationTemplates
CREATE UNIQUE INDEX IF NOT EXISTS "IX_NotificationTemplates_TemplateKey" 
ON "NotificationTemplates" ("TemplateKey");

CREATE INDEX IF NOT EXISTS "IX_NotificationTemplates_Channel" 
ON "NotificationTemplates" ("Channel");

CREATE INDEX IF NOT EXISTS "IX_NotificationTemplates_IsActive" 
ON "NotificationTemplates" ("IsActive");

CREATE INDEX IF NOT EXISTS "IX_NotificationTemplates_Channel_IsActive" 
ON "NotificationTemplates" ("Channel", "IsActive");

-- Create indexes for NotificationChannelConfigurations
CREATE UNIQUE INDEX IF NOT EXISTS "IX_NotificationChannelConfigurations_Channel" 
ON "NotificationChannelConfigurations" ("Channel");

CREATE INDEX IF NOT EXISTS "IX_NotificationChannelConfigurations_IsEnabled" 
ON "NotificationChannelConfigurations" ("IsEnabled");

-- Create indexes for NotificationDispatchAttempts
CREATE INDEX IF NOT EXISTS "IX_NotificationDispatchAttempts_NotificationId" 
ON "NotificationDispatchAttempts" ("NotificationId");

CREATE INDEX IF NOT EXISTS "IX_NotificationDispatchAttempts_Status" 
ON "NotificationDispatchAttempts" ("Status");

CREATE INDEX IF NOT EXISTS "IX_NotificationDispatchAttempts_Channel" 
ON "NotificationDispatchAttempts" ("Channel");

CREATE INDEX IF NOT EXISTS "IX_NotificationDispatchAttempts_AttemptedAt" 
ON "NotificationDispatchAttempts" ("AttemptedAt");

CREATE INDEX IF NOT EXISTS "IX_NotificationDispatchAttempts_Status_NextRetryAt" 
ON "NotificationDispatchAttempts" ("Status", "NextRetryAt");

-- Insert default channel configurations
INSERT INTO "NotificationChannelConfigurations" 
("Channel", "IsEnabled", "Configuration", "MaxRetryAttempts", "RetryDelay", "CreatedAt", "UpdatedAt")
VALUES 
(1, true, '{"realTimeEnabled": true}', 3, INTERVAL '5 minutes', NOW(), NOW()),
(2, true, '{"smtpHost": "", "smtpPort": 587, "enableSsl": true, "username": "", "password": ""}', 5, INTERVAL '10 minutes', NOW(), NOW()),
(3, false, '{"apiKey": "", "fromNumber": "", "provider": ""}', 3, INTERVAL '15 minutes', NOW(), NOW())
ON CONFLICT ("Channel") DO NOTHING;

-- Insert default notification templates
INSERT INTO "NotificationTemplates" 
("TemplateKey", "Name", "Description", "Channel", "Subject", "BodyTemplate", "Variables", "IsActive", "CreatedAt", "UpdatedAt")
VALUES 
-- InApp templates
('quotation_approved_inapp', 'Quotation Approved (In-App)', 'In-app notification for quotation approval', 1, 'Quotation Approved', 'Your quotation {{QuotationNumber}} has been approved.', '["QuotationNumber", "ClientName"]', true, NOW(), NOW()),
('payment_request_inapp', 'Payment Request (In-App)', 'In-app notification for payment request', 1, 'Payment Request', 'Payment is requested for quotation {{QuotationNumber}}. Amount: {{Amount}}', '["QuotationNumber", "Amount", "ClientName"]', true, NOW(), NOW()),

-- Email templates
('quotation_approved_email', 'Quotation Approved (Email)', 'Email notification for quotation approval', 2, 'Quotation {{QuotationNumber}} Approved', '<h2>Quotation Approved</h2><p>Dear {{ClientName}},</p><p>Your quotation {{QuotationNumber}} has been approved.</p><p>Best regards,<br/>The Team</p>', '["QuotationNumber", "ClientName"]', true, NOW(), NOW()),
('payment_request_email', 'Payment Request (Email)', 'Email notification for payment request', 2, 'Payment Request for Quotation {{QuotationNumber}}', '<h2>Payment Request</h2><p>Dear {{ClientName}},</p><p>Payment is requested for quotation {{QuotationNumber}}.</p><p>Amount: {{Amount}}</p><p>Best regards,<br/>The Team</p>', '["QuotationNumber", "Amount", "ClientName"]', true, NOW(), NOW()),

-- SMS templates
('quotation_approved_sms', 'Quotation Approved (SMS)', 'SMS notification for quotation approval', 3, '', 'Quotation {{QuotationNumber}} approved. Contact us for details.', '["QuotationNumber"]', true, NOW(), NOW()),
('payment_request_sms', 'Payment Request (SMS)', 'SMS notification for payment request', 3, '', 'Payment requested for quotation {{QuotationNumber}}. Amount: {{Amount}}', '["QuotationNumber", "Amount"]', true, NOW(), NOW())
ON CONFLICT ("TemplateKey") DO NOTHING;

-- Add foreign key constraint for NotificationDispatchAttempts if Notifications table exists
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Notifications') THEN
        -- Check if foreign key doesn't already exist
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.table_constraints 
            WHERE constraint_name = 'FK_NotificationDispatchAttempts_Notifications_NotificationId'
        ) THEN
            ALTER TABLE "NotificationDispatchAttempts" 
            ADD CONSTRAINT "FK_NotificationDispatchAttempts_Notifications_NotificationId" 
            FOREIGN KEY ("NotificationId") REFERENCES "Notifications" ("NotificationId") ON DELETE CASCADE;
        END IF;
    END IF;
END $$;

-- Update migration history if __EFMigrationsHistory table exists
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '__EFMigrationsHistory') THEN
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20251124120000_AddNotificationDispatchInfrastructure', '8.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING;
    END IF;
END $$;

COMMIT;