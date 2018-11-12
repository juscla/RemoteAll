namespace RemoteCore
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Net.Sockets;

    internal static class Extensions
    {
        public static MessageAsBytes ToMessageAsBytes(this object source, BaseRemoteObjectSerializer reader) => reader.ToByteArray(source);

        public static T ToObject<T>(this byte[] raw, BaseRemoteObjectSerializer reader) => reader.ToObject<T>(raw);

        public static object ToObject(this IEnumerable<byte> raw, BaseRemoteObjectSerializer reader) => reader.ToObject<object>(raw.ToArray());

        public static bool IsActive(this TcpListener listener) =>
            (bool)listener.GetType()
            .GetProperty("Active", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(listener, null);


        public static void Wait(this RemoteBase o, int time)
        {
#if NET35
            System.Threading.Thread.Sleep(time);
#else
            System.Threading.Tasks.Task.Delay(time).Wait();
#endif
        }
    }
}
