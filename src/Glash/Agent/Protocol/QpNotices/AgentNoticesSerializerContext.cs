using System.Text.Json.Serialization;

namespace Glash.Agent.Protocol.QpNotices;

[JsonSerializable(typeof(G.D))]
[JsonSerializable(typeof(Core.TunnelClosed))]
internal partial class AgentNoticesSerializerContext : JsonSerializerContext { }