using TT_Network_Photon.Application;

namespace TT_Network_Photon.Server
{
    public abstract class DefaultRequestHandler: PhotonServerHandler
    {
        public DefaultRequestHandler(PhotonApplication application): base(application) { }
    }
}