using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.Entities;
using CRM.Domain.Admin;
using CRM.Domain.UserManagement;
using CRM.Domain.Imports;

namespace CRM.Application.Common.Persistence
{
    public interface IAppDbContext
    {
        DbSet<Tenant> Tenants { get; }
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<UserRole> UserRoles { get; }
        DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<PasswordResetToken> PasswordResetTokens { get; }
        DbSet<Client> Clients { get; }
        DbSet<SavedSearch> SavedSearches { get; }
        DbSet<ClientHistory> ClientHistories { get; }
        DbSet<SuspiciousActivityFlag> SuspiciousActivityFlags { get; }
        DbSet<Quotation> Quotations { get; }
        DbSet<QuotationLineItem> QuotationLineItems { get; }
        DbSet<QuotationAccessLink> QuotationAccessLinks { get; }
        DbSet<QuotationStatusHistory> QuotationStatusHistory { get; }
        DbSet<QuotationResponse> QuotationResponses { get; }
        DbSet<ClientPortalOtp> ClientPortalOtps { get; }
        DbSet<QuotationPageView> QuotationPageViews { get; }
        DbSet<QuotationTemplate> QuotationTemplates { get; }
        DbSet<QuotationTemplateLineItem> QuotationTemplateLineItems { get; }
        DbSet<DocumentTemplate> DocumentTemplates { get; }
        DbSet<TemplatePlaceholder> TemplatePlaceholders { get; }
        DbSet<DiscountApproval> DiscountApprovals { get; }
        DbSet<UserNotification> Notifications { get; }
        DbSet<UserNotification> UserNotifications { get; }
        DbSet<NotificationType> NotificationTypes { get; }
        DbSet<NotificationPreference> NotificationPreferences { get; }
        DbSet<EmailNotificationLog> EmailNotificationLogs { get; }
        DbSet<NotificationDispatchAttempt> NotificationDispatchAttempts { get; }
        DbSet<NotificationTemplate> NotificationTemplates { get; }
        DbSet<NotificationChannelConfiguration> NotificationChannelConfigurations { get; }
        DbSet<NotificationOperationLog> NotificationOperationLogs { get; }
        DbSet<NotificationAlert> NotificationAlerts { get; }
        DbSet<Payment> Payments { get; }
        DbSet<PaymentGatewayConfig> PaymentGatewayConfigs { get; }
        DbSet<AnalyticsMetricsSnapshot> AnalyticsMetricsSnapshots { get; }
        DbSet<DashboardBookmark> DashboardBookmarks { get; }
        DbSet<ScheduledReport> ScheduledReports { get; }
        DbSet<ExportedReport> ExportedReports { get; }
        DbSet<Refund> Refunds { get; }
        DbSet<Adjustment> Adjustments { get; }
        DbSet<RefundTimeline> RefundTimeline { get; }
        DbSet<Currency> Currencies { get; }
        DbSet<ExchangeRate> ExchangeRates { get; }
        DbSet<UserPreferences> UserPreferences { get; }
        DbSet<CompanyPreferences> CompanyPreferences { get; }
        DbSet<LocalizationResource> LocalizationResources { get; }
        DbSet<SupportedLanguage> SupportedLanguages { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<SystemSettings> SystemSettings { get; }
        DbSet<IntegrationKey> IntegrationKeys { get; }
        DbSet<CustomBranding> CustomBranding { get; }
        DbSet<DataRetentionPolicy> DataRetentionPolicies { get; }
        DbSet<NotificationSettings> NotificationSettings { get; }

        // Company Details
        DbSet<Domain.Entities.CompanyDetails> CompanyDetails { get; }
        DbSet<Domain.Entities.BankDetails> BankDetails { get; }
        
        // Country-Specific Company Identifiers (Spec-023)
        DbSet<Domain.Entities.IdentifierType> IdentifierTypes { get; }
        DbSet<Domain.Entities.CountryIdentifierConfiguration> CountryIdentifierConfigurations { get; }
        DbSet<Domain.Entities.BankFieldType> BankFieldTypes { get; }
        DbSet<Domain.Entities.CountryBankFieldConfiguration> CountryBankFieldConfigurations { get; }

        // User Management
        DbSet<Team> Teams { get; }
        DbSet<TeamMember> TeamMembers { get; }
        DbSet<UserGroup> UserGroups { get; }
        DbSet<UserGroupMember> UserGroupMembers { get; }
        DbSet<TaskAssignment> TaskAssignments { get; }
        DbSet<UserActivity> UserActivities { get; }
        DbSet<Mention> Mentions { get; }

        // Tax Management
        DbSet<Country> Countries { get; }
        DbSet<Jurisdiction> Jurisdictions { get; }
        DbSet<TaxFramework> TaxFrameworks { get; }
        DbSet<TaxRate> TaxRates { get; }
        DbSet<ProductServiceCategory> ProductServiceCategories { get; }
        DbSet<TaxCalculationLog> TaxCalculationLogs { get; }

        // Product Catalog
        DbSet<Product> Products { get; }
        DbSet<ProductCategory> ProductCategories { get; }
        DbSet<ProductPriceHistory> ProductPriceHistory { get; }

        // Import Templates (Spec-029)
        DbSet<ImportSession> ImportSessions { get; }
        DbSet<ImportedTemplate> ImportedTemplates { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
