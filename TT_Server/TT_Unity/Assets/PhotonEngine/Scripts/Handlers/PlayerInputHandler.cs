// Class to handle player input-related network operations.

using System;
using System.Collections.Generic;

using ExitGames.Client.Photon;

public class PlayerInputHandler : PhotonOperationHandler
{
    private Dictionary<Guid, PlayerInput> _playerInputs;

    public Dictionary<Guid, PlayerInput> PlayerInputs { get { return _playerInputs; } }

    public PlayerInputHandler(ViewController controller) : base(controller)
    {
        _playerInputs = new Dictionary<Guid, PlayerInput>();
    }

    public override byte Code { get { return 1; } }

    public override void OnHandleResponse(OperationResponse response)
    {
        Guid peerId = new Guid((byte[])response.Parameters[1]);

        if (_playerInputs.ContainsKey(peerId))
        {
            PlayerInput playerInput = _playerInputs[peerId];

            playerInput.powerInput = (float)response.Parameters[2];
            playerInput.turnInput = (float)response.Parameters[3];

            playerInput.isJumping = (bool)response.Parameters[4];

            _playerInputs[peerId] = playerInput;
        }
    }
}