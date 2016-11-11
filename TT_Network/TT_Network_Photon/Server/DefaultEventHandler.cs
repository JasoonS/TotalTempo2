// Class to handle network events.

using TT_Network_Photon.Application;

namespace TT_Network_Photon.Server
{
    public abstract class DefaultEventHandler: PhotonServerHandler
    {
        public DefaultEventHandler(PhotonApplication application) : base(application) { }
    }
}