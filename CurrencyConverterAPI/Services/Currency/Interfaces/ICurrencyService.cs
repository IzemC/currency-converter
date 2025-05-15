using CurrencyConverterAPI.Models;

namespace CurrencyConverterAPI.Services.Interfaces;

public interface ICurrencyService
{
    Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency);
    Task<ExchangeRateResponse> ConvertCurrencyAsync(ConversionRequest request);
    Task<HistoricalRatesResponse> GetHistoricalRatesAsync(HistoricalRatesRequest request);
}

public interface ICurrencyProvider
{
    Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency);
    Task<ExchangeRateResponse> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount);
    Task<HistoricalRatesResponse> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime? endDate);
}

public interface ICurrencyProviderFactory
{
    ICurrencyProvider CreateProvider();
}