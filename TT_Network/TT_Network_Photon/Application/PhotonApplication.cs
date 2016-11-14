using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

using Photon.SocketServer;
using Photon.SocketServer.ServerToServer;

using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

using log4net;
using log4net.Config;

using Autofac;

using TT_Network_Framework;
using TT_Network_Photon.Server;
using TT_Network_Photon.Client;

namespace TT_Network_Photon.Application
{
    public abstract class PhotonApplication: ApplicationBase
    {
        public abstract byte SubCodeParameterKey { get; }

        public PhotonConnectionCollection ConnectionCollection { get; set; }

        public static readonly Guid ServerId = Guid.NewGuid();

        public static readonly ILogger Log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public abstract IPEndPoint MasterEndPoint { get; }

        public abstract int? TCPPort { get; }
        public abstract int? UDPPort { get; }

        public abstract IPAddress PublicIpAddress { get; }

        public abstract int ServerType { get; }

        public abstract bool ConnectsToMaster { get; }

        protected abstract int ConnectRetryIntervalSeconds { get; }

        private static PhotonServerPeer _masterPeer;

        private byte _isReconnecting;

        private Timer _retry;

        protected PhotonPeerFactory _factory;

        private IEnumerable<IBackgroundThread> _backgroundThreads;

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            Log.Debug(initRequest.RemoteIP);

            return _factory.CreatePeer(initRequest);
        }

        protected override void Setup()
        {
            GlobalContext.Properties["LogFileName"] = ApplicationName;

            GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(ApplicationRootPath, "log");

            FileInfo configFileInfo = new FileInfo(Path.Combine(BinaryPath, "log4net.config"));

            if (configFileInfo.Exists)
            {
                ExitGames.Logging.LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);

                XmlConfigurator.ConfigureAndWatch(configFileInfo);
            }

            ContainerBuilder builder = new ContainerBuilder();

            Initialize(builder);

            IContainer container = builder.Build();

            _factory = container.Resolve<PhotonPeerFactory>();

            ConnectionCollection = container.Resolve<PhotonConnectionCollection>();

            _backgroundThreads = container.Resolve<IEnumerable<IBackgroundThread>>();

            ResolveParameters(container);

            foreach (IBackgroundThread backgroundThread in _backgroundThreads)
            {
                backgroundThread.Setup();

                ThreadPool.QueueUserWorkItem(backgroundThread.Run);
            }

            if (ConnectsToMaster)
            {
                ConnectToMaster();
            }
        }

        protected void Initialize(ContainerBuilder builder)
        {
            builder.RegisterType<PhotonPeerFactory>();
            builder.RegisterType<PhotonServerPeer>();
            builder.RegisterType<PhotonClientPeer>();
            builder.RegisterType<PhotonClientHandlerList>();
            builder.RegisterType<PhotonServerHandlerList>();

            RegisterContainerObjects(builder);
        }

        protected override void TearDown()
        {
        }

        protected override void OnStopRequested()
        {
            foreach (IBackgroundThread backGroundThread in _backgroundThreads)
            {
                backGroundThread.Stop();
            }

            foreach (KeyValuePair<Guid, PhotonServerPeer> photonServerPeer in ConnectionCollection.Servers)
            {
                photonServerPeer.Value.Disconnect();
            }

            foreach (KeyValuePair<Guid, PhotonClientPeer> photonClientPeer in ConnectionCollection.Clients)
            {
                photonClientPeer.Value.Disconnect();
            }

            base.OnStopRequested();
        }

        public void ConnectToMaster()
        {
            if (!ConnectToServerTcp(MasterEndPoint, "Master", "Master"))
            {
                Log.Warn("master connection refused");

                return;
            }

            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat((_isReconnecting == 0) ? "Connection to master at {0}" : "Reconnecting to master at {0}", MasterEndPoint);
            }
        }

        protected override void OnServerConnectionFailed(int errorCode, string errorMessage, object state)
        {
            if (_isReconnecting == 0)
            {
                Log.ErrorFormat("Master connection failed with error {0} : {1}", errorCode, errorMessage);
            }

            else if (Log.IsDebugEnabled)
            {
                Log.ErrorFormat("Master connection failed with error {0} : {1}", errorCode, errorMessage);
            }

            string stateString = state as string;

            if ((state != null) && (stateString.Equals("Master")))
            {
                ReconnectToMaster();
            }
        }

        public void ReconnectToMaster()
        {
            Thread.VolatileWrite(ref _isReconnecting, 1);

            _retry = new Timer(o => ConnectToMaster(), null, ConnectRetryIntervalSeconds * 1000, 0);
        }

        protected override ServerPeerBase CreateServerPeer(InitResponse initResponse, object state)
        {
            return _factory.CreatePeer(initResponse);
        }

        protected abstract void RegisterContainerObjects(ContainerBuilder builder);

        protected abstract void ResolveParameters(IContainer container);

        public abstract void Register(PhotonServerPeer peer);
    }
}