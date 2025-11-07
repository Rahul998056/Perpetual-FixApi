using QuickFix;
using QuickFix.Fields;

namespace Perpetuals.Fix.Shared.FixMessages;

public class CustomRequest : Message
{
    public const string MsgType = "U1";

    public CustomRequest() : base()
    {
        this.Header.SetField(new MsgType(MsgType));
    }

    public CustomRequest(string endpoint, string payload) : this()
    {
        this.SetField(new StringField(9000, endpoint));
        this.SetField(new StringField(9001, payload));
    }
}
