using System.Text.Json.Serialization;

namespace Glash.Client.Protocol.QpNotices;

[JsonSerializable(typeof(G.D))]
[JsonSerializable(typeof(Glash.Core.TunnelClosed))]
[JsonSerializable(typeof(AgentLoginStatusChanged))]
internal partial class ClientNoticesSerializerContext : JsonSerializerContext { }