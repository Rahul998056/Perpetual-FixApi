namespace Perpetuals.Fix.Core.Configuration;

public class UpstreamApiOptions
{
    public string BaseUrl { get; set; }
    public string CreateOrder { get; set; }
    public string CancelOrder { get; set; }
    public string CancelAllOrders { get; set; }
    public string Markets { get; set; }
    public string Positions { get; set; }
    public string MarketHistory { get; set; }
    public string OrderMeta { get; set; }
    public string Orders { get; set; }
    public string Trades { get; set; }
    public string Margin { get; set; }
    public string MarketRecent { get; set; }
    public string TradeMeta { get; set; }
}
