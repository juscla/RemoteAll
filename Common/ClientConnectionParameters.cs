namespace RemoteCore
{
    public struct ClientConnectionParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnectionParameters"/> struct.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="port">The port.</param>
        /// <param name="rawMode">if set to <c>true</c> [raw mode].</param>
        public ClientConnectionParameters(
            string address,
            BaseRemoteObjectSerializer serializer = null,
            int port = RemoteConstants.DefaultPort,
            bool rawMode = false)
        {
            this.Address = address;
            this.Port = port;
            this.Serializer = serializer ?? RemoteBase.DefaultSerializer;
            this.RawMode = rawMode;
        }

        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public string Address { get; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port { get; }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>
        /// The serializer.
        /// </value>
        public BaseRemoteObjectSerializer Serializer { get; }

        /// <summary>
        /// Gets a value indicating whether [raw mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [raw mode]; otherwise, <c>false</c>.
        /// </value>
        public bool RawMode { get; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid => !this.Equals(default(ClientConnectionParameters));
    }

}