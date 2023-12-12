using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Core
{
    [JsonSerializable(typeof(TunnelClosed))]
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    internal partial class TunnelClosedSerializerContext : JsonSerializerContext { }

    [DisplayName("Tunnel closed")]
    public class TunnelClosed : AbstractQpSerializer<TunnelClosed>
    {
        protected override JsonTypeInfo<TunnelClosed> GetTypeInfo() => TunnelClosedSerializerContext.Default.TunnelClosed;
        public int TunnelId { get; set; }
    }
}
