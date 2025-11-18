using Xunit;
using CRM.Application.Localization.Services;
using CRM.Application.Common.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using System;
using CRM.Domain.Entities;

namespace CRM.Tests.Unit.Localization;

public class CurrencyServiceTests
{
    private readonly Mock<IAppDbContext> _mockDbContext;
    private readonly Mock<ILogger<CurrencyService>> _mockLogger;
    private readonly CurrencyService _service;

    public CurrencyServiceTests()
    {
        _mockDbContext = new Mock<IAppDbContext>();
        _mockLogger = new Mock<ILogger<CurrencyService>>();
        _service = new CurrencyService(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetSupportedCurrenciesAsync_ReturnsActiveCurrencies()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            new Currency { CurrencyCode = "USD", DisplayName = "US Dollar", Symbol = "$", IsActive = true },
            new Currency { CurrencyCode = "INR", DisplayName = "Indian Rupee", Symbol = "₹", IsActive = true },
            new Currency { CurrencyCode = "EUR", DisplayName = "Euro", Symbol = "€", IsActive = false },
        };

        var mockSet = TestHelpers.CreateMockDbSet(currencies.AsQueryable());
        _mockDbContext.Setup(x => x.Currencies).Returns(mockSet.Object);

        // Act
        var result = await _service.GetSupportedCurrenciesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.True(c.IsActive));
    }

    [Fact]
    public async Task GetCurrencyByCodeAsync_ReturnsCurrency_WhenExists()
    {
        // Arrange
        var currency = new Currency { CurrencyCode = "USD", DisplayName = "US Dollar", Symbol = "$", IsActive = true };
        var currencies = new List<Currency> { currency };

        var mockSet = TestHelpers.CreateMockDbSet(currencies.AsQueryable());
        _mockDbContext.Setup(x => x.Currencies).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCurrencyByCodeAsync("USD");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.CurrencyCode);
    }

    [Fact]
    public async Task GetCurrencyByCodeAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var currencies = new List<Currency>();
        var mockSet = TestHelpers.CreateMockDbSet(currencies.AsQueryable());
        _mockDbContext.Setup(x => x.Currencies).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCurrencyByCodeAsync("XYZ");

        // Assert
        Assert.Null(result);
    }
}

