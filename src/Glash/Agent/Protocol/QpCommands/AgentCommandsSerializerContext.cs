using System.Text.Json.Serialization;

namespace Glash.Agent.Protocol.QpCommands;

[JsonSerializable(typeof(CreateTunnel.Request))]
[JsonSerializable(typeof(CreateTunnel.Response))]
internal partial class AgentCreateTunnelCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(Login.Request))]
[JsonSerializable(typeof(Login.Response))]
internal partial class AgentLoginCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(StartTunnel.Request))]
[JsonSerializable(typeof(StartTunnel.Response))]
internal partial class AgentStartTunnelCommandSerializerContext : JsonSerializerContext { }
