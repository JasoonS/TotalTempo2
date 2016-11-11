// Class to receive player inputs, and move the player racing agent.

using UnityEngine;

[RequireComponent(typeof(HoverMotor))]
public class HoverCarUserControlOffline : MonoBehaviour
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
    }

    public void FixedUpdate()
    {
        GetPlayerInputs();

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
}