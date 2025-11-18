using Xunit;
using CRM.Application.Localization.Services;
using CRM.Application.Common.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using System;
using CRM.Domain.Entities;

namespace CRM.Tests.Unit.Localization;

public class LocalizationServiceTests
{
    private readonly Mock<IAppDbContext> _mockDbContext;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly LocalizationService _service;

    public LocalizationServiceTests()
    {
        _mockDbContext = new Mock<IAppDbContext>();
        _mockLogger = new Mock<ILogger<LocalizationService>>();
        _service = new LocalizationService(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetLocalizedStringAsync_ReturnsTranslation_WhenExists()
    {
        // Arrange
        var resources = new List<LocalizationResource>
        {
            new LocalizationResource 
            { 
                ResourceId = Guid.NewGuid(),
                LanguageCode = "en",
                ResourceKey = "common.save",
                ResourceValue = "Save"
            },
            new LocalizationResource 
            { 
                ResourceId = Guid.NewGuid(),
                LanguageCode = "hi",
                ResourceKey = "common.save",
                ResourceValue = "सहेजें"
            }
        };

        var mockSet = TestHelpers.CreateMockDbSet(resources.AsQueryable());
        _mockDbContext.Setup(x => x.LocalizationResources).Returns(mockSet.Object);

        // Act
        var result = await _service.GetLocalizedStringAsync("common.save", "hi");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("सहेजें", result);
    }

    [Fact]
    public async Task GetLocalizedStringAsync_ReturnsFallback_WhenTranslationMissing()
    {
        // Arrange
        var resources = new List<LocalizationResource>();
        var mockSet = TestHelpers.CreateMockDbSet(resources.AsQueryable());
        _mockDbContext.Setup(x => x.LocalizationResources).Returns(mockSet.Object);

        // Act
        var result = await _service.GetLocalizedStringAsync("common.missing", "hi");

        // Assert
        Assert.NotNull(result);
        // Should return English fallback or key itself
    }
}

