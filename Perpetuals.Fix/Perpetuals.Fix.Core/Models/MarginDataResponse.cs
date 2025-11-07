using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Perpetuals.Fix.Core.Models
{
    public class MarginDataResponse
    {
        public List<MarginTradeData> Response { get; set; } = new();
    }

    public class MarginTradeData
    {
        [JsonIgnore]
        public string? Symbol { get; set; }

        [JsonPropertyName("avg_price")]
        public string? AvgPrice { get; set; }

        [JsonPropertyName("position_size")]
        public string? PositionSize { get; set; }

        [JsonPropertyName("position_volume")]
        public string? PositionVolume { get; set; }

        [JsonPropertyName("total_traded_lots")]
        public string? TotalTradedLots { get; set; }

        [JsonPropertyName("total_traded_volume")]
        public string? TotalTradedVolume { get; set; }

        [JsonPropertyName("total_trades_count")]
        public int TotalTradesCount { get; set; }
    }

   
}
