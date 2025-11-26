using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationsForSpec25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_RecipientUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_DeliveryStatus",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientUserId_IsRead",
                table: "Notifications");

            // Guard for environments where this legacy index may not exist
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Notifications_RelatedEntityType_RelatedEntityId\";");

            // Guarded additions for environments where these columns may already exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'QuotationTemplates' AND column_name = 'OriginalFileName'
                    ) THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""OriginalFileName"" text;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'QuotationTemplates' AND column_name = 'ProcessingErrorMessage'
                    ) THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""ProcessingErrorMessage"" text;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'QuotationTemplates' AND column_name = 'ProcessingStatus'
                    ) THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""ProcessingStatus"" text;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'QuotationTemplates' AND column_name = 'TemplateFilePath'
                    ) THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""TemplateFilePath"" text;
                    END IF;
                END $$;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "RelatedEntityId",
                table: "Notifications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Meta",
                table: "Notifications",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "Notifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryStatus",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "SENT");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveredChannels",
                table: "Notifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<Guid>(
                name: "NotificationId",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Guard AddColumn for Notifications
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Notifications' AND column_name = 'NotificationTypeId'
                    ) THEN
                        ALTER TABLE ""Notifications"" ADD COLUMN ""NotificationTypeId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Notifications' AND column_name = 'SentVia'
                    ) THEN
                        ALTER TABLE ""Notifications"" ADD COLUMN ""SentVia"" character varying(100) NOT NULL DEFAULT '';
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Notifications' AND column_name = 'Title'
                    ) THEN
                        ALTER TABLE ""Notifications"" ADD COLUMN ""Title"" character varying(255) NOT NULL DEFAULT '';
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Notifications' AND column_name = 'UpdatedAt'
                    ) THEN
                        ALTER TABLE ""Notifications"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT NOW();
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Notifications' AND column_name = 'UserId'
                    ) THEN
                        ALTER TABLE ""Notifications"" ADD COLUMN ""UserId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
                    END IF;
                END $$;
            ");

            // Create DocumentTemplates if not exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""DocumentTemplates"" (
                    ""TemplateId"" uuid PRIMARY KEY,
                    ""Name"" text NOT NULL,
                    ""Description"" text NOT NULL,
                    ""TemplateType"" text NOT NULL,
                    ""FilePath"" text NOT NULL,
                    ""OriginalFileName"" text NOT NULL,
                    ""FileSizeBytes"" bigint NOT NULL,
                    ""IsActive"" boolean NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NULL,
                    ""CreatedByUserId"" uuid NOT NULL
                );
            ");

            // Create NotificationTypes if not exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""NotificationTypes"" (
                    ""NotificationTypeId"" uuid NOT NULL DEFAULT gen_random_uuid(),
                    ""TypeName"" character varying(100) NOT NULL,
                    ""Description"" character varying(1000),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT NOW(),
                    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_NotificationTypes"" PRIMARY KEY (""NotificationTypeId"")
                );
            ");

            // Create TemplatePlaceholders if not exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""TemplatePlaceholders"" (
                    ""PlaceholderId"" uuid PRIMARY KEY,
                    ""TemplateId"" uuid NOT NULL,
                    ""PlaceholderName"" character varying(100) NOT NULL,
                    ""PlaceholderType"" character varying(50) NOT NULL,
                    ""OriginalText"" text NULL,
                    ""DefaultValue"" text NULL,
                    ""PositionInDocument"" integer NULL,
                    ""IsManuallyAdded"" boolean NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""QuotationTemplateTemplateId"" uuid NULL
                );
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Notifications_CreatedAt'
                    ) THEN
                        CREATE INDEX ""IX_Notifications_CreatedAt"" ON ""Notifications""(""CreatedAt"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Notifications_NotificationTypeId'
                    ) THEN
                        CREATE INDEX ""IX_Notifications_NotificationTypeId"" ON ""Notifications""(""NotificationTypeId"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Notifications_RecipientUserId_IsArchived'
                    ) THEN
                        CREATE INDEX ""IX_Notifications_RecipientUserId_IsArchived"" ON ""Notifications""(""RecipientUserId"", ""IsArchived"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Notifications_RecipientUserId_IsRead'
                    ) THEN
                        CREATE INDEX ""IX_Notifications_RecipientUserId_IsRead"" ON ""Notifications""(""RecipientUserId"", ""IsRead"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Notifications_RelatedEntityId'
                    ) THEN
                        CREATE INDEX ""IX_Notifications_RelatedEntityId"" ON ""Notifications""(""RelatedEntityId"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Notifications_UserId'
                    ) THEN
                        CREATE INDEX ""IX_Notifications_UserId"" ON ""Notifications""(""UserId"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Notifications_UserId_CreatedAt'
                    ) THEN
                        CREATE INDEX ""IX_Notifications_UserId_CreatedAt"" ON ""Notifications""(""UserId"", ""CreatedAt"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Notifications_UserId_IsRead'
                    ) THEN
                        CREATE INDEX ""IX_Notifications_UserId_IsRead"" ON ""Notifications""(""UserId"", ""IsRead"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'UQ_NotificationTypes_TypeName'
                    ) THEN
                        CREATE UNIQUE INDEX ""UQ_NotificationTypes_TypeName"" ON ""NotificationTypes""(""TypeName"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_TemplatePlaceholders_QuotationTemplateTemplateId'
                    ) THEN
                        CREATE INDEX ""IX_TemplatePlaceholders_QuotationTemplateTemplateId"" ON ""TemplatePlaceholders""(""QuotationTemplateTemplateId"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_TemplatePlaceholders_TemplateId'
                    ) THEN
                        CREATE INDEX ""IX_TemplatePlaceholders_TemplateId"" ON ""TemplatePlaceholders""(""TemplateId"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_TemplatePlaceholders_TemplateId_PlaceholderName'
                    ) THEN
                        CREATE UNIQUE INDEX ""IX_TemplatePlaceholders_TemplateId_PlaceholderName"" ON ""TemplatePlaceholders""(""TemplateId"", ""PlaceholderName"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_TemplatePlaceholders_TemplateId_Type'
                    ) THEN
                        CREATE INDEX ""IX_TemplatePlaceholders_TemplateId_Type"" ON ""TemplatePlaceholders""(""TemplateId"", ""PlaceholderType"");
                    END IF;
                END $$;
            ");

            // Guarded foreign keys for Notifications
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Notifications_NotificationTypes_NotificationTypeId'
                    ) THEN
                        ALTER TABLE ""Notifications"" ADD CONSTRAINT ""FK_Notifications_NotificationTypes_NotificationTypeId""
                        FOREIGN KEY (""NotificationTypeId"") REFERENCES ""NotificationTypes""(""NotificationTypeId"") ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Notifications_Users_UserId'
                    ) THEN
                        ALTER TABLE ""Notifications"" ADD CONSTRAINT ""FK_Notifications_Users_UserId""
                        FOREIGN KEY (""UserId"") REFERENCES ""Users""(""UserId"") ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationTypes_NotificationTypeId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationTypes");

            migrationBuilder.DropTable(
                name: "TemplatePlaceholders");

            migrationBuilder.DropTable(
                name: "DocumentTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_NotificationTypeId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientUserId_IsArchived",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientUserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "QuotationTemplates");

            migrationBuilder.DropColumn(
                name: "ProcessingErrorMessage",
                table: "QuotationTemplates");

            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "QuotationTemplates");

            migrationBuilder.DropColumn(
                name: "TemplateFilePath",
                table: "QuotationTemplates");

            migrationBuilder.DropColumn(
                name: "NotificationTypeId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SentVia",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RelatedEntityId",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Meta",
                table: "Notifications",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10000)",
                oldMaxLength: 10000);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryStatus",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "SENT",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "PENDING");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveredChannels",
                table: "Notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<Guid>(
                name: "NotificationId",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DeliveryStatus",
                table: "Notifications",
                column: "DeliveryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "IsRead" },
                filter: "\"IsRead\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedEntityType_RelatedEntityId",
                table: "Notifications",
                columns: new[] { "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_RecipientUserId",
                table: "Notifications",
                column: "RecipientUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
