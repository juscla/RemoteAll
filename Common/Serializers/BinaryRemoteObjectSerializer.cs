namespace RemoteCore
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public class BinaryRemoteObjectSerializer : BaseRemoteObjectSerializer
    {
        private static readonly BinaryFormatter Converter = new BinaryFormatter();

        public override MessageAsBytes ToByteArray(object source)
        {
            if (source.GetType().IsSerializable)
            {
                using (var s = new MemoryStream())
                {
                    Converter.Serialize(s, source);
                    return new MessageAsBytes(s.ToArray());
                }
            }

            return default(MessageAsBytes);
        }

        public override T ToObject<T>(byte[] raw)
        {
            using (var s = new MemoryStream(raw))
                return (T)Converter.Deserialize(s);
        }
    }

}
