namespace ThingsGateway.Gateway.Application;

public interface INode
{
#if !Management
    public const string ModuleBasePath = "_content/ThingsGateway.Gateway.Razor/";
    public const string FileModulePath = "ThingsGateway.Gateway.Razor";
#else
    public const string ModuleBasePath = "_content/ThingsGateway.Management.Razor/";
    public const string FileModulePath = "ThingsGateway.Management.Razor";
#endif
    public TouchSocket.Core.ILog Logger { get; set; }
    string RulesEngineName { get; set; }
}

public interface IConditionNode : INode
{
    public Task<bool> ExecuteAsync(NodeInput input, CancellationToken cancellationToken);
}

public interface IExpressionNode : INode
{
    public Task<OperResult<NodeOutput>> ExecuteAsync(NodeInput input, CancellationToken cancellationToken);
}
public interface IActuatorNode : INode
{
    public Task<OperResult<NodeOutput>> ExecuteAsync(NodeInput input, CancellationToken cancellationToken);
}

public interface ITriggerNode : INode
{
    public Task StartAsync(Func<NodeOutput, CancellationToken, Task> func, CancellationToken cancellationToken);
}
public interface IExexcuteExpressionsBase
{
}
public interface IExexcuteExpressions : IExexcuteExpressionsBase
{
    public TouchSocket.Core.ILog Logger { get; set; }
    Task<NodeOutput> ExecuteAsync(NodeInput input, CancellationToken cancellationToken);
}
