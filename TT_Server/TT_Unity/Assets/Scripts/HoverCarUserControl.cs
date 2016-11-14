using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(HoverMotor))]
public class HoverCarUserControl : MonoBehaviour
{
    private HoverMotor _hoverMotor;

    private HoverCarNetworkInterface _networkInterface;

    private ViewController _controller;

    private byte _currentInputSequenceNo = 0;

    private float _powerInput = 0.0f;
    private float _turnInput = 0.0f;

    private bool _isJumping = false;

    private uint _updateTickThreshold = 60;

    private uint _updateTick = 60;

    public void Start()
    {
        _hoverMotor = GetComponent<HoverMotor>();

        _networkInterface = GetComponent<HoverCarNetworkInterface>();

        _controller = (ViewController)GameObject.Find("Login").GetComponent<Login>().Controller;
    }

    public void Update()
    {
        if (!PhotonEngine.Instance.IsServer)
        {
            if (_networkInterface.IsLocalPeer)
            {
                GetPlayerInputs();
            }
        }
    }

    public void FixedUpdate()
    {
        ++_updateTick;

        if (PhotonEngine.Instance.IsServer)
        {
            // SERVER_SIDE.

            _networkInterface.RequestClientInputs();

            UpdateInputs();

            if (_updateTick > _updateTickThreshold)
            {
                _updateTick = 0;

                _networkInterface.SendServerTransform();
            }
        }

        else
        {
            // CLIENT_SIDE.

            if (_updateTick > _updateTickThreshold)
            {
                _networkInterface.RequestServerTransform();

                UpdateTransform();
            }

            PlayerInput playerInput = new PlayerInput();

            playerInput.isACKed = false;

            playerInput.powerInput = _powerInput;
            playerInput.turnInput = _turnInput;

            playerInput.isJumping = _isJumping;

            ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs[_networkInterface.PeerId][_currentInputSequenceNo] = playerInput;

            IncrementInputSequenceNo();

            SendInputs();
        }

        _hoverMotor.Move(_powerInput, _turnInput);

        if (_isJumping)
        {
            _hoverMotor.Jump();
        }
    }

    // Method to increment the sequence number (with ROLLOVER).

    private void IncrementInputSequenceNo()
    {
        int currentInputSequenceNoTemp = _currentInputSequenceNo + 1;

        if (currentInputSequenceNoTemp > 255)
            currentInputSequenceNoTemp = 0;

        _currentInputSequenceNo = (byte)currentInputSequenceNoTemp;
    }

    // Method to get the current player inputs (CLIENT_SIDE).

    private void GetPlayerInputs()
    {
        _powerInput = Input.GetAxis("Vertical");
        _turnInput = Input.GetAxis("Horizontal");

        _isJumping = Input.GetButtonDown("Jump");
    }

    // Method to send all unACKed inputs (CLIENT_SIDE).

    private void SendInputs()
    {
        Debug.Log("***********************************************************************************************************************");

        PlayerInput[] playerInputs = ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs[_networkInterface.PeerId];

        for (int i = 0; i < 256; ++i)
        {
            PlayerInput playerInput = playerInputs[i];

            if (!playerInput.isACKed)
            {
                Debug.LogFormat("{0}|{1},{2},{3}", i, _powerInput, _turnInput, _isJumping);

                _networkInterface.SendClientInputs((byte)(i), playerInput.powerInput, playerInput.turnInput, playerInput.isJumping);
            }
        }
    }

    // Method to update inputs (SERVER_SIDE).

    private void UpdateInputs()
    {
        PlayerInput playerInput = ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs[_networkInterface.PeerId][_currentInputSequenceNo];

        if (!playerInput.isACKed)
        {
            _powerInput = playerInput.powerInput;
            _turnInput = playerInput.turnInput;

            _isJumping = playerInput.isJumping;

            ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs[_networkInterface.PeerId][_currentInputSequenceNo].isACKed = true;

            IncrementInputSequenceNo();
        }

        else
        {
            _powerInput = 0.0f;
            _turnInput = 0.0f;

            _isJumping = false;
        }
    }

    // Method to update transform (CLIENT_SIDE).

    private void UpdateTransform()
    {
        PlayerTransform currentTransform = ((PlayerTransformHandler)_controller.OperationHandlers[2]).PlayerTransforms[_networkInterface.PeerId];

        if (currentTransform.isUpdated)
        {
            transform.position = new Vector3(currentTransform.xPosition, currentTransform.yPosition, currentTransform.zPosition);
            transform.eulerAngles = new Vector3(currentTransform.xRotation, currentTransform.yRotation, currentTransform.zRotation);

            currentTransform.isUpdated = false;
        }
    }
}