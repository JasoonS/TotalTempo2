using System;
using System.Collections.Generic;

using ExitGames.Client.Photon;

public class PlayerInputHandler : PhotonOperationHandler
{
    private Dictionary<Guid, PlayerInput[]> _playerInputs;

    public Dictionary<Guid, PlayerInput[]> PlayerInputs { get { return _playerInputs; } }

    public PlayerInputHandler(ViewController controller) : base(controller)
    {
        _playerInputs = new Dictionary<Guid, PlayerInput[]>();
    }

    public override byte Code { get { return 1; } }

    public override void OnHandleResponse(OperationResponse response)
    {
        Guid peerId;

        if (response.OperationCode == 3)
        {
            // SERVER_SIDE.

            peerId = new Guid((byte[])response.Parameters[1]);

            if (_playerInputs.ContainsKey(peerId))
            {
                byte sequenceNo = (byte)response.Parameters[2];

                PlayerInput playerInput = new PlayerInput();

                playerInput.isACKed = false;

                playerInput.powerInput = (float)response.Parameters[3];
                playerInput.turnInput = (float)response.Parameters[4];

                playerInput.isJumping = (bool)response.Parameters[5];

                _playerInputs[peerId][sequenceNo] = playerInput;
            }
        }

        else if (response.OperationCode == 4)
        {
            // CLIENT_SIDE.

            Guid myPeerId = ((PeerIdHandler)_controller.OperationHandlers[0]).MyPeerId;

            for (int i = 1; i < (response.Parameters.Count + 1); ++i)
            {
                _playerInputs[myPeerId][(byte)(response.Parameters[(byte)(i)])].isACKed = true;
            }
        }
    }
}