// Class to handle requests and responses for a PhotonClientPeer.

using ExitGames.Logging;

using TT_Network_Framework;
using TT_Network_Photon.Application;

namespace TT_Network_Photon.Client
{
    public abstract class PhotonClientHandler: IHandler<PhotonClientPeer>
    {
        public abstract MessageType Type { get; }
        public abstract byte Code { get; }
        public abstract int? SubCode { get; }

        protected PhotonApplication Server;

        protected ILogger Log = LogManager.GetCurrentClassLogger();

        protected PhotonClientHandler(PhotonApplication application)
        {
            Server = application;
        }

        public bool HandleMessage(IMessage message, PhotonClientPeer peer)
        {
            OnHandleMessage(message, peer);

            return true;
        }

        protected abstract bool OnHandleMessage(IMessage message, PhotonClientPeer peer);
    }
}