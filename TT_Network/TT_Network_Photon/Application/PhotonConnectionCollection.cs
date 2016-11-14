using System;
using System.Collections.Generic;

using Photon.SocketServer;

using ExitGames.Logging;

using TT_Network_Framework;
using TT_Network_Photon.Client;
using TT_Network_Photon.Server;

namespace TT_Network_Photon.Application
{
    public abstract class PhotonConnectionCollection : IConnectionCollection<PhotonServerPeer, PhotonClientPeer>
    {
        protected readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public Dictionary<Guid, PhotonClientPeer> Clients { get; protected set; }

        public Dictionary<Guid, PhotonServerPeer> Servers { get; protected set; }

        public PhotonConnectionCollection()
        {
            Servers = new Dictionary<Guid, PhotonServerPeer>();

            Clients = new Dictionary<Guid, PhotonClientPeer>();
        }

        public void OnConnect(PhotonServerPeer serverPeer)
        {
            if (!serverPeer.ServerId.HasValue)
            {
                throw new InvalidOperationException("Server Id cannot be null");
            }

            Guid id = serverPeer.ServerId.Value;

            lock (this)
            {
                PhotonServerPeer peer;

                if (Servers.TryGetValue(id, out peer))
                {
                    peer.Disconnect();

                    Servers.Remove(id);

                    Disconnect(peer);
                }

                Servers.Add(id, serverPeer);

                Log.Warn("Sending to connect");

                Connect(serverPeer);

                ResetServers();
            }
        }

        public void OnDisconnect(PhotonServerPeer serverPeer)
        {
            if (!serverPeer.ServerId.HasValue)
            {
                Disconnect(serverPeer);

                throw new InvalidOperationException("Server Id cannot be null");
            }

            lock (this)
            {
                PhotonServerPeer peer;

                if (serverPeer.ServerId.HasValue)
                {
                    Guid id = serverPeer.ServerId.Value;

                    if (!Servers.TryGetValue(id, out peer)) return;

                    if (peer == serverPeer)
                    {
                        Servers.Remove(id);

                        Disconnect(peer);

                        ResetServers();
                    }
                }
            }
        }

        public void OnClientConnect(PhotonClientPeer clientPeer)
        {
            ClientConnect(clientPeer);

            Clients.Add(clientPeer.PeerId, clientPeer);
        }

        public void OnClientDisconnect(PhotonClientPeer clientPeer)
        {
            ClientDisconnect(clientPeer);

            Clients.Remove(clientPeer.PeerId);
        }

        public PhotonServerPeer GetServerByType(int serverType)
        {
            return OnGetServerByType(serverType);
        }

        public abstract void Connect(PhotonServerPeer serverPeer);
        public abstract void Disconnect(PhotonServerPeer serverPeer);

        public abstract void ClientConnect(PhotonClientPeer clientPeer);
        public abstract void ClientDisconnect(PhotonClientPeer clientPeer);

        public abstract void ResetServers();

        public abstract bool IsServerPeer(InitRequest initRequest);

        public abstract PhotonServerPeer OnGetServerByType(int serverType);
    }
}