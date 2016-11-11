// Class to handle a PhotonServerPeer.

using ExitGames.Logging;

using TT_Network_Framework;
using TT_Network_Photon.Application;

namespace TT_Network_Photon.Server
{
    public abstract class PhotonServerHandler: IHandler<PhotonServerPeer>
    {
        public abstract MessageType Type { get; }
        public abstract byte Code { get; }
        public abstract int? SubCode { get; }

        public PhotonApplication Server;

        protected ILogger Log = LogManager.GetCurrentClassLogger();

        protected PhotonServerHandler(PhotonApplication application)
        {
            Server = application;
        }

        public bool HandleMessage(IMessage message, PhotonServerPeer peer)
        {
            OnHandleMessage(message, peer);

            return true;
        }

        protected abstract bool OnHandleMessage(IMessage message, PhotonServerPeer peer);
    }
}