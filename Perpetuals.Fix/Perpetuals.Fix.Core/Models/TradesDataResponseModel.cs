using System.Text.Json.Serialization;

namespace Perpetuals.Fix.Core.Models
{
    public class TradesDetailResponseModel
    {
        [JsonPropertyName("response")]
        public List<TradesDetail> Response { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }

    public class TradesDetail
    {
        [JsonPropertyName("charged_fee")]
        public string ChargedFee { get; set; }

        [JsonPropertyName("charged_fee_currency")]
        public string ChargedFeeCurrency { get; set; }

        [JsonPropertyName("charged_fee_rate")]
        public string ChargedFeeRate { get; set; }

        [JsonPropertyName("counterparty_order_uuid")]
        public string CounterpartyOrderUuid { get; set; }

        [JsonPropertyName("created")]
        public string Created { get; set; }

        [JsonPropertyName("created_timestamp")]
        public double CreatedTimestamp { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("exec_price")]
        public string ExecPrice { get; set; }

        [JsonPropertyName("exec_size")]
        public string ExecSize { get; set; }

        [JsonPropertyName("external_fill_id")]
        public string ExternalFillId { get; set; }

        [JsonPropertyName("fill_percent")]
        public string FillPercent { get; set; }

        [JsonPropertyName("is_margin_call_trade")]
        public bool IsMarginCallTrade { get; set; }

        [JsonPropertyName("leverage")]
        public int Leverage { get; set; }

        [JsonPropertyName("liquidity_pool")]
        public string LiquidityPool { get; set; }

        [JsonPropertyName("margin_blocked")]
        public string MarginBlocked { get; set; }

        [JsonPropertyName("margin_call_trigger")]
        public string MarginCallTrigger { get; set; }

        [JsonPropertyName("margin_called_percent")]
        public string MarginCalledPercent { get; set; }

        [JsonPropertyName("margin_called_vol")]
        public string MarginCalledVol { get; set; }

        [JsonPropertyName("matching_engine_client_id")]
        public string MatchingEngineClientId { get; set; }

        [JsonPropertyName("matching_engine_order_id")]
        public string MatchingEngineOrderId { get; set; }

        [JsonPropertyName("order_uuid")]
        public string OrderUuid { get; set; }

        [JsonPropertyName("passivity")]
        public string Passivity { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("trade_uuid")]
        public string TradeUuid { get; set; }
    }
}
