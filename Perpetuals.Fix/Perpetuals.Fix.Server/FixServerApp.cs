using Microsoft.Extensions.DependencyInjection;
using Perpetuals.Fix.Core.Models;
using Perpetuals.Fix.Core.Services.Interface;
using QuickFix;
using QuickFix.Fields;


namespace Perpetuals.Fix.Server;

public class FixServerApp : MessageCracker, IApplication
{
    private readonly IServiceScopeFactory _scopeFactory;

    int orderID = 0;
    int execID = 0;
    string authToken = string.Empty;
    string sessionCookie = string.Empty;

    public FixServerApp(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private string GenOrderID() => (++orderID).ToString();
    private string GenExecID() => (++execID).ToString();

    public void FromApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("IN: " + message);

        var messageType = message.Header.GetString(Tags.MsgType);

        if (messageType == "BI")
        {
            CallEndpoint(message, sessionID);
        }
        else
        {
            Crack(message, sessionID);
        }
    }

    public void ToApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("OUT: " + message);
    }

    public void FromAdmin(Message message, SessionID sessionID)
    {
        if (message.Header.GetString(QuickFix.Fields.Tags.MsgType) == QuickFix.Fields.MsgType.LOGON)
        {
            Console.WriteLine("Received LOGON message from client.");

            bool hasAuthToken = message.IsSetField(6042);
            bool hasSessionKey = message.IsSetField(6043);

            authToken = hasAuthToken ? message.GetString(6042) : "(missing)";
            sessionCookie = hasSessionKey ? message.GetString(6043) : "(missing)";
        }
    }

    public void OnCreate(SessionID sessionID) => Console.WriteLine("Session created: " + sessionID);
    public void OnLogout(SessionID sessionID) => Console.WriteLine("Logout: " + sessionID);
    public void OnLogon(SessionID sessionID) => Console.WriteLine("Logon: " + sessionID);
    public void ToAdmin(Message message, SessionID sessionID) { }

    public void CallEndpoint(Message message, SessionID sessionID)
    {
        switch (message.GetInt(6000))
        {
            case 7: MarketDataEndPoint(message, sessionID); break;
            case 8: MarketPositionUpstreamEndPoint(message, sessionID); break;
            case 9: HistoricalDataEndPoint(message, sessionID); break;
            case 10: TradeDataEndpoint(message, sessionID); break;
            case 11: TradesDataEndpoint(message, sessionID); break;
            case 12: MargineDataEndPoint(message, sessionID); break;
            case 13: RecenetDataEndpoint(message, sessionID); break;
            case 14: OrderMetaEndPoint(message, sessionID); break;
            case 15: OrdersEndPoint(message, sessionID); break;             

            default: Console.WriteLine($"Unknown endpoint type: {message.GetInt(6000)}"); break;
        }
    }

    public void MarketDataEndPoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();

        var response = upstream.GetMarketData(authToken, sessionCookie).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildMarketsResponseFix(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, sessionID);
        }
    }
    public void MarketPositionUpstreamEndPoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();

        var response = upstream.GetMarketPositions(authToken, sessionCookie).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildMarketsPositionFix(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, sessionID);
        }
    }

    public void HistoricalDataEndPoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();

        int min = message.GetInt(6039);
        string market = message.GetString(6002);

        var response = upstream.GetTradeFillHistoryAsync(authToken, sessionCookie, min, market).Result;

        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.TradeFillHistoryFix(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, sessionID);
        }
    }

    public async Task TradeDataEndpoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();
        string MarketUuid = message.GetString(6004);

        var response = upstream.GetTradeDataAsyn(authToken, sessionCookie, MarketUuid).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildTradeDataFix(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, sessionID);
        }
    }

    public void TradesDataEndpoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();

        var response = upstream.GetTradesData(authToken, sessionCookie).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildTradesFixMessages(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, sessionID);
        }
    }

    private void MargineDataEndPoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();

        var response = upstream.GetMarginDataAsync(authToken, sessionCookie).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildMarginDataFix(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, sessionID);
        }
    }

    public void RecenetDataEndpoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();

        int min = message.GetInt(6039);
        string market = message.GetString(6002);

        var response = upstream.GetRecentDataAsync(authToken, sessionCookie, min, market).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildRecentDataFix(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, sessionID);
        }
    }

    public void OrderMetaEndPoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();

        string MarketUuid = message.GetString(6004);

        var response = upstream.GetOrderMetaUpstream(authToken, sessionCookie, MarketUuid).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildOrderMetaFix(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {

            Session.SendToTarget(Message, sessionID);
        }
    }

    public void OrdersEndPoint(Message message, SessionID sessionID)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();

        var response = upstream.GetOrders(authToken, sessionCookie).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildOrdersFix(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, sessionID);
        }
    }
  
    public void OnMessage(QuickFix.FIX44.OrderCancelRequest n, SessionID s)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();
        var orderIdString = n.OrderID.getValue();
        var orderIds = orderIdString.Split(',').ToList();

        var request = new UpstreamCancelOrderRequest
        {
            order_ids = orderIds
        };
        var response = upstream.CancelOrder(request, authToken, sessionCookie).Result;
        List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildCancelOrderFixMessages(response);
        foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
        {
            Session.SendToTarget(Message, s);
        }
    }

    [Obsolete]
    public void OnMessage(QuickFix.FIX44.NewOrderSingle n, SessionID s)
    {
        using var scope = _scopeFactory.CreateScope();
        var upstream = scope.ServiceProvider.GetRequiredService<IUpstreamService>();
        var fixBuilder = scope.ServiceProvider.GetRequiredService<IFixServices>();


        var type1 = n.OrdType.getValue();
        char sideChar = n.Side.getValue();
        char typeChar = n.OrdType.getValue();

        string type = typeChar == OrdType.MARKET ? "market" :
                    typeChar == OrdType.LIMIT ? "limit" :
                    typeChar == OrdType.STOP ? "stop" :
                    typeChar == OrdType.STOP_LIMIT ? "stop_limit" :
                    typeChar == OrdType.LIMIT_OR_BETTER ? "ICEBERG" :
                    "unknown";

        if (type == "limit")
        {
            var market = n.Symbol.getValue();
            var quantity = n.OrderQty.getValue();
            var limit_price = n.Price.getValue();
            var side1 = n.Side.getValue();

            string side = sideChar == Side.BUY ? "buy" :
                          sideChar == Side.SELL ? "sell" :
                          "unknown";

            var request = new UpstreamCreateOrderRequest
            {
                Market = market,
                Quantity = quantity.ToString(),
                Side = side.ToString(),
                LimitPrice = limit_price,
                Type = type.ToString()
            };
            var response = upstream.PostOrder(request, authToken, sessionCookie).Result;

            List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildCreateOrderFixMessages(response);

            foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
            {
                Session.SendToTarget(Message, s);
            }

        }
        if (type == "market")
        {
            var market = n.Symbol.getValue();
            var quantity = n.OrderQty.getValue();

            var side1 = n.Side.getValue();
            string side = sideChar == Side.BUY ? "buy" :
                          sideChar == Side.SELL ? "sell" :
                          "unknown";

            var request = new UpstreamCreateOrderRequest
            {
                Market = market,
                Quantity = quantity.ToString(),
                Side = side,
                Type = type
            };
            var response = upstream.PostOrder(request, authToken, sessionCookie).Result;
            List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildCreateOrderFixMessages(response);
            foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
            {
                Session.SendToTarget(Message, s);
            }
        }
        if (type == "stop")
        {
            var market = n.Symbol.getValue();
            var quantity = n.OrderQty.getValue();

            var side1 = n.Side.getValue();
            string side = sideChar == Side.BUY ? "buy" :
                          sideChar == Side.SELL ? "sell" :
                          "unknown";
            var stopPrice = n.StopPx.getValue();

            var request = new UpstreamCreateOrderRequest
            {
                Market = market,
                Quantity = quantity.ToString(),
                Side = side,
                StopPrice = stopPrice,
                Type = type
            };
            var response = upstream.PostOrder(request, authToken, sessionCookie).Result;
            List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildCreateOrderFixMessages(response);
            foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
            {
                Session.SendToTarget(Message, s);
            }
        }
        if (type == "stop_limit")
        {
            var market = n.Symbol.getValue();
            var quantity = n.OrderQty.getValue();

            var side1 = n.Side.getValue();
            string side = sideChar == Side.BUY ? "buy" :
                          sideChar == Side.SELL ? "sell" :
                          "unknown";
            var stopPrice = n.StopPx.getValue();
            var limitPrice = n.Price.getValue();

            var request = new UpstreamCreateOrderRequest
            {
                Market = market,
                Quantity = quantity.ToString(),
                Side = side,
                LimitPrice = limitPrice,
                StopPrice = stopPrice,
                Type = type
            };
            var response = upstream.PostOrder(request, authToken, sessionCookie).Result;
            List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildCreateOrderFixMessages(response);
            foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
            {
                Session.SendToTarget(Message, s);
            }
        }

        if (type == "ICEBERG")
        {
            var market = n.Symbol.getValue();
            var quantity = n.OrderQty.getValue();

            var side1 = n.Side.getValue();
            string side = sideChar == Side.BUY ? "buy" :
                          sideChar == Side.SELL ? "sell" :
                          "unknown";
            var limitPrice = n.Price.getValue();
            var tipquantity = n.MinQty.getValue();
            var request = new UpstreamCreateOrderRequest
            {
                Market = market,
                Quantity = quantity.ToString(),
                Side = side,
                TipQuantity = tipquantity,
                LimitPrice = limitPrice,
                Type = type
            };
            var response = upstream.PostOrder(request, authToken, sessionCookie).Result;
            List<QuickFix.FIX44.ExecutionReport> fixMessage = fixBuilder.BuildCreateOrderFixMessages(response);
            foreach (QuickFix.FIX44.ExecutionReport Message in fixMessage)
            {
                Session.SendToTarget(Message, s);
            }
        }

    }

    public void OnMessage1(QuickFix.FIX44.NewOrderSingle n, SessionID s)
    {
        var price = new Price(10); 

        var report = new QuickFix.FIX44.ExecutionReport(
            new OrderID(GenOrderID()),
            new ExecID(GenExecID()),
            new ExecType(ExecType.FILL),
            new OrdStatus(OrdStatus.FILLED),
            n.Symbol,
            n.Side,
            new LeavesQty(0),
            new CumQty(n.OrderQty.getValue()),
            new AvgPx(price.getValue()));

        report.Set(n.ClOrdID);
        report.Set(n.Symbol);
        report.Set(n.OrderQty);
        report.Set(new LastQty(n.OrderQty.getValue()));
        report.Set(new LastPx(price.getValue()));

        try
        {
            Console.WriteLine("Sending ExecutionReport to client...");

            if (!Session.LookupSession(s)?.IsLoggedOn ?? false)
            {
                Console.WriteLine("Server session not logged on");
            }
            try
            {
                Console.WriteLine("Sending ExecutionReport to client...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send message: " + ex);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to send message: " + ex);
        }
    }

}
