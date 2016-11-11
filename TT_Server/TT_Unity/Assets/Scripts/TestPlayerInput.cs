using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HoverMotor))]
public class TestPlayerInput : MonoBehaviour
{
    private HoverMotor HoverCar; // the car controller we want to use

    private void Awake()
    {
        HoverCar = GetComponent<HoverMotor>();
    }


    private void FixedUpdate()
    {
        float powerInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        HoverCar.Move(powerInput, turnInput);

        if (Input.GetButtonDown("Jump"))
        {
            HoverCar.Jump();
        }
    }
}