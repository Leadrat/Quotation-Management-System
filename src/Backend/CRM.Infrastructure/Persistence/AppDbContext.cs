using Microsoft.EntityFrameworkCore;
using CRM.Domain.Entities;
using CRM.Domain.Admin;
using CRM.Application.Common.Persistence;

namespace CRM.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

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
    public DbSet<QuotationTemplate> QuotationTemplates => Set<QuotationTemplate>();
    public DbSet<QuotationTemplateLineItem> QuotationTemplateLineItems => Set<QuotationTemplateLineItem>();
    public DbSet<DiscountApproval> DiscountApprovals => Set<DiscountApproval>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<EmailNotificationLog> EmailNotificationLogs => Set<EmailNotificationLog>();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
