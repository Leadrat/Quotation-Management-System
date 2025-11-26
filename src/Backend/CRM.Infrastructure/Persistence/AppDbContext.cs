using Microsoft.EntityFrameworkCore;
using CRM.Domain.Entities;
using CRM.Domain.Imports;
using CRM.Domain.Admin;
using CRM.Domain.UserManagement;
using CRM.Application.Common.Persistence;

namespace CRM.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();
    public DbSet<ClientHistory> ClientHistories => Set<ClientHistory>();
    public DbSet<SuspiciousActivityFlag> SuspiciousActivityFlags => Set<SuspiciousActivityFlag>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationLineItem> QuotationLineItems => Set<QuotationLineItem>();
    public DbSet<QuotationAccessLink> QuotationAccessLinks => Set<QuotationAccessLink>();
    public DbSet<QuotationStatusHistory> QuotationStatusHistory => Set<QuotationStatusHistory>();
    public DbSet<QuotationResponse> QuotationResponses => Set<QuotationResponse>();
    public DbSet<ClientPortalOtp> ClientPortalOtps => Set<ClientPortalOtp>();
    public DbSet<QuotationPageView> QuotationPageViews => Set<QuotationPageView>();
    public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
    public DbSet<QuotationTemplate> QuotationTemplates => Set<QuotationTemplate>();
    public DbSet<QuotationTemplateLineItem> QuotationTemplateLineItems => Set<QuotationTemplateLineItem>();
    public DbSet<TemplatePlaceholder> TemplatePlaceholders => Set<TemplatePlaceholder>();
    public DbSet<DiscountApproval> DiscountApprovals => Set<DiscountApproval>();
    public DbSet<UserNotification> Notifications => Set<UserNotification>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<EmailNotificationLog> EmailNotificationLogs => Set<EmailNotificationLog>();
    public DbSet<NotificationDispatchAttempt> NotificationDispatchAttempts => Set<NotificationDispatchAttempt>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationChannelConfiguration> NotificationChannelConfigurations => Set<NotificationChannelConfiguration>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<NotificationOperationLog> NotificationOperationLogs => Set<NotificationOperationLog>();
    public DbSet<NotificationAlert> NotificationAlerts => Set<NotificationAlert>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentGatewayConfig> PaymentGatewayConfigs => Set<PaymentGatewayConfig>();
    public DbSet<AnalyticsMetricsSnapshot> AnalyticsMetricsSnapshots => Set<AnalyticsMetricsSnapshot>();
    public DbSet<DashboardBookmark> DashboardBookmarks => Set<DashboardBookmark>();
    public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();
    public DbSet<ExportedReport> ExportedReports => Set<ExportedReport>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<Adjustment> Adjustments => Set<Adjustment>();
    public DbSet<RefundTimeline> RefundTimeline => Set<RefundTimeline>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<CompanyPreferences> CompanyPreferences => Set<CompanyPreferences>();
    public DbSet<LocalizationResource> LocalizationResources => Set<LocalizationResource>();
    public DbSet<SupportedLanguage> SupportedLanguages => Set<SupportedLanguage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<IntegrationKey> IntegrationKeys => Set<IntegrationKey>();
    public DbSet<CustomBranding> CustomBranding => Set<CustomBranding>();
    public DbSet<DataRetentionPolicy> DataRetentionPolicies => Set<DataRetentionPolicy>();
    public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();

    // Company Details
    public DbSet<CompanyDetails> CompanyDetails => Set<CompanyDetails>();
    public DbSet<BankDetails> BankDetails => Set<BankDetails>();
    
    // Country-Specific Identifiers & Bank Details
    public DbSet<IdentifierType> IdentifierTypes => Set<IdentifierType>();
    public DbSet<CountryIdentifierConfiguration> CountryIdentifierConfigurations => Set<CountryIdentifierConfiguration>();
    public DbSet<BankFieldType> BankFieldTypes => Set<BankFieldType>();
    public DbSet<CountryBankFieldConfiguration> CountryBankFieldConfigurations => Set<CountryBankFieldConfiguration>();

    // User Management
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<UserGroupMember> UserGroupMembers => Set<UserGroupMember>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    public DbSet<Mention> Mentions => Set<Mention>();

    // Tax Management
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Jurisdiction> Jurisdictions => Set<Jurisdiction>();
    public DbSet<TaxFramework> TaxFrameworks => Set<TaxFramework>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<ProductServiceCategory> ProductServiceCategories => Set<ProductServiceCategory>();
    public DbSet<TaxCalculationLog> TaxCalculationLogs => Set<TaxCalculationLog>();

    // Product Catalog
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductPriceHistory> ProductPriceHistory => Set<ProductPriceHistory>();
    
    // Imports
    public DbSet<ImportSession> ImportSessions => Set<ImportSession>();
    public DbSet<ImportedTemplate> ImportedTemplates => Set<ImportedTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
