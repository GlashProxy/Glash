using Quick.Protocol;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace G
{
    [JsonSerializable(typeof(D))]
    internal partial class GDSerializerContext : JsonSerializerContext { }

    public class D : AbstractQpSerializer<D>
    {
        protected override JsonTypeInfo<D> GetTypeInfo() => GDSerializerContext.Default.D;

        public int TunnelId { get; set; }
        public byte[] Data { get; set; }
    }
}