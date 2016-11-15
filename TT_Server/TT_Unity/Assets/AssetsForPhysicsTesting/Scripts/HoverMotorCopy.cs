﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotorCopy : MonoBehaviour
{
    public float HoverForce = 100.0f;
    public float HoverHeight = 4.0f;
    public float FloatHeight = 8.0f;
    public float Speed = 200.0f;
    public float BackwardsSpeed = 100.0f;
    public float TurnSpeed = 80f;
    public float MaxDrag = 4f;
    public float TrackSuction = 1f;

    public Transform[] Thrusters;
    public Transform Centre;

    Rigidbody CarRigidBody;

    private int TrackMask;

    private float powerInput = 0.0f;
    private float turnInput = 0.0f;

    private float _distanceRatio;
    private float _amountOverThrustors;

    void Start()
    {
        CarRigidBody = GetComponent<Rigidbody>();

        TrackMask = 1 << LayerMask.NameToLayer("Track");
    }

    internal void Move(float powerInputIn, float turnInputIn)
    {
        powerInput = powerInputIn;
        turnInput = turnInputIn;
    }

    void FixedUpdate()
    {
        RaycastHit hit;

        float ratioSqrd;

        // if 'close to track' move the vehicle with inputs.
        if (Physics.Raycast(Centre.position, -Centre.up, out hit, FloatHeight, TrackMask))
        {
            //float floatRatio = Mathf.Pow((1.0f - (hit.distance / FloatHeight)), 2);
            _distanceRatio = (1.0f - (hit.distance / FloatHeight));
            if (hit.distance > HoverHeight)
            {
                _amountOverThrustors = (hit.distance - HoverHeight);
            }

            ratioSqrd = _distanceRatio * _distanceRatio;

            CarRigidBody.AddRelativeForce(ratioSqrd * Vector3.forward * ((powerInput > 0) ? powerInput * Speed * 4 : powerInput * BackwardsSpeed * 4));

            CarRigidBody.AddRelativeTorque(0f, ratioSqrd * turnInput * TurnSpeed, 0f);

            CarRigidBody.drag = _distanceRatio * MaxDrag;
        }
        else
        {
            _distanceRatio = 1f;
        }

        foreach (Transform thruster in Thrusters)
        {
            if (Physics.Raycast(thruster.position, -thruster.up, out hit, HoverHeight, TrackMask))
            {
                CarRigidBody.AddForceAtPosition(Vector3.up * HoverForce * Mathf.Pow((1.0f - (hit.distance / HoverHeight)), 2), thruster.position);
            } else
            {
                CarRigidBody.AddForceAtPosition(Vector3.down * HoverForce * _amountOverThrustors * TrackSuction, thruster.position);
            }
        }
    }

    // Gizmo's
    void OnDrawGizmos()
    {
        RaycastHit hit;

        foreach (Transform thruster in Thrusters)
        {
            if (Physics.Raycast(thruster.position, thruster.up * -1, out hit, HoverHeight, TrackMask))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(thruster.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(thruster.position, thruster.position - Vector3.up * HoverHeight);
            }

            if (Physics.Raycast(thruster.position, Centre.up, out hit, HoverHeight, TrackMask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(thruster.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.25f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(thruster.position, thruster.position - Centre.up * HoverHeight);
            }
        }
    }
}