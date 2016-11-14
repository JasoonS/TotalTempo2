using System;
using System.Collections.Generic;

using UnityEngine;

using UnityStandardAssets.Cameras;

using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviour
{
    public GameObject PlayerPrefab;

    private ViewController _controller;

    private Dictionary<Guid, GameObject> _players;

    private List<Transform> _spawnsAvailable;
    private Dictionary<Guid, Transform> _spawnsTaken;

    private AutoCam _camera;

    private uint _updateTickThreshold = 60;

    private uint _updateTick = 60;

    public Dictionary<Guid, GameObject> Players { get { return _players; } }

    public void Start()
    {
        _controller = (ViewController)transform.parent.GetComponent<Login>().Controller;

        _players = new Dictionary<Guid, GameObject>();

        _spawnsAvailable = new List<Transform>();
        _spawnsTaken = new Dictionary<Guid, Transform>();

        Transform playerSpawns = transform.parent.parent.GetChild(1);

        for (int i = 0; i < playerSpawns.childCount; ++i)
        {
            _spawnsAvailable.Add(playerSpawns.GetChild(i));
        }

        _camera = GameObject.Find("Cameras").transform.GetChild(0).GetComponent<AutoCam>();
    }

    public void FixedUpdate()
    {
        ++_updateTick;

        OperationRequest operationRequest;

        if ((PhotonEngine.Instance.IsServer) && (!((PeerIdHandler)_controller.OperationHandlers[0]).HasSentServerStatus))
        {
            operationRequest = new OperationRequest();

            operationRequest.OperationCode = 6;

            PhotonEngine.Instance.SendOp(operationRequest, false, 0, false);
        }

        else if (!((PeerIdHandler)_controller.OperationHandlers[0]).HasRequestedMyPeerId)
        {
            operationRequest = new OperationRequest();

            operationRequest.OperationCode = 5;

            PhotonEngine.Instance.SendOp(operationRequest, false, 0, false);
        }

        else
        {
            if (_updateTick > _updateTickThreshold)
            {
                _updateTick = 0;

                operationRequest = new OperationRequest();

                operationRequest.OperationCode = 0;

                PhotonEngine.Instance.SendOp(operationRequest, false, 1, false);
            }

            List<Guid> peerIds = ((PeerIdHandler)_controller.OperationHandlers[0]).PeerIds;
            Dictionary<Guid, bool> peerIdsAlive = ((PeerIdHandler)_controller.OperationHandlers[0]).PeerIdsAlive;

            for (int i = 0; i < peerIds.Count; ++i)
            {
                Guid peerId = peerIds[i];

                if (!peerIdsAlive[peerId])
                {
                    if (_players[peerId].GetComponent<HoverCarNetworkInterface>().IsLocalPeer)
                    {
                        Debug.Log("You were disconnected from the server.");
                    }

                    peerIds.RemoveAt(i);
                    peerIdsAlive.Remove(peerId);

                    _spawnsAvailable.Add(_spawnsTaken[peerId]);

                    _spawnsTaken.Remove(peerId);

                    _players[peerId].GetComponent<HoverCarNetworkInterface>().IsDestroyed = true;
                    _players.Remove(peerId);

                    ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs.Remove(peerId);
                    ((PlayerTransformHandler)_controller.OperationHandlers[2]).PlayerTransforms.Remove(peerId);
                }

                else if ((!_players.ContainsKey(peerId)) && (_spawnsAvailable.Count > 0))
                {
                    CreatePlayer(peerId);
                }
            }
        }
    }

    private void CreatePlayer(Guid peerId)
    {
        Transform playerSpawn = _spawnsAvailable[0];

        _spawnsAvailable.RemoveAt(0);

        _spawnsTaken.Add(peerId, playerSpawn);

        GameObject player = (GameObject)Instantiate(PlayerPrefab, new Vector3(playerSpawn.position.x, playerSpawn.position.y + 8, playerSpawn.position.z), playerSpawn.rotation);

        player.GetComponent<HoverCarNetworkInterface>().PeerId = peerId;

        if (PhotonEngine.Instance.IsServer)
        {
            ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs.Add(peerId, new PlayerInput[256]);
        }

        else if (peerId == ((PeerIdHandler)_controller.OperationHandlers[0]).MyPeerId)
        {
            player.GetComponent<HoverCarNetworkInterface>().IsLocalPeer = true;

            _camera.Target = player.transform;

            _camera.gameObject.SetActive(true);

            PlayerInput[] playerInputs = new PlayerInput[256];

            for (int i = 0; i < 256; ++i)
            {
                playerInputs[i] = new PlayerInput();
            }

            ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs.Add(peerId, playerInputs);
        }

        ((PlayerTransformHandler)_controller.OperationHandlers[2]).PlayerTransforms.Add(peerId, new PlayerTransform());

        _players.Add(peerId, player);
    }
}