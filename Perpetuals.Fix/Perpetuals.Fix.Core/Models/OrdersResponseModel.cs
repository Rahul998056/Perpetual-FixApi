using System.Text.Json.Serialization;

namespace Perpetuals.Fix.Core.Models;

public class OrdersResponseModel
{
    [JsonPropertyName("response")]
    public List<OrderResponse> Response { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

public class OrderResponse
{
    [JsonPropertyName("base_currency")]
    public string BaseCurrency { get; set; }

    [JsonPropertyName("book_or_cancel")]
    public bool BookOrCancel { get; set; }

    [JsonPropertyName("cancel_reason")]
    public string CancelReason { get; set; }

    [JsonPropertyName("canceled")]
    public bool? Canceled { get; set; }

    [JsonPropertyName("client_ref")]
    public string ClientRef { get; set; }

    [JsonPropertyName("created")]
    public string Created { get; set; }

    [JsonPropertyName("created_timestamp")]
    public double CreatedTimestamp { get; set; }

    [JsonPropertyName("direction")]
    public string Direction { get; set; }

    [JsonPropertyName("em_uuid")]
    public string EmUuid { get; set; }

    [JsonPropertyName("engine_id")]
    public string EngineId { get; set; }

    [JsonPropertyName("estim_market_value")]
    public string EstimMarketValue { get; set; }

    [JsonPropertyName("exec_vol")]
    public string ExecVol { get; set; }

    [JsonPropertyName("external_ref")]
    public string ExternalRef { get; set; }

    [JsonPropertyName("fill_or_kill")]
    public bool FillOrKill { get; set; }

    [JsonPropertyName("fill_percent")]
    public string FillPercent { get; set; }

    [JsonPropertyName("immediate_or_cancel")]
    public bool ImmediateOrCancel { get; set; }

    [JsonPropertyName("is_triggered")]
    public bool IsTriggered { get; set; }

    [JsonPropertyName("leverage")]
    public string Leverage { get; set; }

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; }

    [JsonPropertyName("liquidity_pool")]
    public string LiquidityPool { get; set; }

    [JsonPropertyName("margin_blocked")]
    public string MarginBlocked { get; set; }

    [JsonPropertyName("margin_call_trigger")]
    public string MarginCallTrigger { get; set; }

    [JsonPropertyName("market")]
    public string Market { get; set; }

    [JsonPropertyName("on_stop_loss")]
    public string OnStopLoss { get; set; }

    [JsonPropertyName("open_vol")]
    public string OpenVol { get; set; }

    [JsonPropertyName("order_group")]
    public string OrderGroup { get; set; }

    [JsonPropertyName("order_sequence")]
    public string OrderSequence { get; set; }

    [JsonPropertyName("order_size")]
    public string OrderSize { get; set; }

    [JsonPropertyName("order_type")]
    public string OrderType { get; set; }

    [JsonPropertyName("order_uuid")]
    public string OrderUuid { get; set; }

    [JsonPropertyName("quote_currency")]
    public string QuoteCurrency { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("tip_quantity")]
    public string TipQuantity { get; set; }

    [JsonPropertyName("trades")]
    public object Trades { get; set; } // Adjust if structure is known

    [JsonPropertyName("updated")]
    public string Updated { get; set; }

    [JsonPropertyName("vault_uuid")]
    public string VaultUuid { get; set; }
}
