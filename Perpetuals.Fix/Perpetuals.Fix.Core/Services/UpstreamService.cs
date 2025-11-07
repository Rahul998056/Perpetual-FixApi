using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Perpetuals.Fix.Core.Configuration;
using Perpetuals.Fix.Core.Models;
using Perpetuals.Fix.Core.Services.Interface;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Perpetuals.Fix.Core.Services;

public class UpstreamService : IUpstreamService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UpstreamService> _logger;
    private readonly UpstreamApiOptions _options;

    public UpstreamService(HttpClient httpClient, ILogger<UpstreamService> logger, IOptions<UpstreamApiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

     public async Task<UpstreamCreateOrderResponse?> PostOrder(object body, string authToken, string sessionCookie)
     {
        try
        {
            var url = $"{_options.BaseUrl}/{_options.CreateOrder}";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"sessionid={sessionCookie}");

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(body, options);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);


            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json1 = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Upstream failed: {json1}");
            }
            var model = JsonSerializer.Deserialize<UpstreamCreateOrderResponse>(json1, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting create market order from upstream");
            return null;
        }
     }


    public async Task<UpstreamCancelOrderResponse?> CancelOrder(object body, string authToken, string sessionCookie)
    {
        try
        {
            var url = string.Empty;
            var list = body.GetType().GetProperty("order_ids")?.GetValue(body) as List<string>;
            if (list.Count == 1)
            {
                url = $"{_options.BaseUrl}/{_options.CancelOrder}";
            }
            else
            {
                url = $"{_options.BaseUrl}/{_options.CancelAllOrders}";
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"sessionid={sessionCookie}");
            var json = JsonSerializer.Serialize(body);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json1 = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Upstream failed: {json1}");
            }
            var model = JsonSerializer.Deserialize<UpstreamCancelOrderResponse>(json1, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting create market order from upstream");
            return null;
        }
    }


    public async Task<object> GetMarketData(string authToken, string sessionCookie)
    {
        try
        {
            string url = $"{_options.BaseUrl}/{_options.Markets}";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"sessionid={sessionCookie}");

            var response = await _httpClient.GetAsync(url);


            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();

            var marketData = JsonSerializer.Deserialize<MarketsResponseModel>(json);

            if (marketData != null && marketData.Success)
            {
                foreach (var market in marketData.Response)
                {
                    string symbol = market.Key;
                    MarketDetail details = market.Value;

                    Console.WriteLine($"Market: {symbol}, Type: {details.MarketType}, Price Currency: {details.PriceCurrency}");
                }
            }
           
            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market data from upstream");
            return null;
        }
    }

    public async Task<object> GetMarketPositions(string authToken, string sessionCookie)
    {
        try
        {
            var url = $"{_options.BaseUrl}/{_options.Positions}";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"sessionid={sessionCookie}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            var marketPosition = JsonSerializer.Deserialize<MarketsPositionResponseModel>(json);

            if (marketPosition != null)
            {
                foreach (var market in marketPosition.Positions)
                {
                    string symbol = market.Key;
                    PositionDetail details = market.Value;

                    Console.WriteLine($"MarketPosition: {symbol}, AvgPrice: {details.AvgPrice}, TotalTradesCount: {details.TotalTradesCount}");
                }
            }

            return marketPosition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market data from upstream");
            return null;
        }
    }

    public async Task<TradeFillHistoryResponseModel?> GetTradeFillHistoryAsync(string authToken, string sessionCookie, int minutes, string market)
    {
        try
        {
            string requestUrl = $"{_options.BaseUrl}/{_options.MarketHistory}?minutes={minutes}&market={market}";

            _httpClient.DefaultRequestHeaders.Clear();

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {authToken}");
            request.Headers.Add("Session", sessionCookie);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<TradeFillHistoryResponseModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trade fill history from upstream");
            return null;
        }
    }
              

    public async Task<OrderMetaResponseModel?> GetOrderMetaUpstream(string authToken, string sessionCookie, string marketUuid)
    {
        try
        {
            string requestUrl = $"{_options.BaseUrl}/{_options.OrderMeta}?order_uuid={marketUuid}";
            
            _httpClient.DefaultRequestHeaders.Clear();

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {authToken}");
            request.Headers.Add("Session", sessionCookie);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<OrderMetaResponseModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trade fill history from upstream");
            return null;
        }
    }

    public async Task<OrdersResponseModel?> GetOrders(string authToken, string sessionCookie)
    {
        try
        {
            string requestUrl = $"{_options.BaseUrl}/{_options.Orders}";

            _httpClient.DefaultRequestHeaders.Clear();

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {authToken}");
            request.Headers.Add("Session", sessionCookie);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<OrdersResponseModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trade fill history from upstream");
            return null;
        }
    }

    public async Task<object> GetTradesData(string authToken, string sessionCookie)
    {
        try
        {
            var requestUrl = $"{_options.BaseUrl}/{_options.Trades}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {authToken}");
            request.Headers.Add("Session", sessionCookie);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonSerializer.Deserialize<TradesDetailResponseModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trade fill history from upstream");
            return null;
        }
    }

    public async Task<MarginDataResponse?> GetMarginDataAsync(string authToken, string sessionCookie)
    {
        try
        {
            var url = $"{_options.BaseUrl}/{_options.Margin}";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"sessionid={sessionCookie}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var rawDict = JsonSerializer.Deserialize<Dictionary<string, MarginTradeData>>(json, options);

            if (rawDict == null)
            {
                _logger.LogWarning("Deserialized margin data is null");
                return null;
            }

            var marginData = new MarginDataResponse();

            foreach (var kvp in rawDict)
            {
                kvp.Value.Symbol = kvp.Key;
                marginData.Response.Add(kvp.Value);
            }

            return marginData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting margin data from upstream");
            return null;
        }
    }

    public async Task<RecentDataResponse?> GetRecentDataAsync(string authToken, string sessionCookie, int minutes, string market)
    {
        try
        {
            string requestUrl = $"{_options.BaseUrl}/{_options.MarketRecent}?minutes={minutes}&market={market}";

            _httpClient.DefaultRequestHeaders.Clear();

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {authToken}");
            request.Headers.Add("Session", sessionCookie);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upstream failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonSerializer.Deserialize<RecentDataResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trade fill history from upstream");
            return null;
        }
    }

    public async Task<TradeResponseModel?> GetTradeDataAsyn(string authToken, string sessionCookie, string tradeUuid)
    {
        try
        {
            string requestUrl = $"{_options.BaseUrl}/{_options.TradeMeta}?trade_uuid={tradeUuid}";
            _httpClient.DefaultRequestHeaders.Clear();

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {authToken}");
            request.Headers.Add("Session", sessionCookie);

            var response = await _httpClient.SendAsync(request);
            var test = response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                foreach (var header in response.Headers)
                {
                    Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }

                _logger.LogError("Upstream request failed with status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning("MTF trade upstream returned empty response body.");
                return null;
            }

            var model = JsonSerializer.Deserialize<TradeResponseModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (model == null)
            {
                _logger.LogError("Deserialization of MTF trade response returned null. Raw JSON: {Json}", json);
                return null;
            }

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching MTF trade data from upstream.");
            return null;
        }
    }  

}
