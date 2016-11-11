// Class to receive player transform and send inputs (if client), or receive players inputs and send transform (if on server) for a player racing agent.

using UnityEngine;

[RequireComponent(typeof(HoverMotor))]
public class HoverCarUserControl : MonoBehaviour
{
    private HoverMotor _hoverMotor;

    private HoverCarNetworkInterface _networkInterface;

    private ViewController _controller;

    private float _powerInput = 0.0f;
    private float _turnInput = 0.0f;

    private bool _isJumping = false;

    public void Start()
    {
        _hoverMotor = GetComponent<HoverMotor>();

        _networkInterface = GetComponent<HoverCarNetworkInterface>();

        _controller = (ViewController)GameObject.Find("Login").GetComponent<Login>().Controller;
    }

    public void FixedUpdate()
    {
        if (PhotonEngine.Instance.IsServer)
        {
            _networkInterface.RequestClientInputs();

            UpdateInputs();

            _networkInterface.SendServerTransform();
        }

        else
        {
            _networkInterface.RequestServerTransform();

            UpdateTransform();

            if (_networkInterface.IsLocalPeer)
            {
                GetPlayerInputs();
            }

            _networkInterface.SendClientInputs(_powerInput, _turnInput, _isJumping);
        }

        _hoverMotor.Move(_powerInput, _turnInput);

        if (_isJumping)
        {
            _hoverMotor.Jump();
        }
    }

    private void GetPlayerInputs()
    {
        _powerInput = Input.GetAxis("Vertical");
        _turnInput = Input.GetAxis("Horizontal");

        _isJumping = Input.GetButtonDown("Jump");
    }

    // Method to update inputs.

    private void UpdateInputs()
    {
        PlayerInput currentInput = ((PlayerInputHandler)_controller.OperationHandlers[1]).PlayerInputs[_networkInterface.PeerId];

        _powerInput = currentInput.powerInput;
        _turnInput = currentInput.turnInput;

        _isJumping = currentInput.isJumping;
    }

    // Method to update transform.

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