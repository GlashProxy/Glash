using System.Text.Json.Serialization;

namespace Glash.Client.Protocol.QpNotices;

[JsonSerializable(typeof(AgentLoginStatusChanged))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ClientNoticesSerializerContext : JsonSerializerContext { }