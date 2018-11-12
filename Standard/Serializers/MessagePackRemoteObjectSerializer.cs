#if !NET35

namespace RemoteAll.Standard.Serializers
{
    using MessagePack;
    using RemoteCore;

    public class MessagePackRemoteObjectSerializer : BaseRemoteObjectSerializer
    {
        public override MessageAsBytes ToByteArray(object source)
        {
            if (source == null)
            {
                return new MessageAsBytes();
            }

            return new MessageAsBytes(MessagePackSerializer.Typeless.Serialize(source));
        }

        public override T ToObject<T>(byte[] raw)
        {
            if (raw == null || raw.Length < 1)
            {
                return default(T);
            }

            return (T)MessagePackSerializer.Typeless.Deserialize(raw);
        }
    }
}

#endif
