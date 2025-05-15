using CurrencyConverterAPI.Services.Interfaces;
using CurrencyConverterAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace CurrencyConverterAPI.Providers;

public class FrankfurterCurrencyProvider : ICurrencyProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FrankfurterCurrencyProvider> _logger;
    private readonly IMemoryCache _cache;
    private string[] _invalidCurrencies = new[] { "TRY", "PLN", "THB", "MXN" };

    public FrankfurterCurrencyProvider(
        IHttpClientFactory httpClientFactory, 
        ILogger<FrankfurterCurrencyProvider> logger,
        IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency)
    {
        ValidateCurrencies(baseCurrency);
        var cacheKey = $"latest_{baseCurrency}";
        
        if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse cachedResponse))
        {
            return cachedResponse;
        }

        var client = _httpClientFactory.CreateClient("Frankfurter");
        var response = await client.GetAsync($"latest?from={baseCurrency}");
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch latest rates from Frankfurter API. Status: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch latest rates. Status: {response.StatusCode}");
        }

        var content = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>();
        content.Rates = content.Rates.Where(kvp => !_invalidCurrencies.Contains(kvp.Key.ToUpper()))
                                      .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        // Cache for 1 hour
        _cache.Set(cacheKey, content, TimeSpan.FromHours(1));
        
        return content;
    }

    public async Task<ExchangeRateResponse> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount)
    {
        ValidateCurrencies(fromCurrency, toCurrency);
        
        var client = _httpClientFactory.CreateClient("Frankfurter");
        var response = await client.GetAsync($"latest?amount={amount}&from={fromCurrency}&to={toCurrency}");
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to convert currency from Frankfurter API. Status: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to convert currency. Status: {response.StatusCode}");
        }

        return await response.Content.ReadFromJsonAsync<ExchangeRateResponse>();
    }

    public async Task<HistoricalRatesResponse> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime? endDate = null)
    {
        var cacheKey = $"historical_{baseCurrency}_{startDate:yyyyMMdd}_{(endDate?.ToString("yyyyMMdd") ?? "latest")}";

         if (_cache.TryGetValue(cacheKey, out HistoricalRatesResponse cachedResponse))
            {
                return cachedResponse;
            }

        var client = _httpClientFactory.CreateClient("Frankfurter");
        var response = await client.GetAsync($"{startDate:yyyy-MM-dd}{(endDate != null ? $"..{endDate:yyyy-MM-dd}" : "..")}?from={baseCurrency}");
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch historical rates from Frankfurter API. Status: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch historical rates. Status: {response.StatusCode}");
        }

        var content = await response.Content.ReadFromJsonAsync<HistoricalRatesAPIResponse>();
        content.Rates = content.Rates.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value.Where(r => !_invalidCurrencies.Contains(r.Key.ToUpper()))
                            .ToDictionary(r => r.Key, r => r.Value))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        var result = new HistoricalRatesResponse{
            Amount = content.Amount,
            Rates = content.Rates,
            Base = content.Base,
            StartDate = content.StartDate,
            EndDate = content.EndDate
        };

        _cache.Set(cacheKey, result, TimeSpan.FromHours(1));

        return result;
    }

    private void ValidateCurrencies(params string[] currencies)
    {
        foreach (var currency in currencies)
        {
            if (_invalidCurrencies.Contains(currency.ToUpper()))
            {
                throw new ArgumentException($"Currency {currency} is not supported");
            }
        }
    }
}