using Microsoft.Extensions.Configuration;
using Perpetuals.Fix.Core.Models;
using Perpetuals.Fix.Core.Services.Interface;
using Perpetuals.Fix.Core.Utils;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Perpetuals.Fix.Core.Services;
public class FixServices : IFixServices
{
    private readonly string _senderCompID;
    private readonly string _targetCompID;
    private readonly int _msgSeqNum;

    public FixServices(IConfiguration configuration)
    {
        _senderCompID = configuration["Fix:SenderCompID"] ?? "SENDER";
        _targetCompID = configuration["Fix:TargetCompID"] ?? "TARGET";
        _msgSeqNum = int.Parse(configuration["Fix:MsgSeqNum"] ?? "1");
        Console.WriteLine($"SenderCompID: {_senderCompID}, TargetCompID: {_targetCompID}, MsgSeqNum: {_msgSeqNum}");
    }

    public List<ExecutionReport> BuildCreateOrderFixMessages(object model)
    {
        var response = model as UpstreamCreateOrderResponse;
        if (response == null)
            throw new InvalidCastException("Invalid model type for BuildFixMessages");

        var fixMessages = new List<ExecutionReport>();

        try
        {
            if (response?.Response?.Payload?.Record == null || response.Response.Payload.Record.Meta == null)
                throw new Exception("Invalid JSON: Record or Meta is missing.");

            var record = response.Response.Payload.Record;
            var meta = record.Meta;

            if (string.IsNullOrEmpty(record.Market_Symbol)) throw new Exception("Market_Symbol is required.");
            if (!decimal.TryParse(record.Open_Quantity, out var openQty)) ;
            if (!decimal.TryParse(record.Price, out var priceVal)) ;

            if (!DateTime.TryParse(response.Response.Event_Manager_Timestamp, out var timestamp))
                throw new Exception("Invalid event_manager_timestamp format.");

            var execReport = new ExecutionReport(
                new OrderID(record.Order_Uuid ?? Guid.NewGuid().ToString()),
                new ExecID(Guid.NewGuid().ToString()),
                new ExecType(ExecType.FILL),
                new OrdStatus(OrdStatus.FILLED),
                new Symbol(record.Market_Symbol),
                new Side(record.Side.ToUpper() == "BUY" ? Side.BUY : Side.SELL),
                new LeavesQty(0),
                new CumQty(openQty),
                new AvgPx(priceVal)
            );
            execReport.Set(new ClOrdID(record.Matching_Engine_Client_Id.ToString()));
            execReport.Set(new OrderQty(openQty));
            execReport.Set(new LastPx(priceVal));
            execReport.SetField(new StringField(9601, response.Response.Event_Manager_Id));
            execReport.SetField(new StringField(9602, response.Response.Event_Manager_Timestamp));
            execReport.SetField(new StringField(9603, response.Response.Exec_Time_Ms.ToString()));
            execReport.SetField(new StringField(9604, response.Response.Message));
            execReport.SetField(new StringField(9605, response.Response.Payload.Environment_Tag));
            execReport.SetField(new StringField(9606, response.Response.Payload.Model));
            execReport.SetField(new StringField(9607, response.Response.Payload.Operation));
            execReport.SetField(new IntField(9608, response.Response.Payload.Record.Market_Id));
            execReport.SetField(new StringField(9609, response.Response.Payload.Record.Market_Symbol));
            execReport.SetField(new StringField(6004, response.Response.Payload.Record.Market_Uuid));
            execReport.SetField(new IntField(9610, response.Response.Payload.Record.Matching_Engine_Client_Id));

            execReport.SetField(new StringField(6027, meta.Estimated_Market_Value));
            execReport.SetField(new IntField(6031, meta.Leverage));
            execReport.SetField(new StringField(9505, meta.Margin_Blocked));
            execReport.SetField(new StringField(9505, meta.Margin_Blocked));
            execReport.SetField(new StringField(9506, meta.Client_Reference ?? " "));
            execReport.SetField(new StringField(7107, meta.Margin_Call_Trigger ?? " "));
            execReport.SetField(new DecimalField(9612, decimal.Parse(record.Open_Quantity ?? "0.0")));
            execReport.SetField(new StringField(6041, record.Order_Uuid));
            execReport.SetField(new StringField(9614, record.Pmx_Id));
            execReport.SetField(new StringField(9615, record.Pmx_Uuid));
            execReport.SetField(new StringField(9617, record.Sender_Message_Id));
            if (!string.IsNullOrWhiteSpace(record.Tip_quantity) &&
                 decimal.TryParse(record.Tip_quantity, out var tipQty))
            {
                execReport.SetField(new DecimalField(6036, tipQty));
            }
            execReport.SetField(new StringField(9621, record.Total_quantity ?? " "));
            if (record.Stop_Price != null)
                execReport.SetField(new StringField(99, record.Stop_Price));
            char sideChar = record.Side?.ToLower() switch
            {
                "buy" => Side.BUY,
                "sell" => Side.SELL,
                _ => throw new Exception($"Invalid side value: {record.Side}")
            };
            execReport.SetField(new Side(sideChar));

            execReport.SetField(new StringField(9619, response.Response.Source));
            execReport.SetField(new DecimalField(9620, response.Response.Submission_time_ms));

            var header = execReport.Header;
            header.SetField(new BeginString("FIX.4.4"));
            header.SetField(new MsgType(MsgType.EXECUTION_REPORT));
            header.SetField(new SenderCompID(_senderCompID));
            header.SetField(new TargetCompID(_targetCompID));
            header.SetField(new MsgSeqNum((ulong)_msgSeqNum));
            header.SetField(new SendingTime(timestamp));
            fixMessages.Add(execReport);
            return fixMessages;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to build ExecutionReport from upstream response.", ex);
        }
    }

    public List<ExecutionReport> BuildCancelOrderFixMessages(object model)
    {
        var upstreamResponse = model as UpstreamCancelOrderResponse;
        if (upstreamResponse == null)
            throw new InvalidCastException("Invalid model type for BuildFixMessages");

        var fixMessages = new List<ExecutionReport>();

        try
        {
            if (upstreamResponse.Response == null || upstreamResponse.Response.Count == 0)
                throw new Exception("Invalid JSON: Response dictionary is empty.");

            foreach (var kvp in upstreamResponse.Response)
            {
                string orderUuid = kvp.Key;
                var entry = kvp.Value;

                char execType = entry.Success ? ExecType.CANCELED : ExecType.REJECTED;
                char ordStatus = entry.Success ? OrdStatus.CANCELED : OrdStatus.REJECTED;

                var execReport = new ExecutionReport(
                    new OrderID(orderUuid),
                    new ExecID(Guid.NewGuid().ToString()),
                    new ExecType(execType),
                    new OrdStatus(ordStatus),
                    new Symbol("N/A"),
                    new Side(Side.BUY),
                    new LeavesQty(0),
                    new CumQty(0),
                    new AvgPx(0)
                );

                execReport.Set(new ClOrdID(Guid.NewGuid().ToString()));
                execReport.SetField(new StringField(6041, orderUuid));
                execReport.SetField(new StringField(9604, entry.Message ?? "No message"));

                if (entry.Payload != null)
                {
                    execReport.SetField(new StringField(9601, entry.Event_Manager_Id ?? "null"));
                    execReport.SetField(new StringField(9603, entry.Exec_Time_Ms.ToString()));
                    execReport.SetField(new StringField(9605, entry.Payload.Environment_Tag));
                    execReport.SetField(new StringField(9606, entry.Payload.Model));
                    execReport.SetField(new StringField(9607, entry.Payload.Operation));

                    if (entry.Payload.Update != null)
                    {
                        execReport.SetField(new StringField(9621, entry.Payload.Update.Cancellation_Attempted.ToString()));
                    }
                }
             
                var header = execReport.Header;
                header.SetField(new BeginString("FIX.4.4"));
                header.SetField(new MsgType(MsgType.EXECUTION_REPORT));
                header.SetField(new SenderCompID(_senderCompID));
                header.SetField(new TargetCompID(_targetCompID));
                header.SetField(new MsgSeqNum((ulong)_msgSeqNum));
                header.SetField(new SendingTime(DateTime.UtcNow));

                fixMessages.Add(execReport);
            }
            return fixMessages;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to build ExecutionReport from cancel order response.", ex);
        }
    }

    public List<ExecutionReport> BuildTradeDataFix(TradeResponseModel model)
    {
        var response = model?.Response;
        if (response == null)
            throw new ArgumentNullException(nameof(model), "Trade fill response is null.");
        var fixMessages = new List<ExecutionReport>();
        var message = new ExecutionReport();
        message.SetField(new ClOrdID(response.OrderUuid));
        message.SetField(new OrderID(response.OrderUuid));
        message.SetField(new ExecID(response.OrderUuid));
        message.SetField(new ExecType(ExecType.FILL));
        message.SetField(new OrdStatus(OrdStatus.FILLED));
        message.SetField(new Side(Side.BUY)); 
        message.SetField(new OrderQty(decimal.Parse(response.ExecSize)));
        message.SetField(new LastQty(decimal.Parse(response.ExecSize)));
        message.SetField(new LastPx(decimal.Parse(response.ExecPrice)));
        message.SetField(new Symbol("UNKNOWN"));
        message.SetField(new LeavesQty(0));
        message.SetField(new CumQty(decimal.Parse(response.ExecSize)));
        message.SetField(new AvgPx(decimal.Parse(response.ExecPrice)));
        var header = message.Header;
        header.SetField(new BeginString("FIX.4.4"));
        header.SetField(new MsgType("8"));
        header.SetField(new SenderCompID("CLIENT1"));
        header.SetField(new TargetCompID("BROKER1"));

        fixMessages.Add(message);

        return fixMessages;
    }

    public List<ExecutionReport> BuildMarketsPositionFix(object model)
    {
        var response = model as MarketsPositionResponseModel;
        if (response == null)
            throw new InvalidCastException("Invalid model type for MarketInfoFixBuilder");

        var fixMessages = new List<ExecutionReport>();

        foreach (var kvp in response.Positions)
        {
            string symbol = kvp.Key;
            var market = kvp.Value;

            var execReport = new ExecutionReport(
                new OrderID(Guid.NewGuid().ToString()),
                new ExecID(Guid.NewGuid().ToString()),
                new ExecType(ExecType.FILL),
                new OrdStatus(OrdStatus.FILLED),
                new Symbol(market.MarketSymbol),
                new Side(Side.BUY),
                new LeavesQty(0),
                new CumQty(market.MinimumExecutionQuantity),
                new AvgPx(0)
            );

            execReport.Set(new ClOrdID("CL-" + Guid.NewGuid().ToString("N").Substring(0, 10)));
            execReport.Set(new Symbol(symbol));
            execReport.Set(new OrderQty(market.OpenOrderCount));
            execReport.Set(new LastPx(0));

            execReport.SetField(new StringField(CustomFixTags.PositionSize, market.PositionSize));
            execReport.SetField(new StringField(CustomFixTags.PositionVolume, market.PositionVolume));
            execReport.SetField(new StringField(CustomFixTags.OpenOrderVolume, market.OpenOrderVolume));
            execReport.SetField(new StringField(CustomFixTags.TotalTradedVolume, market.TotalTradedVolume));
            execReport.SetField(new IntField(CustomFixTags.TotalTradesCount, market.TotalTradesCount));
            execReport.SetField(new IntField(CustomFixTags.OpenOrderCount, market.OpenOrderCount));
            execReport.SetField(new StringField(CustomFixTags.TotalTradedLots, market.TotalTradedLots));
            execReport.SetField(new StringField(CustomFixTags.AvgPrice, market.AvgPrice));

            var header = execReport.Header;
            header.SetField(new BeginString("FIX.4.4"));
            header.SetField(new MsgType("8"));
            header.SetField(new SenderCompID("CLIENT1"));
            header.SetField(new TargetCompID("BROKER1"));

            fixMessages.Add(execReport);
        }
        return fixMessages;
    }

    public List<ExecutionReport> TradeFillHistoryFix(TradeFillHistoryResponseModel model)
    {
        if (model == null || model.Response == null || model.Response.Count == 0)
            throw new ArgumentNullException(nameof(model), "Trade fill response is null or empty.");

        var fixMessages = new List<ExecutionReport>();

        foreach (var tradeGroup in model.Response)
        {
            foreach (var kvp in tradeGroup)
            {
                var uuid = kvp.Key;
                var trade = kvp.Value;

                var message = new ExecutionReport();
                message.SetField(new ClOrdID(trade.OrderUuid));
                message.SetField(new ExecID(trade.ExternalFillId));
                message.SetField(new ExecType(ExecType.FILL));
                message.SetField(new OrdStatus(OrdStatus.FILLED));
                message.SetField(new Side(trade.Direction == "LONG" ? Side.BUY : Side.SELL));
                message.SetField(new OrderQty(decimal.Parse(trade.ExecSize)));
                message.SetField(new LastQty(decimal.Parse(trade.ExecSize)));
                message.SetField(new LastPx(decimal.Parse(trade.ExecPrice)));
                message.SetField(new Symbol("UNKNOWN"));
                message.SetField(new LeavesQty(0));
                message.Set(new OrderQty(decimal.Parse(trade.ExecSize)));
                message.SetField(new CumQty(decimal.Parse(trade.ExecSize)));
                message.SetField(new AvgPx(decimal.Parse(trade.ExecPrice)));
                message.SetField(new OrderID(trade.OrderUuid));

                var header = message.Header;
                header.SetField(new BeginString("FIX.4.4"));
                header.SetField(new MsgType("8"));
                header.SetField(new SenderCompID("CLIENT1"));
                header.SetField(new TargetCompID("BROKER1"));

                fixMessages.Add(message);
            }
        }
        return fixMessages;
    }

    public List<ExecutionReport> BuildMarketsResponseFix(object model)
    {
        var response = model as MarketsResponseModel;
        if (response == null)
            throw new InvalidCastException("Invalid model type for MarketInfoFixBuilder");

        var fixMessages = new List<ExecutionReport>();

        foreach (var market in response.Response.Values)
        {
            var execReport = new ExecutionReport(
                new OrderID(Guid.NewGuid().ToString()),
                new ExecID(Guid.NewGuid().ToString()),
                new ExecType(ExecType.FILL),
                new OrdStatus(OrdStatus.FILLED),
                new Symbol(market.MarketSymbol),
                new Side(Side.BUY), 
                new LeavesQty(0),
                new CumQty(market.MinimumExecutionQuantity),
                new AvgPx(0) 
            );          
            execReport.Set(new ClOrdID("CL-" + Guid.NewGuid().ToString("N").Substring(0, 10)));
            execReport.Set(new OrderQty(market.ContractSize));
            execReport.Set(new LastQty(market.MinimumExecutionQuantity));
            execReport.Set(new LastPx(0)); 
            
            execReport.SetField(new StringField(CustomFixTags.PriceCurrency, market.PriceCurrency));
            execReport.SetField(new StringField(CustomFixTags.MarketType, market.MarketType));
            execReport.SetField(new StringField(CustomFixTags.InstrumentIsin, market.InstrumentIsin));
            execReport.SetField(new StringField(CustomFixTags.MarketUuid, market.MarketUuid));
            execReport.SetField(new StringField(CustomFixTags.OperatingMic, market.OperatingMic));
            execReport.SetField(new StringField(CustomFixTags.SegmentMic, market.SegmentMic));
            execReport.SetField(new DecimalField(CustomFixTags.MinimumQuantity, market.MinimumQuantity));
            execReport.SetField(new DecimalField(CustomFixTags.MinimumExecutionQuantity, market.MinimumExecutionQuantity));
     
            var header = execReport.Header;
            header.SetField(new BeginString("FIX.4.4"));
            header.SetField(new MsgType("8")); 
            header.SetField(new SenderCompID("CLIENT1"));
            header.SetField(new TargetCompID("BROKER1"));

            fixMessages.Add(execReport);
        }
        return fixMessages;
    }

    public List<ExecutionReport> BuildOrderMetaFix(object model)
    {
        var response = model as OrderMetaResponseModel;
        if (response?.response == null)
            throw new InvalidCastException("Invalid model type for OrderMetaFixBuilder");

        var data = response.response;
        var report = new ExecutionReport();
        var header = report.Header;
        header.SetField(new BeginString("FIX.4.4"));
        header.SetField(new MsgType("8")); 
        header.SetField(new SenderCompID("CLIENT1"));
        header.SetField(new TargetCompID("BROKER1"));
       
        report.SetField(new OrderID(data.order_uuid)); 
        report.SetField(new ClOrdID(data.order_uuid));
        report.SetField(new ExecID(Guid.NewGuid().ToString()));
        report.SetField(new ExecType(ExecType.FILL)); 
        report.SetField(new OrdStatus(OrdStatus.FILLED)); 
        report.SetField(new Symbol(data.symbol));
        report.SetField(new Side(data.direction == "BUY" ? Side.BUY : Side.SELL));
        report.SetField(new OrdType(data.order_type == "LIMIT" ? OrdType.LIMIT : OrdType.MARKET));
        report.SetField(new OrderQty(decimal.Parse(data.order_size)));
        report.SetField(new CumQty(decimal.Parse(data.open_vol))); 
        report.SetField(new AvgPx(decimal.Parse(data.limit_price)));
        report.SetField(new LeavesQty(0)); 
        report.SetField(new TransactTime(DateTime.UtcNow));
       
        report.SetField(new StringField(CustomFixTags.OrderGroup, data.order_group));
        report.SetField(new StringField(CustomFixTags.OrderSequence, data.order_sequence));
        report.SetField(new StringField(CustomFixTags.Status, data.status));
        report.SetField(new StringField(CustomFixTags.Created, data.created));
        report.SetField(new StringField(CustomFixTags.Updated, data.updated));
        report.SetField(new StringField(CustomFixTags.EngineId, data.engine_id));
        report.SetField(new StringField(CustomFixTags.EMUuid, data.em_uuid));
        report.SetField(new StringField(CustomFixTags.Market, data.market));
        report.SetField(new StringField(CustomFixTags.BaseCurrency, data.base_currency));
        report.SetField(new StringField(CustomFixTags.QuoteCurrency, data.quote_currency));
        report.SetField(new DecimalField(CustomFixTags.EstimatedMarketValue, decimal.Parse(data.estim_market_value)));
        report.SetField(new DecimalField(CustomFixTags.LimitPrice, decimal.Parse(data.limit_price)));
        report.SetField(new DecimalField(CustomFixTags.OpenVolume, decimal.Parse(data.open_vol)));
        report.SetField(new DecimalField(CustomFixTags.OrderSize, decimal.Parse(data.order_size)));
        report.SetField(new IntField(CustomFixTags.Leverage, data.leverage));
        report.SetField(new StringField(CustomFixTags.LiquidityPool, data.liquidity_pool));
        report.SetField(new StringField(CustomFixTags.VaultUuid, data.vault_uuid));

        if (data.client_ref != null)
            report.SetField(new StringField(CustomFixTags.ClientReference, data.client_ref));
        if (data.external_ref != null)
            report.SetField(new StringField(CustomFixTags.ExternalReference, data.external_ref));
        if (data.tip_quantity != null)
            report.SetField(new StringField(CustomFixTags.TipQuantity, data.tip_quantity.ToString()));
        if (data.cancel_reason != null)
            report.SetField(new StringField(CustomFixTags.CancelReason, data.cancel_reason));

        return new List<ExecutionReport> { report };
    }

    public List<ExecutionReport> BuildOrdersFix(object model)
    {
        var orderModel = model as OrdersResponseModel;
        if (orderModel?.Response == null)
            throw new InvalidCastException("Invalid model type for BuildOrdersFix");

        var reports = new List<ExecutionReport>();

        foreach (var data in orderModel.Response)
        {
            var report = new ExecutionReport();           
            var header = report.Header;
            header.SetField(new BeginString("FIX.4.4"));
            header.SetField(new MsgType("8"));
            header.SetField(new SenderCompID("CLIENT1"));
            header.SetField(new TargetCompID("BROKER1"));
          
            report.SetField(new OrderID(data.OrderUuid));
            report.SetField(new ClOrdID(data.OrderUuid));
            report.SetField(new ExecID(Guid.NewGuid().ToString()));
            report.SetField(new ExecType(ExecType.NEW));
            report.SetField(new OrdStatus(OrdStatus.NEW));
            report.SetField(new Side(data.Direction == "BUY" ? Side.BUY : Side.SELL));
            report.SetField(new OrdType(OrdType.LIMIT));
            if (decimal.TryParse(data.OrderSize, out var orderSize))
                report.SetField(new LeavesQty(orderSize));
            report.SetField(new CumQty(0));
            report.SetField(new AvgPx(0));

            if (DateTime.TryParse(data.Created, out var createdTime))
                report.SetField(new TransactTime(createdTime));

            report.SetField(new Currency(data.BaseCurrency));
            report.SetField(new Symbol(data.Symbol));

          
            if (data.ImmediateOrCancel)
                report.SetField(new TimeInForce(TimeInForce.IMMEDIATE_OR_CANCEL));
            else if (data.FillOrKill)
                report.SetField(new TimeInForce(TimeInForce.FILL_OR_KILL));
            else if (data.BookOrCancel)
                report.SetField(new TimeInForce(TimeInForce.AT_THE_CLOSE));

            if (DateTime.TryParse(data.Updated, out var updatedTime))
                report.SetField(new TransactTime(updatedTime));
           
            report.SetField(new StringField(CustomFixTags.Status, data.Status));
            report.SetField(new StringField(CustomFixTags.OrderGroup, data.OrderGroup));

            if (!string.IsNullOrWhiteSpace(data.OrderSequence))
                report.SetField(new StringField(CustomFixTags.OrderSequence, data.OrderSequence));

            if (!string.IsNullOrWhiteSpace(data.EngineId))
                report.SetField(new StringField(CustomFixTags.EngineId, data.EngineId));

            if (decimal.TryParse(data.EstimMarketValue, out var emv))
                report.SetField(new DecimalField(CustomFixTags.EstimatedMarketValue, emv));

            if (!string.IsNullOrWhiteSpace(data.LiquidityPool))
                report.SetField(new StringField(CustomFixTags.LiquidityPool, data.LiquidityPool));

            if (!string.IsNullOrWhiteSpace(data.VaultUuid))
                report.SetField(new StringField(CustomFixTags.VaultUuid, data.VaultUuid));

            if (!string.IsNullOrEmpty(data.OnStopLoss))
                report.SetField(new StringField(CustomFixTags.OnStopLoss, data.OnStopLoss));

            if (!string.IsNullOrWhiteSpace(data.ExternalRef))
                report.SetField(new StringField(CustomFixTags.ExternalReference, data.ExternalRef));

            if (!string.IsNullOrWhiteSpace(data.CancelReason))
                report.SetField(new StringField(CustomFixTags.CancelReason, data.CancelReason));

            reports.Add(report);
        }
        return reports;
    }

    public List<ExecutionReport> BuildTradesFixMessages(object model)
    {
        var tradeModel = model as TradesDetailResponseModel;
        if (tradeModel?.Response == null || tradeModel.Response.Count == 0)
            throw new ArgumentNullException(nameof(model), "Trade response is null or empty.");

        var fixMessages = new List<ExecutionReport>();

        foreach (var trade in tradeModel.Response)
        {
            var message = new ExecutionReport();

            message.SetField(new ClOrdID(trade.OrderUuid));
            message.SetField(new ExecID(trade.ExternalFillId));
            message.SetField(new ExecType(ExecType.FILL));
            message.SetField(new OrdStatus(OrdStatus.FILLED));
            message.SetField(new Side(trade.Direction == "LONG" ? Side.BUY : Side.SELL));

            var execSize = decimal.TryParse(trade.ExecSize, out var size) ? size : 0;
            var execPrice = decimal.TryParse(trade.ExecPrice, out var price) ? price : 0;

            message.SetField(new OrderQty(execSize));
            message.SetField(new LastQty(execSize));
            message.SetField(new LastPx(execPrice));
            message.SetField(new Symbol(trade.Symbol ?? "UNKNOWN"));
            message.SetField(new LeavesQty(0));
            message.SetField(new CumQty(execSize));
            message.SetField(new AvgPx(execPrice));
            message.SetField(new OrderID(trade.OrderUuid));

            var header = message.Header;
            header.SetField(new BeginString("FIX.4.4"));
            header.SetField(new MsgType("8")); 
            header.SetField(new SenderCompID("CLIENT1"));
            header.SetField(new TargetCompID("BROKER1"));

            fixMessages.Add(message);
        }

        return fixMessages;
    }

    public List<ExecutionReport> BuildMarginDataFix(MarginDataResponse model)
    {
        var fixMessages = new List<QuickFix.FIX44.ExecutionReport>();

        if (model?.Response == null)
            return fixMessages;
        var symbol = model.Response;
        foreach (var trade in model.Response)
        {
            var message = new QuickFix.FIX44.ExecutionReport();
            var header = message.Header;

            header.SetField(new BeginString("FIX.4.4"));
            header.SetField(new MsgType("8"));
            header.SetField(new SenderCompID("CLIENT1"));
            header.SetField(new TargetCompID("BROKER1"));

            message.SetField(new OrderID(Guid.NewGuid().ToString()));
            message.SetField(new ClOrdID(Guid.NewGuid().ToString()));
            message.SetField(new ExecID(Guid.NewGuid().ToString()));
            message.SetField(new ExecType(ExecType.FILL));
            message.SetField(new OrdStatus(OrdStatus.FILLED));
            message.SetField(new Symbol(trade.Symbol));
            message.SetField(new Side(Side.BUY));
            message.SetField(new TransactTime(DateTime.UtcNow));
            message.SetField(new OrderQty((0)));      
            message.SetField(new LeavesQty((0)));  
            message.SetField(new CumQty(0));                                 
            message.SetField(new AvgPx(0));

            if (decimal.TryParse(trade.AvgPrice, out var avgPrice))
                message.SetField(new DecimalField(6009, avgPrice));

            if (decimal.TryParse(trade.PositionSize, out var posSize))
                message.SetField(new DecimalField(6012, posSize));

            if (decimal.TryParse(trade.PositionVolume, out var posVolume))
                message.SetField(new DecimalField(6013, posVolume));

            if (decimal.TryParse(trade.TotalTradedLots, out var tradedLots))
                message.SetField(new DecimalField(6014, tradedLots));

            if (decimal.TryParse(trade.TotalTradedVolume, out var tradedVolume))
                message.SetField(new DecimalField(6015, tradedVolume));

            message.SetField(new IntField(6016, trade.TotalTradesCount));

            fixMessages.Add(message);
        }
        return fixMessages;
    }

    public List<ExecutionReport> BuildRecentDataFix(RecentDataResponse model)
    {
        if (model == null || model.Response == null || model.Response.Count == 0)
            throw new ArgumentNullException(nameof(model), "Trade fill response is null or empty.");

        var fixMessages = new List<ExecutionReport>();

        foreach (var kvp in model.Response)
        {
            var uuid = kvp.Key;
            var trade = kvp.Value;

            var message = new ExecutionReport();

            message.SetField(new ClOrdID(trade.OrderUuid));
            message.SetField(new ExecID(trade.ExternalFillId));
            message.SetField(new ExecType(ExecType.FILL));
            message.SetField(new OrdStatus(OrdStatus.FILLED));
            message.SetField(new Side(trade.Direction == "LONG" ? Side.BUY : Side.SELL));
            message.SetField(new OrderQty(decimal.Parse(trade.ExecSize)));
            message.SetField(new LastQty(decimal.Parse(trade.ExecSize)));
            message.SetField(new LastPx(decimal.Parse(trade.ExecPrice)));
            message.SetField(new Symbol("UNKNOWN"));
            message.SetField(new LeavesQty(0));
            message.SetField(new CumQty(decimal.Parse(trade.ExecSize)));
            message.SetField(new AvgPx(decimal.Parse(trade.ExecPrice)));
            message.SetField(new OrderID(trade.OrderUuid));

            message.SetField(new StringField(6041, trade.OrderUuid));
            message.SetField(new StringField(7113, trade.ExternalFillId));
            message.SetField(new StringField(6020, DateTime.Parse(trade.Created).ToString("yyyyMMdd-HH:mm:ss")));
            message.SetField(new StringField(7102, trade.CreatedTimestamp.ToString("F")));
            message.SetField(new StringField(7208, trade.ChargedFee));
            message.SetField(new StringField(7209, trade.ChargedFeeCurrency));
            message.SetField(new StringField(7210, trade.ChargedFeeRate));
            message.SetField(new StringField(6032, trade.LiquidityPool));
            message.SetField(new StringField(7104, trade.Direction));
            message.SetField(new StringField(6031, trade.Leverage.ToString()));
            message.SetField(new StringField(7105, trade.MarginBlocked));
            message.SetField(new StringField(7108, trade.MarginCalledVol));
            message.SetField(new StringField(7109, trade.MarginCalledPercent));
            if (!string.IsNullOrWhiteSpace(trade.CounterpartyOrderUuid))
                message.SetField(new StringField(7101, trade.CounterpartyOrderUuid));
            if (!string.IsNullOrWhiteSpace(trade.Passivity))
                message.SetField(new StringField(7103, trade.Passivity));
            if (!string.IsNullOrWhiteSpace(trade.MarginCallTrigger))
                message.SetField(new StringField(7107, trade.MarginCallTrigger));
            if (!string.IsNullOrWhiteSpace(trade.MatchingEngineOrderId))
                message.SetField(new StringField(7118, trade.MatchingEngineOrderId));
            message.SetField(new StringField(7112, trade.IsMarginCallTrade ? "Y" : "N"));

            var header = message.Header;
            header.SetField(new BeginString("FIX.4.4"));
            header.SetField(new MsgType("8"));
            header.SetField(new SenderCompID("CLIENT1"));
            header.SetField(new TargetCompID("BROKER1"));

            fixMessages.Add(message);
        }
        return fixMessages;
    }
  
}


