using CurrencyConverterAPI.Models;
using CurrencyConverterAPI.Services.Interfaces; 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Net.Http.Json;
using FluentAssertions;

namespace CurrencyConverterAPI.Tests;

public class CurrencyControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CurrencyControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateAuthenticatedClient()
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", options => { });
                
                services.AddAuthorization(options =>
                {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("Test")
                    .Build();
                });     
                
                services.AddSingleton<ICurrencyService>(new MockCurrencyService());
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetLatestRates_ReturnsData_ForAuthenticatedUser()
    {
        var client = CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Test", "UserToken");

        var response = await client.GetAsync("/api/v1/currency/latest?baseCurrency=EUR");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>();
        content.Should().NotBeNull();
        content.Base.Should().Be("EUR");
        content.Rates.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetHistoricalRates_ReturnsForbidden_ForRegularUser()
    {
        var client = CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Test", "UserToken");

        var response = await client.GetAsync("/api/v1/currency/historical");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetHistoricalRates_ReturnsData_ForAdminUser()
    {
        var client = CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Test", "AdminUser");

        var response = await client.GetAsync("/api/v1/currency/historical?BaseCurrency=EUR&StartDate=2025-05-01");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<HistoricalRatesResponse>();
        content.Should().NotBeNull();
        content.Rates.Should().NotBeEmpty();
    }

    private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));
            }

            var authHeader = Context.Request.Headers["Authorization"].ToString();
            var role = authHeader.Contains("Admin") ? "Admin" : "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim(ClaimTypes.Role, role)
            }; 

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    private class MockCurrencyService : ICurrencyService
    {
        public Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency)
        {
            return Task.FromResult(new ExchangeRateResponse
            {
                Base = baseCurrency,
                Rates = new Dictionary<string, decimal>
                {
                    ["USD"] = 1.05m,
                    ["GBP"] = 0.85m,
                    ["JPY"] = 130.50m
                }
            });
        }

        public Task<ExchangeRateResponse> ConvertCurrencyAsync(ConversionRequest request)
        {
            return Task.FromResult(new ExchangeRateResponse
            {
                Base = request.FromCurrency,
                Rates = new Dictionary<string, decimal>
                {
                    [request.ToCurrency] = request.Amount * 1.05m
                }
            });
        }

        public Task<HistoricalRatesResponse> GetHistoricalRatesAsync(HistoricalRatesRequest request)
        {
            return Task.FromResult(new HistoricalRatesResponse
            {
                Base = request.BaseCurrency,
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    ["2023-01-01"] = new() { ["USD"] = 1.10m, ["GBP"] = 0.90m },
                    ["2023-01-02"] = new() { ["USD"] = 1.09m, ["GBP"] = 0.89m }
                },
                StartDate = request.StartDate,
                EndDate = request.EndDate ?? request.StartDate
            });
        }

    }
}