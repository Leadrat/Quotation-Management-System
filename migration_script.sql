CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE EXTENSION IF NOT EXISTS citext;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE TABLE "Roles" (
        "RoleId" uuid NOT NULL,
        "RoleName" citext NOT NULL,
        "Description" character varying(500),
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_Roles" PRIMARY KEY ("RoleId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE TABLE "Users" (
        "UserId" uuid NOT NULL,
        "Email" citext NOT NULL,
        "PasswordHash" character varying(255) NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "Mobile" character varying(20),
        "PhoneCode" character varying(5),
        "IsActive" boolean NOT NULL,
        "RoleId" uuid NOT NULL,
        "ReportingManagerId" uuid,
        "LastLoginAt" timestamp with time zone,
        "LoginAttempts" integer NOT NULL,
        "IsLockedOut" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_Users" PRIMARY KEY ("UserId"),
        CONSTRAINT "FK_Users_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("RoleId") ON DELETE RESTRICT,
        CONSTRAINT "FK_Users_Users_ReportingManagerId" FOREIGN KEY ("ReportingManagerId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE TABLE "Clients" (
        "ClientId" uuid NOT NULL,
        "CompanyName" character varying(255) NOT NULL,
        "ContactName" character varying(255),
        "Email" character varying(255) NOT NULL,
        "Mobile" character varying(20) NOT NULL,
        "PhoneCode" character varying(5),
        "Gstin" character varying(15),
        "StateCode" character varying(2),
        "Address" text,
        "City" character varying(100),
        "State" character varying(100),
        "PinCode" character varying(10),
        "CreatedByUserId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_Clients" PRIMARY KEY ("ClientId"),
        CONSTRAINT "FK_Clients_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE TABLE "EmailVerificationTokens" (
        "TokenId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" character varying(256) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "ConsumedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        CONSTRAINT "PK_EmailVerificationTokens" PRIMARY KEY ("TokenId"),
        CONSTRAINT "FK_EmailVerificationTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE TABLE "PasswordResetTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" bytea NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "UsedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PasswordResetTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE TABLE "RefreshTokens" (
        "RefreshTokenId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenJti" character varying(255) NOT NULL,
        "IsRevoked" boolean NOT NULL,
        "RevokedAt" timestamp with time zone,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "LastUsedAt" timestamp with time zone,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("RefreshTokenId"),
        CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE TABLE "UserRoles" (
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_UserRoles" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("RoleId") ON DELETE RESTRICT,
        CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Clients_CreatedAt" ON "Clients" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Clients_CreatedByUserId_DeletedAt" ON "Clients" ("CreatedByUserId", "DeletedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Clients_DeletedAt" ON "Clients" ("DeletedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Clients_Gstin" ON "Clients" ("Gstin");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Clients_UpdatedAt" ON "Clients" ("UpdatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_EmailVerificationTokens_UserId" ON "EmailVerificationTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_PasswordResetToken_ExpiresAt" ON "PasswordResetTokens" ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_PasswordResetToken_User_Active" ON "PasswordResetTokens" ("UserId", "UsedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_RefreshTokens_ExpiresAt" ON "RefreshTokens" ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_RefreshTokens_IsRevoked" ON "RefreshTokens" ("IsRevoked");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE UNIQUE INDEX "IX_RefreshTokens_TokenJti" ON "RefreshTokens" ("TokenJti");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Roles_IsActive" ON "Roles" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE UNIQUE INDEX "UX_Roles_RoleName" ON "Roles" ("RoleName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Users_CreatedAt" ON "Users" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Users_DeletedAt" ON "Users" ("DeletedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Users_IsActive" ON "Users" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Users_ReportingManagerId" ON "Users" ("ReportingManagerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Users_RoleId" ON "Users" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    CREATE INDEX "IX_Users_UpdatedAt" ON "Users" ("UpdatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251114062356_UserRoles_AddAndBackfill') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251114062356_UserRoles_AddAndBackfill', '8.0.8');
    END IF;
END $EF$;
COMMIT;

