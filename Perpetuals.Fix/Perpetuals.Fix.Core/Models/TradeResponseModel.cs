using System.Text.Json.Serialization;

namespace Perpetuals.Fix.Core.Models;

public class TradeResponseModel
{
    [JsonPropertyName("response")]
    public TradeDetail Response { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

public class TradeDetail
{
    [JsonPropertyName("charged_fee")]
    public string ChargedFee { get; set; }

    [JsonPropertyName("charged_fee_currency")]
    public string ChargedFeeCurrency { get; set; }

    [JsonPropertyName("charged_fee_rate")]
    public string ChargedFeeRate { get; set; }

    [JsonPropertyName("created")]
    public string Created { get; set; }

    [JsonPropertyName("exec_price")]
    public string ExecPrice { get; set; }

    [JsonPropertyName("exec_size")]
    public string ExecSize { get; set; }

    [JsonPropertyName("external_fill_id")]
    public string ExternalFillId { get; set; }

    [JsonPropertyName("internal_cost")]
    public string InternalCost { get; set; }

    [JsonPropertyName("internal_cost_rate")]
    public string InternalCostRate { get; set; }

    [JsonPropertyName("liquidity_pool")]
    public string LiquidityPool { get; set; }

    [JsonPropertyName("order_uuid")]
    public string OrderUuid { get; set; }

    [JsonPropertyName("uuid")]
    public string TradeUuid { get; set; }
}
