using System.Text.Json.Serialization;

namespace Glash.Agent.Protocol.QpCommands;

[JsonSerializable(typeof(CreateTunnel.Request))]
[JsonSerializable(typeof(CreateTunnel.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AgentCreateTunnelCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(Login.Request))]
[JsonSerializable(typeof(Login.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AgentLoginCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(StartTunnel.Request))]
[JsonSerializable(typeof(StartTunnel.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AgentStartTunnelCommandSerializerContext : JsonSerializerContext { }
