using System;
using System.Net;

using IHI.Server.Events;
using IHI.Server.Useful;

using Nito.Async;
using Nito.Async.Sockets;
using IHI.Server.Network.GameSockets;

namespace IHI.Server.Network
{
    public class GameSocketManager
    {
        public GameSocketProtocol Protocol
        {
            get;
            set;
        }

        public IPAddress Address
        {
            get;
            set;
        }

        public ushort Port
        {
            get;
            set;
        }

        #region Method: GameSocketManager (Constructor)
        internal GameSocketManager()
        {
        }
        #endregion

        private ServerTcpSocket _listeningSocket;
        private ActionThread _actionThread;

        public GameSocketManager Start()
        {
            _actionThread = new ActionThread
                                {
                                    Name = "IHI-GameSocketThread"
                                };

            _actionThread.Start();
            _actionThread.Do(() =>
            {
                _listeningSocket = new ServerTcpSocket();
                _listeningSocket.AcceptCompleted += IncomingConnectedAccepted;
                _listeningSocket.Bind(Address, Port);
                _listeningSocket.AcceptAsync();
            });

            return this;
        }

        public GameSocketManager Stop()
        {
            CoreManager.ServerCore.ConsoleManager.Notice("Game Socket Manager", "Stopping...");
            _listeningSocket.Close();
            _actionThread.Join();
            CoreManager.ServerCore.ConsoleManager.Notice("Game Socket Manager", "Stopped!");

            return this;
        }

        private void IncomingConnectedAccepted(AsyncResultEventArgs<ServerChildTcpSocket> args)
        {
            if(args.Error != null)
            {
                string dumpPath = CoreManager.ServerCore.DumpException(args.Error);
                CoreManager.ServerCore.ConsoleManager.Error("Game Socket Manager", "Incoming connection failed!!");
                CoreManager.ServerCore.ConsoleManager.Error("Game Socket Manager", "    An exception dump has been saved to " + dumpPath);
                _listeningSocket.AcceptAsync();
                return;
            }

            ServerChildTcpSocket internalSocket = args.Result;
            GameSocket socket = new GameSocket(internalSocket, this);

            GameSocketEventArgs eventArgs = new GameSocketEventArgs(socket);

            CoreManager.ServerCore.OfficalEventFirer.Fire("incoming_game_connection:before", eventArgs);
            if (eventArgs.IsCancelled)
            {
                socket.Disconnect("Connection rejected from " + internalSocket.RemoteEndPoint + "(" + eventArgs.CancelReason + ")");
                return;
            }
            socket.Start();
            CoreManager.ServerCore.OfficalEventFirer.Fire("incoming_game_connection:after", eventArgs);
            CoreManager.ServerCore.ConsoleManager.Notice("Game Socket Manager", "Incoming connection accepted: " + internalSocket.RemoteEndPoint);

            _listeningSocket.AcceptAsync();
        }
    }
}