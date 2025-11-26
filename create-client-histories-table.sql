-- Create ClientHistories table
-- This migration creates the ClientHistories and SuspiciousActivityFlags tables

-- Create ClientHistories table
CREATE TABLE IF NOT EXISTS "ClientHistories" (
    "HistoryId" uuid NOT NULL,
    "ClientId" uuid NOT NULL,
    "ActorUserId" uuid NULL,
    "ActionType" character varying(50) NOT NULL,
    "ChangedFields" text[] NOT NULL DEFAULT ARRAY[]::text[],
    "BeforeSnapshot" jsonb NULL,
    "AfterSnapshot" jsonb NULL,
    "Reason" character varying(500) NULL,
    "Metadata" jsonb NOT NULL DEFAULT '{}'::jsonb,
    "SuspicionScore" smallint NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ClientHistories" PRIMARY KEY ("HistoryId"),
    CONSTRAINT "FK_ClientHistories_Clients_ClientId" FOREIGN KEY ("ClientId") 
        REFERENCES "Clients" ("ClientId") ON DELETE RESTRICT,
    CONSTRAINT "FK_ClientHistories_Users_ActorUserId" FOREIGN KEY ("ActorUserId") 
        REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Create SuspiciousActivityFlags table
CREATE TABLE IF NOT EXISTS "SuspiciousActivityFlags" (
    "FlagId" uuid NOT NULL,
    "HistoryId" uuid NOT NULL,
    "ClientId" uuid NOT NULL,
    "Score" smallint NOT NULL,
    "Reasons" text[] NOT NULL DEFAULT ARRAY[]::text[],
    "DetectedAt" timestamp with time zone NOT NULL,
    "ReviewedBy" uuid NULL,
    "ReviewedAt" timestamp with time zone NULL,
    "Status" character varying(32) NOT NULL DEFAULT 'OPEN',
    "Metadata" jsonb NOT NULL DEFAULT '{}'::jsonb,
    CONSTRAINT "PK_SuspiciousActivityFlags" PRIMARY KEY ("FlagId"),
    CONSTRAINT "FK_SuspiciousActivityFlags_ClientHistories_HistoryId" FOREIGN KEY ("HistoryId") 
        REFERENCES "ClientHistories" ("HistoryId") ON DELETE CASCADE
);

-- Create indexes for ClientHistories
CREATE INDEX IF NOT EXISTS "IX_ClientHistories_ClientId_CreatedAt" 
    ON "ClientHistories" ("ClientId", "CreatedAt");

CREATE INDEX IF NOT EXISTS "IX_ClientHistories_ActorUserId_CreatedAt" 
    ON "ClientHistories" ("ActorUserId", "CreatedAt");

-- Create GIN index for JSONB metadata column
CREATE INDEX IF NOT EXISTS "IX_ClientHistories_Metadata_GIN" 
    ON "ClientHistories" 
    USING GIN ("Metadata");

-- Create indexes for SuspiciousActivityFlags
CREATE INDEX IF NOT EXISTS "IX_SuspiciousActivityFlags_ClientId_DetectedAt" 
    ON "SuspiciousActivityFlags" ("ClientId", "DetectedAt");

CREATE INDEX IF NOT EXISTS "IX_SuspiciousActivityFlags_HistoryId" 
    ON "SuspiciousActivityFlags" ("HistoryId");

CREATE INDEX IF NOT EXISTS "IX_SuspiciousActivityFlags_Status_DetectedAt" 
    ON "SuspiciousActivityFlags" ("Status", "DetectedAt");

-- Verify tables were created
SELECT 
    table_name,
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = t.table_name) as column_count
FROM information_schema.tables t
WHERE table_name IN ('ClientHistories', 'SuspiciousActivityFlags')
ORDER BY table_name;

