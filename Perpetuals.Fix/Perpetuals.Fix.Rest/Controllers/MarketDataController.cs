using Microsoft.AspNetCore.Mvc;
using Perpetuals.Fix.Core.Models;
using Perpetuals.Fix.Core.Services;

namespace Perpetuals.Fix.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MarketDataController : ControllerBase
{
    private readonly IUpstreamService _upstream;
    private readonly IFixServices _fixBuilder;

    public MarketDataController( 
        IUpstreamService upstream, 
        IFixServices fixBuilder)
    {
        _upstream = upstream;
        _fixBuilder = fixBuilder;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder(
       [FromBody] UpstreamCreateOrderRequest dto,
       [FromHeader(Name = "Authorization")] string authToken,
       [FromHeader(Name = "Session")] string sessionId)
    {
        try
        {

            var upstreamRequest = new CreateOrderUpstreamModel
            {
                Market = dto.Market ?? "BTCUSD",
                Quantity = dto.Quantity,
                Side = dto.Side,
                LimitPrice = dto.LimitPrice,
                Type = dto.Type,
                Leverage = dto.Leverage,
                Reference = dto.Reference
            };

            var upstreamResponse = await _upstream.GetCreateOrder(
                upstreamRequest, 
                authToken, 
                sessionId);
            if (upstreamResponse == null)
            {
                return BadRequest("Failed to create order upstream.");
            }
            var fix = _fixBuilder.BuildFixMessage(upstreamResponse);
            if (fix == null)
            {
                return BadRequest("Failed to build FIX message from upstream response.");
            }
            return Ok(fix);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Error = "An unexpected error occurred while creating order.",
                Details = ex.Message
            });
        }
    }
    [HttpGet("market-data")]
    public async Task<IActionResult> GetAllMarketData(
       [FromHeader(Name = "Authorization")] string authToken,
       [FromHeader(Name = "Session")] string sessionId)
    {
        try
        {
            var upstreamResponse = await _upstream.GetMarketData(
                authToken, 
                sessionId);

            var FixMessage = _fixBuilder.BuildMarketsResponseFix(upstreamResponse);
            return Ok(FixMessage);
        }
        catch (Exception ex) {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Error = "An unexpected error occurred while fetching market data.",
                Details = ex.Message
            });
        }
    }

    [HttpGet("market-positions")]
    public async Task<IActionResult> GetMarketPositions(
    [FromHeader(Name = "Authorization")] string authToken,
    [FromHeader(Name = "Session")] string sessionId)
    {
        try
        {
            var upstreamResponse = await _upstream.GetMarketPositionsUpstream(authToken, sessionId);

            if (upstreamResponse == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { Error = "Failed to fetch market positions from upstream." });

            var fixMessages = _fixBuilder.BuildMarketsPositionFixMessages(upstreamResponse);

            return Ok(fixMessages);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Error = "An unexpected error occurred while fetching market positions.",
                Details = ex.Message
            });
        }
    }
    [HttpGet("market-history")]
    public async Task<IActionResult> GetTradeFillHistory(
    [FromHeader(Name = "Authorization")] string authToken,
    [FromHeader(Name = "Session")] string sessionId,
    [FromQuery(Name = "minutes")] int minutes,
    [FromQuery(Name = "market")] string market)
    {
        try
        {
            var upstreamResponse = await _upstream.GetTradeFillHistoryAsync(authToken, sessionId, minutes, market);

            if (upstreamResponse == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { Error = "Failed to fetch trade fills from upstream." });

            var fixMessages = _fixBuilder.BuildTradeFillHistoryFixMessages(upstreamResponse);

            return Ok(fixMessages);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Error = "An unexpected error occurred while fetching trade fill history.",
                Details = ex.Message
            });
        }
    }
    [HttpGet("order-meta")]
    public async Task<IActionResult> OrderMeta(
    [FromHeader(Name = "Authorization")] string authToken,
    [FromHeader(Name = "Session")] string sessionId,
    [FromQuery(Name = "order_uuid")] string order_uuid)
    {
        try
        {
            var upstreamResponse = await _upstream.GetOrderMetaUpstream(authToken, sessionId, order_uuid);

            if (upstreamResponse == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { Error = "Failed to fetch trade fills from upstream." });

            var fixMessages = _fixBuilder.BuildOrderMetaFix(upstreamResponse);

            return Ok(fixMessages);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Error = "An unexpected error occurred while fetching trade fill history.",
                Details = ex.Message
            });
        }
    }


}
