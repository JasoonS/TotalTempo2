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

    private uint _updateTickInputThreshold = 2;
    private uint _updateTickTransformThreshold = 2;

    private uint _updateTickInput = 2;
    private uint _updateTickTransform = 2;

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

                AddPlayerInputs();

                IncrementInputSequenceNo();
            }
        }
    }

    public void FixedUpdate()
    {
        ++_updateTickInput;
        ++_updateTickTransform;

        if (PhotonEngine.Instance.IsServer)
        {
            // SERVER_SIDE.

            //if (_updateTickInput > _updateTickInputThreshold)
            //{
            //    _updateTickInput = 0;

            //    _networkInterface.RequestClientInputs();
            //}

            _networkInterface.RequestClientInputs();

            UpdateInputs();

            //if (_updateTickTransform > _updateTickTransformThreshold)
            //{
            //    _updateTickTransform = 0;

            //    _networkInterface.SendServerTransform();
            //}

            _networkInterface.SendServerTransform();
        }

        else
        {
            // CLIENT_SIDE.

            if (_updateTickTransform > _updateTickTransformThreshold)
            {
                _updateTickTransform = 0;

                _networkInterface.RequestServerTransform();

                UpdateTransform();
            }

            if (_updateTickInput > _updateTickInputThreshold)
            {
                _updateTickInput = 0;

                SendInputs();
            }
        }

        //_hoverMotor.Move(_powerInput, _turnInput);

        //if (_isJumping)
        //{
        //    _hoverMotor.Jump();
        //}
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
        _powerInput = _powerInput = Input.GetAxis("Xbox360ControllerTriggersF") - Input.GetAxis("Xbox360ControllerTriggersB");
        _turnInput = Input.GetAxis("Horizontal");

        _isJumping = Input.GetButtonDown("Jump");
    }

    // Method to add the current player inputs to a rolling array of inputs (CLIENT_SIDE).

    private void AddPlayerInputs()
    {
        PlayerInput playerInput = new PlayerInput();

        playerInput.isACKed = false;

        playerInput.powerInput = _powerInput;
        playerInput.turnInput = _turnInput;

        playerInput.isJumping = _isJumping;

        ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs[_networkInterface.PeerId][_currentInputSequenceNo] = playerInput;
    }

    // Method to send all unACKed inputs (CLIENT_SIDE).

    private void SendInputs()
    {
        Dictionary<byte, PlayerInput> clientInputs = new Dictionary<byte, PlayerInput>();

        PlayerInput[] playerInputs = ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs[_networkInterface.PeerId];

        for (int i = 0; i < 256; ++i)
        {
            PlayerInput playerInput = playerInputs[i];

            if (!playerInput.isACKed)
            {
                clientInputs.Add((byte)(i), playerInput);
            }
        }

        _networkInterface.SendClientInputs(clientInputs);
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

        transform.position = new Vector3(currentTransform.xPosition, currentTransform.yPosition, currentTransform.zPosition);
        transform.eulerAngles = new Vector3(currentTransform.xRotation, currentTransform.yRotation, currentTransform.zRotation);
    }
}