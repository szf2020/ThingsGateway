// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "<挂起>", Scope = "member", Target = "~M:ThingsGateway.Foundation.Demo.Program.Main(System.String[])~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "<挂起>", Scope = "member", Target = "~M:ThingsGateway.Foundation.Demo.ModbusMasterDemo.TestRead~System.Threading.Tasks.Task")]
