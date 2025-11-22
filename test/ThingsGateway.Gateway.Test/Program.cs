using System.Text.Json.Nodes;

using ThingsGateway.Foundation.OpcUa;

namespace ThingsGateway.Gateway.Test
{
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {
            OpcUaMaster opcUaMaster = new();
            OpcUaProperty opcUaProperty = new();
            opcUaProperty.OpcUrl = "opc.tcp://localhost:49321";

            opcUaMaster.OpcUaProperty = opcUaProperty;

            await opcUaMaster.ConnectAsync(default).ConfigureAwait(false);

            var read = await opcUaMaster.WriteNodeAsync( new Dictionary<string, JsonNode>()
            {
                {"ns=2;s=modbusDevice743916562268229.modbus41",21 }, 
                {"ns=2;s=modbusDevice743916562268232.modbus41",12 },
            }).ConfigureAwait(false);

            Console.WriteLine("Hello World!");

            Console.ReadLine();
        }
    }
}
