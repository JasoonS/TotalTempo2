using System;
using System.Net;

using Photon.SocketServer;

using Autofac;

using TT_Network_Photon.Application;
using TT_Network_Photon.Server;

using TT_Common;

namespace TT_Server
{
    public class TT_Server : PhotonApplication
    {
        private TTConnectionCollection _connectionCollection;

        //public override PhotonConnectionCollection ConnectionCollection { get { return _connectionCollection; } set { _connectionCollection = value as TTConnectionCollection; } }

        public override bool ConnectsToMaster
        {
            get { return false; }
        }

        public override IPEndPoint MasterEndPoint
        {
            get { throw new NotImplementedException(); }
        }

        public override IPAddress PublicIpAddress
        {
            get { throw new NotImplementedException(); }
        }

        public override int ServerType
        {
            get { throw new NotImplementedException(); }
        }

        public override byte SubCodeParameterKey
        {
            get { return (byte)ClientParameterCode.SubOperationCode; }
        }

        public override int? TCPPort
        {
            get { throw new NotImplementedException(); }
        }

        public override int? UDPPort
        {
            get { throw new NotImplementedException(); }
        }

        protected override int ConnectRetryIntervalSeconds
        {
            get { throw new NotImplementedException(); }
        }

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return _factory.CreatePeer(initRequest);
        }

        public override void Register(PhotonServerPeer peer) {}

        protected override void RegisterContainerObjects(ContainerBuilder builder)
        {
            builder.RegisterInstance(this).As<PhotonApplication>().SingleInstance();

            builder.RegisterType<TTConnectionCollection>().As<PhotonConnectionCollection>().SingleInstance();

            // Add Handlers
            // builder.RegisterType<LoginRequestHandler>().As<IClientHandler>().SingleInstance();
        }

        protected override void ResolveParameters(IContainer container) {}
    }
}