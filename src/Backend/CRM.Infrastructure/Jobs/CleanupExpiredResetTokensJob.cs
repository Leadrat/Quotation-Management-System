using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CRM.Infrastructure.Jobs
{
    public class CleanupExpiredResetTokensJob : BackgroundService
    {
        private readonly ILogger<CleanupExpiredResetTokensJob> _logger;
        private readonly IServiceProvider _sp;
        public CleanupExpiredResetTokensJob(ILogger<CleanupExpiredResetTokensJob> logger, IServiceProvider sp)
        {
            _logger = logger;
            _sp = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTimeOffset.UtcNow;
                    var count = await db.PasswordResetTokens
                        .Where(t => t.ExpiresAt < now || t.UsedAt != null)
                        .ExecuteDeleteAsync(stoppingToken);
                    if (count > 0)
                    {
                        _logger.LogInformation("CleanupExpiredResetTokensJob removed {Count} tokens", count);
                    }
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
                {
                    _logger.LogWarning("Table does not exist yet, skipping job execution: {Message}", pgEx.Message);
                    // Wait longer before retrying when tables don't exist
                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                    continue;
                }
                catch (NpgsqlException npgEx) when (npgEx.InnerException is System.TimeoutException || npgEx.Message.Contains("Timeout"))
                {
                    _logger.LogWarning("Database connection timeout in CleanupExpiredResetTokensJob, will retry: {Message}", npgEx.Message);
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                    continue;
                }
                catch (System.InvalidOperationException invEx) when (invEx.Message.Contains("transient failure") || invEx.InnerException is System.TimeoutException)
                {
                    _logger.LogWarning("Transient database failure in CleanupExpiredResetTokensJob, will retry: {Message}", invEx.Message);
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CleanupExpiredResetTokensJob failed");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
