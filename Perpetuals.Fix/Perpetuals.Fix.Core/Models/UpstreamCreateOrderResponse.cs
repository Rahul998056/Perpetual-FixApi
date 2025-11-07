namespace Perpetuals.Fix.Core.Models;

public class UpstreamCreateOrderResponse
{
    public UpstreamResponse Response { get; set; }
    public bool Success { get; set; }
}

public class UpstreamResponse
{
    public object? Context { get; set; }
    public string Event_Manager_Id { get; set; }
    public string Event_Manager_Timestamp { get; set; }
    public double Exec_Time_Ms { get; set; }
    public string Message { get; set; }
    public UpstreamPayload Payload { get; set; }
    public string Source { get; set; }
    public bool Success { get; set; }
    public decimal Submission_time_ms{ get; set; }

}

public class UpstreamPayload
{
    public string Environment_Tag { get; set; }
    public string Model { get; set; }
    public string Operation { get; set; }
    public UpstreamOrderRecord Record { get; set; }
}

public class UpstreamOrderRecord
{
    public int Market_Id { get; set; }
    public string Market_Symbol { get; set; }
    public string Market_Uuid { get; set; }
    public int Matching_Engine_Client_Id { get; set; }
    public UpstreamOrderMeta Meta { get; set; }
    public string Open_Quantity { get; set; }
    public int? Order_Condition { get; set; }
    public string Order_Uuid { get; set; }
    public string Pmx_Id { get; set; }
    public string Pmx_Uuid { get; set; }
    public string? Price { get; set; }
    public string Sender_Message_Id { get; set; }
    public string Side { get; set; }
    public string? Stop_Price { get; set; }
    public string?  Total_quantity { get; set; }
    public string? Tip_quantity { get; set; }
}

public class UpstreamOrderMeta
{
    public string Client_Reference { get; set; }
    public string Estimated_Market_Value { get; set; }
    public int Leverage { get; set; }
    public string Margin_Blocked { get; set; }
    public string Margin_Call_Trigger { get; set; }
}