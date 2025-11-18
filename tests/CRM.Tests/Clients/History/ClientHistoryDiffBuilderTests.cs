using System;
using System.Collections.Generic;
using CRM.Application.Clients.Queries;
using CRM.Application.Clients.Services;
using CRM.Application.Clients.Validation;
using Xunit;

namespace CRM.Tests.Clients.History
{
    public class ClientHistoryDiffBuilderTests
    {
        [Fact]
        public void Build_Masks_Sensitive_Values()
        {
            var builder = new ClientHistoryDiffBuilder();
            var before = new Dictionary<string, object?>
            {
                { "Password", "SuperSecret" },
                { "City", "Pune" }
            };
            var after = new Dictionary<string, object?>
            {
                { "Password", "NewSecret" },
                { "City", "Mumbai" }
            };

            var diff = builder.Build(before, after);

            Assert.Contains("City", diff.ChangedFields);
            Assert.NotNull(diff.BeforeSnapshotJson);
            Assert.Contains("***", diff.BeforeSnapshotJson!);
            Assert.Contains("\"Mumbai\"", diff.AfterSnapshotJson);
        }

        [Fact]
        public void HistoryValidator_Flags_Invalid_Pagination()
        {
            var validator = new GetClientHistoryQueryValidator();
            var result = validator.Validate(new GetClientHistoryQuery
            {
                ClientId = Guid.Empty,
                RequestorUserId = Guid.Empty,
                PageNumber = 0,
                PageSize = 101
            });

            Assert.False(result.IsValid);
        }

        [Fact]
        public void TimelineValidator_Passes_With_Valid_Data()
        {
            var validator = new GetClientTimelineQueryValidator();
            var result = validator.Validate(new GetClientTimelineQuery
            {
                ClientId = Guid.NewGuid(),
                RequestorUserId = Guid.NewGuid()
            });

            Assert.True(result.IsValid);
        }
    }
}

