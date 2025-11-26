using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.RateLimiting;
using CRM.Infrastructure.Persistence;
using CRM.Application.Common.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using CRM.Api.Filters;
using CRM.Infrastructure.Logging;
using CRM.Shared.Exceptions;
using CRM.Shared.Config;
using CRM.Application.Auth.Services;
using CRM.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CRM.Application.Common.Security;
using CRM.Infrastructure.Security;
using CRM.Infrastructure.Notifications;
using CRM.Infrastructure.Jobs;
using CRM.Api.Utilities;
using CRM.Infrastructure.Services;
using CRM.Application.Quotations.Services;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using QuestPDF.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
// using Finbuckle.MultiTenant;
// using Finbuckle.MultiTenant.Stores;

DotEnv.Load(".env.local", ".env");

// Configure Npgsql to handle DateTime values without requiring UTC Kind
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

var envConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
var connectionString = !string.IsNullOrWhiteSpace(envConnection)
    ? envConnection
    : builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString) && builder.Environment.IsDevelopment())
{
    connectionString = "Host=localhost;Port=5432;Database=crm;Username=postgres;Password=postgres";
    Console.WriteLine("POSTGRES_CONNECTION not set. Using default local development connection string.");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("POSTGRES_CONNECTION not set. Provide environment variable or ConnectionStrings:Default configuration.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// TODO: Configure Finbuckle MultiTenant once package issues are resolved
// builder.Services.AddMultiTenant<FinbuckleTenantInfo>()
//     .WithStore(ServiceLifetime.Scoped, sp =>
//     {
//         var dbContext = sp.GetRequiredService<AppDbContext>();
//         return new EFCoreStore<FinbuckleTenantInfo>(dbContext);
//     })
//     .WithStrategy<HeaderStrategy>(ServiceLifetime.Scoped, options =>
//     {
//         options.HeaderName = "X-Tenant-Id";
//     });

// Map IAppDbContext to concrete AppDbContext
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// Add HttpContextAccessor and CurrentUserService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CRM.Application.Common.Interfaces.ICurrentUserService, CRM.Infrastructure.Services.CurrentUserService>();

// Add TenantContext for multitenant support
builder.Services.AddScoped<CRM.Application.Common.Interfaces.ITenantContext, CRM.Infrastructure.Services.TenantContext>();

// Configure Hangfire for notification dispatch
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(connectionString);
    }, new Hangfire.PostgreSql.PostgreSqlStorageOptions
    {
        SchemaName = "hangfire"
    });
});
builder.Services.AddHangfireServer();

// Configure notification dispatch services
builder.Services.AddNotificationDispatchServices();

builder.Services.AddSignalR();

// Add MediatR services
builder.Services.AddMediatR(typeof(CRM.Application.Common.Persistence.IAppDbContext).Assembly);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CRM API",
        Version = "v1",
        Description = "Spec-009: Quotation Entity & CRUD Operations endpoints. Includes create, read, update, delete quotations with automatic tax calculation (GST), line items management, and status lifecycle."
    });
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            scheme,
            new string[]{}
        }
    });
});
// Spec-029: Import Templates services
builder.Services.AddScoped<CRM.Infrastructure.Services.FileStorageService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.Imports.ImportStorageService>();
builder.Services.AddScoped<CRM.Application.Imports.Services.ParseService>();
builder.Services.AddScoped<CRM.Application.Imports.Services.MappingService>();
builder.Services.AddHttpClient<CRM.Application.Imports.LLM.GeminiClient>();
builder.Services.AddScoped<AdminOnlyAttribute>();
builder.Services.AddSingleton<IAuditLogger, AuditLogger>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<HistorySettings>(builder.Configuration.GetSection("History"));
builder.Services.Configure<SuspiciousActivitySettings>(builder.Configuration.GetSection("SuspiciousActivity"));
builder.Services.Configure<QuotationSettings>(builder.Configuration.GetSection("Quotations"));
builder.Services.Configure<CompanySettings>(builder.Configuration.GetSection("Company"));
builder.Services.Configure<QuotationManagementSettings>(builder.Configuration.GetSection("QuotationManagement"));
builder.Services.AddScoped<CRM.Application.Quotations.Services.QuotationNumberGenerator>();
builder.Services.AddScoped<CRM.Application.Quotations.Services.QuotationTotalsCalculator>();
builder.Services.AddScoped<CRM.Application.Quotations.Services.TaxCalculationService>(); // Legacy service
builder.Services.AddScoped<CRM.Application.TaxManagement.Services.ITaxCalculationService, CRM.Application.TaxManagement.Services.TaxCalculationService>();
builder.Services.AddScoped<IQuotationPdfGenerationService, CRM.Application.Quotations.Services.QuotationPdfGenerationService>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Services.ITemplateProcessingService, CRM.Application.QuotationTemplates.Services.TemplateProcessingService>();
builder.Services.AddScoped<IQuotationEmailService, CRM.Application.Quotations.Services.QuotationEmailService>();
builder.Services.AddScoped<CRM.Application.Quotations.Services.IClientPortalOtpService, CRM.Application.Quotations.Services.ClientPortalOtpService>();
builder.Services.AddScoped<IQuotationSendWorkflow, CRM.Application.Quotations.Services.QuotationSendWorkflow>();
builder.Services.AddScoped<CRM.Application.Quotations.Services.QuotationReminderService>();

// Company Details Services
builder.Services.AddScoped<CRM.Application.CompanyDetails.Services.ICompanyDetailsService, CRM.Application.CompanyDetails.Services.CompanyDetailsService>();
builder.Services.AddScoped<CRM.Application.CompanyDetails.Queries.Handlers.GetCompanyDetailsQueryHandler>();
builder.Services.AddScoped<CRM.Application.CompanyDetails.Commands.Handlers.UpdateCompanyDetailsCommandHandler>();
// Company Identifiers (Spec-023)
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Queries.Handlers.GetIdentifierTypesQueryHandler>();
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Queries.Handlers.GetCountryIdentifierConfigurationsQueryHandler>();
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Commands.Handlers.CreateIdentifierTypeCommandHandler>();
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Commands.Handlers.UpdateIdentifierTypeCommandHandler>();
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Commands.Handlers.ConfigureCountryIdentifierCommandHandler>();
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Commands.Handlers.UpdateCountryIdentifierConfigurationCommandHandler>();
// Company Identifiers Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyIdentifiers.DTOs.CreateIdentifierTypeRequest>, CRM.Application.CompanyIdentifiers.Validators.CreateIdentifierTypeRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyIdentifiers.DTOs.UpdateIdentifierTypeRequest>, CRM.Application.CompanyIdentifiers.Validators.UpdateIdentifierTypeRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyIdentifiers.DTOs.ConfigureCountryIdentifierRequest>, CRM.Application.CompanyIdentifiers.Validators.ConfigureCountryIdentifierRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyIdentifiers.DTOs.UpdateCountryIdentifierConfigurationRequest>, CRM.Application.CompanyIdentifiers.Validators.UpdateCountryIdentifierConfigurationRequestValidator>();
// Company Bank Details (Spec-023)
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Queries.Handlers.GetBankFieldTypesQueryHandler>();
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Queries.Handlers.GetCountryBankFieldConfigurationsQueryHandler>();
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Commands.Handlers.CreateBankFieldTypeCommandHandler>();
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Commands.Handlers.UpdateBankFieldTypeCommandHandler>();
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Commands.Handlers.ConfigureCountryBankFieldCommandHandler>();
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Commands.Handlers.UpdateCountryBankFieldConfigurationCommandHandler>();
// Company Bank Details Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyBankDetails.DTOs.CreateBankFieldTypeRequest>, CRM.Application.CompanyBankDetails.Validators.CreateBankFieldTypeRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyBankDetails.DTOs.UpdateBankFieldTypeRequest>, CRM.Application.CompanyBankDetails.Validators.UpdateBankFieldTypeRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyBankDetails.DTOs.ConfigureCountryBankFieldRequest>, CRM.Application.CompanyBankDetails.Validators.ConfigureCountryBankFieldRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyBankDetails.DTOs.UpdateCountryBankFieldConfigurationRequest>, CRM.Application.CompanyBankDetails.Validators.UpdateCountryBankFieldConfigurationRequestValidator>();
// Company Identifiers - Value Management (Spec-023)
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Services.ICompanyIdentifierValidationService, CRM.Application.CompanyIdentifiers.Services.CompanyIdentifierValidationService>();
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Queries.Handlers.GetCompanyIdentifierValuesQueryHandler>();
builder.Services.AddScoped<CRM.Application.CompanyIdentifiers.Commands.Handlers.SaveCompanyIdentifierValuesCommandHandler>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyIdentifiers.DTOs.SaveCompanyIdentifierValuesRequest>, CRM.Application.CompanyIdentifiers.Validators.SaveCompanyIdentifierValuesRequestValidator>();
// Company Bank Details - Value Management (Spec-023)
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Services.ICompanyBankDetailsValidationService, CRM.Application.CompanyBankDetails.Services.CompanyBankDetailsValidationService>();
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Queries.Handlers.GetCompanyBankDetailsQueryHandler>();
builder.Services.AddScoped<CRM.Application.CompanyBankDetails.Commands.Handlers.SaveCompanyBankDetailsCommandHandler>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.CompanyBankDetails.DTOs.SaveCompanyBankDetailsRequest>, CRM.Application.CompanyBankDetails.Validators.SaveCompanyBankDetailsRequestValidator>();
// Quotation Company Details Service (Spec-023)
builder.Services.AddScoped<CRM.Application.Quotations.Services.QuotationCompanyDetailsService>();
builder.Services.AddScoped<CRM.Application.Quotations.Commands.Handlers.CreateQuotationCommandHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Commands.Handlers.UpdateQuotationCommandHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Commands.Handlers.DeleteQuotationCommandHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Commands.Handlers.SendQuotationCommandHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Commands.Handlers.ResendQuotationCommandHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Commands.Handlers.MarkQuotationAsViewedCommandHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Commands.Handlers.SubmitQuotationResponseCommandHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Commands.Handlers.MarkQuotationAsExpiredCommandHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Queries.Handlers.GetQuotationByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Queries.Handlers.GetAllQuotationsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Queries.Handlers.GetQuotationsByClientQueryHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Queries.Handlers.GetQuotationStatusHistoryQueryHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Queries.Handlers.GetQuotationResponseQueryHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Queries.Handlers.GetQuotationAccessLinkQueryHandler>();
builder.Services.AddScoped<CRM.Application.Quotations.Queries.Handlers.GetQuotationByAccessTokenQueryHandler>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Dtos.CreateQuotationRequest>, CRM.Application.Quotations.Validators.CreateQuotationRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Dtos.UpdateQuotationRequest>, CRM.Application.Quotations.Validators.UpdateQuotationRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Dtos.SendQuotationRequest>, CRM.Application.Quotations.Validators.SendQuotationRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Commands.MarkQuotationAsViewedCommand>, CRM.Application.Quotations.Validators.MarkQuotationAsViewedCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Dtos.SubmitQuotationResponseRequest>, CRM.Application.Quotations.Validators.SubmitQuotationResponseRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Commands.SubmitQuotationResponseCommand>, CRM.Application.Quotations.Validators.SubmitQuotationResponseCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Commands.MarkQuotationAsExpiredCommand>, CRM.Application.Quotations.Validators.MarkQuotationAsExpiredCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Commands.ResendQuotationCommand>, CRM.Application.Quotations.Validators.ResendQuotationCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Queries.GetQuotationStatusHistoryQuery>, CRM.Application.Quotations.Validators.GetQuotationStatusHistoryQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Queries.GetQuotationResponseQuery>, CRM.Application.Quotations.Validators.GetQuotationResponseQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Queries.GetQuotationAccessLinkQuery>, CRM.Application.Quotations.Validators.GetQuotationAccessLinkQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Quotations.Queries.GetQuotationByAccessTokenQuery>, CRM.Application.Quotations.Validators.GetQuotationByAccessTokenQueryValidator>();
// Quotation Template Handlers
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Commands.Handlers.CreateQuotationTemplateCommandHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Commands.Handlers.UploadQuotationTemplateCommandHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Commands.Handlers.UpdateQuotationTemplateCommandHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Commands.Handlers.DeleteQuotationTemplateCommandHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Commands.Handlers.RestoreQuotationTemplateCommandHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Commands.Handlers.ApproveQuotationTemplateCommandHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Commands.Handlers.ApplyTemplateToQuotationCommandHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Queries.Handlers.GetTemplateByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Queries.Handlers.GetAllTemplatesQueryHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Queries.Handlers.GetTemplateVersionsQueryHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Queries.Handlers.GetPublicTemplatesQueryHandler>();
builder.Services.AddScoped<CRM.Application.QuotationTemplates.Queries.Handlers.GetTemplateUsageStatsQueryHandler>();
// Quotation Template Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.QuotationTemplates.Dtos.CreateQuotationTemplateRequest>, CRM.Application.QuotationTemplates.Validators.CreateQuotationTemplateRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.QuotationTemplates.Dtos.UpdateQuotationTemplateRequest>, CRM.Application.QuotationTemplates.Validators.UpdateQuotationTemplateRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.QuotationTemplates.Commands.ApproveQuotationTemplateCommand>, CRM.Application.QuotationTemplates.Validators.ApproveQuotationTemplateCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.QuotationTemplates.Commands.ApplyTemplateToQuotationCommand>, CRM.Application.QuotationTemplates.Validators.ApplyTemplateToQuotationCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.QuotationTemplates.Queries.GetTemplateByIdQuery>, CRM.Application.QuotationTemplates.Validators.GetTemplateByIdQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.QuotationTemplates.Queries.GetAllTemplatesQuery>, CRM.Application.QuotationTemplates.Validators.GetAllTemplatesQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.QuotationTemplates.Queries.GetTemplateVersionsQuery>, CRM.Application.QuotationTemplates.Validators.GetTemplateVersionsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.QuotationTemplates.Queries.GetTemplateUsageStatsQuery>, CRM.Application.QuotationTemplates.Validators.GetTemplateUsageStatsQueryValidator>();
// Discount Approval Handlers
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Commands.Handlers.RequestDiscountApprovalCommandHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Commands.Handlers.ApproveDiscountApprovalCommandHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Commands.Handlers.RejectDiscountApprovalCommandHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Commands.Handlers.EscalateDiscountApprovalCommandHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Commands.Handlers.ResubmitDiscountApprovalCommandHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Commands.Handlers.BulkApproveDiscountApprovalsCommandHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Queries.Handlers.GetPendingApprovalsQueryHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Queries.Handlers.GetApprovalByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Queries.Handlers.GetApprovalTimelineQueryHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Queries.Handlers.GetQuotationApprovalsQueryHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.Queries.Handlers.GetApprovalMetricsQueryHandler>();
// Discount Approval Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Commands.RequestDiscountApprovalCommand>, CRM.Application.DiscountApprovals.Validators.RequestDiscountApprovalCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Commands.ApproveDiscountApprovalCommand>, CRM.Application.DiscountApprovals.Validators.ApproveDiscountApprovalCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Commands.RejectDiscountApprovalCommand>, CRM.Application.DiscountApprovals.Validators.RejectDiscountApprovalCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Commands.EscalateDiscountApprovalCommand>, CRM.Application.DiscountApprovals.Validators.EscalateDiscountApprovalCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Commands.ResubmitDiscountApprovalCommand>, CRM.Application.DiscountApprovals.Validators.ResubmitDiscountApprovalCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Commands.BulkApproveDiscountApprovalsCommand>, CRM.Application.DiscountApprovals.Validators.BulkApproveDiscountApprovalsCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Queries.GetPendingApprovalsQuery>, CRM.Application.DiscountApprovals.Validators.GetPendingApprovalsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Queries.GetApprovalByIdQuery>, CRM.Application.DiscountApprovals.Validators.GetApprovalByIdQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Queries.GetApprovalTimelineQuery>, CRM.Application.DiscountApprovals.Validators.GetApprovalTimelineQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Queries.GetQuotationApprovalsQuery>, CRM.Application.DiscountApprovals.Validators.GetQuotationApprovalsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DiscountApprovals.Queries.GetApprovalMetricsQuery>, CRM.Application.DiscountApprovals.Validators.GetApprovalMetricsQueryValidator>();
// Discount Approval Event Handlers
builder.Services.AddScoped<CRM.Application.DiscountApprovals.EventHandlers.DiscountApprovalRequestedEventHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.EventHandlers.DiscountApprovalApprovedEventHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.EventHandlers.DiscountApprovalRejectedEventHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.EventHandlers.DiscountApprovalEscalatedEventHandler>();
builder.Services.AddScoped<CRM.Application.DiscountApprovals.EventHandlers.DiscountApprovalResubmittedEventHandler>();
// Notification Services
builder.Services.AddScoped<CRM.Application.Notifications.Services.IEmailNotificationService, CRM.Application.Notifications.Services.EmailNotificationService>();
builder.Services.AddScoped<CRM.Application.Notifications.Services.INotificationTemplateService, CRM.Infrastructure.Services.NotificationTemplateService>();
builder.Services.AddScoped<CRM.Application.Notifications.Services.INotificationTemplateValidationService, CRM.Infrastructure.Services.NotificationTemplateValidationService>();
builder.Services.AddScoped<CRM.Application.Notifications.Repositories.INotificationTemplateRepository, CRM.Infrastructure.Repositories.NotificationTemplateRepository>();
builder.Services.AddScoped<CRM.Application.Notifications.Services.INotificationEventTemplateService, CRM.Application.Notifications.Services.NotificationEventTemplateService>();
builder.Services.AddScoped<CRM.Application.Notifications.Services.INotificationCreationService, CRM.Application.Notifications.Services.NotificationCreationService>();
builder.Services.AddScoped<CRM.Application.Notifications.Services.INotificationDispatchService, CRM.Infrastructure.Services.NotificationDispatchService>();

// Channel-specific dispatch services
builder.Services.AddScoped<CRM.Infrastructure.Services.InAppNotificationDispatchService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.EmailNotificationDispatchService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.SmsNotificationDispatchService>();

// Real-time and communication services
builder.Services.AddScoped<CRM.Application.Common.Interfaces.IRealTimeNotificationService, CRM.Infrastructure.Services.RealTimeNotificationService>();
builder.Services.AddScoped<CRM.Application.Common.Interfaces.ISmsService, CRM.Infrastructure.Services.SmsService>();

// Retry and background services
builder.Services.AddScoped<CRM.Application.Notifications.Services.IRetryPolicyService, CRM.Infrastructure.Services.RetryPolicyService>();
builder.Services.AddHostedService<CRM.Infrastructure.Services.NotificationRetryBackgroundService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.NotificationRetryJob>();
builder.Services.AddScoped<CRM.Infrastructure.Services.RetryMonitoringJob>();

// Queue and monitoring services
builder.Services.AddScoped<CRM.Infrastructure.Services.INotificationQueueService, CRM.Infrastructure.Services.NotificationQueueService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.INotificationSynchronizationService, CRM.Infrastructure.Services.NotificationSynchronizationService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.INotificationChannelConfigurationService, CRM.Infrastructure.Services.NotificationChannelConfigurationService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.INotificationMonitoringService, CRM.Infrastructure.Services.NotificationMonitoringService>();
builder.Services.AddScoped<CRM.Application.Notifications.Services.INotificationService>(sp =>
{
    var db = sp.GetRequiredService<CRM.Application.Common.Persistence.IAppDbContext>();
    var emailService = sp.GetRequiredService<CRM.Application.Notifications.Services.IEmailNotificationService>();
    var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CRM.Application.Notifications.Services.NotificationService>>();
    return new CRM.Application.Notifications.Services.NotificationService(db, emailService, null, logger);
});
// Notification handlers are automatically registered by MediatR assembly scanning
// Notification Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Commands.MarkNotificationsReadCommand>, CRM.Application.Notifications.Validators.MarkNotificationsReadCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Commands.ArchiveNotificationsCommand>, CRM.Application.Notifications.Validators.ArchiveNotificationsCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Commands.UnarchiveNotificationsCommand>, CRM.Application.Notifications.Validators.UnarchiveNotificationsCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Commands.UpdateNotificationPreferencesCommand>, CRM.Application.Notifications.Validators.UpdateNotificationPreferencesCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Queries.GetNotificationsQuery>, CRM.Application.Notifications.Validators.GetNotificationsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Queries.GetUnreadCountQuery>, CRM.Application.Notifications.Validators.GetUnreadCountQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Queries.GetNotificationPreferencesQuery>, CRM.Application.Notifications.Validators.GetNotificationPreferencesQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Queries.GetEntityNotificationsQuery>, CRM.Application.Notifications.Validators.GetEntityNotificationsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Notifications.Queries.GetEmailNotificationLogsQuery>, CRM.Application.Notifications.Validators.GetEmailNotificationLogsQueryValidator>();

// Payment Services
builder.Services.AddScoped<CRM.Application.Payments.Services.IPaymentGatewayService, CRM.Application.Payments.Services.StripePaymentGatewayService>();
builder.Services.AddScoped<CRM.Application.Payments.Services.IPaymentGatewayService, CRM.Application.Payments.Services.RazorpayPaymentGatewayService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.IPaymentGatewayEncryptionService, CRM.Infrastructure.Services.PaymentGatewayEncryptionService>();
builder.Services.AddScoped<CRM.Application.Payments.Services.IPaymentGatewayFactory, CRM.Infrastructure.Services.PaymentGatewayFactory>();
// Payment Command Handlers
builder.Services.AddScoped<CRM.Application.Payments.Commands.Handlers.InitiatePaymentCommandHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Commands.Handlers.UpdatePaymentStatusCommandHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Commands.Handlers.RefundPaymentCommandHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Commands.Handlers.CancelPaymentCommandHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Commands.Handlers.CreatePaymentGatewayConfigCommandHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Commands.Handlers.UpdatePaymentGatewayConfigCommandHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Commands.Handlers.DeletePaymentGatewayConfigCommandHandler>();
// Payment Query Handlers
builder.Services.AddScoped<CRM.Application.Payments.Queries.Handlers.GetPaymentByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Queries.Handlers.GetPaymentByQuotationQueryHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Queries.Handlers.GetPaymentsByUserQueryHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Queries.Handlers.GetPaymentsDashboardQueryHandler>();
builder.Services.AddScoped<CRM.Application.Payments.Queries.Handlers.GetPaymentGatewayConfigQueryHandler>();
// Payment Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Dtos.InitiatePaymentRequest>, CRM.Application.Payments.Validators.InitiatePaymentRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Dtos.UpdatePaymentStatusRequest>, CRM.Application.Payments.Validators.UpdatePaymentStatusRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Dtos.RefundPaymentRequest>, CRM.Application.Payments.Validators.RefundPaymentRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Dtos.CreatePaymentGatewayConfigRequest>, CRM.Application.Payments.Validators.CreatePaymentGatewayConfigRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Dtos.UpdatePaymentGatewayConfigRequest>, CRM.Application.Payments.Validators.UpdatePaymentGatewayConfigRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Queries.GetPaymentByIdQuery>, CRM.Application.Payments.Validators.GetPaymentByIdQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Queries.GetPaymentByQuotationQuery>, CRM.Application.Payments.Validators.GetPaymentByQuotationQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Queries.GetPaymentsByUserQuery>, CRM.Application.Payments.Validators.GetPaymentsByUserQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Queries.GetPaymentsDashboardQuery>, CRM.Application.Payments.Validators.GetPaymentsDashboardQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Payments.Queries.GetPaymentGatewayConfigQuery>, CRM.Application.Payments.Validators.GetPaymentGatewayConfigQueryValidator>();
// Payment Event Handlers
builder.Services.AddScoped<CRM.Application.Payments.EventHandlers.PaymentSuccessEventHandler>();
builder.Services.AddScoped<CRM.Application.Payments.EventHandlers.PaymentFailedEventHandler>();
builder.Services.AddScoped<CRM.Application.Payments.EventHandlers.PaymentRefundedEventHandler>();

// Spec-028: Manual Payments aggregation/domain services
builder.Services.AddScoped<CRM.Application.Payments.Services.PaymentAggregationService>();
builder.Services.AddScoped<CRM.Application.Payments.Services.PaymentDomainService>();

// Refund Command Handlers
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.InitiateRefundCommandHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.ApproveRefundCommandHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.RejectRefundCommandHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.ProcessRefundCommandHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.ReverseRefundCommandHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.BulkProcessRefundsCommandHandler>();
// Refund Query Handlers
builder.Services.AddScoped<CRM.Application.Refunds.Queries.Handlers.GetRefundByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Queries.Handlers.GetRefundsByPaymentQueryHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Queries.Handlers.GetRefundsByQuotationQueryHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Queries.Handlers.GetPendingRefundsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Queries.Handlers.GetRefundTimelineQueryHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Queries.Handlers.GetRefundMetricsQueryHandler>();
// Refund Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Refunds.Dtos.CreateRefundRequest>, CRM.Application.Refunds.Validators.CreateRefundRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Refunds.Dtos.ApproveRefundRequest>, CRM.Application.Refunds.Validators.ApproveRefundRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Refunds.Dtos.RejectRefundRequest>, CRM.Application.Refunds.Validators.RejectRefundRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Refunds.Dtos.ReverseRefundRequest>, CRM.Application.Refunds.Validators.ReverseRefundRequestValidator>();
// Refund Event Handlers
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.RefundRequestedEventHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.RefundApprovedEventHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.RefundRejectedEventHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.RefundCompletedEventHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.RefundFailedEventHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.RefundReversedEventHandler>();

// Adjustment Command Handlers
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.InitiateAdjustmentCommandHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.ApproveAdjustmentCommandHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.RejectAdjustmentCommandHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Commands.Handlers.ApplyAdjustmentCommandHandler>();
// Adjustment Query Handlers
builder.Services.AddScoped<CRM.Application.Refunds.Queries.Handlers.GetAdjustmentsByQuotationQueryHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.Queries.Handlers.GetPendingAdjustmentsQueryHandler>();
// Adjustment Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Refunds.Dtos.CreateAdjustmentRequest>, CRM.Application.Refunds.Validators.CreateAdjustmentRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Refunds.Dtos.ApproveAdjustmentRequest>, CRM.Application.Refunds.Validators.ApproveAdjustmentRequestValidator>();
// Adjustment Event Handlers
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.AdjustmentRequestedEventHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.AdjustmentApprovedEventHandler>();
builder.Services.AddScoped<CRM.Application.Refunds.EventHandlers.AdjustmentAppliedEventHandler>();

// Report Services
builder.Services.AddScoped<CRM.Application.Reports.Services.IReportExportService, CRM.Application.Reports.Services.PdfExportService>();
builder.Services.AddScoped<CRM.Application.Reports.Services.IReportExportService, CRM.Application.Reports.Services.ExcelExportService>();
builder.Services.AddScoped<CRM.Application.Reports.Services.IReportExportService, CRM.Application.Reports.Services.CsvExportService>();
builder.Services.AddScoped<CRM.Application.Reports.Services.IFileStorageService, CRM.Infrastructure.Services.FileStorageService>();
// Report Command Handlers
builder.Services.AddScoped<CRM.Application.Reports.Commands.Handlers.GenerateReportCommandHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Commands.Handlers.ExportReportCommandHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Commands.Handlers.ScheduleReportCommandHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Commands.Handlers.CancelScheduledReportCommandHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Commands.Handlers.SaveDashboardBookmarkCommandHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Commands.Handlers.DeleteDashboardBookmarkCommandHandler>();
// Report Query Handlers
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GetSalesDashboardMetricsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GetTeamPerformanceMetricsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GetApprovalWorkflowMetricsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GetDiscountAnalyticsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GetPaymentAnalyticsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GetClientEngagementMetricsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GenerateCustomReportQueryHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GetForecastingDataQueryHandler>();
builder.Services.AddScoped<CRM.Application.Reports.Queries.Handlers.GetAuditComplianceReportQueryHandler>();
// Report Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Queries.GetSalesDashboardMetricsQuery>, CRM.Application.Reports.Validators.GetSalesDashboardMetricsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Queries.GetTeamPerformanceMetricsQuery>, CRM.Application.Reports.Validators.GetTeamPerformanceMetricsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Queries.GetApprovalWorkflowMetricsQuery>, CRM.Application.Reports.Validators.GetApprovalWorkflowMetricsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Queries.GetDiscountAnalyticsQuery>, CRM.Application.Reports.Validators.GetDiscountAnalyticsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Queries.GetPaymentAnalyticsQuery>, CRM.Application.Reports.Validators.GetPaymentAnalyticsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Queries.GetClientEngagementMetricsQuery>, CRM.Application.Reports.Validators.GetClientEngagementMetricsQueryValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Commands.GenerateReportCommand>, CRM.Application.Reports.Validators.GenerateReportCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Commands.ExportReportCommand>, CRM.Application.Reports.Validators.ExportReportCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Commands.ScheduleReportCommand>, CRM.Application.Reports.Validators.ScheduleReportCommandValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Reports.Commands.SaveDashboardBookmarkCommand>, CRM.Application.Reports.Validators.SaveDashboardBookmarkCommandValidator>();

// Localization Services
builder.Services.AddScoped<CRM.Application.Localization.Services.ICurrencyService, CRM.Application.Localization.Services.CurrencyService>();
builder.Services.AddScoped<CRM.Application.Localization.Services.IExchangeRateService, CRM.Application.Localization.Services.ExchangeRateService>();
builder.Services.AddScoped<CRM.Application.Localization.Services.IExchangeRateUpdaterService, CRM.Application.Localization.Services.ExchangeRateUpdaterService>();
builder.Services.AddScoped<CRM.Application.Localization.Services.ILocalizationService, CRM.Application.Localization.Services.LocalizationService>();
builder.Services.AddScoped<CRM.Application.Localization.Services.ILocalizationResourceManager, CRM.Application.Localization.Services.LocalizationResourceManager>();
builder.Services.AddScoped<CRM.Application.Localization.Services.IUserPreferenceService, CRM.Application.Localization.Services.UserPreferenceService>();
builder.Services.AddScoped<CRM.Application.Localization.Services.ICompanyPreferenceService, CRM.Application.Localization.Services.CompanyPreferenceService>();
builder.Services.AddScoped<CRM.Application.Localization.Services.ILocaleFormatter, CRM.Application.Localization.Services.LocaleFormatter>();
// Localization Command Handlers
builder.Services.AddScoped<CRM.Application.Localization.Commands.Handlers.UpdateUserPreferencesCommandHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Commands.Handlers.UpdateCompanyPreferencesCommandHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Commands.Handlers.CreateLocalizationResourceCommandHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Commands.Handlers.UpdateLocalizationResourceCommandHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Commands.Handlers.DeleteLocalizationResourceCommandHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Commands.Handlers.CreateCurrencyCommandHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Commands.Handlers.UpdateExchangeRateCommandHandler>();
// Localization Query Handlers
builder.Services.AddScoped<CRM.Application.Localization.Queries.Handlers.GetSupportedCurrenciesQueryHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Queries.Handlers.GetSupportedLanguagesQueryHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Queries.Handlers.GetUserPreferencesQueryHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Queries.Handlers.GetCompanyPreferencesQueryHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Queries.Handlers.GetLocalizationResourcesQueryHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Queries.Handlers.GetExchangeRatesQueryHandler>();
builder.Services.AddScoped<CRM.Application.Localization.Queries.Handlers.ConvertCurrencyQueryHandler>();
// Localization Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Localization.Dtos.CreateCurrencyRequest>, CRM.Application.Localization.Validators.CreateCurrencyRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Localization.Dtos.UpdateUserPreferencesRequest>, CRM.Application.Localization.Validators.UpdateUserPreferencesRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Localization.Dtos.UpdateExchangeRateRequest>, CRM.Application.Localization.Validators.UpdateExchangeRateRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.Localization.Dtos.CreateLocalizationResourceRequest>, CRM.Application.Localization.Validators.CreateLocalizationResourceRequestValidator>();

// User Management Command Handlers
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.CreateTeamCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.UpdateTeamCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.DeleteTeamCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.AddTeamMemberCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.RemoveTeamMemberCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.CreateUserGroupCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.UpdateUserGroupCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.AddUserGroupMemberCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.RemoveUserGroupMemberCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.AssignTaskCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.UpdateTaskStatusCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.DeleteTaskAssignmentCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.CreateMentionCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.MarkMentionReadCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.UpdateUserProfileCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.SetOutOfOfficeCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.UpdatePresenceCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.BulkInviteUsersCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.BulkUpdateUsersCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.BulkDeactivateUsersCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.CreateCustomRoleCommandHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Commands.Handlers.UpdateRolePermissionsCommandHandler>();
// User Management Query Handlers
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetTeamsQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetTeamByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetTeamMembersQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetUserGroupsQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetUserGroupByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetUserTasksQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetActivityFeedQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetUserActivityQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetMentionsQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetUnreadMentionsCountQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetUserProfileQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.ExportUsersQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetAvailablePermissionsQueryHandler>();
builder.Services.AddScoped<CRM.Application.UserManagement.Queries.Handlers.GetCustomRolesQueryHandler>();
// User Management Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.CreateTeamRequest>, CRM.Application.UserManagement.Validators.CreateTeamRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.UpdateTeamRequest>, CRM.Application.UserManagement.Validators.UpdateTeamRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.AddTeamMemberRequest>, CRM.Application.UserManagement.Validators.AddTeamMemberRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.CreateUserGroupRequest>, CRM.Application.UserManagement.Validators.CreateUserGroupRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.AssignTaskRequest>, CRM.Application.UserManagement.Validators.AssignTaskRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.UpdateTaskStatusRequest>, CRM.Application.UserManagement.Validators.UpdateTaskStatusRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.CreateMentionRequest>, CRM.Application.UserManagement.Validators.CreateMentionRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.UpdateUserProfileRequest>, CRM.Application.UserManagement.Validators.UpdateUserProfileRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.SetOutOfOfficeRequest>, CRM.Application.UserManagement.Validators.SetOutOfOfficeRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.BulkInviteUsersRequest>, CRM.Application.UserManagement.Validators.BulkInviteUsersRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.BulkUpdateUsersRequest>, CRM.Application.UserManagement.Validators.BulkUpdateUsersRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.UserManagement.Requests.CreateCustomRoleRequest>, CRM.Application.UserManagement.Validators.CreateCustomRoleRequestValidator>();
// User Management Services
builder.Services.AddScoped<CRM.Application.UserManagement.Services.IActivityService, CRM.Application.UserManagement.Services.ActivityService>();
builder.Services.AddScoped<CRM.Application.UserManagement.Services.IPresenceService, CRM.Application.UserManagement.Services.PresenceService>();

// Tax Management Command Handlers
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.CreateCountryCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.UpdateCountryCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.DeleteCountryCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.CreateTaxFrameworkCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.UpdateTaxFrameworkCommandHandler>();

// Tax Management Query Handlers
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetAllCountriesQueryHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetCountryByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetAllTaxFrameworksQueryHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetTaxFrameworkByIdQueryHandler>();

// Tax Management - Jurisdiction Handlers
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.CreateJurisdictionCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.UpdateJurisdictionCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.DeleteJurisdictionCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetJurisdictionsByCountryQueryHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetJurisdictionByIdQueryHandler>();

// Tax Management - ProductServiceCategory Handlers
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.CreateProductServiceCategoryCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.UpdateProductServiceCategoryCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetAllProductServiceCategoriesQueryHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetProductServiceCategoryByIdQueryHandler>();

// Tax Management - TaxRate Handlers
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.CreateTaxRateCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.UpdateTaxRateCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Commands.Handlers.DeleteTaxRateCommandHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetAllTaxRatesQueryHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetTaxRatesByJurisdictionQueryHandler>();

// Tax Management - Tax Calculation Handlers
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.PreviewTaxCalculationQueryHandler>();
builder.Services.AddScoped<CRM.Application.TaxManagement.Queries.Handlers.GetTaxCalculationLogQueryHandler>();

// Tax Management Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.CreateCountryRequest>, CRM.Application.TaxManagement.Validators.CreateCountryRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.UpdateCountryRequest>, CRM.Application.TaxManagement.Validators.UpdateCountryRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.CreateTaxFrameworkRequest>, CRM.Application.TaxManagement.Validators.CreateTaxFrameworkRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.UpdateTaxFrameworkRequest>, CRM.Application.TaxManagement.Validators.UpdateTaxFrameworkRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.CreateJurisdictionRequest>, CRM.Application.TaxManagement.Validators.CreateJurisdictionRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.UpdateJurisdictionRequest>, CRM.Application.TaxManagement.Validators.UpdateJurisdictionRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.CreateProductServiceCategoryRequest>, CRM.Application.TaxManagement.Validators.CreateProductServiceCategoryRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.UpdateProductServiceCategoryRequest>, CRM.Application.TaxManagement.Validators.UpdateProductServiceCategoryRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.CreateTaxRateRequest>, CRM.Application.TaxManagement.Validators.CreateTaxRateRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.UpdateTaxRateRequest>, CRM.Application.TaxManagement.Validators.UpdateTaxRateRequestValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.TaxManagement.Requests.PreviewTaxCalculationRequest>, CRM.Application.TaxManagement.Validators.PreviewTaxCalculationRequestValidator>();

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddAutoMapper(typeof(CRM.Application.Mapping.RoleProfile).Assembly, typeof(CRM.Application.Mapping.LocalizationProfile).Assembly, typeof(CRM.Application.Admin.Mapping.AdminProfile).Assembly, typeof(CRM.Application.Mapping.UserManagementProfile).Assembly, typeof(CRM.Application.TaxManagement.Mapping.TaxManagementProfile).Assembly, typeof(CRM.Application.Mapping.ProductProfile).Assembly, typeof(CRM.Application.Mapping.CompanyIdentifiersProfile).Assembly, typeof(CRM.Application.Mapping.CompanyBankDetailsProfile).Assembly, typeof(CRM.Application.Notifications.Mapping.NotificationTemplateProfile).Assembly);
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<CRM.Application.Products.Services.IProductPricingService, CRM.Application.Products.Services.ProductPricingService>();
builder.Services.AddMemoryCache();

var emailSection = builder.Configuration.GetSection("Email");
var emailFrom = emailSection.GetValue<string>("From") ?? "no-reply@crm.com";
var emailFromName = emailSection.GetValue<string>("FromName") ?? "CRM Notifications";
var emailProvider = emailSection.GetValue<string>("Provider") ?? "InMemory";

// Configure Resend if provider is Resend
if (emailProvider.Equals("Resend", StringComparison.OrdinalIgnoreCase))
{
    var resendApiKey = emailSection.GetSection("Resend").GetValue<string>("ApiKey");
    if (string.IsNullOrWhiteSpace(resendApiKey))
    {
        throw new InvalidOperationException("Resend API key is required when Provider is set to 'Resend'");
    }
    
    // Add HttpClient for Resend API calls
    builder.Services.AddHttpClient();
}

// Keep FluentEmail for backward compatibility and email template rendering
var fluentEmailBuilder = builder.Services
    .AddFluentEmail(emailFrom, emailFromName)
    .AddRazorRenderer();

if (emailProvider.Equals("Smtp", StringComparison.OrdinalIgnoreCase))
{
    var smtpHost = emailSection.GetValue<string>("SmtpHost") ?? "localhost";
    var smtpPort = emailSection.GetValue<int?>("SmtpPort") ?? 25;
    var enableSsl = emailSection.GetValue<bool?>("EnableSsl") ?? true;
    var smtpUsername = emailSection.GetValue<string>("SmtpUsername");
    var smtpPassword = emailSection.GetValue<string>("SmtpPassword");

    fluentEmailBuilder.AddSmtpSender(() =>
    {
        var client = new SmtpClient(smtpHost)
        {
            Port = smtpPort,
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(smtpUsername))
        {
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
        }

        return client;
    });
}
else if (!emailProvider.Equals("Resend", StringComparison.OrdinalIgnoreCase))
{
    // For InMemory or other providers, use a default SMTP sender that logs instead of sending
    // This prevents NullReferenceException when trying to send emails
    fluentEmailBuilder.AddSmtpSender(() =>
    {
        // Use a dummy SMTP client that won't actually send but won't throw null reference
        var client = new SmtpClient("localhost")
        {
            Port = 25,
            EnableSsl = false
        };
        // Note: This will fail to send but won't cause NullReferenceException
        // In production, configure proper SMTP settings
        return client;
    });
}

// Email queue for notifications
builder.Services.AddEmailQueue();
builder.Services.AddScoped<IRefreshTokenRevoker, RefreshTokenRevoker>();
builder.Services.AddScoped<IResetTokenGenerator, ResetTokenGenerator>();

// Admin Configuration Services (Spec-018)
builder.Services.AddScoped<CRM.Infrastructure.Admin.Encryption.IDataEncryptionService, CRM.Infrastructure.Admin.Encryption.AesDataEncryptionService>();
builder.Services.AddScoped<CRM.Application.Common.Services.IDataEncryptionService, CRM.Api.Adapters.DataEncryptionServiceAdapter>();
builder.Services.AddScoped<CRM.Infrastructure.Admin.FileStorage.IFileStorageService, CRM.Infrastructure.Admin.FileStorage.LocalFileStorageService>();
// Register Application layer file storage service (adapter pattern)
builder.Services.AddScoped<CRM.Application.Common.Services.IFileStorageService, CRM.Api.Adapters.FileStorageServiceAdapter>();
builder.Services.AddScoped<CRM.Infrastructure.Admin.HtmlSanitization.IHtmlSanitizer, CRM.Infrastructure.Admin.HtmlSanitization.HtmlSanitizerService>();
builder.Services.AddScoped<CRM.Application.Common.Services.IHtmlSanitizer, CRM.Api.Adapters.HtmlSanitizerAdapter>();
builder.Services.AddScoped<CRM.Application.Admin.Services.IAuditLogService, CRM.Application.Admin.Services.AuditLogService>();
builder.Services.AddScoped<CRM.Application.Admin.Services.ISystemSettingsService, CRM.Application.Admin.Services.SystemSettingsService>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetSystemSettingsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Commands.Handlers.UpdateSystemSettingsCommandHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Services.IIntegrationKeyService, CRM.Application.Admin.Services.IntegrationKeyService>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetIntegrationKeysQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetIntegrationKeyByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetIntegrationKeyWithValueQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Commands.Handlers.CreateIntegrationKeyCommandHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Commands.Handlers.UpdateIntegrationKeyCommandHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Commands.Handlers.DeleteIntegrationKeyCommandHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetAuditLogsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetAuditLogByIdQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.ExportAuditLogsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Services.IBrandingService, CRM.Application.Admin.Services.BrandingService>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetBrandingQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Commands.Handlers.UpdateBrandingCommandHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Commands.Handlers.UploadLogoCommandHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Services.IDataRetentionService, CRM.Application.Admin.Services.DataRetentionService>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetDataRetentionPoliciesQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Commands.Handlers.UpdateDataRetentionPolicyCommandHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Services.INotificationSettingsService, CRM.Application.Admin.Services.NotificationSettingsService>();
builder.Services.AddScoped<CRM.Application.Admin.Queries.Handlers.GetNotificationSettingsQueryHandler>();
builder.Services.AddScoped<CRM.Application.Admin.Commands.Handlers.UpdateNotificationSettingsCommandHandler>();

// Document Template Processing Services (Spec-024)
builder.Services.AddScoped<CRM.Infrastructure.Services.DocumentProcessing.IPdfParserService, CRM.Infrastructure.Services.DocumentProcessing.PdfParserService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.DocumentProcessing.IWordDocumentService, CRM.Infrastructure.Services.DocumentProcessing.WordDocumentService>();
builder.Services.AddScoped<CRM.Application.DocumentTemplates.Services.IDocumentProcessingService, CRM.Infrastructure.Services.DocumentProcessingService>();
builder.Services.AddScoped<CRM.Application.DocumentTemplates.Services.IPlaceholderIdentificationService, CRM.Application.DocumentTemplates.Services.PlaceholderIdentificationService>();
builder.Services.AddScoped<CRM.Application.DocumentTemplates.Services.IPlaceholderMappingService, CRM.Application.DocumentTemplates.Services.PlaceholderMappingService>();
// Document Template Command Handlers
builder.Services.AddScoped<CRM.Application.DocumentTemplates.Commands.Handlers.UploadDocumentCommandHandler>();
// builder.Services.AddScoped<CRM.Application.DocumentTemplates.Commands.Handlers.ConvertDocumentCommandHandler>(); // Commented out - depends on ITemplateConversionService which doesn't exist
builder.Services.AddScoped<CRM.Application.DocumentTemplates.Queries.Handlers.ListTemplatesQueryHandler>();
// Document Template Validators
builder.Services.AddScoped<FluentValidation.IValidator<CRM.Application.DocumentTemplates.Dtos.UploadDocumentRequest>, CRM.Application.DocumentTemplates.Validators.UploadDocumentRequestValidator>();

// Configure file upload limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
    options.ValueLengthLimit = 52428800;
    options.MultipartHeadersLengthLimit = 52428800;
});

builder.Services.AddHttpContextAccessor();
// Temporarily disabled during first boot; will re-enable after schema is applied
// builder.Services.AddHostedService<CleanupExpiredResetTokensJob>();
// builder.Services.AddHostedService<SuspiciousActivityAggregationJob>();
builder.Services.AddHostedService<QuotationExpirationCheckJob>();
builder.Services.AddHostedService<UnviewedQuotationReminderJob>();
builder.Services.AddHostedService<PendingResponseFollowUpJob>();
builder.Services.AddHostedService<DiscountApprovalEscalationJob>();
builder.Services.AddHostedService<CRM.Infrastructure.Jobs.DailyMetricsCalculationJob>();
builder.Services.AddHostedService<CRM.Infrastructure.Jobs.ScheduledReportExecutionJob>();
builder.Services.AddHostedService<CRM.Infrastructure.Jobs.ReportCleanupJob>();

// CORS for separate subdomains
// Antiforgery registration (outside of AddCors to avoid modifying service collection during options building)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-Token";
    options.Cookie.Name = "XSRF-TOKEN";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("app-to-api", policy =>
    {
        var allowed = new System.Collections.Generic.List<string>();
        
        // Check for configured origin from appsettings
        var configured = builder.Configuration["Cors:FrontendOrigin"];
        if (!string.IsNullOrWhiteSpace(configured)) 
        {
            allowed.Add(configured);
        }
        
        // Always allow localhost in development - use multiple detection methods
        var envName = builder.Environment.EnvironmentName ?? "";
        var isDevelopment = builder.Environment.IsDevelopment() || 
                           envName.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
                           envName.Equals("Dev", StringComparison.OrdinalIgnoreCase) ||
                           string.IsNullOrWhiteSpace(envName) ||
                           !builder.Environment.IsProduction();
        
        // ALWAYS add localhost origins for local development
        // This ensures CORS works even if environment detection fails
        allowed.Add("http://localhost:3000");
        allowed.Add("http://127.0.0.1:3000");
        allowed.Add("http://localhost:3001");
        
        // Add production origin only if no other origins are configured
        if (allowed.Count == 0) 
        {
            allowed.Add("https://app.crm.com");
        }

        var originsArray = allowed.Distinct().ToArray();
        Console.WriteLine($"[CORS] Environment: {envName}, IsDevelopment: {isDevelopment}");
        Console.WriteLine($"[CORS] Configured allowed origins: {string.Join(", ", originsArray)}");
        
        policy.WithOrigins(originsArray)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // Cache preflight for 24 hours
    });
});

// JWT authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection.GetValue<string>("Secret") ?? Environment.GetEnvironmentVariable("JWT__SECRET") ?? "";
var issuer = jwtSection.GetValue<string>("Issuer") ?? "crm.system";
var audience = jwtSection.GetValue<string>("Audience") ?? "crm.api";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Map JWT standard claims (sub, email, etc.) to .NET claim types
        options.MapInboundClaims = true;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            RoleClaimType = ClaimTypes.Role, // Use standard ASP.NET Core role claim type for [Authorize(Roles="...")]
            NameClaimType = ClaimTypes.NameIdentifier // Use standard name claim type (maps "sub" claim)
        };
    });

// Configure authorization to use role-based authorization
builder.Services.AddAuthorization(options =>
{
    // Default policy requires authentication
    options.FallbackPolicy = options.DefaultPolicy;
});

// Temporarily disabled during first boot; will re-enable after schema is applied
// builder.Services.AddRateLimiter(options =>
// {
//     options.AddPolicy("register-ip", httpContext =>
//         RateLimitPartition.GetFixedWindowLimiter(
//             partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
//             factory: key => new FixedWindowRateLimiterOptions
//             {
//                 PermitLimit = 5,
//                 Window = TimeSpan.FromHours(1),
//                 QueueLimit = 0,
//                 AutoReplenishment = true
//             }));
// });

var app = builder.Build();

// Ensure database schema is present (for environments without compiled migrations, use EnsureCreated)
// Run database initialization in background to not block server startup
_ = Task.Run(async () =>
{
    await Task.Delay(1000); // Give server time to start
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            try
            {
                db.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS citext;");
                Console.WriteLine("Ensured PostgreSQL extension 'citext' is available.");
            }
            catch (Exception extEx)
            {
                Console.WriteLine($"Warning: could not create 'citext' extension automatically: {extEx.Message}");
            }
            var created = db.Database.EnsureCreated();
            Console.WriteLine(created ? "Database schema created via EnsureCreated." : "EnsureCreated found existing schema.");
            DbSeeder.Seed(db);

        // Spec-7: Ensure SavedSearches table and FTS/indexes exist (idempotent)
        try
        {
            // SavedSearches table
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""SavedSearches"" (
                  ""SavedSearchId"" uuid PRIMARY KEY,
                  ""UserId"" uuid NOT NULL,
                  ""SearchName"" varchar(255) NOT NULL,
                  ""FilterCriteria"" jsonb NOT NULL,
                  ""SortBy"" varchar(50),
                  ""IsActive"" boolean NOT NULL DEFAULT true,
                  ""CreatedAt"" timestamptz NOT NULL,
                  ""UpdatedAt"" timestamptz NOT NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_SavedSearches_UserId"" ON ""SavedSearches"" (""UserId"");
                CREATE INDEX IF NOT EXISTS ""IX_SavedSearches_IsActive"" ON ""SavedSearches"" (""IsActive"");
            ");

            // Extensions for FTS
            db.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS unaccent;");
            db.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // Try to make unaccent function immutable (requires superuser, but safe to try)
            try
            {
                db.Database.ExecuteSqlRaw(@"
                    DO $$
                    BEGIN
                        BEGIN
                            ALTER FUNCTION unaccent(text) IMMUTABLE;
                        EXCEPTION WHEN OTHERS THEN
                            NULL;
                        END;
                    END $$;
                ");
            }
            catch { }

            // FTS and filter indexes on Clients (with fallback if unaccent is not immutable)
            try
            {
                db.Database.ExecuteSqlRaw(@"
                    DO $$
                    BEGIN
                        BEGIN
                            CREATE INDEX IF NOT EXISTS ""IX_Clients_FTS"" ON ""Clients"" USING GIN (
                              to_tsvector('simple',
                                coalesce(unaccent(""CompanyName""),'') || ' ' ||
                                coalesce(unaccent(""ContactName""),'') || ' ' ||
                                coalesce(unaccent(""Email""),'')
                              )
                            );
                        EXCEPTION WHEN OTHERS THEN
                            -- If unaccent is not immutable, create index without it
                            DROP INDEX IF EXISTS ""IX_Clients_FTS"";
                            CREATE INDEX IF NOT EXISTS ""IX_Clients_FTS"" ON ""Clients"" USING GIN (
                              to_tsvector('simple',
                                coalesce(""CompanyName"",'') || ' ' ||
                                coalesce(""ContactName"",'') || ' ' ||
                                coalesce(""Email"",'')
                              )
                            );
                        END;
                    END $$;
                ");
            }
            catch (Exception ftsEx)
            {
                Console.WriteLine($"Warning: Could not create FTS index with unaccent, trying without: {ftsEx.Message}");
                // Fallback: create index without unaccent
                try
                {
                    db.Database.ExecuteSqlRaw(@"
                        DROP INDEX IF EXISTS ""IX_Clients_FTS"";
                        CREATE INDEX IF NOT EXISTS ""IX_Clients_FTS"" ON ""Clients"" USING GIN (
                          to_tsvector('simple',
                            coalesce(""CompanyName"",'') || ' ' ||
                            coalesce(""ContactName"",'') || ' ' ||
                            coalesce(""Email"",'')
                          )
                        );
                    ");
                }
                catch { }
            }

            // Other filter indexes
            db.Database.ExecuteSqlRaw(@"
                CREATE INDEX IF NOT EXISTS ""IX_Clients_State"" ON ""Clients"" (""State"");
                CREATE INDEX IF NOT EXISTS ""IX_Clients_City"" ON ""Clients"" (""City"");
                CREATE INDEX IF NOT EXISTS ""IX_Clients_StateCode"" ON ""Clients"" (""StateCode"");
                CREATE INDEX IF NOT EXISTS ""IX_Clients_Email"" ON ""Clients"" (""Email"");
                CREATE INDEX IF NOT EXISTS ""IX_Clients_CompanyName"" ON ""Clients"" (""CompanyName"");
            ");
            Console.WriteLine("Spec-7 schema ensured (SavedSearches + FTS/indexes).");
        }
        catch (Exception spec7Ex)
        {
            Console.WriteLine($"Warning: Spec-7 ensure failed: {spec7Ex.Message}");
        }

        // Ensure QuotationTemplates tables exist
        try
        {
            db.Database.ExecuteSqlRaw(@"
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
            Console.WriteLine("QuotationTemplates tables ensured.");
        }
        catch (Exception templateEx)
        {
            Console.WriteLine($"Warning: QuotationTemplates ensure failed: {templateEx.Message}");
        }

        // Ensure correct unique index for versioned templates
        try
        {
            db.Database.ExecuteSqlRaw(@"
                DO $$
                BEGIN
                    -- Drop incorrect unique index if it exists (blocked multiple versions per name/owner)
                    IF EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE schemaname = 'public' AND indexname = 'IX_QuotationTemplates_Name_OwnerUserId'
                    ) THEN
                        DROP INDEX ""IX_QuotationTemplates_Name_OwnerUserId"";
                    END IF;

                    -- Create correct composite unique index including Version
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE schemaname = 'public' AND indexname = 'IX_QuotationTemplates_Name_OwnerUserId_Version'
                    ) THEN
                        CREATE UNIQUE INDEX ""IX_QuotationTemplates_Name_OwnerUserId_Version"" 
                        ON ""QuotationTemplates""(""Name"", ""OwnerUserId"", ""Version"") 
                        WHERE ""DeletedAt"" IS NULL;
                    END IF;
                END $$;
            ");
            Console.WriteLine("QuotationTemplates unique index verified (Name, OwnerUserId, Version).");
        }
        catch (Exception idxEx)
        {
            Console.WriteLine($"Warning: Could not adjust QuotationTemplates unique index: {idxEx.Message}");
        }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Schema ensure/seed failed: {ex}");
        }
    }
    catch (Exception outerEx)
    {
        Console.WriteLine($"Database initialization task failed: {outerEx.Message}");
    }
});

// CORS must be at the very beginning to handle preflight requests
// This must come before any other middleware that might modify headers
// Enable static file serving for uploads (logos, etc.)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = ""
});

app.UseCors("app-to-api");

// Debug middleware to log CORS-related requests
if (builder.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Method == "OPTIONS")
        {
            Console.WriteLine($"[CORS DEBUG] OPTIONS request from origin: {context.Request.Headers["Origin"]}");
        }
        else if (context.Request.Headers.ContainsKey("Origin"))
        {
            Console.WriteLine($"[CORS DEBUG] {context.Request.Method} request from origin: {context.Request.Headers["Origin"]}");
        }
        await next();
        if (context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
        {
            Console.WriteLine($"[CORS DEBUG] Response includes Access-Control-Allow-Origin: {context.Response.Headers["Access-Control-Allow-Origin"]}");
        }
    });
}

// Security headers and HSTS (enable HSTS in production scenarios)
// Note: This runs after CORS so it doesn't interfere with CORS headers
app.Use(async (context, next) =>
{
    // Don't set security headers for OPTIONS requests (CORS preflight)
    if (context.Request.Method != "OPTIONS")
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["X-XSS-Protection"] = "0";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
    }
    await next();
});

app.UseRouting();

// TODO: Add Finbuckle MultiTenant middleware once package issues are resolved
// app.UseMultiTenant();

app.UseSwagger();
app.UseSwaggerUI();
// app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Configure Hangfire Dashboard (only in development for now)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new Hangfire.DashboardOptions
    {
        Authorization = new[] { new Hangfire.Dashboard.BasicAuthorization.BasicAuthAuthorizationFilter(
            new Hangfire.Dashboard.BasicAuthorization.BasicAuthAuthorizationFilterOptions
            {
                RequireSsl = false,
                SslRedirect = false,
                LoginCaseSensitive = true,
                Users = new[]
                {
                    new Hangfire.Dashboard.BasicAuthorization.BasicAuthAuthorizationUser
                    {
                        Login = "admin",
                        PasswordClear = "admin123"
                    }
                }
            })
        }
    });
}

app.UseHsts();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var ex = feature?.Error;
        var (status, message) = ex switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, ex!.Message),
            DomainValidationException => (StatusCodes.Status422UnprocessableEntity, ex!.Message),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, ex!.Message),
            UserNotActiveException => (StatusCodes.Status403Forbidden, ex!.Message),
            InvalidTokenException => (StatusCodes.Status401Unauthorized, ex!.Message),
            TokenExpiredException => (StatusCodes.Status401Unauthorized, ex!.Message),
            RoleNotFoundException => (StatusCodes.Status404NotFound, ex!.Message),
            DuplicateRoleNameException => (StatusCodes.Status409Conflict, ex!.Message),
            CannotModifyBuiltInRoleException => (StatusCodes.Status400BadRequest, ex!.Message),
            CannotDeleteRoleInUseException => (StatusCodes.Status400BadRequest, ex!.Message),
            InvalidRoleException => (StatusCodes.Status422UnprocessableEntity, ex!.Message),
            InvalidCurrentPasswordException => (StatusCodes.Status401Unauthorized, ex!.Message),
            WeakPasswordException => (StatusCodes.Status400BadRequest, ex!.Message),
            PasswordReuseException => (StatusCodes.Status400BadRequest, ex!.Message),
            AccountLockedException => (StatusCodes.Status403Forbidden, ex!.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, ex!.Message),
            _ => (StatusCodes.Status500InternalServerError, app.Environment.IsDevelopment() ? (ex?.ToString() ?? "An unexpected error occurred") : "An unexpected error occurred")
        };
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.MapControllers();
app.MapHub<CRM.Api.Hubs.PresenceHub>("/hubs/presence");
app.MapHub<CRM.Infrastructure.Hubs.NotificationHub>("/hubs/notifications");

// Serve the Spec-005 contract file for reference
app.MapGet("/contracts/user-profile-password.openapi.yaml", (IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.ContentRootPath, "specs", "5-user-profile-password", "contracts", "user-profile-password.openapi.yaml");
    if (!System.IO.File.Exists(path)) return Results.NotFound();
    var content = System.IO.File.ReadAllText(path);
    return Results.Text(content, "application/yaml");
});

// Serve the Spec-006 contract file for reference
app.MapGet("/contracts/clients.openapi.yaml", (IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.ContentRootPath, "specs", "006-client-crud", "contracts", "clients.openapi.yaml");
    if (!System.IO.File.Exists(path)) return Results.NotFound();
    var content = System.IO.File.ReadAllText(path);
    return Results.Text(content, "application/yaml");
});

// Serve the Spec-008 contract file for reference
app.MapGet("/contracts/client-history.openapi.yaml", (IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.ContentRootPath, "specs", "008-client-history", "contracts", "client-history.openapi.yaml");
    if (!System.IO.File.Exists(path)) return Results.NotFound();
    var content = System.IO.File.ReadAllText(path);
    return Results.Text(content, "application/yaml");
});

// Serve the Spec-009 contract file for reference
app.MapGet("/contracts/quotations.openapi.yaml", (IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.ContentRootPath, "specs", "009-quotation-crud", "contracts", "quotations.openapi.yaml");
    if (!System.IO.File.Exists(path)) return Results.NotFound();
    var content = System.IO.File.ReadAllText(path);
    return Results.Text(content, "application/yaml");
});

// Serve the Spec-010 contract file for reference
app.MapGet("/contracts/quotation-management.openapi.yaml", (IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.ContentRootPath, "specs", "010-quotation-management", "contracts", "quotation-management.openapi.yaml");
    if (!System.IO.File.Exists(path)) return Results.NotFound();
    var content = System.IO.File.ReadAllText(path);
    return Results.Text(content, "application/yaml");
});

app.Run();
