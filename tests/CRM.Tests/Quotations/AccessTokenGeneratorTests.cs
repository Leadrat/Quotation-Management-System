using System;
using System.Collections.Generic;
using CRM.Application.Quotations.Services;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class AccessTokenGeneratorTests
    {
        [Fact]
        public void Generate_ReturnsUrlSafeString()
        {
            var token = AccessTokenGenerator.Generate();

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.DoesNotContain("+", token);
            Assert.DoesNotContain("/", token);
            Assert.DoesNotContain("=", token);
            Assert.True(token.Length >= 43); // Base64URL 32 bytes => 43 chars without padding
        }

        [Fact]
        public void Generate_CustomLengthLessThan16_Throws()
        {
            Assert.Throws<ArgumentException>(() => AccessTokenGenerator.Generate(8));
        }

        [Fact]
        public void Generate_ProducesUniqueTokens()
        {
            var tokens = new HashSet<string>();

            for (var i = 0; i < 100; i++)
            {
                tokens.Add(AccessTokenGenerator.Generate());
            }

            Assert.Equal(100, tokens.Count);
        }
    }
}


