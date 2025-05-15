using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverterAPI.Models; 

public class ExchangeRateResponse
{
    public decimal Amount { get; set; }
    public string Base { get; set; }
    public DateTime Date { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}

public class HistoricalRatesResponse
{
    public decimal Amount { get; set; }
    public string Base { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
    public PaginationMetadata Metadata { get; set; }
}

public class HistoricalRatesAPIResponse
{
    public decimal Amount { get; set; }
    public string Base { get; set; }
    
   [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime EndDate { get; set; }

    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
}

public class ConversionRequest
{
    [Required]
    public string FromCurrency { get; set; }
    
    [Required]
    public string ToCurrency { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}

public class HistoricalRatesRequest
{
    [Required]
    public string BaseCurrency { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
    
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
}

 public class PaginationMetadata
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }