using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HoverMotorCopy))]
public class TestPlayerInput : MonoBehaviour
{
    private HoverMotorCopy HoverCar; // the car controller we want to use

    private void Awake()
    {
        HoverCar = GetComponent<HoverMotorCopy>();
    }


    private void FixedUpdate()
    {
        float powerInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        HoverCar.Move(powerInput, turnInput);
    }
}