using System;
using System.Net;
using System.Net.Sockets;
using IHI.Server.Events;
using IHI.Server.Habbos;

using Nito.Async;
using Nito.Async.Sockets;
using IHI.Server.Network.GameSockets;

namespace IHI.Server.Network
{
    public class GameSocket : IMessageable
    {
        #region Events
        #region Event: PacketArrived
        /// <summary>
        /// Indicates the completion of a packet read from the socket.
        /// </summary>
        private event Action<AsyncResultEventArgs<byte[]>> PacketArrived;
        #endregion
        #endregion

        #region Fields
        private readonly ServerChildTcpSocket _internalSocket;
        private int _bytesReceived;
        private readonly byte[] _lengthBuffer;
        private byte[] _dataBuffer;
        #endregion

        #region Properties
        #region Property: Habbo
        public Habbo Habbo
        {
            get;
            internal set;
        }
        #endregion
        #region Property: IPAddress
        public IPAddress IPAddress
        {
            get
            {
                try
                {
                    if (_internalSocket.RemoteEndPoint.AddressFamily == AddressFamily.InterNetwork)
                    {
                        byte[] ipv6Bytes =
                        {
                            // First 80 bits should be 0.
                            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

                            // Next 16 bits should be 1.
                            0xFF, 0xFF,

                            // The renaming 32 bits should be the IPv4 bits. Initialised to 0. 
                            0, 0, 0, 0
                        };

                        byte[] ipv4Bytes = _internalSocket.RemoteEndPoint.Address.GetAddressBytes();
                        ipv4Bytes.CopyTo(ipv6Bytes, 12);

                        return new IPAddress(ipv6Bytes);
                    }
                    return _internalSocket.RemoteEndPoint.Address;
                }
                catch (ObjectDisposedException)
                {
                    return null;
                }
            }
        }
        #endregion
        #region Property: GameSocketManager
        public GameSocketManager GameSocketManager
        {
            get;
            private set;
        }
        #endregion
        #region Property: PacketHandlers
        public GameSocketMessageHandlerInvoker PacketHandlers
        {
            get
            {
                return GameSocketManager.Protocol.HandlerInvokerManager[this];
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Method: GameSocket (Constructor)
        internal GameSocket(ServerChildTcpSocket socket, GameSocketManager manager)
        {
            _internalSocket = socket;
            _lengthBuffer = new byte[manager.Protocol.Reader.LengthBytes];

            GameSocketManager = manager;
            GameSocketManager.Protocol.HandlerInvokerManager[this] = new GameSocketMessageHandlerInvoker();

            Habbo = HabboDistributor.GetPreLoginHabbo(this);
        }
        #endregion

        #region Method: Start
        /// <summary>
        /// Begins reading from the socket.
        /// </summary>
        internal GameSocket Start()
        {
            _internalSocket.ReadCompleted += SocketReadCompleted;
            PacketArrived += ParsePacket;

            ContinueReading();
            return this;
        }
        #endregion
        #region Method: Disconnect
        public GameSocket Disconnect(string reason = "No reason given")
        {
            GameSocketEventArgs eventArgs = new GameSocketEventArgs(this, false);
            CoreManager.ServerCore.OfficalEventFirer.Fire("gamesocket_disconnected:before", eventArgs);

            if (_internalSocket != null)
                _internalSocket.Close();
            CoreManager.ServerCore.ConsoleManager.Notice("Game Socket Manager", CoreManager.ServerCore.StringLocale.GetString("CORE:INFO_NETWORK_CONNECTION_CLOSED", reason));

            GameSocketManager.Protocol.HandlerInvokerManager.DeregisterGameSocket(this);
            Habbo.LoggedIn = false;
            Habbo.Socket = null;
            Habbo = null;
            return this;
        }
        #endregion

        #region Method: ContinueReading
        /// <summary>
        /// Requests a read directly into the correct buffer.
        /// </summary>
        private void ContinueReading()
        {
            try
            {
                // Read into the appropriate buffer: length or data
                if (_dataBuffer != null)
                {
                    _internalSocket.ReadAsync(_dataBuffer, _bytesReceived, _dataBuffer.Length - _bytesReceived);
                }
                else
                {
                    _internalSocket.ReadAsync(_lengthBuffer, _bytesReceived, _lengthBuffer.Length - _bytesReceived);
                }
            }
            catch (ObjectDisposedException) { } // Socket closed.
        }
        #endregion
        #region Method: SocketReadCompleted
        private void SocketReadCompleted(AsyncResultEventArgs<int> args)
        {
            if (args.Error != null)
            {
                if (PacketArrived != null)
                    PacketArrived.Invoke(new AsyncResultEventArgs<byte[]>(args.Error));

                return;
            }

            _bytesReceived += args.Result;

            if (args.Result == 0)
            {
                if (PacketArrived != null)
                    PacketArrived.Invoke(new AsyncResultEventArgs<byte[]>(null as byte[]));
                return;
            }

            if (_dataBuffer == null)
            {
                if (_bytesReceived != GameSocketManager.Protocol.Reader.LengthBytes)
                {
                    ContinueReading();
                }
                else
                {
                    int length = GameSocketManager.Protocol.Reader.ParseLength(_lengthBuffer);

                    _dataBuffer = new byte[length];
                    _bytesReceived = 0;
                    ContinueReading();
                }
            }
            else
            {
                if (_bytesReceived != _dataBuffer.Length)
                {
                    ContinueReading();
                }
                else
                {
                    if (PacketArrived != null)
                        PacketArrived.Invoke(new AsyncResultEventArgs<byte[]>(_dataBuffer));

                    _dataBuffer = null;
                    _bytesReceived = 0;
                    ContinueReading();
                }
            }
        }
        #endregion
        #region Method: ParseByteData
        /// <summary>
        /// Parses a byte array as a packet.
        /// </summary>
        /// <param name="data">The byte array to parse.</param>
        public GameSocket ParseByteData(byte[] data)
        {
            IncomingMessage message = GameSocketManager.Protocol.Reader.ParseMessage(data);
#if DEBUG
            CoreManager.ServerCore.ConsoleManager.Debug("Packet Logging", "INCOMING => " + data.ToUtf8String());
#endif
            GameSocketManager.Protocol.HandlerInvokerManager[this].Invoke(Habbo, message);

            return this;
        }
        #endregion
        #region Method: ParsePacket
        private void ParsePacket(AsyncResultEventArgs<byte[]> args)
        {
            try
            {
                if (args.Error != null)
                    throw args.Error;

                if (args.Result == null)
                {
                    if (Habbo.LoggedIn)
                        Habbo.LoggedIn = false;
                    Disconnect("Socket read error!");
                    return;
                }

                ParseByteData(args.Result);
            }
            catch (Exception)
            {
                if (args.Error != null)
                {
                    string dumpPath = CoreManager.ServerCore.DumpException(args.Error);
                    CoreManager.ServerCore.ConsoleManager.Error("Game Socket Manager", CoreManager.ServerCore.StringLocale.GetString("CORE:ERROR_NETWORK_CONNECTION_KILLED"));
                    CoreManager.ServerCore.ConsoleManager.Error("Game Socket Manager", "    An exception dump has been saved to " + dumpPath);
                }
            }
        }
        #endregion

        #region Method: Send
        private void Send(byte[] data)
        {
            try
            {
                _internalSocket.WriteAsync(data);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
        #endregion

        #region Method: LinkHabbo

        public void LinkHabbo(Habbo habbo)
        {
            if (Habbo.LoggedIn)
                Habbo.Socket.Disconnect("Socket-Habbo Link conflict");

            Habbo = habbo;
            Habbo.Socket = this;
        }
        #endregion

        #region Method: ToString
        public override string ToString()
        {
            return _internalSocket.ToString();
        }

        public IMessageable SendMessage(IInternalOutgoingMessage message)
        {
#if DEBUG
            CoreManager.ServerCore.ConsoleManager.Debug("Packet Logging", "OUTGOING => " + message.Header + message.ContentString);
#endif
            Send(message.GetBytes());
            return this;
        }

        #endregion
        #endregion
    }
}
