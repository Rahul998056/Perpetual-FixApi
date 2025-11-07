using QuickFix;
using QuickFix.Fields;

namespace Perpetuals.Fix.Shared.FixMessages;

public class CustomResponse : Message
{
    public const string MsgType = "U2";

    public CustomResponse() : base()
    {
        this.Header.SetField(new MsgType(MsgType));
    }

    public CustomResponse(string endpoint, string payload) : this()
    {
        this.SetField(new StringField(9000, endpoint));
        this.SetField(new StringField(9001, payload));
    }
}
