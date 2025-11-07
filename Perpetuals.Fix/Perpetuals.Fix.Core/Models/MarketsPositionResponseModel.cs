using System.Text.Json;
using System.Text.Json.Serialization;

namespace Perpetuals.Fix.Core.Models;

public class MarketsPositionResponseModel
{

    [JsonExtensionData]
    public Dictionary<string, JsonElement> RawSymbols { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonIgnore]
    public Dictionary<string, PositionDetail> Positions =>
        RawSymbols?.ToDictionary(
            kvp => kvp.Key,
            kvp => JsonSerializer.Deserialize<PositionDetail>(kvp.Value.GetRawText())!
        ) ?? new();
}

public class PositionDetail
{
    [JsonPropertyName("avg_price")]
    public string AvgPrice { get; set; }

    [JsonPropertyName("open_order_count")]
    public int OpenOrderCount { get; set; }

    [JsonPropertyName("open_order_volume")]
    public string OpenOrderVolume { get; set; }

    [JsonPropertyName("position_size")]
    public string PositionSize { get; set; }

    [JsonPropertyName("position_volume")]
    public string PositionVolume { get; set; }

    [JsonPropertyName("total_traded_lots")]
    public string TotalTradedLots { get; set; }

    [JsonPropertyName("total_traded_volume")]
    public string TotalTradedVolume { get; set; }

    [JsonPropertyName("total_trades_count")]
    public int TotalTradesCount { get; set; }

    [JsonPropertyName("market_symbol")]
    public string MarketSymbol { get; set; }

    [JsonPropertyName("minimum_execution_quantity")]
    public decimal MinimumExecutionQuantity { get; set; }
}







