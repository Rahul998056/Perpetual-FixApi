using System.Text.Json.Serialization;

namespace Perpetuals.Fix.Core.Models;

public class UpstreamCreateOrderRequest
{

    [JsonPropertyName("market")]
    public string? Market { get; set; }
    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }
    [JsonPropertyName("tip_quantity")]
    public decimal? TipQuantity { get; set; }
    [JsonPropertyName("side")]
    public string? Side { get; set; }
    [JsonPropertyName("limit_price")]
    public decimal? LimitPrice { get; set; } 
    
    [JsonPropertyName("stop_price")]
    public decimal? StopPrice { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("leverage")]
    public int? Leverage { get; set; }
    [JsonPropertyName("reference")]
    public string? Reference { get; set; }
}
