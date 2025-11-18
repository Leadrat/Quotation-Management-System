using System;
using System.Collections.Generic;
using System.Linq;
using CRM.Domain.Entities;
using CRM.Shared.Config;

namespace CRM.Application.Clients.Services
{
    public class SuspiciousActivityScorer
    {
        private readonly SuspiciousActivitySettings _settings;

        public SuspiciousActivityScorer(SuspiciousActivitySettings settings)
        {
            _settings = settings;
        }

        public short CalculateScore(ClientHistory history, IEnumerable<ClientHistory> recentHistory)
        {
            short score = 0;
            var reasons = new List<string>();

            // Rapid changes: multiple changes within short time window
            var changesInLastHour = recentHistory
                .Where(h => h.ClientId == history.ClientId &&
                           h.CreatedAt >= history.CreatedAt.AddHours(-1) &&
                           h.CreatedAt <= history.CreatedAt)
                .Count();

            if (changesInLastHour >= _settings.RapidChangeThresholdPerHour)
            {
                score += 3;
                reasons.Add($"Rapid changes: {changesInLastHour} changes in last hour");
            }

            // Unusual hours: activity outside business hours (9 AM - 6 PM)
            var hour = history.CreatedAt.Hour;
            if (hour < 9 || hour >= 18)
            {
                score += 2;
                reasons.Add($"Unusual time: activity at {hour}:00");
            }

            // Unknown IP: check metadata for unrecognized IPs
            if (!string.IsNullOrEmpty(history.Metadata))
            {
                // In a real implementation, you'd parse JSON and check against known IP ranges
                // For now, we'll use a simple heuristic: if IP is not in common ranges
                if (history.Metadata.Contains("\"IpAddress\"") && 
                    !history.Metadata.Contains("192.168") && 
                    !history.Metadata.Contains("10.0") &&
                    !history.Metadata.Contains("127.0.0.1"))
                {
                    score += 2;
                    reasons.Add("Unrecognized IP address");
                }
            }

            // Mass updates: many fields changed at once (5+ fields)
            if (history.ChangedFields != null && history.ChangedFields.Count >= 5)
            {
                score += 2;
                reasons.Add($"Mass update: {history.ChangedFields.Count} fields changed");
            }

            // High suspicion score threshold
            if (score >= _settings.InlineThreshold)
            {
                return Math.Min((short)10, score);
            }

            return score;
        }

        public bool ShouldFlagInline(short score)
        {
            return score >= _settings.InlineThreshold;
        }
    }
}

