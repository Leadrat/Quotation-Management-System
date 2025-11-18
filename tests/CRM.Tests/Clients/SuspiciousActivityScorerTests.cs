using System;
using System.Collections.Generic;
using System.Linq;
using CRM.Application.Clients.Services;
using CRM.Domain.Entities;
using CRM.Shared.Config;
using Xunit;

namespace CRM.Tests.Clients
{
    public class SuspiciousActivityScorerTests
    {
        private static SuspiciousActivitySettings NewSettings()
        {
            return new SuspiciousActivitySettings
            {
                InlineThreshold = 7,
                RapidChangeThresholdPerHour = 5
            };
        }

        [Fact]
        public void Rapid_Changes_Increase_Score()
        {
            var settings = NewSettings();
            var scorer = new SuspiciousActivityScorer(settings);

            var history = new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                ActionType = "UPDATED",
                ChangedFields = new List<string> { "Email" },
                CreatedAt = DateTimeOffset.UtcNow
            };

            var recentHistory = Enumerable.Range(0, 6)
                .Select(i => new ClientHistory
                {
                    HistoryId = Guid.NewGuid(),
                    ClientId = history.ClientId,
                    ActionType = "UPDATED",
                    ChangedFields = new List<string> { $"Field{i}" },
                    CreatedAt = history.CreatedAt.AddMinutes(-i * 10)
                })
                .ToList();

            var score = scorer.CalculateScore(history, recentHistory);

            Assert.True(score >= 3); // At least rapid changes penalty
        }

        [Fact]
        public void Unusual_Hours_Increase_Score()
        {
            var settings = NewSettings();
            var scorer = new SuspiciousActivityScorer(settings);

            var history = new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                ActionType = "UPDATED",
                ChangedFields = new List<string> { "Email" },
                CreatedAt = new DateTimeOffset(2025, 1, 1, 2, 0, 0, TimeSpan.Zero) // 2 AM
            };

            var score = scorer.CalculateScore(history, new List<ClientHistory>());

            Assert.True(score >= 2); // Unusual hours penalty
        }

        [Fact]
        public void Mass_Update_Increase_Score()
        {
            var settings = NewSettings();
            var scorer = new SuspiciousActivityScorer(settings);

            var history = new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                ActionType = "UPDATED",
                ChangedFields = Enumerable.Range(0, 12).Select(i => $"Field{i}").ToList(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var score = scorer.CalculateScore(history, new List<ClientHistory>());

            Assert.True(score >= 2); // Mass update penalty
        }

        [Fact]
        public void Low_Score_Does_Not_Flag()
        {
            var settings = NewSettings();
            var scorer = new SuspiciousActivityScorer(settings);

            var history = new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                ActionType = "UPDATED",
                ChangedFields = new List<string> { "Email" },
                CreatedAt = new DateTimeOffset(2025, 1, 1, 14, 0, 0, TimeSpan.Zero) // 2 PM - normal hours
            };

            var score = scorer.CalculateScore(history, new List<ClientHistory>());

            Assert.False(scorer.ShouldFlagInline(score));
        }
    }
}

