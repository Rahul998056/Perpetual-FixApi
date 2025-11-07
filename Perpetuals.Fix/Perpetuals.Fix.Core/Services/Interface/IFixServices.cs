using Perpetuals.Fix.Core.Models;
using QuickFix.FIX44;

namespace Perpetuals.Fix.Core.Services.Interface;

public interface IFixServices
{
    List<ExecutionReport> BuildCreateOrderFixMessages(object model);
    List<ExecutionReport> BuildCancelOrderFixMessages(object model);
    List<ExecutionReport> BuildOrdersFix(object model);
    List<ExecutionReport> BuildOrderMetaFix(object model);
    List<ExecutionReport> BuildTradesFixMessages(object model);
    List<ExecutionReport> BuildMarketsResponseFix(object model);
    List<ExecutionReport> BuildMarketsPositionFix(object model);
    List<ExecutionReport> BuildTradeDataFix(TradeResponseModel model);
    List<ExecutionReport> BuildMarginDataFix(MarginDataResponse model);
    List<ExecutionReport> BuildRecentDataFix(RecentDataResponse model);
    List<ExecutionReport> TradeFillHistoryFix(TradeFillHistoryResponseModel model);
}