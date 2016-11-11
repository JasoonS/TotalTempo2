// Class to handle client/server peer ID-related network operations.

using System;
using System.Collections.Generic;

using ExitGames.Client.Photon;

public class PeerIdHandler : PhotonOperationHandler
{
    private Guid _myPeerId;

    private List<Guid> _peerIds;
    private Dictionary<Guid, bool> _peerIdsAlive;

    public Guid MyPeerId { get { return _myPeerId; } }

    public List<Guid> PeerIds { get { return _peerIds; } }
    public Dictionary<Guid, bool> PeerIdsAlive { get { return _peerIdsAlive; } }

    public PeerIdHandler(ViewController controller) : base(controller)
    {
        _peerIds = new List<Guid>();
        _peerIdsAlive = new Dictionary<Guid, bool>();
    }

    public override byte Code { get { return 0; } }

    public override void OnHandleResponse(OperationResponse response)
    {
        if (response.OperationCode == 0)
        {
            List<Guid> peerIds = _peerIds;
            Dictionary<Guid, bool> peerIdsAlive = _peerIdsAlive;

            for (int i = 0; i < PeerIds.Count; ++i)
            {
                peerIdsAlive[_peerIds[i]] = false;
            }

            for (int i = 1; i < response.Parameters.Count; ++i)
            {
                Guid peerId = new Guid((byte[])response.Parameters[(byte)i]);

                if (!peerIds.Contains(peerId))
                {
                    peerIds.Add(peerId);
                    peerIdsAlive.Add(peerId, true);
                }

                else
                {
                    peerIdsAlive[peerId] = true;
                }
            }

            _peerIds = peerIds;
            _peerIdsAlive = peerIdsAlive;
        }

        else if (response.OperationCode == 5)
        {
            _myPeerId = new Guid((byte[])response.Parameters[1]);
        }
    }
}