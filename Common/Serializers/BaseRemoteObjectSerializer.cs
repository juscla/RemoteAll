using System;

namespace RemoteCore
{
    public abstract class BaseRemoteObjectSerializer
    {
        /// <summary>
        /// Converts the Object into its byte representation.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns></returns>
       public abstract MessageAsBytes ToByteArray(object source);

        /// <summary>
        /// Convert the Raw byte stream back into the object.
        /// </summary>
        /// <typeparam name="T">The type to convert the Raw stream into.</typeparam>
        /// <param name="raw">The raw bytes of the object.</param>
        /// <returns></returns>
        public abstract T ToObject<T>(byte[] raw);
    }
}
