// Class to handle network responses.

using TT_Network_Photon.Application;

namespace TT_Network_Photon.Server
{
    public abstract class DefaultResponseHandler: PhotonServerHandler
    {
        protected DefaultResponseHandler(PhotonApplication application) : base(application) { }
    }
}