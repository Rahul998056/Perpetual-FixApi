using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace Perpetuals.Fix.Client;

public class FixClientApp : MessageCracker, IApplication
{
    private Session? _session = null;
    public IInitiator? MyInitiator = null;

    public void OnCreate(SessionID sessionId)
    {
        _session = Session.LookupSession(sessionId);
        if (_session == null)
            throw new ApplicationException("Session not found");
    }

    public void OnLogon(SessionID sessionId) => Console.WriteLine("Logon - " + sessionId);
    public void OnLogout(SessionID sessionId) => Console.WriteLine("Logout - " + sessionId);

    public void FromAdmin(Message message, SessionID sessionID)
    {
        if (message.Header.GetString(Tags.MsgType) == MsgType.HEARTBEAT)
        {
            Console.WriteLine("Heartbeat received from server");
        }
    }

    public void ToAdmin(Message message, SessionID sessionId)
    {
        if (message.Header.GetString(Tags.MsgType) == MsgType.HEARTBEAT)
        {
            Console.WriteLine("Heartbeat sent to server");
        }

        if (message.Header.GetString(Tags.MsgType) == MsgType.LOGON)
        {
            message.SetField(new StringField(6042, "OGFxRUJQZ05VeEdPN2R4WTY3RzA2Wm1D"));
            message.SetField(new StringField(6043, ".eJylkMFuwjAMht8lZ9og7cY7TOK4W-Smbhpwk8xxYGzauy-AWrQKaYfdos_O99v-UrZkiZMZ4ORtDGoXCtFmphRdNNl_otq9bLcLThwPaMUEmGpF7ZETSgHKaqOGyBaNJY9BjFwSzkofevwwCdyKiBdaocI0gxRZgO5EjSIp77S-wzYtua2NU80WcGaCUCPY-H5WCPJEF1PKA51HL0jQ4dW6f32rf89kOmd64GNFOguIt9pPTndgj45jCX3WA8EpFsaeUTg23bmp72Kxbw_JLRLybpT_WW6HfzLMY3D92F5fuxsbKXJza2hTWHtMfi_Ay53n0rNR_8zoqC6zyriJfoV8_wANhN7_.aGVQtQ.NoD3BdJ_uvHxmQjKxJuzGUEhSa4"));
            Console.WriteLine("Sent Username and Password in Logon.");
        }        
    }

    public void FromApp(Message message, SessionID sessionID)
    {
        try
        {
            var messageType = message.Header.GetString(Tags.MsgType);
            if (messageType == "BI")
            {
                Console.WriteLine($"message:{message}"); 
            }
            else
            {
                Crack(message, sessionID);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while cracking message: " + ex);
        }
    }

    public void ToApp(Message message, SessionID sessionId)
    {
        try
        {
            bool possDupFlag = false;
            if (message.Header.IsSetField(Tags.PossDupFlag))
                possDupFlag = message.Header.GetBoolean(Tags.PossDupFlag);

            if (possDupFlag)
                throw new DoNotSend();
        }
        catch (FieldNotFoundException) { }
         
        Console.WriteLine("\nOUT: " + message.ConstructString());
    }

    private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
    {
        var clOrdID = new ClOrdID("1");
        var symbol = new Symbol("btcusd");
        var side = new Side(Side.BUY);
        var ordType = new OrdType(OrdType.MARKET);
        var timeInForce = new TimeInForce(TimeInForce.DAY);
        var orderQty = new OrderQty(1);

        var newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
            clOrdID,
            symbol,
            side,
            new TransactTime(DateTime.UtcNow),
            ordType);

        newOrderSingle.Set(new HandlInst('1'));
        newOrderSingle.Set(orderQty);
        newOrderSingle.Set(timeInForce);


        return newOrderSingle;
    }

    public void OnMessage(QuickFix.FIX44.OrderCancelReject m, SessionID s)
   {
        Console.WriteLine(m);
   }

    public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s)
    {
        Console.WriteLine("ExecutionReport received:");
        Console.WriteLine(m);
    }

    public void Run()
    {
        if (MyInitiator == null)
            throw new ApplicationException("Initiator is not set");

        Console.WriteLine("Waiting for session logon...");
        while (_session == null || !_session.IsLoggedOn)
        {
            Thread.Sleep(5000);
        }

        Console.WriteLine("Session is active. Press Ctrl+C or close the console to stop the client.");


        while (true) 
        {
            Message message = null;

             message = CreateMarketOrder();
            // message = CreateLimitOrder();
            // message = CreateStopOrder();
            // message = CreateStopLimitOrder();
            // message = CreateIcebergOrder();
            // message = CancelOrder();
            // message = MarketData();
            // message = MarketPosition();
            // message = TradeFillHistory(); 
            // message = TradeData();
            // message = TradesData();
            // message = MarginData();
            // message = RecentData();
            // message = OrderMeta();
            // message = Orders();

            if (message != null)
            {
                NewOrderSingle m = QueryNewOrderSingle44();
                SendMessage(message);
                Console.WriteLine("Message sent. Waiting for response...");
            }
            else
            {
                Console.WriteLine("No message selected. Please uncomment one method.");
            }
            
            Console.ReadLine();
        }
    }

    private void SendMessage(Message m)
    {
        if (_session != null)
            _session.Send(m);
        else
            Console.WriteLine("Can't send message: session not created.");
    }

    private Message MarketData()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 7));

        return messageEndpoint; 
    }

    private Message MarketPosition()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 8));

        return messageEndpoint;
    }

    private Message TradeFillHistory()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 9));
        messageEndpoint.SetField(new StringField(6002, "btc_usd_perp"));
        messageEndpoint.SetField(new IntField(6039, 1));

        return messageEndpoint;
    }

    private Message TradeData()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 10));
        messageEndpoint.SetField(new StringField(6004, "cfedc8d73fe242cb92ecec54bb8934db"));

        return messageEndpoint;
    }

    private Message TradesData()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 11));

        return messageEndpoint;
    }

    private Message MarginData()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 12));

        return messageEndpoint;
    }

    private Message RecentData()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 13));
        messageEndpoint.SetField(new StringField(6002, "btc_usd_perp"));
        messageEndpoint.SetField(new IntField(6039, 1));
        return messageEndpoint;
    }

    private Message OrderMeta()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 14));
        messageEndpoint.SetField(new StringField(6004, "8932ba7df37c40339e9748175e7cb259"));

        return messageEndpoint;
    }

    private Message Orders()
    {
        var messageEndpoint = new QuickFix.Message();
        messageEndpoint.Header.SetField(new QuickFix.Fields.MsgType("BI"));
        messageEndpoint.SetField(new IntField(6000, 15));

        return messageEndpoint;
    }

    private QuickFix.FIX44.OrderCancelRequest CancelOrder()
    {
        var cancelMsg = new QuickFix.FIX44.OrderCancelRequest(
      new OrigClOrdID("1"),
      new ClOrdID(Guid.NewGuid().ToString()),
      new Symbol("BTCUSD"),
      new Side(Side.BUY),
      new TransactTime(DateTime.UtcNow)
        );

        cancelMsg.SetField(new OrderID("afccbe8abf7f41faa0a1b08563a155539617"));
        cancelMsg.SetField(new StringField(58, "CANCEL_ORDER"));
        return cancelMsg;
    }

    private QuickFix.FIX44.NewOrderSingle CreateMarketOrder()
    {
        var clOrdID = new ClOrdID("1");
        var symbol = new Symbol("BTC_USD_PERP");
        var side = new Side(Side.BUY);
        var transactTime = new TransactTime(DateTime.UtcNow);
        var ordType = new OrdType(OrdType.MARKET); 
        var orderQty = new OrderQty(0.5m); 

        var order = new QuickFix.FIX44.NewOrderSingle(clOrdID, symbol, side, transactTime, ordType);
        order.Set(orderQty);
        order.Set(new TimeInForce(TimeInForce.DAY));
        order.Set(new HandlInst('1'));       

        return order;
    }

    private QuickFix.FIX44.NewOrderSingle CreateLimitOrder()
    {
        var clOrdID = new ClOrdID("1");
        var symbol = new Symbol("BTC_USD_PERP");
        var side = new Side(Side.BUY);
        var transactTime = new TransactTime(DateTime.UtcNow);
        var ordType = new OrdType(OrdType.LIMIT);
        var orderQty = new OrderQty(1);
        var price = new Price(1.390m);

        var order = new QuickFix.FIX44.NewOrderSingle(clOrdID, symbol, side, transactTime, ordType);
        order.Set(orderQty);
        order.Set(price);
        order.Set(new TimeInForce(TimeInForce.DAY));
        order.Set(new HandlInst('1'));
        order.SetField(new IntField(6031, 1));
        order.SetField(new StringField(6034, "asdasd"));

        return order;
    }

    private QuickFix.FIX44.NewOrderSingle CreateStopOrder()
    {
        var clOrdID = new ClOrdID("1");
        var symbol = new Symbol("BTC_USD_PERP");
        var side = new Side(Side.BUY);
        var transactTime = new TransactTime(DateTime.UtcNow);
        var ordType = new OrdType(OrdType.STOP); 
        var stopPrice= new StopPx(1500m);
        var orderQty = new OrderQty(0.5m); 

        var order = new QuickFix.FIX44.NewOrderSingle(clOrdID, symbol, side, transactTime, ordType);
        order.Set(stopPrice);
        order.Set(orderQty);
        order.Set(new TimeInForce(TimeInForce.DAY));
        order.Set(new HandlInst('1'));

        return order;
    }
         
    private QuickFix.FIX44.NewOrderSingle CreateStopLimitOrder()
    {
        var clOrdID = new ClOrdID("1");
        var symbol = new Symbol("BTC_USD_PERP");
        var side = new Side(Side.BUY);
        var transactTime = new TransactTime(DateTime.UtcNow);
        var ordType = new OrdType(OrdType.STOP_LIMIT);
        var stopPrice = new StopPx(100m);
        var limitPrice = new Price(101m);
        var orderQty = new OrderQty(0.5m);

        var order = new QuickFix.FIX44.NewOrderSingle(clOrdID, symbol, side, transactTime, ordType);
        order.Set(stopPrice);
        order.Set(orderQty);
        order.Set(new TimeInForce(TimeInForce.DAY));
        order.Set(limitPrice);
        order.Set(new HandlInst('1'));

        return order;
    }

    private QuickFix.FIX44.NewOrderSingle CreateIcebergOrder()
    {
        var clOrdID = new ClOrdID("1");
        var symbol = new Symbol("BTC_USD_PERP");
        var side = new Side(Side.BUY);
        var transactTime = new TransactTime(DateTime.UtcNow);
        var ordType = new OrdType(OrdType.LIMIT_OR_BETTER);
        var tipQuantity = new MinQty(0.1m); 
        var limitPrice = new Price(101m);
        var orderQty = new OrderQty(0.5m);

        var order = new QuickFix.FIX44.NewOrderSingle(clOrdID, symbol, side, transactTime, ordType);
        order.Set(tipQuantity);
        order.Set(orderQty);
        order.Set(new TimeInForce(TimeInForce.DAY));
        order.Set(limitPrice);
        order.Set(new HandlInst('1'));

        return order;
    }
    
}

