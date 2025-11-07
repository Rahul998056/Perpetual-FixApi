using QuickFix;
using QuickFix.Fields;
using System.Text.Json;

public static class FixJsonConverter
{  
    public static string FixToJson(Message fixMessage)
    {
        var dict = new Dictionary<string, string>();

        foreach (var field in fixMessage)
        {
            dict[field.Key.ToString()] = field.Value.ToString();
        }

        foreach (var field in fixMessage.Header)
        {
            dict[$"H_{field.Key}"] = field.Value.ToString();
        }

        foreach (var field in fixMessage.Trailer)
        {
            dict[$"T_{field.Key}"] = field.Value.ToString();
        }

        return JsonSerializer.Serialize(dict, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    // Accept JSON with tag-value map. If "msgType" or "35" is present, use that.
    public static Message JsonToFix(string json, string msgType)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        var fixMsg = new Message();

        fixMsg.Header.SetField(new MsgType(msgType));

        // Standard fields required for all messages
        fixMsg.Header.SetField(new SenderCompID("CLIENT")); // You may want to inject these
        fixMsg.Header.SetField(new TargetCompID("SERVER"));
        fixMsg.Header.SetField(new SendingTime(DateTime.UtcNow));

        // Required custom tags
        if (dict.TryGetValue("endpoint", out var endpoint))
        {
            fixMsg.SetField(new StringField(9000, endpoint));
        }

        if (dict.TryGetValue("payload", out var payload))
        {
            fixMsg.SetField(new StringField(9001, payload));
        }

        return fixMsg;
    } 

}
