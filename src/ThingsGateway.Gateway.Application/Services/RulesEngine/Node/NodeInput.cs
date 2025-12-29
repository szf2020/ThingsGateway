using System.Text.Json.Nodes;

namespace ThingsGateway.Gateway.Application;

public class NodeInput
{
    private dynamic input;
    public JsonNode JToken
    {
        get
        {
            return JsonUtil.GetJsonNodeFromObj(input);
        }
    }

    public dynamic Value
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