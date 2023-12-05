using System.Text.Json.Serialization;

namespace Glash.Client.Protocol.QpCommands;

[JsonSerializable(typeof(CreateTunnel.Request))]
[JsonSerializable(typeof(CreateTunnel.Response))]
internal partial class CreateTunnelCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(DeleteProxyRule.Request))]
[JsonSerializable(typeof(DeleteProxyRule.Response))]
internal partial class DeleteProxyRuleCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetAgentList.Request))]
[JsonSerializable(typeof(GetAgentList.Response))]
internal partial class GetAgentListCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetProxyRuleList.Request))]
[JsonSerializable(typeof(GetProxyRuleList.Response))]
internal partial class GetProxyRuleListCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(Login.Request))]
[JsonSerializable(typeof(Login.Response))]
internal partial class LoginCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(SaveProxyRule.Request))]
[JsonSerializable(typeof(SaveProxyRule.Response))]
internal partial class SaveProxyRuleCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(StartTunnel.Request))]
[JsonSerializable(typeof(StartTunnel.Response))]
internal partial class StartTunnelCommandSerializerContext : JsonSerializerContext { }
