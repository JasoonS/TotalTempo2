using System;
using System.Collections.Generic;

using Photon.SocketServer;

using PhotonHostRuntimeInterfaces;

using ExitGames.Logging;

using TT_Network_Framework;
using TT_Network_Photon.Application;
using TT_Network_Photon.Server;

namespace TT_Network_Photon.Client
{
    public class PhotonClientPeer: PeerBase
    {
        protected ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Guid _peerId;
        private readonly Dictionary<Type, ClientData> _clientData = new Dictionary<Type, ClientData>();
        private readonly PhotonApplication _server;
        private readonly PhotonClientHandlerList _handlerList;

        private bool _isServer = false;

        public PlayerData PlayerData { get; set; }

        public byte CurrentInputSequenceNo { get; set; }

        public PlayerSnapshotInput[] PlayerInputs { get; set; }

        public PhotonServerPeer CurrentServer { get; set; }

        public bool IsServer { get { return _isServer; } }

        #region Factory Method

        public delegate PhotonClientPeer Factory(IRpcProtocol protocol, IPhotonPeer photonPeer);

        #endregion

        public PhotonClientPeer(IRpcProtocol protocol, IPhotonPeer photonPeer, IEnumerable<ClientData> clientData, PhotonClientHandlerList handlerList, PhotonApplication application): base(protocol, photonPeer)
        {
            _peerId = Guid.NewGuid();
            _server = application;
            _handlerList = handlerList;

            foreach (ClientData data in clientData)
            {
                _clientData.Add(data.GetType(), data);
            }

            _server.ConnectionCollection.Clients.Add(_peerId, this);

            CurrentInputSequenceNo = 0;

            PlayerData = new PlayerData();

            PlayerInputs = new PlayerSnapshotInput[256];

            for (int i = 0; i < 256; ++i)
            {
                PlayerInputs[i] = new PlayerSnapshotInput();
            }
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            // Request to receive ALL NON-SERVER server-side peer IDs (OPERATION_CODE_0)

            if (operationRequest.OperationCode == 0)
            {
                OperationResponse operationResponse = new OperationResponse();

                operationResponse.OperationCode = 0;
                operationResponse.ReturnCode = 0;
                operationResponse.Parameters = new Dictionary<byte, object>()
                {
                    { 0, 0 },
                };

                int clientPeerCount = 0;

                foreach (KeyValuePair<Guid, PhotonClientPeer> clientPeer in _server.ConnectionCollection.Clients)
                {
                    if (!clientPeer.Value.IsServer)
                    {
                        ++clientPeerCount;

                        operationResponse.Parameters.Add((byte)clientPeerCount, clientPeer.Key.ToByteArray());
                    }
                }

                SendOperationResponse(operationResponse, sendParameters);
            }

            // Client-side request to receive server-side transform (OPERATION_CODE_1).

            else if (operationRequest.OperationCode == 1)
            {
                Guid peerId = new Guid((byte[])operationRequest.Parameters[1]);

                PlayerData playerData = _server.ConnectionCollection.Clients[peerId].PlayerData;

                OperationResponse operationResponse = new OperationResponse();

                operationResponse.OperationCode = 1;
                operationResponse.ReturnCode = 0;
                operationResponse.Parameters = new Dictionary<byte, object>()
                {
                    { 0, 2 },
                    { 1, peerId.ToByteArray() },
                    { 2, playerData.xPosition },
                    { 3, playerData.yPosition },
                    { 4, playerData.zPosition },
                    { 5, playerData.xRotation },
                    { 6, playerData.yRotation },
                    { 7, playerData.zRotation }
                };

                SendOperationResponse(operationResponse, sendParameters);
            }

            // Server-side request to update server-side transform (OPERATION_CODE_2).

            else if (operationRequest.OperationCode == 2)
            {
                Guid peerId = new Guid((byte[])operationRequest.Parameters[1]);

                PlayerData playerData = _server.ConnectionCollection.Clients[peerId].PlayerData;

                playerData.xPosition = (float)operationRequest.Parameters[2];
                playerData.yPosition = (float)operationRequest.Parameters[3];
                playerData.zPosition = (float)operationRequest.Parameters[4];

                playerData.xRotation = (float)operationRequest.Parameters[5];
                playerData.yRotation = (float)operationRequest.Parameters[6];
                playerData.zRotation = (float)operationRequest.Parameters[7];

                _server.ConnectionCollection.Clients[peerId].PlayerData = playerData;
            }

            // Server-side request to receive server-side inputs (OPERATION_CODE_3).

            else if (operationRequest.OperationCode == 3)
            {
                Guid peerId = new Guid((byte[])operationRequest.Parameters[1]);

                OperationResponse operationResponse = new OperationResponse();

                operationResponse.OperationCode = 3;
                operationResponse.ReturnCode = 0;
                operationResponse.Parameters = new Dictionary<byte, object>()
                {
                    { 0, 1 },
                    { 1, peerId.ToByteArray() }
                };

                byte currentInputSequenceNo = _server.ConnectionCollection.Clients[peerId].CurrentInputSequenceNo;

                PlayerSnapshotInput playerInput = _server.ConnectionCollection.Clients[peerId].PlayerInputs[currentInputSequenceNo];

                int playerInputNo = 0;

                while (playerInput.isValid)
                {
                    float powerInput = playerInput.powerInput;
                    float turnInput = playerInput.turnInput;

                    bool isJumping = playerInput.isJumping;

                    _server.ConnectionCollection.Clients[peerId].PlayerInputs[currentInputSequenceNo].isValid = false;

                    operationResponse.Parameters.Add((byte)((playerInputNo * 4) + 2), currentInputSequenceNo);
                    operationResponse.Parameters.Add((byte)((playerInputNo * 4) + 3), powerInput);
                    operationResponse.Parameters.Add((byte)((playerInputNo * 4) + 4), turnInput);
                    operationResponse.Parameters.Add((byte)((playerInputNo * 4) + 5), isJumping);

                    currentInputSequenceNo = (byte)IncrementInputSequenceNo(currentInputSequenceNo);

                    _server.ConnectionCollection.Clients[peerId].CurrentInputSequenceNo = currentInputSequenceNo;

                    playerInput = _server.ConnectionCollection.Clients[peerId].PlayerInputs[currentInputSequenceNo];

                    ++playerInputNo;
                }

                SendOperationResponse(operationResponse, sendParameters);
            }

            // Client-side request to update server-side inputs (OPERATION_CODE_4).

            else if (operationRequest.OperationCode == 4)
            {
                byte sequenceNo = (byte)operationRequest.Parameters[1];

                PlayerSnapshotInput playerInput = new PlayerSnapshotInput();

                playerInput.isValid = true;

                playerInput.powerInput = (float)operationRequest.Parameters[2];
                playerInput.turnInput = (float)operationRequest.Parameters[3];

                playerInput.isJumping = (bool)operationRequest.Parameters[4];

                PlayerInputs[sequenceNo] = playerInput;

                //int playerInputCount = operationRequest.Parameters.Count / 4;

                //byte[] sequenceNos = new byte[playerInputCount];

                //for (int i = 0; i < playerInputCount; ++i)
                //{
                //    int currentParameterBlock = i * 4;

                //    byte sequenceNo = (byte)operationRequest.Parameters[(byte)(currentParameterBlock + 1)];

                //    PlayerSnapshotInput playerInput = new PlayerSnapshotInput();

                //    playerInput.isValid = true;

                //    playerInput.powerInput = (float)operationRequest.Parameters[(byte)(currentParameterBlock + 2)];
                //    playerInput.turnInput = (float)operationRequest.Parameters[(byte)(currentParameterBlock + 3)];

                //    playerInput.isJumping = (bool)operationRequest.Parameters[(byte)(currentParameterBlock + 4)];

                //    PlayerInputs[sequenceNo] = playerInput;

                //    sequenceNos[i] = sequenceNo;
                //}

                //Log.DebugFormat("{0}|{1},{2},{3}", sequenceNo, playerInput.powerInput, playerInput.turnInput, playerInput.isJumping);

                OperationResponse operationResponse = new OperationResponse();

                operationResponse.OperationCode = 4;
                operationResponse.ReturnCode = 0;

                operationResponse.Parameters = new Dictionary<byte, object>()
                {
                    { 0, 1 },
                    { 1, sequenceNo }
                };

                //operationResponse.Parameters = new Dictionary<byte, object>()
                //{
                //    { 0, 1 }
                //};

                //for (int i = 0; i < sequenceNos.Length; ++i)
                //{
                //    operationResponse.Parameters.Add((byte)(i + 1), sequenceNos[i]);
                //}

                SendOperationResponse(operationResponse, sendParameters);
            }

            // Request to update corresponding server-side peer ID (OPERATION_CODE_5).

            else if (operationRequest.OperationCode == 5)
            {
                OperationResponse operationResponse = new OperationResponse();

                operationResponse.OperationCode = 5;
                operationResponse.ReturnCode = 0;
                operationResponse.Parameters = new Dictionary<byte, object>()
                {
                    { 0, 0 },
                    { 1, _peerId.ToByteArray() }
                };

                SendOperationResponse(operationResponse, sendParameters);
            }

            // Request to set corresponding server-side peer to active server (OPERATION_CODE_6).

            else if (operationRequest.OperationCode == 6)
            {
                _isServer = true;

                OperationResponse operationResponse = new OperationResponse();

                operationResponse.OperationCode = 6;
                operationResponse.ReturnCode = 0;
                operationResponse.Parameters = new Dictionary<byte, object>()
                {
                    { 0, 0 },
                };

                SendOperationResponse(operationResponse, sendParameters);
            }

            _handlerList.HandleMessage(new PhotonRequest(operationRequest.OperationCode, operationRequest.Parameters.ContainsKey(_server.SubCodeParameterKey) ? (int?)Convert.ToInt32(operationRequest.Parameters[_server.SubCodeParameterKey]) : null, operationRequest.Parameters), this);
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            _server.ConnectionCollection.OnClientDisconnect(this);

            Log.DebugFormat("Client {0} disconnected", _peerId);
        }

        public Guid PeerId
        {
            get { return _peerId; }
        }

        public Dictionary<Type, ClientData> ClientData
        {
            get { return _clientData; }
        }

        private int IncrementInputSequenceNo(byte currentInputSequenceNo)
        {
            int nextInputSequenceNoTemp = currentInputSequenceNo + 1;

            if (nextInputSequenceNoTemp > 255)
                nextInputSequenceNoTemp = 0;

            return nextInputSequenceNoTemp;
        }

        //public T ClientData<T>() where T : ClientData
        //{
        //    ClientData result;

        //    _clientData.TryGetValue(typeof(T), out result);

        //    if (result != null)
        //    {
        //        return result as T;
        //    }

        //    return null;
        //}
    }
}