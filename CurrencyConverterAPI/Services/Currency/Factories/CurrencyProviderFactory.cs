using CurrencyConverterAPI.Services.Interfaces;
using CurrencyConverterAPI.Providers;

public class CurrencyProviderFactory : ICurrencyProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CurrencyProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ICurrencyProvider CreateProvider()
    {
        // Currently only Frankfurter provider, but can be extended
        return _serviceProvider.GetRequiredService<FrankfurterCurrencyProvider>();
    }
}