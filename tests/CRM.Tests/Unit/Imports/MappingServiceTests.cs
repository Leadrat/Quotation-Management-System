using System.Text.Json;
using CRM.Application.Imports.Services;
using Xunit;

namespace CRM.Tests.Unit.Imports
{
    public class MappingServiceTests
    {
        [Fact]
        public void ValidateRequiredMappings_ReturnsFalse_WhenMissingCompanyName()
        {
            var svc = new MappingService();
            var json = JsonDocument.Parse("{ \n  \"company\": {}, \n  \"customer\": { \"name\": \"ACME\" } \n}");
            var ok = svc.ValidateRequiredMappings(json.RootElement, out var error);
            Assert.False(ok);
            Assert.Equal("Missing required mapping: company.name", error);
        }

        [Fact]
        public void ValidateRequiredMappings_ReturnsFalse_WhenMissingCustomerName()
        {
            var svc = new MappingService();
            var json = JsonDocument.Parse("{ \n  \"company\": { \"name\": \"MyCo\" }, \n  \"customer\": {} \n}");
            var ok = svc.ValidateRequiredMappings(json.RootElement, out var error);
            Assert.False(ok);
            Assert.Equal("Missing required mapping: customer.name", error);
        }

        [Fact]
        public void ValidateRequiredMappings_ReturnsTrue_WhenBothPresent()
        {
            var svc = new MappingService();
            var json = JsonDocument.Parse("{ \n  \"company\": { \"name\": \"MyCo\" }, \n  \"customer\": { \"name\": \"ACME\" } \n}");
            var ok = svc.ValidateRequiredMappings(json.RootElement, out var error);
            Assert.True(ok);
            Assert.Null(error);
        }
    }
}
