using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.Entities;
using CRM.Domain.Admin;

namespace CRM.Application.Common.Persistence
{
    public interface IAppDbContext
    {
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
        DbSet<DiscountApproval> DiscountApprovals { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<NotificationPreference> NotificationPreferences { get; }
        DbSet<EmailNotificationLog> EmailNotificationLogs { get; }
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

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
