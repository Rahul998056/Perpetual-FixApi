using System.Text.Json.Serialization;

namespace Perpetuals.Fix.Core.Models;

public class UpstreamCancelOrderResponse
{
    [JsonPropertyName("response")]
    public Dictionary<string, CancelOrderResponseDetail> Response { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

public class CancelOrderResponseDetail
{
    [JsonPropertyName("event_manager_id")]
    public string? Event_Manager_Id { get; set; }

    [JsonPropertyName("exec_time_ms")]
    public double Exec_Time_Ms { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("payload")]
    public CancelOrderPayload Payload { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("update")]
    public CancelOrderUpdate Update { get; set; }
}

public class CancelOrderPayload
{
    [JsonPropertyName("environment_tag")]
    public string Environment_Tag { get; set; }

    [JsonPropertyName("event_manager_id")]
    public string? Event_Manager_Id { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("operation")]
    public string Operation { get; set; }

    [JsonPropertyName("update")]
    public CancelOrderUpdate Update { get; set; }
}

public class CancelOrderUpdate
{
    [JsonPropertyName("cancellation_attempted")]
    public bool Cancellation_Attempted { get; set; }

    [JsonPropertyName("event_manager_id")]
    public string? Event_Manager_Id { get; set; }

    [JsonPropertyName("order_uuid")]
    public string? Order_Uuid { get; set; }

    [JsonPropertyName("update_version")]
    public string? Update_Version { get; set; }
}
