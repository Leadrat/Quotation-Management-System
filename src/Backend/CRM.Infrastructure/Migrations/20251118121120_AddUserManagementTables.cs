using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserManagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns to Users table if they don't exist
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'AvatarUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""AvatarUrl"" character varying(500);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'Bio') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Bio"" character varying(500);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'DelegateUserId') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""DelegateUserId"" uuid;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'LastSeenAt') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""LastSeenAt"" timestamp with time zone;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'LinkedInUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""LinkedInUrl"" character varying(255);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'OutOfOfficeMessage') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""OutOfOfficeMessage"" character varying(1000);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'OutOfOfficeStatus') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""OutOfOfficeStatus"" boolean NOT NULL DEFAULT false;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'PresenceStatus') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""PresenceStatus"" integer NOT NULL DEFAULT 0;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'Skills') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Skills"" jsonb;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'TwitterUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""TwitterUrl"" character varying(255);
                    END IF;
                END $$;
            ");

            // Add columns to Roles table if they don't exist
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Roles' AND column_name = 'IsBuiltIn') THEN
                        ALTER TABLE ""Roles"" ADD COLUMN ""IsBuiltIn"" boolean NOT NULL DEFAULT false;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Roles' AND column_name = 'Permissions') THEN
                        ALTER TABLE ""Roles"" ADD COLUMN ""Permissions"" jsonb NOT NULL DEFAULT '[]';
                    END IF;
                END $$;
            ");

            // Alter Quotations.Status column if it's still a string type
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Quotations' 
                        AND column_name = 'Status' 
                        AND data_type = 'character varying'
                    ) THEN
                        ALTER TABLE ""Quotations"" ALTER COLUMN ""Status"" TYPE integer USING ""Status""::integer;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""AuditLog"" (
                    ""Id"" uuid NOT NULL,
                    ""ActionType"" character varying(100) NOT NULL,
                    ""Entity"" character varying(100) NOT NULL,
                    ""EntityId"" uuid,
                    ""PerformedBy"" uuid NOT NULL,
                    ""IpAddress"" character varying(45),
                    ""Timestamp"" timestamp with time zone NOT NULL,
                    ""Changes"" jsonb,
                    CONSTRAINT ""PK_AuditLog"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_AuditLog_Users_PerformedBy"" FOREIGN KEY (""PerformedBy"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");
            
            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_AuditLog_ActionType"" ON ""AuditLog"" (""ActionType"");
                CREATE INDEX IF NOT EXISTS ""IX_AuditLog_Entity"" ON ""AuditLog"" (""Entity"");
                CREATE INDEX IF NOT EXISTS ""IX_AuditLog_EntityId"" ON ""AuditLog"" (""EntityId"") WHERE ""EntityId"" IS NOT NULL;
                CREATE INDEX IF NOT EXISTS ""IX_AuditLog_PerformedBy"" ON ""AuditLog"" (""PerformedBy"");
                CREATE INDEX IF NOT EXISTS ""IX_AuditLog_Timestamp"" ON ""AuditLog"" (""Timestamp"");
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""ClientPortalOtps"" (
                    ""OtpId"" uuid NOT NULL,
                    ""AccessLinkId"" uuid NOT NULL,
                    ""ClientEmail"" character varying(255) NOT NULL,
                    ""OtpCode"" character varying(500) NOT NULL,
                    ""ExpiresAt"" timestamp with time zone NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""VerifiedAt"" timestamp with time zone,
                    ""IsUsed"" boolean NOT NULL DEFAULT FALSE,
                    ""Attempts"" integer NOT NULL DEFAULT 0,
                    ""IpAddress"" character varying(50),
                    CONSTRAINT ""PK_ClientPortalOtps"" PRIMARY KEY (""OtpId""),
                    CONSTRAINT ""FK_ClientPortalOtps_QuotationAccessLinks_AccessLinkId"" FOREIGN KEY (""AccessLinkId"") REFERENCES ""QuotationAccessLinks"" (""AccessLinkId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Currencies"" (
                    ""CurrencyCode"" character varying(3) NOT NULL,
                    ""DisplayName"" character varying(100) NOT NULL,
                    ""Symbol"" character varying(10) NOT NULL,
                    ""DecimalPlaces"" integer NOT NULL DEFAULT 2,
                    ""IsDefault"" boolean NOT NULL DEFAULT FALSE,
                    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""CreatedByUserId"" uuid,
                    ""UpdatedByUserId"" uuid,
                    CONSTRAINT ""PK_Currencies"" PRIMARY KEY (""CurrencyCode""),
                    CONSTRAINT ""FK_Currencies_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL,
                    CONSTRAINT ""FK_Currencies_Users_UpdatedByUserId"" FOREIGN KEY (""UpdatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""CustomBranding"" (
                    ""Id"" uuid NOT NULL,
                    ""LogoUrl"" character varying(500),
                    ""PrimaryColor"" character varying(7),
                    ""SecondaryColor"" character varying(7),
                    ""AccentColor"" character varying(7),
                    ""FooterHtml"" text,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedBy"" uuid NOT NULL,
                    CONSTRAINT ""PK_CustomBranding"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_CustomBranding_Users_UpdatedBy"" FOREIGN KEY (""UpdatedBy"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""DataRetentionPolicy"" (
                    ""Id"" uuid NOT NULL,
                    ""EntityType"" character varying(100) NOT NULL,
                    ""RetentionPeriodMonths"" integer NOT NULL,
                    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                    ""AutoPurgeEnabled"" boolean NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""CreatedBy"" uuid NOT NULL,
                    ""UpdatedBy"" uuid,
                    CONSTRAINT ""PK_DataRetentionPolicy"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_DataRetentionPolicy_Users_CreatedBy"" FOREIGN KEY (""CreatedBy"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_DataRetentionPolicy_Users_UpdatedBy"" FOREIGN KEY (""UpdatedBy"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""IntegrationKeys"" (
                    ""Id"" uuid NOT NULL,
                    ""KeyName"" character varying(255) NOT NULL,
                    ""KeyValueEncrypted"" character varying(2000) NOT NULL,
                    ""Provider"" character varying(100) NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""LastUsedAt"" timestamp with time zone,
                    ""CreatedBy"" uuid NOT NULL,
                    ""UpdatedBy"" uuid,
                    CONSTRAINT ""PK_IntegrationKeys"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_IntegrationKeys_Users_CreatedBy"" FOREIGN KEY (""CreatedBy"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_IntegrationKeys_Users_UpdatedBy"" FOREIGN KEY (""UpdatedBy"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Mentions"" (
                    ""MentionId"" uuid NOT NULL,
                    ""EntityType"" character varying(50) NOT NULL,
                    ""EntityId"" uuid NOT NULL,
                    ""MentionedUserId"" uuid NOT NULL,
                    ""MentionedByUserId"" uuid NOT NULL,
                    ""IsRead"" boolean NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_Mentions"" PRIMARY KEY (""MentionId""),
                    CONSTRAINT ""FK_Mentions_Users_MentionedByUserId"" FOREIGN KEY (""MentionedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_Mentions_Users_MentionedUserId"" FOREIGN KEY (""MentionedUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""NotificationSettings"" (
                    ""Id"" uuid NOT NULL,
                    ""BannerMessage"" text,
                    ""BannerType"" character varying(20),
                    ""IsVisible"" boolean NOT NULL DEFAULT FALSE,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedBy"" uuid NOT NULL,
                    CONSTRAINT ""PK_NotificationSettings"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_NotificationSettings_Users_UpdatedBy"" FOREIGN KEY (""UpdatedBy"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""QuotationPageViews"" (
                    ""ViewId"" uuid NOT NULL,
                    ""AccessLinkId"" uuid NOT NULL,
                    ""QuotationId"" uuid NOT NULL,
                    ""ClientEmail"" character varying(255) NOT NULL,
                    ""ViewStartTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ViewEndTime"" timestamp with time zone,
                    ""DurationSeconds"" integer,
                    ""IpAddress"" character varying(50),
                    ""UserAgent"" character varying(500),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT ""PK_QuotationPageViews"" PRIMARY KEY (""ViewId""),
                    CONSTRAINT ""FK_QuotationPageViews_QuotationAccessLinks_AccessLinkId"" FOREIGN KEY (""AccessLinkId"") REFERENCES ""QuotationAccessLinks"" (""AccessLinkId"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_QuotationPageViews_Quotations_QuotationId"" FOREIGN KEY (""QuotationId"") REFERENCES ""Quotations"" (""QuotationId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""SupportedLanguages"" (
                    ""LanguageCode"" character varying(5) NOT NULL,
                    ""DisplayName"" character varying(100) NOT NULL,
                    ""DisplayNameEn"" character varying(100) NOT NULL,
                    ""NativeName"" character varying(100) NOT NULL,
                    ""IsRTL"" boolean NOT NULL DEFAULT FALSE,
                    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                    ""FlagIcon"" character varying(50),
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_SupportedLanguages"" PRIMARY KEY (""LanguageCode"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""SystemSettings"" (
                    ""Key"" character varying(255) NOT NULL,
                    ""Value"" jsonb NOT NULL,
                    ""LastModifiedAt"" timestamp with time zone NOT NULL,
                    ""LastModifiedBy"" uuid NOT NULL,
                    CONSTRAINT ""PK_SystemSettings"" PRIMARY KEY (""Key""),
                    CONSTRAINT ""FK_SystemSettings_Users_LastModifiedBy"" FOREIGN KEY (""LastModifiedBy"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""TaskAssignments"" (
                    ""AssignmentId"" uuid NOT NULL,
                    ""EntityType"" character varying(50) NOT NULL,
                    ""EntityId"" uuid NOT NULL,
                    ""AssignedToUserId"" uuid NOT NULL,
                    ""AssignedByUserId"" uuid NOT NULL,
                    ""DueDate"" timestamp with time zone,
                    ""Status"" integer NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_TaskAssignments"" PRIMARY KEY (""AssignmentId""),
                    CONSTRAINT ""FK_TaskAssignments_Users_AssignedByUserId"" FOREIGN KEY (""AssignedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_TaskAssignments_Users_AssignedToUserId"" FOREIGN KEY (""AssignedToUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Teams"" (
                    ""TeamId"" uuid NOT NULL,
                    ""Name"" character varying(255) NOT NULL,
                    ""Description"" character varying(1000),
                    ""TeamLeadUserId"" uuid NOT NULL,
                    ""ParentTeamId"" uuid,
                    ""CompanyId"" uuid NOT NULL,
                    ""IsActive"" boolean NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_Teams"" PRIMARY KEY (""TeamId""),
                    CONSTRAINT ""FK_Teams_Teams_ParentTeamId"" FOREIGN KEY (""ParentTeamId"") REFERENCES ""Teams"" (""TeamId"") ON DELETE SET NULL,
                    CONSTRAINT ""FK_Teams_Users_TeamLeadUserId"" FOREIGN KEY (""TeamLeadUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UserActivities"" (
                    ""ActivityId"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""ActionType"" character varying(100) NOT NULL,
                    ""EntityType"" character varying(50),
                    ""EntityId"" uuid,
                    ""IpAddress"" character varying(45) NOT NULL,
                    ""UserAgent"" text NOT NULL,
                    ""Timestamp"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_UserActivities"" PRIMARY KEY (""ActivityId""),
                    CONSTRAINT ""FK_UserActivities_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UserGroups"" (
                    ""GroupId"" uuid NOT NULL,
                    ""Name"" character varying(255) NOT NULL,
                    ""Description"" character varying(1000),
                    ""Permissions"" jsonb NOT NULL DEFAULT '[]',
                    ""CreatedByUserId"" uuid NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_UserGroups"" PRIMARY KEY (""GroupId""),
                    CONSTRAINT ""FK_UserGroups_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""ExchangeRates"" (
                    ""ExchangeRateId"" uuid NOT NULL,
                    ""FromCurrencyCode"" character varying(3) NOT NULL,
                    ""ToCurrencyCode"" character varying(3) NOT NULL,
                    ""Rate"" numeric(18,6) NOT NULL,
                    ""EffectiveDate"" timestamp with time zone NOT NULL,
                    ""ExpiryDate"" timestamp with time zone,
                    ""Source"" character varying(50),
                    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""CreatedByUserId"" uuid,
                    CONSTRAINT ""PK_ExchangeRates"" PRIMARY KEY (""ExchangeRateId""),
                    CONSTRAINT ""FK_ExchangeRates_Currencies_FromCurrencyCode"" FOREIGN KEY (""FromCurrencyCode"") REFERENCES ""Currencies"" (""CurrencyCode"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_ExchangeRates_Currencies_ToCurrencyCode"" FOREIGN KEY (""ToCurrencyCode"") REFERENCES ""Currencies"" (""CurrencyCode"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_ExchangeRates_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""CompanyPreferences"" (
                    ""CompanyId"" uuid NOT NULL,
                    ""DefaultLanguageCode"" character varying(5) NOT NULL DEFAULT 'en',
                    ""DefaultCurrencyCode"" character varying(3) NOT NULL DEFAULT 'INR',
                    ""DateFormat"" character varying(20) NOT NULL DEFAULT 'dd/MM/yyyy',
                    ""TimeFormat"" character varying(10) NOT NULL DEFAULT '24h',
                    ""NumberFormat"" character varying(50) NOT NULL DEFAULT 'en-IN',
                    ""Timezone"" character varying(50),
                    ""FirstDayOfWeek"" integer NOT NULL DEFAULT 1,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedByUserId"" uuid,
                    CONSTRAINT ""PK_CompanyPreferences"" PRIMARY KEY (""CompanyId""),
                    CONSTRAINT ""FK_CompanyPreferences_Currencies_DefaultCurrencyCode"" FOREIGN KEY (""DefaultCurrencyCode"") REFERENCES ""Currencies"" (""CurrencyCode"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_CompanyPreferences_SupportedLanguages_DefaultLanguageCode"" FOREIGN KEY (""DefaultLanguageCode"") REFERENCES ""SupportedLanguages"" (""LanguageCode"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_CompanyPreferences_Users_UpdatedByUserId"" FOREIGN KEY (""UpdatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""LocalizationResources"" (
                    ""ResourceId"" uuid NOT NULL,
                    ""LanguageCode"" character varying(5) NOT NULL,
                    ""ResourceKey"" character varying(200) NOT NULL,
                    ""ResourceValue"" character varying(1000) NOT NULL,
                    ""Category"" character varying(50),
                    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""CreatedByUserId"" uuid,
                    ""UpdatedByUserId"" uuid,
                    CONSTRAINT ""PK_LocalizationResources"" PRIMARY KEY (""ResourceId""),
                    CONSTRAINT ""FK_LocalizationResources_SupportedLanguages_LanguageCode"" FOREIGN KEY (""LanguageCode"") REFERENCES ""SupportedLanguages"" (""LanguageCode"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_LocalizationResources_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL,
                    CONSTRAINT ""FK_LocalizationResources_Users_UpdatedByUserId"" FOREIGN KEY (""UpdatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UserPreferences"" (
                    ""UserId"" uuid NOT NULL,
                    ""LanguageCode"" character varying(5) NOT NULL DEFAULT 'en',
                    ""CurrencyCode"" character varying(3),
                    ""DateFormat"" character varying(20) NOT NULL DEFAULT 'dd/MM/yyyy',
                    ""TimeFormat"" character varying(10) NOT NULL DEFAULT '24h',
                    ""NumberFormat"" character varying(50) NOT NULL DEFAULT 'en-IN',
                    ""Timezone"" character varying(50),
                    ""FirstDayOfWeek"" integer NOT NULL DEFAULT 1,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_UserPreferences"" PRIMARY KEY (""UserId""),
                    CONSTRAINT ""FK_UserPreferences_Currencies_CurrencyCode"" FOREIGN KEY (""CurrencyCode"") REFERENCES ""Currencies"" (""CurrencyCode"") ON DELETE SET NULL,
                    CONSTRAINT ""FK_UserPreferences_SupportedLanguages_LanguageCode"" FOREIGN KEY (""LanguageCode"") REFERENCES ""SupportedLanguages"" (""LanguageCode"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_UserPreferences_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""UserId"") ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""TeamMembers"" (
                    ""TeamMemberId"" uuid NOT NULL,
                    ""TeamId"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""JoinedAt"" timestamp with time zone NOT NULL,
                    ""Role"" character varying(50) NOT NULL,
                    CONSTRAINT ""PK_TeamMembers"" PRIMARY KEY (""TeamMemberId""),
                    CONSTRAINT ""FK_TeamMembers_Teams_TeamId"" FOREIGN KEY (""TeamId"") REFERENCES ""Teams"" (""TeamId"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_TeamMembers_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""UserId"") ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UserGroupMembers"" (
                    ""GroupMemberId"" uuid NOT NULL,
                    ""GroupId"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""AddedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_UserGroupMembers"" PRIMARY KEY (""GroupMemberId""),
                    CONSTRAINT ""FK_UserGroupMembers_UserGroups_GroupId"" FOREIGN KEY (""GroupId"") REFERENCES ""UserGroups"" (""GroupId"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_UserGroupMembers_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""UserId"") ON DELETE CASCADE
                );
            ");

            // Users indexes already created above with conditional SQL

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsBuiltIn",
                table: "Roles",
                column: "IsBuiltIn");

            // AuditLog indexes already created above with IF NOT EXISTS

            // Create indexes conditionally for ClientPortalOtps
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ClientPortalOtps_AccessLinkId') THEN
                        CREATE INDEX ""IX_ClientPortalOtps_AccessLinkId"" ON ""ClientPortalOtps"" (""AccessLinkId"");
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ClientPortalOtps_AccessLinkId_IsUsed_ExpiresAt') THEN
                        CREATE INDEX ""IX_ClientPortalOtps_AccessLinkId_IsUsed_ExpiresAt"" ON ""ClientPortalOtps"" (""AccessLinkId"", ""IsUsed"", ""ExpiresAt"");
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ClientPortalOtps_ClientEmail') THEN
                        CREATE INDEX ""IX_ClientPortalOtps_ClientEmail"" ON ""ClientPortalOtps"" (""ClientEmail"");
                    END IF;
                END $$;
            ");

            // Create all remaining indexes conditionally
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- CompanyPreferences indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_CompanyPreferences_DefaultCurrencyCode') THEN
                        CREATE INDEX ""IX_CompanyPreferences_DefaultCurrencyCode"" ON ""CompanyPreferences"" (""DefaultCurrencyCode"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_CompanyPreferences_DefaultLanguageCode') THEN
                        CREATE INDEX ""IX_CompanyPreferences_DefaultLanguageCode"" ON ""CompanyPreferences"" (""DefaultLanguageCode"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_CompanyPreferences_UpdatedByUserId') THEN
                        CREATE INDEX ""IX_CompanyPreferences_UpdatedByUserId"" ON ""CompanyPreferences"" (""UpdatedByUserId"");
                    END IF;

                    -- Currencies indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Currencies_CreatedByUserId') THEN
                        CREATE INDEX ""IX_Currencies_CreatedByUserId"" ON ""Currencies"" (""CreatedByUserId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Currencies_IsActive') THEN
                        CREATE INDEX ""IX_Currencies_IsActive"" ON ""Currencies"" (""IsActive"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Currencies_IsDefault') THEN
                        CREATE INDEX ""IX_Currencies_IsDefault"" ON ""Currencies"" (""IsDefault"") WHERE ""IsDefault"" = true;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Currencies_UpdatedByUserId') THEN
                        CREATE INDEX ""IX_Currencies_UpdatedByUserId"" ON ""Currencies"" (""UpdatedByUserId"");
                    END IF;

                    -- CustomBranding indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_CustomBranding_Id') THEN
                        CREATE UNIQUE INDEX ""IX_CustomBranding_Id"" ON ""CustomBranding"" (""Id"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_CustomBranding_UpdatedBy') THEN
                        CREATE INDEX ""IX_CustomBranding_UpdatedBy"" ON ""CustomBranding"" (""UpdatedBy"");
                    END IF;

                    -- DataRetentionPolicy indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_DataRetentionPolicy_CreatedBy') THEN
                        CREATE INDEX ""IX_DataRetentionPolicy_CreatedBy"" ON ""DataRetentionPolicy"" (""CreatedBy"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_DataRetentionPolicy_EntityType') THEN
                        CREATE UNIQUE INDEX ""IX_DataRetentionPolicy_EntityType"" ON ""DataRetentionPolicy"" (""EntityType"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_DataRetentionPolicy_IsActive') THEN
                        CREATE INDEX ""IX_DataRetentionPolicy_IsActive"" ON ""DataRetentionPolicy"" (""IsActive"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_DataRetentionPolicy_UpdatedBy') THEN
                        CREATE INDEX ""IX_DataRetentionPolicy_UpdatedBy"" ON ""DataRetentionPolicy"" (""UpdatedBy"");
                    END IF;

                    -- ExchangeRates indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ExchangeRates_CreatedByUserId') THEN
                        CREATE INDEX ""IX_ExchangeRates_CreatedByUserId"" ON ""ExchangeRates"" (""CreatedByUserId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ExchangeRates_EffectiveDate') THEN
                        CREATE INDEX ""IX_ExchangeRates_EffectiveDate"" ON ""ExchangeRates"" (""EffectiveDate"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ExchangeRates_FromCurrencyCode') THEN
                        CREATE INDEX ""IX_ExchangeRates_FromCurrencyCode"" ON ""ExchangeRates"" (""FromCurrencyCode"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ExchangeRates_FromCurrencyCode_ToCurrencyCode_EffectiveDate') THEN
                        CREATE UNIQUE INDEX ""IX_ExchangeRates_FromCurrencyCode_ToCurrencyCode_EffectiveDate"" ON ""ExchangeRates"" (""FromCurrencyCode"", ""ToCurrencyCode"", ""EffectiveDate"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ExchangeRates_IsActive') THEN
                        CREATE INDEX ""IX_ExchangeRates_IsActive"" ON ""ExchangeRates"" (""IsActive"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_ExchangeRates_ToCurrencyCode') THEN
                        CREATE INDEX ""IX_ExchangeRates_ToCurrencyCode"" ON ""ExchangeRates"" (""ToCurrencyCode"");
                    END IF;

                    -- IntegrationKeys indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_IntegrationKeys_CreatedAt') THEN
                        CREATE INDEX ""IX_IntegrationKeys_CreatedAt"" ON ""IntegrationKeys"" (""CreatedAt"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_IntegrationKeys_CreatedBy') THEN
                        CREATE INDEX ""IX_IntegrationKeys_CreatedBy"" ON ""IntegrationKeys"" (""CreatedBy"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_IntegrationKeys_KeyName') THEN
                        CREATE INDEX ""IX_IntegrationKeys_KeyName"" ON ""IntegrationKeys"" (""KeyName"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_IntegrationKeys_Provider') THEN
                        CREATE INDEX ""IX_IntegrationKeys_Provider"" ON ""IntegrationKeys"" (""Provider"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_IntegrationKeys_UpdatedBy') THEN
                        CREATE INDEX ""IX_IntegrationKeys_UpdatedBy"" ON ""IntegrationKeys"" (""UpdatedBy"");
                    END IF;

                    -- LocalizationResources indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocalizationResources_Category') THEN
                        CREATE INDEX ""IX_LocalizationResources_Category"" ON ""LocalizationResources"" (""Category"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocalizationResources_CreatedByUserId') THEN
                        CREATE INDEX ""IX_LocalizationResources_CreatedByUserId"" ON ""LocalizationResources"" (""CreatedByUserId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocalizationResources_IsActive') THEN
                        CREATE INDEX ""IX_LocalizationResources_IsActive"" ON ""LocalizationResources"" (""IsActive"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocalizationResources_LanguageCode') THEN
                        CREATE INDEX ""IX_LocalizationResources_LanguageCode"" ON ""LocalizationResources"" (""LanguageCode"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocalizationResources_LanguageCode_ResourceKey') THEN
                        CREATE UNIQUE INDEX ""IX_LocalizationResources_LanguageCode_ResourceKey"" ON ""LocalizationResources"" (""LanguageCode"", ""ResourceKey"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocalizationResources_ResourceKey') THEN
                        CREATE INDEX ""IX_LocalizationResources_ResourceKey"" ON ""LocalizationResources"" (""ResourceKey"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocalizationResources_UpdatedByUserId') THEN
                        CREATE INDEX ""IX_LocalizationResources_UpdatedByUserId"" ON ""LocalizationResources"" (""UpdatedByUserId"");
                    END IF;

                    -- Mentions indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Mentions_CreatedAt') THEN
                        CREATE INDEX ""IX_Mentions_CreatedAt"" ON ""Mentions"" (""CreatedAt"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Mentions_EntityType_EntityId') THEN
                        CREATE INDEX ""IX_Mentions_EntityType_EntityId"" ON ""Mentions"" (""EntityType"", ""EntityId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Mentions_MentionedByUserId') THEN
                        CREATE INDEX ""IX_Mentions_MentionedByUserId"" ON ""Mentions"" (""MentionedByUserId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Mentions_MentionedUserId_IsRead') THEN
                        CREATE INDEX ""IX_Mentions_MentionedUserId_IsRead"" ON ""Mentions"" (""MentionedUserId"", ""IsRead"");
                    END IF;

                    -- NotificationSettings indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_NotificationSettings_Id') THEN
                        CREATE UNIQUE INDEX ""IX_NotificationSettings_Id"" ON ""NotificationSettings"" (""Id"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_NotificationSettings_UpdatedBy') THEN
                        CREATE INDEX ""IX_NotificationSettings_UpdatedBy"" ON ""NotificationSettings"" (""UpdatedBy"");
                    END IF;

                    -- QuotationPageViews indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_QuotationPageViews_AccessLinkId') THEN
                        CREATE INDEX ""IX_QuotationPageViews_AccessLinkId"" ON ""QuotationPageViews"" (""AccessLinkId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_QuotationPageViews_ClientEmail') THEN
                        CREATE INDEX ""IX_QuotationPageViews_ClientEmail"" ON ""QuotationPageViews"" (""ClientEmail"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_QuotationPageViews_QuotationId') THEN
                        CREATE INDEX ""IX_QuotationPageViews_QuotationId"" ON ""QuotationPageViews"" (""QuotationId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_QuotationPageViews_ViewStartTime') THEN
                        CREATE INDEX ""IX_QuotationPageViews_ViewStartTime"" ON ""QuotationPageViews"" (""ViewStartTime"");
                    END IF;

                    -- SupportedLanguages indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_SupportedLanguages_IsActive') THEN
                        CREATE INDEX ""IX_SupportedLanguages_IsActive"" ON ""SupportedLanguages"" (""IsActive"");
                    END IF;

                    -- SystemSettings indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_SystemSettings_LastModifiedAt') THEN
                        CREATE INDEX ""IX_SystemSettings_LastModifiedAt"" ON ""SystemSettings"" (""LastModifiedAt"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_SystemSettings_LastModifiedBy') THEN
                        CREATE INDEX ""IX_SystemSettings_LastModifiedBy"" ON ""SystemSettings"" (""LastModifiedBy"");
                    END IF;

                    -- TaskAssignments indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_TaskAssignments_AssignedByUserId') THEN
                        CREATE INDEX ""IX_TaskAssignments_AssignedByUserId"" ON ""TaskAssignments"" (""AssignedByUserId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_TaskAssignments_AssignedToUserId_Status') THEN
                        CREATE INDEX ""IX_TaskAssignments_AssignedToUserId_Status"" ON ""TaskAssignments"" (""AssignedToUserId"", ""Status"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_TaskAssignments_CreatedAt') THEN
                        CREATE INDEX ""IX_TaskAssignments_CreatedAt"" ON ""TaskAssignments"" (""CreatedAt"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_TaskAssignments_DueDate') THEN
                        CREATE INDEX ""IX_TaskAssignments_DueDate"" ON ""TaskAssignments"" (""DueDate"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_TaskAssignments_EntityType_EntityId') THEN
                        CREATE INDEX ""IX_TaskAssignments_EntityType_EntityId"" ON ""TaskAssignments"" (""EntityType"", ""EntityId"");
                    END IF;

                    -- TeamMembers indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_TeamMembers_TeamId') THEN
                        CREATE INDEX ""IX_TeamMembers_TeamId"" ON ""TeamMembers"" (""TeamId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_TeamMembers_TeamId_UserId') THEN
                        CREATE UNIQUE INDEX ""IX_TeamMembers_TeamId_UserId"" ON ""TeamMembers"" (""TeamId"", ""UserId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_TeamMembers_UserId') THEN
                        CREATE INDEX ""IX_TeamMembers_UserId"" ON ""TeamMembers"" (""UserId"");
                    END IF;

                    -- Teams indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Teams_CompanyId') THEN
                        CREATE INDEX ""IX_Teams_CompanyId"" ON ""Teams"" (""CompanyId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Teams_CompanyId_Name') THEN
                        CREATE UNIQUE INDEX ""IX_Teams_CompanyId_Name"" ON ""Teams"" (""CompanyId"", ""Name"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Teams_IsActive') THEN
                        CREATE INDEX ""IX_Teams_IsActive"" ON ""Teams"" (""IsActive"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Teams_ParentTeamId') THEN
                        CREATE INDEX ""IX_Teams_ParentTeamId"" ON ""Teams"" (""ParentTeamId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Teams_TeamLeadUserId') THEN
                        CREATE INDEX ""IX_Teams_TeamLeadUserId"" ON ""Teams"" (""TeamLeadUserId"");
                    END IF;

                    -- UserActivities indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserActivities_ActionType_Timestamp') THEN
                        CREATE INDEX ""IX_UserActivities_ActionType_Timestamp"" ON ""UserActivities"" (""ActionType"", ""Timestamp"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserActivities_EntityType') THEN
                        CREATE INDEX ""IX_UserActivities_EntityType"" ON ""UserActivities"" (""EntityType"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserActivities_Timestamp') THEN
                        CREATE INDEX ""IX_UserActivities_Timestamp"" ON ""UserActivities"" (""Timestamp"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserActivities_UserId_Timestamp') THEN
                        CREATE INDEX ""IX_UserActivities_UserId_Timestamp"" ON ""UserActivities"" (""UserId"", ""Timestamp"");
                    END IF;

                    -- UserGroupMembers indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserGroupMembers_GroupId') THEN
                        CREATE INDEX ""IX_UserGroupMembers_GroupId"" ON ""UserGroupMembers"" (""GroupId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserGroupMembers_GroupId_UserId') THEN
                        CREATE UNIQUE INDEX ""IX_UserGroupMembers_GroupId_UserId"" ON ""UserGroupMembers"" (""GroupId"", ""UserId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserGroupMembers_UserId') THEN
                        CREATE INDEX ""IX_UserGroupMembers_UserId"" ON ""UserGroupMembers"" (""UserId"");
                    END IF;

                    -- UserGroups indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserGroups_CreatedAt') THEN
                        CREATE INDEX ""IX_UserGroups_CreatedAt"" ON ""UserGroups"" (""CreatedAt"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserGroups_CreatedByUserId') THEN
                        CREATE INDEX ""IX_UserGroups_CreatedByUserId"" ON ""UserGroups"" (""CreatedByUserId"");
                    END IF;

                    -- UserPreferences indexes
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserPreferences_CurrencyCode') THEN
                        CREATE INDEX ""IX_UserPreferences_CurrencyCode"" ON ""UserPreferences"" (""CurrencyCode"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_UserPreferences_LanguageCode') THEN
                        CREATE INDEX ""IX_UserPreferences_LanguageCode"" ON ""UserPreferences"" (""LanguageCode"");
                    END IF;

                    -- Roles index
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Roles_IsBuiltIn') THEN
                        CREATE INDEX ""IX_Roles_IsBuiltIn"" ON ""Roles"" (""IsBuiltIn"");
                    END IF;
                END $$;
            ");

            // Add foreign key and indexes for Users table if they don't exist
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- Create indexes if they don't exist
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Users_DelegateUserId') THEN
                        CREATE INDEX ""IX_Users_DelegateUserId"" ON ""Users"" (""DelegateUserId"");
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Users_LastSeenAt') THEN
                        CREATE INDEX ""IX_Users_LastSeenAt"" ON ""Users"" (""LastSeenAt"");
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Users_PresenceStatus') THEN
                        CREATE INDEX ""IX_Users_PresenceStatus"" ON ""Users"" (""PresenceStatus"");
                    END IF;
                    
                    -- Add foreign key if it doesn't exist
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_name = 'FK_Users_Users_DelegateUserId'
                    ) THEN
                        ALTER TABLE ""Users"" 
                        ADD CONSTRAINT ""FK_Users_Users_DelegateUserId"" 
                        FOREIGN KEY (""DelegateUserId"") 
                        REFERENCES ""Users"" (""UserId"") 
                        ON DELETE SET NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_DelegateUserId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "ClientPortalOtps");

            migrationBuilder.DropTable(
                name: "CompanyPreferences");

            migrationBuilder.DropTable(
                name: "CustomBranding");

            migrationBuilder.DropTable(
                name: "DataRetentionPolicy");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "IntegrationKeys");

            migrationBuilder.DropTable(
                name: "LocalizationResources");

            migrationBuilder.DropTable(
                name: "Mentions");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "QuotationPageViews");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TaskAssignments");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "UserGroupMembers");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "SupportedLanguages");

            migrationBuilder.DropIndex(
                name: "IX_Users_DelegateUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastSeenAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PresenceStatus",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_IsBuiltIn",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DelegateUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OutOfOfficeMessage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OutOfOfficeStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PresenceStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBuiltIn",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "Roles");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Quotations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
