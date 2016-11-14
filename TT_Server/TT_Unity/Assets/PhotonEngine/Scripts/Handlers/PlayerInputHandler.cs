﻿using System;
using System.Collections.Generic;

using ExitGames.Client.Photon;

public class PlayerInputHandler : PhotonOperationHandler
{
    private Dictionary<Guid, Dictionary<byte, PlayerInput>> _playerInputs;

    public Dictionary<Guid, Dictionary<byte, PlayerInput>> PlayerInputs { get { return _playerInputs; } }

    public PlayerInputHandler(ViewController controller) : base(controller)
    {
        _playerInputs = new Dictionary<Guid, Dictionary<byte, PlayerInput>>();
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

                playerInput.powerInput = (float)response.Parameters[3];
                playerInput.turnInput = (float)response.Parameters[4];

                playerInput.isJumping = (bool)response.Parameters[5];

                if (_playerInputs[peerId].ContainsKey(sequenceNo))
                {
                    _playerInputs[peerId][sequenceNo] = playerInput;
                }

                else
                {
                    _playerInputs[peerId].Add(sequenceNo, playerInput);
                }
            }
        }

        else if (response.OperationCode == 4)
        {
            // CLIENT_SIDE.

            Guid myPeerId = ((PeerIdHandler)_controller.OperationHandlers[0]).MyPeerId;

            byte sequenceNo = (byte)response.Parameters[1];

            _playerInputs[myPeerId].Remove(sequenceNo);
        }
    }
}