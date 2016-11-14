﻿using System;
using System.Collections.Generic;

using UnityEngine;

using ExitGames.Client.Photon;

[RequireComponent(typeof(HoverMotor))]
public class HoverCarNetworkInterface : MonoBehaviour
{
    public Guid PeerId;

    public bool IsLocalPeer = false;

    public bool IsDestroyed = false;

    public void FixedUpdate()
    {
        if (IsDestroyed)
        {
            Destroy(gameObject);
        }
    }

    // Server-side request to receive server-side inputs (OPERATION_CODE_3).

    public void RequestClientInputs()
    {
        OperationRequest operationRequest = new OperationRequest();

        operationRequest.OperationCode = 3;
        operationRequest.Parameters = new Dictionary<byte, object>()
        {
            { 1, PeerId.ToByteArray() }
        };

        PhotonEngine.Instance.SendOp(operationRequest, false, 0, false);
    }

    // Client-side request to receive server-side transform (OPERATION_CODE_1).

    public void RequestServerTransform()
    {
        OperationRequest operationRequest = new OperationRequest();

        operationRequest.OperationCode = 1;
        operationRequest.Parameters = new Dictionary<byte, object>()
        {
            { 1, PeerId.ToByteArray() }
        };

        PhotonEngine.Instance.SendOp(operationRequest, false, 0, false);
    }

    // Client-side request to update server-side inputs (OPERATION_CODE_4).

    public void SendClientInputs(byte sequenceNo, float powerInput, float turnInput, bool isJumping)
    {
        OperationRequest operationRequest = new OperationRequest();

        operationRequest.OperationCode = 4;
        operationRequest.Parameters = new Dictionary<byte, object>()
        {
            { 1, sequenceNo },
            { 2, powerInput },
            { 3, turnInput },
            { 4, isJumping }
        };

        PhotonEngine.Instance.SendOp(operationRequest, false, 0, false);
    }

    // Server-side request to update server-side transform (OPERATION_CODE_2).

    public void SendServerTransform()
    {
        OperationRequest operationRequest = new OperationRequest();

        operationRequest.OperationCode = 2;
        operationRequest.Parameters = new Dictionary<byte, object>()
        {
            { 1, PeerId.ToByteArray() },
            { 2, transform.position.x },
            { 3, transform.position.y },
            { 4, transform.position.z },
            { 5, transform.rotation.eulerAngles.x },
            { 6, transform.rotation.eulerAngles.y },
            { 7, transform.rotation.eulerAngles.z }
        };

        PhotonEngine.Instance.SendOp(operationRequest, false, 0, false);
    }
}