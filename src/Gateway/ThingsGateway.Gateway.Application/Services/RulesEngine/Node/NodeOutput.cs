using Newtonsoft.Json.Linq;

using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Application;

public class NodeOutput
{
    private object output;
    public JToken JToken
    {
        get
        {
            return (output).GetJTokenFromObj();
        }
    }

    public object Value
    {
        get
        {
            return output;
        }
        set
        {
            output = value;
        }
    }
}