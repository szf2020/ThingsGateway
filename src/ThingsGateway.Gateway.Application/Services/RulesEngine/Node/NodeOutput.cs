using System.Text.Json.Nodes;

namespace ThingsGateway.Gateway.Application;

public class NodeOutput
{
    private dynamic output;
    public JsonNode JToken
    {
        get
        {
            return JsonUtil.GetJsonNodeFromObj(output);
        }
    }

    public dynamic Value
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