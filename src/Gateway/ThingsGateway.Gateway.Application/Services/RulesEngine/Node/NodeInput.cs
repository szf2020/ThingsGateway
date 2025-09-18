using Newtonsoft.Json.Linq;

using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Application;

public class NodeInput
{
    private object input;
    public JToken JToken
    {
        get
        {
            return (input).GetJTokenFromObj();
        }
    }

    public object Value
    {
        get
        {
            return input;
        }
        set
        {
            input = value;
        }
    }
}