using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateQuotationTemplatesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create QuotationTemplates table if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'QuotationTemplates') THEN
                        CREATE TABLE ""QuotationTemplates"" (
                            ""TemplateId"" uuid PRIMARY KEY,
                            ""Name"" character varying(100) NOT NULL,
                            ""Description"" character varying(255),
                            ""OwnerUserId"" uuid NOT NULL,
                            ""OwnerRole"" character varying(50) NOT NULL DEFAULT 'SalesRep',
                            ""Visibility"" character varying(50) NOT NULL,
                            ""IsApproved"" boolean NOT NULL DEFAULT false,
                            ""ApprovedByUserId"" uuid,
                            ""ApprovedAt"" timestamp with time zone,
                            ""Version"" integer NOT NULL DEFAULT 1,
                            ""PreviousVersionId"" uuid,
                            ""UsageCount"" integer NOT NULL DEFAULT 0,
                            ""LastUsedAt"" timestamp with time zone,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""DeletedAt"" timestamp with time zone,
                            ""DiscountDefault"" numeric(5,2),
                            ""Notes"" character varying(2000),
                            ""TemplateType"" character varying(50),
                            ""IsFileBased"" boolean NOT NULL DEFAULT false,
                            ""FileName"" character varying(255),
                            ""FileUrl"" text,
                            ""FileSize"" bigint,
                            ""MimeType"" character varying(100),
                            CONSTRAINT ""FK_QuotationTemplates_Users_OwnerUserId"" 
                                FOREIGN KEY (""OwnerUserId"") REFERENCES ""Users""(""UserId"") ON DELETE RESTRICT,
                            CONSTRAINT ""FK_QuotationTemplates_Users_ApprovedByUserId"" 
                                FOREIGN KEY (""ApprovedByUserId"") REFERENCES ""Users""(""UserId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_QuotationTemplates_QuotationTemplates_PreviousVersionId"" 
                                FOREIGN KEY (""PreviousVersionId"") REFERENCES ""QuotationTemplates""(""TemplateId"") ON DELETE SET NULL
                        );

                        CREATE INDEX ""IX_QuotationTemplates_OwnerUserId"" ON ""QuotationTemplates""(""OwnerUserId"");
                        CREATE INDEX ""IX_QuotationTemplates_ApprovedByUserId"" ON ""QuotationTemplates""(""ApprovedByUserId"");
                        CREATE INDEX ""IX_QuotationTemplates_Name"" ON ""QuotationTemplates""(""Name"") WHERE ""DeletedAt"" IS NULL;
                        CREATE INDEX ""IX_QuotationTemplates_OwnerUserId_Visibility"" ON ""QuotationTemplates""(""OwnerUserId"", ""Visibility"") WHERE ""DeletedAt"" IS NULL;
                        CREATE INDEX ""IX_QuotationTemplates_IsApproved_Visibility"" ON ""QuotationTemplates""(""IsApproved"", ""Visibility"") WHERE ""DeletedAt"" IS NULL;
                        CREATE INDEX ""IX_QuotationTemplates_UpdatedAt"" ON ""QuotationTemplates""(""UpdatedAt"") WHERE ""DeletedAt"" IS NULL;
                        CREATE INDEX ""IX_QuotationTemplates_PreviousVersionId"" ON ""QuotationTemplates""(""PreviousVersionId"") WHERE ""PreviousVersionId"" IS NOT NULL;
                        CREATE UNIQUE INDEX ""IX_QuotationTemplates_Name_OwnerUserId"" ON ""QuotationTemplates""(""Name"", ""OwnerUserId"") WHERE ""DeletedAt"" IS NULL;
                    END IF;
                END $$;
            ");

            // Create QuotationTemplateLineItems table if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'QuotationTemplateLineItems') THEN
                        CREATE TABLE ""QuotationTemplateLineItems"" (
                            ""LineItemId"" uuid PRIMARY KEY,
                            ""TemplateId"" uuid NOT NULL,
                            ""SequenceNumber"" integer NOT NULL,
                            ""ItemName"" character varying(255) NOT NULL,
                            ""Description"" character varying(1000),
                            ""Quantity"" numeric(10,2) NOT NULL,
                            ""UnitRate"" numeric(12,2) NOT NULL,
                            ""Amount"" numeric(12,2) NOT NULL,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            CONSTRAINT ""FK_QuotationTemplateLineItems_QuotationTemplates_TemplateId"" 
                                FOREIGN KEY (""TemplateId"") REFERENCES ""QuotationTemplates""(""TemplateId"") ON DELETE CASCADE
                        );

                        CREATE INDEX ""IX_QuotationTemplateLineItems_TemplateId"" ON ""QuotationTemplateLineItems""(""TemplateId"");
                        CREATE INDEX ""IX_QuotationTemplateLineItems_TemplateId_SequenceNumber"" ON ""QuotationTemplateLineItems""(""TemplateId"", ""SequenceNumber"");
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationTemplateLineItems");

            migrationBuilder.DropTable(
                name: "QuotationTemplates");
        }
    }
}
