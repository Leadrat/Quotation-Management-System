using CRM.Application.Clients.Queries;
using CRM.Application.Clients.Validators;
using Xunit;

namespace CRM.Tests.Unit.Clients
{
    public class SearchClientsQueryValidatorTests
    {
        [Fact]
        public void Invalid_PageNumber_Should_Fail()
        {
            var v = new SearchClientsQueryValidator();
            var q = new SearchClientsQuery { PageNumber = 0, PageSize = 10 };
            var r = v.Validate(q);
            Assert.False(r.IsValid);
        }

        [Fact]
        public void Invalid_PageSize_Should_Fail()
        {
            var v = new SearchClientsQueryValidator();
            var q = new SearchClientsQuery { PageNumber = 1, PageSize = 1000 };
            var r = v.Validate(q);
            Assert.False(r.IsValid);
        }

        [Fact]
        public void Invalid_DateRange_Should_Fail()
        {
            var v = new SearchClientsQueryValidator();
            var q = new SearchClientsQuery
            {
                PageNumber = 1,
                PageSize = 10,
                CreatedDateFrom = System.DateTimeOffset.Parse("2025-02-01T00:00:00Z"),
                CreatedDateTo = System.DateTimeOffset.Parse("2025-01-01T00:00:00Z")
            };
            var r = v.Validate(q);
            Assert.False(r.IsValid);
        }

        [Fact]
        public void Valid_Minimal_Should_Pass()
        {
            var v = new SearchClientsQueryValidator();
            var q = new SearchClientsQuery { PageNumber = 1, PageSize = 10 };
            var r = v.Validate(q);
            Assert.True(r.IsValid);
        }
    }
}
