
using ThingsGateway.DependencyInjection;

namespace System;

[SuppressSniffer, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class RequestAuditAttribute : Attribute
{

}