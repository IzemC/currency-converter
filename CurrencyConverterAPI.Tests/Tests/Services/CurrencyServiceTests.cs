using CurrencyConverterAPI.Models;
using CurrencyConverterAPI.Services;
using CurrencyConverterAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CurrencyConverterAPI.Tests;

public class CurrencyServiceTests
{
    private readonly Mock<ICurrencyProviderFactory> _mockFactory;
    private readonly Mock<ICurrencyProvider> _mockProvider;
    private readonly CurrencyService _service;
    private readonly Mock<ILogger<CurrencyService>> _mockLogger;

    public CurrencyServiceTests()
    {
        _mockFactory = new Mock<ICurrencyProviderFactory>();
        _mockProvider = new Mock<ICurrencyProvider>();
        _mockLogger = new Mock<ILogger<CurrencyService>>();
        
        _mockFactory.Setup(x => x.CreateProvider()).Returns(_mockProvider.Object);
        
        _service = new CurrencyService(_mockFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetLatestRatesAsync_CallsProviderWithCorrectParameters()
    {
        // Arrange
        var expected = new ExchangeRateResponse { Base = "EUR", Rates = new Dictionary<string, decimal> { ["USD"] = 1.2m } };
        _mockProvider.Setup(x => x.GetLatestRatesAsync("EUR")).ReturnsAsync(expected);
        
        // Act
        var result = await _service.GetLatestRatesAsync("EUR");
        
        // Assert
        result.Should().BeEquivalentTo(expected);
        _mockProvider.Verify(x => x.GetLatestRatesAsync("EUR"), Times.Once);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_ThrowsForInvalidCurrencies()
    {
        // Arrange
        var request = new ConversionRequest { FromCurrency = "TRY", ToCurrency = "USD", Amount = 100 };
        _mockProvider.Setup(x => x.ConvertCurrencyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
            .ThrowsAsync(new ArgumentException("Currency TRY is not supported"));
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ConvertCurrencyAsync(request));
    }
}