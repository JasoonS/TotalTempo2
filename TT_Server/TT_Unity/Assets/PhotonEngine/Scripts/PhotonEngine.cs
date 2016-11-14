using UnityEngine;

using ExitGames.Client.Photon;

public class PhotonEngine : MonoBehaviour, IPhotonPeerListener
{
    private PhotonPeer _peer;
    private GameState _state;
    private ViewController _controller;

    private static PhotonEngine _instance;

    private bool _isServer;

    private uint _updateTickThreshold = 2;

    private uint _updateTick = 2;

    public PhotonPeer Peer { get { return _peer; } protected set { _peer = value; } }
    public GameState State { get { return _state; } protected set { _state = value; } }
    public ViewController Controller { get { return _controller; } set { _controller = value; } }

    public static PhotonEngine Instance { get { return _instance; } }

    public bool IsServer { get { return _isServer; } }

    public string ServerAddress;
    public string ApplicationName;

    public void Start()
    {
        DontDestroyOnLoad(this);

        _state = new Disconnected();

        Application.runInBackground = true;

        Initialise();
    }

    public void Awake()
    {
        _instance = this;
    }

    public void FixedUpdate()
    {
        ++_updateTick;

        if ((_updateTick > _updateTickThreshold) || !((((PeerIdHandler)_controller.OperationHandlers[0]).HasSentServerStatus) && ((PeerIdHandler)_controller.OperationHandlers[0]).HasRequestedMyPeerId))
        {
            _updateTick = 0;

            _state.OnUpdate();
        }
    }

    public void Initialise()
    {
        _peer = new PhotonPeer(this, ConnectionProtocol.Udp);

        _peer.ChannelCount = 2;
        _peer.LimitOfUnreliableCommands = 0;
        _peer.SentCountAllowance = 15;
        _peer.DisconnectTimeout = 30000;

        _peer.Connect(ServerAddress, ApplicationName);

        _state = new WaitingForConnection();
    }

    public void Disconnect()
    {
        if (_peer != null)
        {
            _peer.Disconnect();
        }

        _state = new Disconnected();
    }

    public void SendOp(OperationRequest request, bool sendReliable, byte channelId, bool encrypt)
    {
        _state.SendOperation(request, sendReliable, channelId, encrypt);
    }

    public static void UseExistingOrCreateNewPhotonEngine(string serverAddress, string applicationName, bool isServer)
    {
        GameObject tempEngine;
        PhotonEngine myEngine;

        tempEngine = GameObject.Find("PhotonEngine");

        if (tempEngine == null)
        {
            tempEngine = new GameObject("PhotonEngine");

            tempEngine.AddComponent<PhotonEngine>();
        }

        myEngine = tempEngine.GetComponent<PhotonEngine>();

        myEngine.ApplicationName = applicationName;
        myEngine.ServerAddress = serverAddress;

        myEngine._isServer = isServer;
    }

    #region Implementation of IPhotonPeerListener

    public void DebugReturn(DebugLevel level, string message)
    {
        _controller.DebugReturn(level, message);
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        _controller.OnOperationResponse(operationResponse);
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                Peer.EstablishEncryption();

                break;

            case StatusCode.Disconnect:
            case StatusCode.DisconnectByServer:
            case StatusCode.DisconnectByServerLogic:
            case StatusCode.DisconnectByServerUserLimit:
            case StatusCode.Exception:
            case StatusCode.ExceptionOnConnect:
            case StatusCode.TimeoutDisconnect:
                _controller.OnDisconnected("" + statusCode);

                _state = new Disconnected();

                break;

            case StatusCode.EncryptionEstablished:
                _state = new Connected();

                break;

            default:
                _controller.OnUnexpectedStatusCode(statusCode);

                _state = new Disconnected();

                break;
        }
    }

    public void OnEvent(EventData eventData)
    {
        _controller.OnEvent(eventData);
    }

    #endregion
}