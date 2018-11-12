namespace RemoteCore
{
    using System;
    using System.Linq;
    using System.Net.Sockets;

    public abstract partial class RemoteBase : IDisposable
    {
        /// <summary>
        /// The default serializer
        /// </summary>
        public static readonly BaseRemoteObjectSerializer DefaultSerializer = new JsonRemoteObjectSerializer();

        /// <summary>
        /// The message size in bytes, should be 4 bytes for an Int.
        /// </summary>
        private const int MessageSizeInBytes = sizeof(int);

        /// <summary>
        /// The size buffer to read the packet size into. 
        /// </summary>
        private readonly byte[] sizeBuffer = new byte[MessageSizeInBytes];

        /// <summary>
        /// Gets the name of the instance.
        /// This is based on the Type.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name => this.GetType().Name;

        /// <summary>
        /// Gets or sets the serialization.
        /// [By default we will use messagepack].
        /// </summary>
        /// <value>
        /// The serialization.
        /// </value>
        public BaseRemoteObjectSerializer Serialization { get; set; } = DefaultSerializer;

        /// <summary>
        /// Gets or sets a value indicating whether raw mode.
        /// This mode is useful for Sending Binary data like Files. 
        /// </summary>
        public bool RawMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="RemoteBase"/> is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        public bool Disposed { get; protected set; }

        /// <summary>
        /// Occurs when [error].
        /// </summary>
        public event Action<Exception> Error;

        /// <summary>
        /// The message received.
        /// </summary>
        public event Action<Socket, object, bool> MessageReceived;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose() => this.Disposed = true;

        /// <summary>
        /// Sends the specified socket.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected virtual bool Send(Socket socket, object data)
        {
            if (!socket.Connected || data == null || this.RawMode)
            {
                // we are not connected
                // the data is null or we should be in RawMode.
                return false;
            }

            // convert the data to a byte array.
            // using the current Serializer Type.
            // Then call to Array to have the packet match our expected 
            // output [Size- 4bytes] [Data]
            var message = data.ToMessageAsBytes(this.Serialization).AsArray();

            // send the packet.
            return this.SendRaw(socket, message);
        }

        /// <summary>
        /// Sends the raw.
        /// Use this API to send raw Bytes when you wish to avoid
        /// Sending our protocol. This is useful for File Sends.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="packet">The packet.</param>
        /// <returns></returns>
        protected virtual bool SendRaw(Socket socket, params byte[] packet)
        {
            if (!socket.Connected || packet.Length < 1)
            {
                // we are not connected
                // the packet is empty?
                return false;
            }

            try
            {
                return socket.Send(packet) == packet.Length;
            }
            catch (Exception ex)
            {
                this.Error?.Invoke(ex);
            }

            return false;
        }

        /// <summary>
        /// Readers the specified socket.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns></returns>
#if NET35
        protected virtual void SocketReader(Socket socket, int rawBufferSize = RemoteConstants.RawBufferSize)
#else
        protected virtual async System.Threading.Tasks.Task SocketReader(Socket socket, int rawBufferSize = RemoteConstants.RawBufferSize)
#endif
        {
            while (socket.Connected && !this.Disposed)
            {
                if (socket.Available < 1)
                {
                   this.Wait(150);
                }

                try
                {
                    if (this.RawMode)
                    {
                        // create a buffer to read our raw data into.
                        var buffer = new byte[rawBufferSize];
                        var read = socket.Receive(buffer);
                        if (read == 0)
                        {
                            // Socket is no longer Valid. 
                            // disconnect the socket. 
                            socket.Shutdown(SocketShutdown.Both);
                            return;
                        }

                        this.MessageReceived?.Invoke(socket, buffer.Take(read).ToArray(), true);
                    }
                    else
                    {
                        // read the first 4 bytes to determine size and type. 
                        var messageSizeBytes = socket.Receive(sizeBuffer, 0, MessageSizeInBytes, SocketFlags.None);

                        if (messageSizeBytes != MessageSizeInBytes)
                        {
                            continue;
                        }

                        // calculate the total size of the packet.
                        var size = BitConverter.ToInt32(sizeBuffer, 0);

                        // create a new buffer based on the requested size
                        var buffer = new byte[size];

                        // start our offset counter.
                        // we should read the ammount of bytes we expect
                        // based on the passed size.
                        var offset = 0;

                        while (offset < size)
                        {
                            // read all the bytes available until we have reached our Expected Size
                            offset += socket.Receive(buffer, offset, Math.Min(size, socket.Available), SocketFlags.None);
                        }

                        // once we get all the bytes,
                        // parse the packet read from the socket.
                        this.ProcessBuffer(buffer, socket);
                    }
                }
                catch (Exception ex)
                {
                    // some exception occured so lets assume the socket disconnected.
                    // give the error back if anyone is interested. 
                    this.OnError(ex);
                }
            }
        }

        /// <summary>
        /// Processes the buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="socket">The socket.</param>
        protected virtual void ProcessBuffer(byte[] buffer, Socket socket)
        {
            var message = buffer.ToObject(this.Serialization);
            if (message == null)
            {
                return;
            }

            switch (message)
            {
                case string s:
                    switch (s)
                    {
                        case RemoteConstants.ClientDisconnnecting:
                        case RemoteConstants.ServerDisconnecting:
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Disconnect(true);
                            return;
                    }
                    break;
            }

            this.MessageReceived?.Invoke(socket, message, false);
        }

        /// <summary>
        /// Called when [error].
        /// </summary>
        /// <param name="e">The exception.</param>
        protected virtual void OnError(Exception e) => this.Error?.Invoke(e);
    }
}
