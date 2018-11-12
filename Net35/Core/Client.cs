#if NET35

using System;
using System.Net.Sockets;
using System.Timers;

namespace RemoteCore
{
    public class Client : RemoteBase
    {
        /// <summary>
        /// The connection timer
        /// </summary>
        private static readonly Timer connectionTimer = new Timer(500);

        /// <summary>
        /// The previous connected state
        /// </summary>
        private bool previousConnectedState;

        /// <summary>
        /// The socket we talk through.
        /// </summary>
        private Socket socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public Client(ClientConnectionParameters parameters)
        {
            this.ConnectionParameters = parameters;
            this.Serialization = ConnectionParameters.Serializer;

            this.socket =
                ConnectionParameters.IsValid ?
                new TcpClient(this.Address, this.Port).Client :
                new TcpClient().Client;

            this.Initialize();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Client"/> class from being created.
        /// </summary>
        private Client()
        {
            this.Serialization = ConnectionParameters.Serializer ?? DefaultSerializer;

            this.socket =
                ConnectionParameters.IsValid ?
                new TcpClient(this.Address, this.Port).Client :
                new TcpClient().Client;

            this.Initialize();
        }


        /// <summary>
        /// Occurs when [the connection changed].
        /// </summary>
        public Action<bool> ConnectionChanged { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Client"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected => this.socket.Connected;

        /// <summary>
        /// The connection parameters
        /// </summary>
        public ClientConnectionParameters ConnectionParameters { get; private set; }

        /// <summary>
        /// The address
        /// </summary>
        public string Address => ConnectionParameters.Address;

        /// <summary>
        /// The port
        /// </summary>
        public int Port => ConnectionParameters.Port;

        /// <summary>
        /// Waits for server.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static Client WaitForServer(ClientConnectionParameters connectionParameters, TimeSpan? timeout = null)
        {
            var end = DateTime.Now.Add(timeout ?? DateTime.Now.AddYears(1).Subtract(DateTime.Now));

            // get the response. 
            var response = WaitForServer(connectionParameters, end, null);

            // always initialize on the public API
            response.Initialize();

            return response;
        }

        /// <summary>
        /// Waits for server.
        /// This is the internal call to handle Reconnection. 
        /// </summary>
        /// <param name="connectionParameters">The connection parameters.</param>
        /// <param name="until">try until Time.</param>
        /// <returns></returns>
        private static Client WaitForServer(ClientConnectionParameters connectionParameters, DateTime until, Action<bool> connectionChange)
        {
            var response = new Client
            {
                Serialization = connectionParameters.Serializer,
                ConnectionParameters = connectionParameters,
                ConnectionChanged = connectionChange
            };

            var tcp = new TcpClient();

            while (DateTime.Now < until)
            {
                try
                {
                    // create a connection.
                    var connect = tcp.BeginConnect(connectionParameters.Address, connectionParameters.Port, _ => { }, null);

                    // wait 500 ms
                    connect.AsyncWaitHandle.WaitOne(250);

                    tcp.EndConnect(connect);

                    // set our socket to the currently connected socket.
                    response.socket = tcp.Client;

                    // we should be in a good state so break out of the loop.
                    return response;
                }
                catch
                {
                }

                Extensions.Wait(null, 150);
            }

            return response;
        }

        /// <summary>
        /// Reconnects this instance.
        /// The default Timeout will be 1 minute. 
        /// this can be overriden.
        /// </summary>
        /// <returns></returns>
        public bool Reconnect(TimeSpan? timeout = null)
        {
            if (this.IsConnected)
            {
                // already connnected?
                return true;
            }

            // grab the socket.
            this.socket =
                WaitForServer(
                    this.ConnectionParameters,
                    DateTime.Now.Add(timeout ?? TimeSpan.FromMinutes(1)),
                    this.ConnectionChanged).socket;

            // re-initialize.
            this.Initialize();

            // return the result of the connection.
            return this.IsConnected;
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Send(object message) => base.Send(this.socket, message);

        /// <summary>
        /// Sends the raw.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendRaw(params byte[] message) => base.SendRaw(this.socket, message);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (this.Disposed || !this.IsConnected)
            {
                return;
            }

            if (!this.RawMode)
                // notify the server we are exiting. 
                this.Send(this.socket, RemoteConstants.ClientDisconnnecting);

            this.socket.Shutdown(SocketShutdown.Both);

            // Stop our connection Timer.
            connectionTimer.Enabled = false;

            // Dispose the base
            base.Dispose();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            if (connectionTimer.Enabled == false)
            {
                connectionTimer.Elapsed += this.ConnectionChecker;
                connectionTimer.Enabled = true;
            }

            if (this.IsConnected)
            {
                new System.Threading.Thread(() => this.SocketReader(this.socket)).Start();
            }
        }

        /// <summary>
        /// Connections the checker.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ConnectionChecker(object sender, EventArgs e)
        {
            if (this.IsConnected != this.previousConnectedState)
            {
                this.previousConnectedState = this.IsConnected;
                this.ConnectionChanged?.Invoke(this.IsConnected);
            }
        }
    }
}

#endif