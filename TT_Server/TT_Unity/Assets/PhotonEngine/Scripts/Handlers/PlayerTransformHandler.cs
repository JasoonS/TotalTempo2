using System;
using System.Collections.Generic;

using ExitGames.Client.Photon;

public class PlayerTransformHandler : PhotonOperationHandler
{
    private Dictionary<Guid, PlayerTransform> _playerTransforms;

    public Dictionary<Guid, PlayerTransform> PlayerTransforms { get { return _playerTransforms; } }

    public PlayerTransformHandler(ViewController controller) : base(controller)
    {
        _playerTransforms = new Dictionary<Guid, PlayerTransform>();
    }

    public override byte Code { get { return 2; } }

    public override void OnHandleResponse(OperationResponse response)
    {
        Guid peerId = new Guid((byte[])response.Parameters[1]);

        if (_playerTransforms.ContainsKey(peerId))
        {
            PlayerTransform playerTransform = _playerTransforms[peerId];

            playerTransform.xPosition = (float)response.Parameters[2];
            playerTransform.yPosition = (float)response.Parameters[3];
            playerTransform.zPosition = (float)response.Parameters[4];

            playerTransform.xRotation = (float)response.Parameters[5];
            playerTransform.yRotation = (float)response.Parameters[6];
            playerTransform.zRotation = (float)response.Parameters[7];

            _playerTransforms[peerId] = playerTransform;
        }
    }
}