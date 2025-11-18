using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CRM.Infrastructure.Persistence;

var services = new ServiceCollection();

string conn = args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
    ? args[0]
    : (Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=crm;Username=postgres;Password=postgres");

services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

var sp = services.BuildServiceProvider();

try
{
    using var scope = sp.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Create missing tables if they don't exist
    Console.WriteLine("Checking for missing tables...");
    
    // Create ScheduledReports table
    try
    {
        var scheduledReportsSql = @"
CREATE TABLE IF NOT EXISTS ""ScheduledReports"" (
    ""ReportId"" uuid NOT NULL,
    ""CreatedByUserId"" uuid NOT NULL,
    ""ReportName"" character varying(200) NOT NULL,
    ""ReportType"" character varying(100) NOT NULL,
    ""ReportConfig"" jsonb NOT NULL,
    ""RecurrencePattern"" character varying(20) NOT NULL,
    ""EmailRecipients"" text NOT NULL,
    ""IsActive"" boolean NOT NULL DEFAULT true,
    ""LastSentAt"" timestamp with time zone,
    ""NextScheduledAt"" timestamp with time zone NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT NOW(),
    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT ""PK_ScheduledReports"" PRIMARY KEY (""ReportId""),
    CONSTRAINT ""CK_ScheduledReports_RecurrencePattern"" CHECK (""RecurrencePattern"" IN ('daily', 'weekly', 'monthly')),
    CONSTRAINT ""FK_ScheduledReports_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ""IX_ScheduledReports_CreatedByUserId"" ON ""ScheduledReports"" (""CreatedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_ScheduledReports_IsActive_NextScheduledAt"" ON ""ScheduledReports"" (""IsActive"", ""NextScheduledAt"");
";
        await ctx.Database.ExecuteSqlRawAsync(scheduledReportsSql);
        Console.WriteLine("✓ ScheduledReports table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (ScheduledReports): {tableEx.Message}");
    }
    
    // Create DiscountApprovals table (without Quotations FK if Quotations doesn't exist)
    try
    {
        // Check if Quotations table exists
        bool quotationsTableExists = false;
        try
        {
            await ctx.Database.ExecuteSqlRawAsync(@"SELECT 1 FROM ""Quotations"" LIMIT 1");
            quotationsTableExists = true;
        }
        catch { }
        
        // Create table without Quotations FK first, then add FK if Quotations exists
        var createTableSql = @"
CREATE TABLE IF NOT EXISTS ""DiscountApprovals"" (
    ""ApprovalId"" uuid NOT NULL,
    ""QuotationId"" uuid NOT NULL,
    ""RequestedByUserId"" uuid NOT NULL,
    ""ApproverUserId"" uuid,
    ""Status"" character varying(50) NOT NULL,
    ""RequestDate"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""ApprovalDate"" timestamp with time zone,
    ""RejectionDate"" timestamp with time zone,
    ""CurrentDiscountPercentage"" numeric(5,2) NOT NULL,
    ""Threshold"" numeric(5,2) NOT NULL,
    ""ApprovalLevel"" character varying(50) NOT NULL,
    ""Reason"" TEXT NOT NULL,
    ""Comments"" TEXT,
    ""EscalatedToAdmin"" boolean NOT NULL DEFAULT false,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ""PK_DiscountApprovals"" PRIMARY KEY (""ApprovalId""),
    CONSTRAINT ""CK_DiscountApprovals_CurrentDiscountPercentage"" CHECK (""CurrentDiscountPercentage"" >= 0),
    CONSTRAINT ""CK_DiscountApprovals_Threshold"" CHECK (""Threshold"" >= 0),
    CONSTRAINT ""FK_DiscountApprovals_Users_ApproverUserId"" FOREIGN KEY (""ApproverUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL,
    CONSTRAINT ""FK_DiscountApprovals_Users_RequestedByUserId"" FOREIGN KEY (""RequestedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
);
";
        await ctx.Database.ExecuteSqlRawAsync(createTableSql);
        
        // Add Quotations FK if table exists and FK doesn't exist
        if (quotationsTableExists)
        {
            try
            {
                await ctx.Database.ExecuteSqlRawAsync(@"
ALTER TABLE ""DiscountApprovals""
ADD CONSTRAINT ""FK_DiscountApprovals_Quotations_QuotationId""
FOREIGN KEY (""QuotationId"") REFERENCES ""Quotations"" (""QuotationId"") ON DELETE RESTRICT;
");
            }
            catch (Exception fkEx)
            {
                // FK might already exist, ignore
                if (!fkEx.Message.Contains("already exists"))
                {
                    Console.WriteLine($"Note (DiscountApprovals FK): {fkEx.Message}");
                }
            }
        }
        
        // Create indexes
        var indexesSql = @"
CREATE INDEX IF NOT EXISTS ""IX_DiscountApprovals_ApproverUserId_Status"" ON ""DiscountApprovals"" (""ApproverUserId"", ""Status"") WHERE ""ApproverUserId"" IS NOT NULL;
CREATE INDEX IF NOT EXISTS ""IX_DiscountApprovals_QuotationId"" ON ""DiscountApprovals"" (""QuotationId"");
CREATE INDEX IF NOT EXISTS ""IX_DiscountApprovals_RequestedByUserId"" ON ""DiscountApprovals"" (""RequestedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_DiscountApprovals_CurrentDiscountPercentage"" ON ""DiscountApprovals"" (""CurrentDiscountPercentage"");
CREATE INDEX IF NOT EXISTS ""IX_DiscountApprovals_Status"" ON ""DiscountApprovals"" (""Status"");
CREATE INDEX IF NOT EXISTS ""IX_DiscountApprovals_CreatedAt_Status"" ON ""DiscountApprovals"" (""CreatedAt"", ""Status"");
";
        await ctx.Database.ExecuteSqlRawAsync(indexesSql);
        Console.WriteLine($"✓ DiscountApprovals table created/verified.{(quotationsTableExists ? " (with Quotations FK)" : " (without Quotations FK - will be added when Quotations table exists)")}");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (DiscountApprovals): {tableEx.Message}");
    }
    
    // Create Notifications table
    try
    {
        var notificationsSql = @"
CREATE TABLE IF NOT EXISTS ""Notifications"" (
    ""NotificationId"" uuid NOT NULL,
    ""RecipientUserId"" uuid NOT NULL,
    ""RelatedEntityType"" character varying(50) NOT NULL,
    ""RelatedEntityId"" uuid NOT NULL,
    ""EventType"" character varying(50) NOT NULL,
    ""Message"" character varying(500) NOT NULL,
    ""IsRead"" boolean NOT NULL DEFAULT false,
    ""IsArchived"" boolean NOT NULL DEFAULT false,
    ""DeliveredChannels"" character varying(255),
    ""DeliveryStatus"" character varying(50) NOT NULL DEFAULT 'SENT',
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""ReadAt"" timestamp with time zone,
    ""ArchivedAt"" timestamp with time zone,
    ""Meta"" jsonb,
    CONSTRAINT ""PK_Notifications"" PRIMARY KEY (""NotificationId""),
    CONSTRAINT ""FK_Notifications_Users_RecipientUserId"" FOREIGN KEY (""RecipientUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ""IX_Notifications_RecipientUserId"" ON ""Notifications"" (""RecipientUserId"");
CREATE INDEX IF NOT EXISTS ""IX_Notifications_IsRead"" ON ""Notifications"" (""IsRead"");
CREATE INDEX IF NOT EXISTS ""IX_Notifications_IsArchived"" ON ""Notifications"" (""IsArchived"");
CREATE INDEX IF NOT EXISTS ""IX_Notifications_RelatedEntity"" ON ""Notifications"" (""RelatedEntityType"", ""RelatedEntityId"");
CREATE INDEX IF NOT EXISTS ""IX_Notifications_DeliveryStatus"" ON ""Notifications"" (""DeliveryStatus"");
CREATE INDEX IF NOT EXISTS ""IX_Notifications_CreatedAt"" ON ""Notifications"" (""CreatedAt"" DESC);
CREATE INDEX IF NOT EXISTS ""IX_Notifications_Unread"" ON ""Notifications"" (""RecipientUserId"", ""IsRead"") WHERE ""IsRead"" = false;
";
        await ctx.Database.ExecuteSqlRawAsync(notificationsSql);
        Console.WriteLine("✓ Notifications table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (Notifications): {tableEx.Message}");
    }
    
    // Create NotificationPreferences table
    try
    {
        var notificationPrefsSql = @"
CREATE TABLE IF NOT EXISTS ""NotificationPreferences"" (
    ""UserId"" uuid NOT NULL,
    ""PreferenceData"" jsonb NOT NULL DEFAULT '{}',
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ""PK_NotificationPreferences"" PRIMARY KEY (""UserId""),
    CONSTRAINT ""FK_NotificationPreferences_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""UserId"") ON DELETE CASCADE
);
";
        await ctx.Database.ExecuteSqlRawAsync(notificationPrefsSql);
        Console.WriteLine("✓ NotificationPreferences table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (NotificationPreferences): {tableEx.Message}");
    }
    
    // Create UserPreferences table
    try
    {
        var userPrefsSql = @"
CREATE TABLE IF NOT EXISTS ""UserPreferences"" (
    ""UserId"" uuid NOT NULL,
    ""LanguageCode"" character varying(5) NOT NULL DEFAULT 'en',
    ""CurrencyCode"" character varying(3),
    ""DateFormat"" character varying(20) NOT NULL DEFAULT 'dd/MM/yyyy',
    ""TimeFormat"" character varying(10) NOT NULL DEFAULT '24h',
    ""NumberFormat"" character varying(50) NOT NULL DEFAULT 'en-IN',
    ""Timezone"" character varying(50),
    ""FirstDayOfWeek"" integer NOT NULL DEFAULT 1,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ""PK_UserPreferences"" PRIMARY KEY (""UserId""),
    CONSTRAINT ""FK_UserPreferences_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""UserId"") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ""IX_UserPreferences_LanguageCode"" ON ""UserPreferences"" (""LanguageCode"");
CREATE INDEX IF NOT EXISTS ""IX_UserPreferences_CurrencyCode"" ON ""UserPreferences"" (""CurrencyCode"");
";
        await ctx.Database.ExecuteSqlRawAsync(userPrefsSql);
        Console.WriteLine("✓ UserPreferences table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (UserPreferences): {tableEx.Message}");
    }
    
    // Create Quotations table
    try
    {
        var quotationsSql = @"
CREATE TABLE IF NOT EXISTS ""Quotations"" (
    ""QuotationId"" uuid NOT NULL,
    ""ClientId"" uuid NOT NULL,
    ""CreatedByUserId"" uuid NOT NULL,
    ""QuotationNumber"" character varying(50) NOT NULL,
    ""Status"" integer NOT NULL DEFAULT 0,
    ""QuotationDate"" date NOT NULL,
    ""ValidUntil"" date NOT NULL,
    ""SubTotal"" numeric(12,2) NOT NULL DEFAULT 0,
    ""DiscountAmount"" numeric(12,2) NOT NULL DEFAULT 0,
    ""DiscountPercentage"" numeric(5,2) NOT NULL DEFAULT 0,
    ""TaxAmount"" numeric(12,2) NOT NULL DEFAULT 0,
    ""CgstAmount"" numeric(12,2),
    ""SgstAmount"" numeric(12,2),
    ""IgstAmount"" numeric(12,2),
    ""TotalAmount"" numeric(12,2) NOT NULL DEFAULT 0,
    ""Notes"" text,
    ""IsPendingApproval"" boolean NOT NULL DEFAULT false,
    ""PendingApprovalId"" uuid,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ""PK_Quotations"" PRIMARY KEY (""QuotationId""),
    CONSTRAINT ""FK_Quotations_Clients_ClientId"" FOREIGN KEY (""ClientId"") REFERENCES ""Clients"" (""ClientId"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_Quotations_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT,
    CONSTRAINT ""UQ_Quotations_QuotationNumber"" UNIQUE (""QuotationNumber""),
    CONSTRAINT ""CK_Quotations_DiscountPercentage"" CHECK (""DiscountPercentage"" >= 0 AND ""DiscountPercentage"" <= 100),
    CONSTRAINT ""CK_Quotations_ValidUntil"" CHECK (""ValidUntil"" >= ""QuotationDate"")
);

CREATE INDEX IF NOT EXISTS ""IX_Quotations_ClientId"" ON ""Quotations"" (""ClientId"");
CREATE INDEX IF NOT EXISTS ""IX_Quotations_CreatedByUserId"" ON ""Quotations"" (""CreatedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_Quotations_Status"" ON ""Quotations"" (""Status"");
CREATE INDEX IF NOT EXISTS ""IX_Quotations_QuotationDate"" ON ""Quotations"" (""QuotationDate"");
CREATE INDEX IF NOT EXISTS ""IX_Quotations_ValidUntil"" ON ""Quotations"" (""ValidUntil"");
CREATE INDEX IF NOT EXISTS ""IX_Quotations_CreatedAt"" ON ""Quotations"" (""CreatedAt"" DESC);
CREATE INDEX IF NOT EXISTS ""IX_Quotations_ClientId_Status"" ON ""Quotations"" (""ClientId"", ""Status"");
CREATE INDEX IF NOT EXISTS ""IX_Quotations_CreatedByUserId_Status_CreatedAt"" ON ""Quotations"" (""CreatedByUserId"", ""Status"", ""CreatedAt"" DESC);
";
        await ctx.Database.ExecuteSqlRawAsync(quotationsSql);
        Console.WriteLine("✓ Quotations table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (Quotations): {tableEx.Message}");
    }
    
    // Create QuotationLineItems table
    try
    {
        var lineItemsSql = @"
CREATE TABLE IF NOT EXISTS ""QuotationLineItems"" (
    ""LineItemId"" uuid NOT NULL,
    ""QuotationId"" uuid NOT NULL,
    ""SequenceNumber"" integer NOT NULL,
    ""ItemName"" character varying(255) NOT NULL,
    ""Description"" text,
    ""Quantity"" numeric(10,2) NOT NULL,
    ""UnitRate"" numeric(12,2) NOT NULL,
    ""Amount"" numeric(12,2) NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ""PK_QuotationLineItems"" PRIMARY KEY (""LineItemId""),
    CONSTRAINT ""FK_QuotationLineItems_Quotations_QuotationId"" FOREIGN KEY (""QuotationId"") REFERENCES ""Quotations"" (""QuotationId"") ON DELETE CASCADE,
    CONSTRAINT ""CK_QuotationLineItems_Quantity"" CHECK (""Quantity"" > 0),
    CONSTRAINT ""CK_QuotationLineItems_UnitRate"" CHECK (""UnitRate"" > 0),
    CONSTRAINT ""UQ_QuotationLineItems_QuotationId_SequenceNumber"" UNIQUE (""QuotationId"", ""SequenceNumber"")
);

CREATE INDEX IF NOT EXISTS ""IX_QuotationLineItems_QuotationId"" ON ""QuotationLineItems"" (""QuotationId"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationLineItems_QuotationId_SequenceNumber"" ON ""QuotationLineItems"" (""QuotationId"", ""SequenceNumber"");
";
        await ctx.Database.ExecuteSqlRawAsync(lineItemsSql);
        Console.WriteLine("✓ QuotationLineItems table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (QuotationLineItems): {tableEx.Message}");
    }
    
    // Create QuotationAccessLinks table
    try
    {
        var accessLinksSql = @"
CREATE TABLE IF NOT EXISTS ""QuotationAccessLinks"" (
    ""AccessLinkId"" uuid NOT NULL,
    ""QuotationId"" uuid NOT NULL,
    ""ClientEmail"" character varying(255) NOT NULL,
    ""AccessToken"" character varying(500) NOT NULL,
    ""IsActive"" boolean NOT NULL DEFAULT true,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""ExpiresAt"" timestamp with time zone,
    ""SentAt"" timestamp with time zone,
    ""FirstViewedAt"" timestamp with time zone,
    ""LastViewedAt"" timestamp with time zone,
    ""ViewCount"" integer NOT NULL DEFAULT 0,
    ""IpAddress"" character varying(50),
    CONSTRAINT ""PK_QuotationAccessLinks"" PRIMARY KEY (""AccessLinkId""),
    CONSTRAINT ""FK_QuotationAccessLinks_Quotations_QuotationId"" FOREIGN KEY (""QuotationId"") REFERENCES ""Quotations"" (""QuotationId"") ON DELETE CASCADE,
    CONSTRAINT ""UQ_QuotationAccessLinks_AccessToken"" UNIQUE (""AccessToken"")
);

CREATE INDEX IF NOT EXISTS ""IX_QuotationAccessLinks_QuotationId"" ON ""QuotationAccessLinks"" (""QuotationId"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationAccessLinks_ClientEmail"" ON ""QuotationAccessLinks"" (""ClientEmail"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationAccessLinks_QuotationId_IsActive"" ON ""QuotationAccessLinks"" (""QuotationId"", ""IsActive"");
";
        await ctx.Database.ExecuteSqlRawAsync(accessLinksSql);
        Console.WriteLine("✓ QuotationAccessLinks table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (QuotationAccessLinks): {tableEx.Message}");
    }
    
    // Create QuotationStatusHistory table
    try
    {
        var statusHistorySql = @"
CREATE TABLE IF NOT EXISTS ""QuotationStatusHistory"" (
    ""HistoryId"" uuid NOT NULL,
    ""QuotationId"" uuid NOT NULL,
    ""PreviousStatus"" character varying(50),
    ""NewStatus"" character varying(50) NOT NULL,
    ""ChangedByUserId"" uuid,
    ""Reason"" character varying(500),
    ""ChangedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""IpAddress"" character varying(50),
    CONSTRAINT ""PK_QuotationStatusHistory"" PRIMARY KEY (""HistoryId""),
    CONSTRAINT ""FK_QuotationStatusHistory_Quotations_QuotationId"" FOREIGN KEY (""QuotationId"") REFERENCES ""Quotations"" (""QuotationId"") ON DELETE CASCADE,
    CONSTRAINT ""FK_QuotationStatusHistory_Users_ChangedByUserId"" FOREIGN KEY (""ChangedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ""IX_QuotationStatusHistory_QuotationId"" ON ""QuotationStatusHistory"" (""QuotationId"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationStatusHistory_QuotationId_ChangedAt"" ON ""QuotationStatusHistory"" (""QuotationId"", ""ChangedAt"" DESC);
CREATE INDEX IF NOT EXISTS ""IX_QuotationStatusHistory_ChangedByUserId"" ON ""QuotationStatusHistory"" (""ChangedByUserId"");
";
        await ctx.Database.ExecuteSqlRawAsync(statusHistorySql);
        Console.WriteLine("✓ QuotationStatusHistory table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (QuotationStatusHistory): {tableEx.Message}");
    }
    
    // Create ClientPortalOtps table
    try
    {
        var clientPortalOtpSql = @"
CREATE TABLE IF NOT EXISTS ""ClientPortalOtps"" (
    ""OtpId"" uuid NOT NULL,
    ""AccessLinkId"" uuid NOT NULL,
    ""ClientEmail"" character varying(255) NOT NULL,
    ""OtpCode"" character varying(500) NOT NULL,
    ""ExpiresAt"" timestamp with time zone NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""VerifiedAt"" timestamp with time zone,
    ""IsUsed"" boolean NOT NULL DEFAULT false,
    ""Attempts"" integer NOT NULL DEFAULT 0,
    ""IpAddress"" character varying(50),
    CONSTRAINT ""PK_ClientPortalOtps"" PRIMARY KEY (""OtpId""),
    CONSTRAINT ""FK_ClientPortalOtps_QuotationAccessLinks_AccessLinkId"" FOREIGN KEY (""AccessLinkId"") REFERENCES ""QuotationAccessLinks"" (""AccessLinkId"") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ""IX_ClientPortalOtps_AccessLinkId"" ON ""ClientPortalOtps"" (""AccessLinkId"");
CREATE INDEX IF NOT EXISTS ""IX_ClientPortalOtps_ClientEmail"" ON ""ClientPortalOtps"" (""ClientEmail"");
CREATE INDEX IF NOT EXISTS ""IX_ClientPortalOtps_AccessLinkId_IsUsed_ExpiresAt"" ON ""ClientPortalOtps"" (""AccessLinkId"", ""IsUsed"", ""ExpiresAt"");
";
        await ctx.Database.ExecuteSqlRawAsync(clientPortalOtpSql);
        Console.WriteLine("✓ ClientPortalOtps table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (ClientPortalOtps): {tableEx.Message}");
    }

    // Create QuotationPageViews table
    try
    {
        var quotationPageViewSql = @"
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

CREATE INDEX IF NOT EXISTS ""IX_QuotationPageViews_AccessLinkId"" ON ""QuotationPageViews"" (""AccessLinkId"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationPageViews_QuotationId"" ON ""QuotationPageViews"" (""QuotationId"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationPageViews_ClientEmail"" ON ""QuotationPageViews"" (""ClientEmail"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationPageViews_ViewStartTime"" ON ""QuotationPageViews"" (""ViewStartTime"");
";
        await ctx.Database.ExecuteSqlRawAsync(quotationPageViewSql);
        Console.WriteLine("✓ QuotationPageViews table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (QuotationPageViews): {tableEx.Message}");
    }

    // Create QuotationResponses table
    try
    {
        var quotationResponsesSql = @"
CREATE TABLE IF NOT EXISTS ""QuotationResponses"" (
    ""ResponseId"" uuid NOT NULL,
    ""QuotationId"" uuid NOT NULL,
    ""ResponseType"" character varying(50) NOT NULL,
    ""ClientEmail"" character varying(255) NOT NULL,
    ""ClientName"" character varying(255),
    ""ResponseMessage"" character varying(2000),
    ""ResponseDate"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""IpAddress"" character varying(50),
    ""UserAgent"" character varying(500),
    ""NotifiedAdminAt"" timestamp with time zone,
    CONSTRAINT ""PK_QuotationResponses"" PRIMARY KEY (""ResponseId""),
    CONSTRAINT ""FK_QuotationResponses_Quotations_QuotationId"" FOREIGN KEY (""QuotationId"") REFERENCES ""Quotations"" (""QuotationId"") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_QuotationResponses_QuotationId"" ON ""QuotationResponses"" (""QuotationId"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationResponses_ResponseType"" ON ""QuotationResponses"" (""ResponseType"");
CREATE INDEX IF NOT EXISTS ""IX_QuotationResponses_ClientEmail"" ON ""QuotationResponses"" (""ClientEmail"");
";
        await ctx.Database.ExecuteSqlRawAsync(quotationResponsesSql);
        Console.WriteLine("✓ QuotationResponses table created/verified.");
    }
    catch (Exception tableEx)
    {
        Console.WriteLine($"Note (QuotationResponses): {tableEx.Message}");
    }
    
    Console.WriteLine("Applying migrations to database...");
    await ctx.Database.MigrateAsync();
    Console.WriteLine("Migrations applied successfully.");
}
catch (Exception ex)
{
    Console.Error.WriteLine("Migration failed: " + ex.Message);
    Console.Error.WriteLine("Attempting EnsureCreated as a fallback...");
    try
    {
        using var scope2 = sp.CreateScope();
        var ctx2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var created = await ctx2.Database.EnsureCreatedAsync();
        Console.WriteLine(created ? "Database schema created via EnsureCreated." : "EnsureCreated found existing schema.");
    }
    catch (Exception inner)
    {
        Console.Error.WriteLine("EnsureCreated failed: " + inner.ToString());
        Environment.ExitCode = 1;
    }
}
