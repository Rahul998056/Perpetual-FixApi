using Perpetuals.Fix.Core.Models;

namespace Perpetuals.Fix.Core.Services.Interface;

public interface IUpstreamService
{
    Task<UpstreamCreateOrderResponse?> PostOrder(object body, string authToken, string sessionCookie);
    Task<UpstreamCancelOrderResponse> CancelOrder(object body, string authToken, string sessionCookie);
    Task<object> GetTradesData(string authToken, string sessionCookie);
    Task<object> GetMarketData(string authToken, string sessionCookie);
    Task<object> GetMarketPositions(string authToken, string sessionCookie);
    Task<OrdersResponseModel?> GetOrders(string authToken, string sessionCookie);
    Task<MarginDataResponse?> GetMarginDataAsync(string authToken, string sessionCookie);
    Task<OrderMetaResponseModel?> GetOrderMetaUpstream(string authToken, string sessionCookie, string marketUuid);
    Task<RecentDataResponse?> GetRecentDataAsync(string authToken, string sessionCookie, int minutes, string market);
    Task<TradeResponseModel?> GetTradeDataAsyn(string authToken, string sessionCookie, string tradeUuid);
    Task<TradeFillHistoryResponseModel?> GetTradeFillHistoryAsync(string authToken, string sessionCookie, int minutes, string market);
}
