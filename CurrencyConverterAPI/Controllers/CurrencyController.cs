using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using CurrencyConverterAPI.Services.Interfaces;
using CurrencyConverterAPI.Models;
using CurrencyConverterAPI.Attributes;


namespace CurrencyConverterAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CurrencyController> _logger;

    public CurrencyController(
        ICurrencyService currencyService,
        ILogger<CurrencyController> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    [HttpGet("latest")]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeRoles(Roles.Admin, Roles.User)] 
    public async Task<IActionResult> GetLatestRates([FromQuery] string baseCurrency = "EUR")
    {
        try
        {
            var result = await _currencyService.GetLatestRatesAsync(baseCurrency);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid currency requested");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest rates");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    [HttpPost("convert")]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeRoles(Roles.Admin, Roles.User)] 
    public async Task<IActionResult> ConvertCurrency([FromBody] ConversionRequest request)
    {
        try
        {
            var result = await _currencyService.ConvertCurrencyAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid currency conversion requested");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    [HttpGet("historical")]
    [ProducesResponseType(typeof(HistoricalRatesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeRoles(Roles.Admin)] 
    public async Task<IActionResult> GetHistoricalRates([FromQuery] HistoricalRatesRequest request)
    {
        try
        {
            var result = await _currencyService.GetHistoricalRatesAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid historical rates request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical rates");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
}