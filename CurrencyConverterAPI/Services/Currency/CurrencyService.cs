using CurrencyConverterAPI.Services.Interfaces;
using CurrencyConverterAPI.Models;


namespace CurrencyConverterAPI.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyProviderFactory _providerFactory;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(
        ICurrencyProviderFactory providerFactory,
        ILogger<CurrencyService> logger)
    {
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency)
    {
        var provider = _providerFactory.CreateProvider();
        return await provider.GetLatestRatesAsync(baseCurrency);
    }

    public async Task<ExchangeRateResponse> ConvertCurrencyAsync(ConversionRequest request)
    {
        var provider = _providerFactory.CreateProvider();
        return await provider.ConvertCurrencyAsync(request.FromCurrency, request.ToCurrency, request.Amount);
    }

    public async Task<HistoricalRatesResponse> GetHistoricalRatesAsync(HistoricalRatesRequest request)
    {
        var provider = _providerFactory.CreateProvider();
        var response = await provider.GetHistoricalRatesAsync(
            request.BaseCurrency, 
            request.StartDate, 
            request.EndDate);

        // Implement pagination
        var ratesCount = response.Rates?.Count ?? 0;
        var totalPages = (int)Math.Ceiling(ratesCount / (double)request.PageSize);
        
        var paginatedRates = response.Rates?
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return new HistoricalRatesResponse
        {
            Amount = response.Amount,
            Base = response.Base,
            StartDate = response.StartDate,
            EndDate = response.EndDate,
            Rates = paginatedRates,
            Metadata = new PaginationMetadata
            {
                TotalCount = ratesCount,
                PageSize = request.PageSize,
                CurrentPage = request.PageNumber,
                TotalPages = totalPages
            }
        };
    }
}