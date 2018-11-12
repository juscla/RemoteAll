namespace RemoteCore
{
    using System;

    public class RemoteConstants
    {
        /// <summary>
        /// The default port
        /// </summary>
        public const int DefaultPort = 888;

        /// <summary>
        /// The raw buffer size
        /// 3 MB by default. 
        /// </summary>
        public const int RawBufferSize = 3 * 10000000;

        /// <summary>
        /// The client disconnnecting
        /// </summary>
        internal const string ClientDisconnnecting = "79BFCD0C-77BB-45E9-89A3-CC3447B5980D";

        /// <summary>
        /// The server disconnecting
        /// </summary>
        internal const string ServerDisconnecting = "0E621D2C-3F3D-4F3D-8CBC-C847D1206472";
    }
}
