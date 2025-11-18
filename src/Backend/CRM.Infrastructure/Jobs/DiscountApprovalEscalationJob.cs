using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.DiscountApprovals.Commands;
using CRM.Application.DiscountApprovals.Commands.Handlers;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CRM.Infrastructure.Jobs
{
    public class DiscountApprovalEscalationJob : CronBackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DiscountApprovalEscalationJob> _logger;
        private const int AutoEscalationHours = 24;

        public DiscountApprovalEscalationJob(
            IServiceProvider serviceProvider,
            ILogger<DiscountApprovalEscalationJob> logger)
            : base("0 * * * * ?", null, logger) // Every hour
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var escalateHandler = scope.ServiceProvider.GetRequiredService<EscalateDiscountApprovalCommandHandler>();

                var cutoffTime = DateTimeOffset.UtcNow.AddHours(-AutoEscalationHours);

                // Find approvals pending > 24 hours at manager level
                var pendingApprovals = await db.DiscountApprovals
                    .Where(a =>
                        a.Status == ApprovalStatus.Pending &&
                        a.ApprovalLevel == ApprovalLevel.Manager &&
                        !a.EscalatedToAdmin &&
                        a.RequestDate < cutoffTime)
                    .Select(a => a.ApprovalId)
                    .ToListAsync(stoppingToken);

                var escalated = 0;
                foreach (var approvalId in pendingApprovals)
                {
                    try
                    {
                        // Get the manager who should escalate (or find any manager)
                        var approval = await db.DiscountApprovals
                            .Include(a => a.ApproverUser)
                            .FirstOrDefaultAsync(a => a.ApprovalId == approvalId, stoppingToken);

                        if (approval?.ApproverUserId != null)
                        {
                            var command = new EscalateDiscountApprovalCommand
                            {
                                ApprovalId = approvalId,
                                EscalatedByUserId = approval.ApproverUserId.Value,
                                Reason = $"Auto-escalated after {AutoEscalationHours} hours of pending status"
                            };

                            await escalateHandler.Handle(command);
                            escalated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to auto-escalate approval {ApprovalId}", approvalId);
                    }
                }

                if (escalated > 0)
                {
                    _logger.LogInformation("DiscountApprovalEscalationJob auto-escalated {Count} approvals", escalated);
                }
            }
            catch (NpgsqlException npgEx) when (npgEx.InnerException is System.TimeoutException || npgEx.Message.Contains("Timeout"))
            {
                _logger.LogWarning("Database connection timeout in DiscountApprovalEscalationJob: {Message}", npgEx.Message);
            }
            catch (System.InvalidOperationException invEx) when (invEx.Message.Contains("transient failure") || invEx.InnerException is System.TimeoutException)
            {
                _logger.LogWarning("Transient database failure in DiscountApprovalEscalationJob: {Message}", invEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background job DiscountApprovalEscalationJob");
            }
        }
    }
}

