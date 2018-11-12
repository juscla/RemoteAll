#if NET35
namespace RemoteCore
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Server 
    /// </summary>
    /// <seealso cref="RemoteBase" />
    public class Server : RemoteBase
    {
        /// <summary>
        /// The listener which allows clients to connect.
        /// </summary>
        private readonly TcpListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public Server(BaseRemoteObjectSerializer serial = null, int port = RemoteConstants.DefaultPort)
        {
            this.Serialization = serial ?? DefaultSerializer;
            this.listener = new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        /// Occurs when [clients are added].
        /// </summary>
        public event Action<Socket> ClientAdded;

        /// <summary>
        /// Occurs when [clients are removed].
        /// </summary>
        public event Action<Socket> ClientRemoved;

        /// <summary>
        /// Gets the Servers Address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public string Address => this.listener?.LocalEndpoint?.ToString();

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        public List<Socket> Clients { get; private set; } = new List<Socket>();

        /// <summary>
        /// Gets a value indicating whether this <see cref="Server"/> is active and listening.
        /// </summary>
        /// <value>
        ///   <c>true</c> if listening; otherwise, <c>false</c>.
        /// </value>
        public bool Active => this.listener.IsActive();

        /// <summary>
        /// Starts the new.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public static Server StartNew(BaseRemoteObjectSerializer serial = null, int port = RemoteConstants.DefaultPort)
        {
            var server = new Server(serial, port);
            server.Start();
            return server;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (this.listener.IsActive())
            {
                return;
            }

            this.listener.Start();
            new Thread(() => this.AwaitClients()).Start();
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (!this.RawMode)
                this.NotifyClients(RemoteConstants.ServerDisconnecting);

            lock (this)
                this.Clients.Clear();

            base.Dispose();
        }

        /// <summary>
        /// Notifies all the clients.
        /// </summary>
        /// <param name="message">The message.</param>
        public void NotifyClients(object message) => this.Clients.ForEach(c => this.NotifyClient(c, message));

        /// <summary>
        /// Raws message to all of the clients.
        /// </summary>
        /// <param name="message">The message.</param>
        public void RawNotifyClients(byte[] message) => this.Clients.ForEach(c => this.RawNotifyClient(c, message));

        /// <summary>
        /// Notifies the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        public void NotifyClient(Socket client, object message) => this.Send(client, message);

        /// <summary>
        /// Sends Raw Message to the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        public void RawNotifyClient(Socket client, byte[] message) => this.SendRaw(client, message);

        /// <summary>
        /// Awaits the clients.
        /// </summary>
        private void AwaitClients()
        {
            while (!this.Disposed)
            {
                if (!this.listener.Pending())
                {
                    this.Wait(150);
                    continue;
                }

                // accept the client
                var socket = this.listener.AcceptTcpClient();

                // add the client
                this.Clients.Add(socket.Client);

                // notify those interested there is a new client. 
                this.ClientAdded?.Invoke(socket.Client);

                // read the client.
                new Thread(() => this.ClientReader(socket.Client)).Start();
            }

            this.listener.Stop();
        }

        /// <summary>
        /// Clients the reader.
        /// </summary>
        /// <param name="client">The client.</param>
        private void ClientReader(Socket client)
        {
            // reads the socket
            this.SocketReader(client);

            lock (this)
            {
                // remove the client 
                this.Clients.Remove(client);
            }

            // notify those interested.
            this.ClientRemoved?.Invoke(client);
        }
    }
}
#endif