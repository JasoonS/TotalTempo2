using Photon.SocketServer;

using ExitGames.Logging;

using TT_Network_Photon.Client;
using TT_Network_Photon.Server;

namespace TT_Network_Photon.Application
{
    public class PhotonPeerFactory
    {
        private readonly PhotonServerPeer.Factory _serverPeerFactory;
        private readonly PhotonClientPeer.Factory _clientPeerFactory;
        private readonly PhotonConnectionCollection _subServerCollection;
        private readonly PhotonApplication _application;

        protected readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public PhotonPeerFactory(PhotonServerPeer.Factory serverPeerFactory, PhotonClientPeer.Factory clientPeerFactory, PhotonConnectionCollection subServerCollection, PhotonApplication application)
        {
            _serverPeerFactory = serverPeerFactory;
            _clientPeerFactory = clientPeerFactory;
            _subServerCollection = subServerCollection;
            _application = application;
        }

        public PeerBase CreatePeer(InitRequest initRequest)
        {
            if (IsServerPeer(initRequest))
            {
                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Received init request from sub server");
                }

                return _serverPeerFactory(initRequest.Protocol, initRequest.PhotonPeer);
            }

            Log.DebugFormat("Received init request from client");

            return _clientPeerFactory(initRequest.Protocol, initRequest.PhotonPeer);
        }

        public PhotonServerPeer CreatePeer(InitResponse initResponse)
        {
            var subServerPeer = _serverPeerFactory(initResponse.Protocol, initResponse.PhotonPeer);

            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("Received init request from sub server");
            }

            if (initResponse.RemotePort == _application.MasterEndPoint.Port)
            {
                _application.Register(subServerPeer);

                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Registering sub server");
                }
            }

            return subServerPeer;
        }

        protected bool IsServerPeer(InitRequest initRequest)
        {
            return _subServerCollection.IsServerPeer(initRequest);
        }
    }
}