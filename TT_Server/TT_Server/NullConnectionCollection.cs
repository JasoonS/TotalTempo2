// Class to maintain a collection of connected clients and servers, inheriting from PhotonConnectionCollection.

using System;
using System.Collections.Generic;

using Photon.SocketServer;

using TT_Network_Photon.Application;
using TT_Network_Photon.Client;
using TT_Network_Photon.Server;

namespace TT_Server
{
    public class NullConnectionCollection : PhotonConnectionCollection
    {
        public Dictionary<Guid, PhotonClientPeer> Characters { get; protected set; }

        public NullConnectionCollection() : base()
        {
            Characters = new Dictionary<Guid, PhotonClientPeer>();
        }

        public override void ClientConnect(PhotonClientPeer clientPeer)
        {
            throw new NotImplementedException();
        }

        public override void ClientDisconnect(PhotonClientPeer clientPeer)
        {
            //Log.InfoFormat("Logged out {0}", clientPeer.ClientData[((byte)ClientDataCode.UserId).GetType()]);
        }

        public override bool IsServerPeer(InitRequest initRequest)
        {
            return false;
        }

        public override void Connect(PhotonServerPeer serverPeer)
        {
            throw new NotImplementedException();
        }

        public override void Disconnect(PhotonServerPeer serverPeer)
        {
            throw new NotImplementedException();
        }

        public override PhotonServerPeer OnGetServerByType(int serverType)
        {
            throw new NotImplementedException();
        }

        public override void ResetServers()
        {
            throw new NotImplementedException();
        }
    }
}