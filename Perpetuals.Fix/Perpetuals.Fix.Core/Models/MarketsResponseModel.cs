using System.Text.Json.Serialization;

namespace Perpetuals.Fix.Core.Models;

public class MarketsResponseModel
{
    [JsonPropertyName("response")]
    public Dictionary<string, MarketDetail> Response { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

public class MarketDetail
{
    [JsonPropertyName("contract_size")]
    public decimal ContractSize { get; set; }

    [JsonPropertyName("expiration_date")]
    public string? ExpirationDate { get; set; }

    [JsonPropertyName("fill_rate")]
    public decimal FillRate { get; set; }

    [JsonPropertyName("instrument_isin")]
    public string InstrumentIsin { get; set; }

    [JsonPropertyName("is_cancel_only")]
    public bool IsCancelOnly { get; set; }

    [JsonPropertyName("is_maker_only")]
    public bool IsMakerOnly { get; set; }

    [JsonPropertyName("is_suspended")]
    public bool IsSuspended { get; set; }

    [JsonPropertyName("market_id")]
    public int MarketId { get; set; }

    [JsonPropertyName("market_symbol")]
    public string MarketSymbol { get; set; }

    [JsonPropertyName("market_type")]
    public string MarketType { get; set; }

    [JsonPropertyName("market_uuid")]
    public string MarketUuid { get; set; }

    [JsonPropertyName("minimum_execution_quantity")]
    public decimal MinimumExecutionQuantity { get; set; }

    [JsonPropertyName("minimum_quantity")]
    public decimal MinimumQuantity { get; set; }

    [JsonPropertyName("operating_mic")]
    public string OperatingMic { get; set; }

    [JsonPropertyName("price_currency")]
    public string PriceCurrency { get; set; }

    [JsonPropertyName("segment_mic")]
    public string SegmentMic { get; set; }
    public decimal PositionSize { get; internal set; }
    public decimal AvgPrice { get; internal set; }
    public decimal PositionVolume { get; internal set; }
    public decimal TotalTradedLots { get; internal set; }
    public decimal TotalTradedVolume { get; internal set; }
    public int TotalTradesCount { get; internal set; }
    public string OrderUuid { get; internal set; }
    public int Leverage { get; internal set; }
}
